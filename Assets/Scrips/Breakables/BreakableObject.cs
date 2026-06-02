using System;
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

    private int vidaActual;
    private bool dropsSpawneados;

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
        Debug.Log(name + " recibio " + cantidad + " de dano. Vida: " + vidaActual, this);

        if(vidaActual <= 0)
        {
            Romper();
        }
    }

    private void Romper()
    {
        if(EstaRoto)
        {
            return;
        }

        EstaRoto = true;
        Roto?.Invoke();

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
    }
}
