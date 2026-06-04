using System;
using System.Collections;
using UnityEngine;

// Base reutilizable para barriles, cajas, rocas u otros objetos rompibles.
// Implementa IDamageable para que el ataque del Player pueda romperlo.
[RequireComponent(typeof(Collider2D))]
public class BreakableObject : MonoBehaviour, IDamageable
{
    [Header("Vida")]
    [SerializeField] private int vidaMaxima = 1;
    [SerializeField] private bool destruirAlRomper = true;
    [SerializeField] private float tiempoAntesDeDestruir = 0.6f;

    [Header("Animacion")]
    [SerializeField] private Animator animator;
    [SerializeField] private string nombreAnimacionRuptura = "break";

    [Header("Drops")]
    [SerializeField] private DropSpawner dropSpawner;
    [SerializeField] private float demoraSpawnDrops = 0.15f;

    [Header("Colisiones")]
    [SerializeField] private bool desactivarColisionesAlRomper = true;

    [Header("Feedback")]
    [SerializeField] private float duracionFeedbackGolpe = 0.08f;
    [SerializeField] private float escalaFeedbackGolpe = 1.08f;
    [SerializeField] private Color colorFeedbackGolpe = new Color(1f, 0.65f, 0.35f, 1f);

    private int vidaActual;
    private bool dropsSpawneados;
    private SpriteRenderer[] spriteRenderers;
    private Color[] coloresOriginales;
    private Vector3 escalaOriginal;
    private Coroutine feedbackGolpeCoroutine;

    public int VidaActual => vidaActual;
    public int VidaMaxima => vidaMaxima;
    public bool EstaRoto { get; private set; }

    public event Action Roto;

    void Awake()
    {
        vidaActual = vidaMaxima;
        AsegurarReferencias();
    }

    public void RecibirDano(int cantidad, Vector2 puntoGolpe, GameObject atacante)
    {
        if(EstaRoto || cantidad <= 0)
        {
            return;
        }

        vidaActual = Mathf.Max(vidaActual - cantidad, 0);

        if(vidaActual <= 0)
        {
            Romper();
            return;
        }

        ReproducirFeedbackGolpe();
    }

    private void Romper()
    {
        if(EstaRoto)
        {
            return;
        }

        EstaRoto = true;
        Roto?.Invoke();
        GameAudioManager.ReproducirRompible();

        ReproducirAnimacionRuptura();

        if(desactivarColisionesAlRomper)
        {
            DesactivarColisiones();
        }

        if(demoraSpawnDrops <= 0)
        {
            SpawnearDrops();
        }
        else
        {
            Invoke(nameof(SpawnearDrops), demoraSpawnDrops);
        }

        if(destruirAlRomper)
        {
            Destroy(gameObject, tiempoAntesDeDestruir);
        }
    }

    private void ReproducirAnimacionRuptura()
    {
        if(animator == null || string.IsNullOrWhiteSpace(nombreAnimacionRuptura))
        {
            return;
        }

        int hash = Animator.StringToHash(nombreAnimacionRuptura);

        if(animator.HasState(0, hash))
        {
            animator.Play(hash, 0, 0f);
        }
    }

    private void SpawnearDrops()
    {
        if(dropsSpawneados || dropSpawner == null)
        {
            return;
        }

        dropsSpawneados = true;
        dropSpawner.Spawnear();
    }

    private void DesactivarColisiones()
    {
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();

        foreach(Collider2D colliderRompible in colliders)
        {
            colliderRompible.enabled = false;
        }
    }

    private void AsegurarReferencias()
    {
        if(animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if(dropSpawner == null)
        {
            dropSpawner = GetComponent<DropSpawner>();
        }

        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        coloresOriginales = new Color[spriteRenderers.Length];

        for(int i = 0; i < spriteRenderers.Length; i++)
        {
            coloresOriginales[i] = spriteRenderers[i].color;
        }

        escalaOriginal = transform.localScale;
    }

    private void ReproducirFeedbackGolpe()
    {
        if(duracionFeedbackGolpe <= 0)
        {
            return;
        }

        if(feedbackGolpeCoroutine != null)
        {
            StopCoroutine(feedbackGolpeCoroutine);
            RestaurarFeedbackGolpe();
        }

        feedbackGolpeCoroutine = StartCoroutine(FeedbackGolpe());
    }

    private IEnumerator FeedbackGolpe()
    {
        transform.localScale = escalaOriginal * escalaFeedbackGolpe;

        foreach(SpriteRenderer spriteRenderer in spriteRenderers)
        {
            if(spriteRenderer != null)
            {
                spriteRenderer.color = colorFeedbackGolpe;
            }
        }

        yield return new WaitForSeconds(duracionFeedbackGolpe);

        RestaurarFeedbackGolpe();
        feedbackGolpeCoroutine = null;
    }

    private void RestaurarFeedbackGolpe()
    {
        transform.localScale = escalaOriginal;

        for(int i = 0; i < spriteRenderers.Length; i++)
        {
            if(spriteRenderers[i] != null)
            {
                spriteRenderers[i].color = coloresOriginales[i];
            }
        }
    }
}
