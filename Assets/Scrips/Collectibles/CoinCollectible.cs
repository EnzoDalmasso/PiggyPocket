using UnityEngine;

// Pickup de moneda reutilizable.
// El valor permite crear monedas bronze, silver, gold u otras variantes con el mismo script.
[RequireComponent(typeof(Collider2D))]
public class CoinCollectible : MonoBehaviour
{
    // Cantidad de monedas que suma al Player al recolectarla.
    [SerializeField] private int valor = 1;

    [Header("Audio")]
    [SerializeField] private AudioClip sonidoRecolectar;

    [Header("Efecto visual")]
    [SerializeField] private Sprite[] spritesEfectoRecolectar;
    [SerializeField] private float cuadrosPorSegundoEfecto = 18f;
    [SerializeField] private Vector3 offsetEfecto = new Vector3(0f, 0.05f, 0f);
    [SerializeField] private int ordenExtraEfecto = 2;

    private Collider2D trigger;
    private SpriteRenderer spriteRenderer;
    private bool recolectado;

    void Awake()
    {
        trigger = GetComponent<Collider2D>();
        trigger.isTrigger = true;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        IntentarRecolectar(other);
    }

    private void IntentarRecolectar(Collider2D other)
    {
        if(recolectado)
        {
            return;
        }

        PlayerWallet playerWallet = other.GetComponentInParent<PlayerWallet>();

        if(playerWallet == null)
        {
            return;
        }

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();

        if(playerHealth != null && playerHealth.EstaMuerto)
        {
            return;
        }

        recolectado = true;
        trigger.enabled = false;
        playerWallet.AgregarMonedas(valor);
        GameAudioManager.ReproducirMoneda(sonidoRecolectar);
        ReproducirEfectoRecolectar();
        Destroy(gameObject);
    }

    private void ReproducirEfectoRecolectar()
    {
        if(spritesEfectoRecolectar == null || spritesEfectoRecolectar.Length == 0)
        {
            return;
        }

        GameObject efecto = new GameObject("VFX_CoinCollect");
        efecto.transform.position = transform.position + offsetEfecto;
        efecto.transform.localScale = transform.lossyScale;

        SpriteRenderer efectoRenderer = efecto.AddComponent<SpriteRenderer>();
        efectoRenderer.sprite = spritesEfectoRecolectar[0];

        if(spriteRenderer != null)
        {
            efectoRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
            efectoRenderer.sortingOrder = spriteRenderer.sortingOrder + ordenExtraEfecto;
            efectoRenderer.sharedMaterial = spriteRenderer.sharedMaterial;
        }

        CoinCollectEffect efectoAnimado = efecto.AddComponent<CoinCollectEffect>();
        efectoAnimado.Configurar(spritesEfectoRecolectar, cuadrosPorSegundoEfecto);
    }
}

// Reproduce una secuencia corta de sprites y destruye el objeto al terminar.
// Se usa para efectos simples sin crear un Animator extra.
public class CoinCollectEffect : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Sprite[] frames;
    private float cuadrosPorSegundo;
    private float contador;
    private int frameActual;

    public void Configurar(Sprite[] nuevosFrames, float nuevosCuadrosPorSegundo)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        frames = nuevosFrames;
        cuadrosPorSegundo = Mathf.Max(1f, nuevosCuadrosPorSegundo);

        if(spriteRenderer == null || frames == null || frames.Length == 0)
        {
            Destroy(gameObject);
            return;
        }

        frameActual = 0;
        contador = 0f;
        spriteRenderer.sprite = frames[frameActual];
    }

    void Update()
    {
        if(spriteRenderer == null || frames == null || frames.Length == 0)
        {
            Destroy(gameObject);
            return;
        }

        contador += Time.deltaTime;
        int nuevoFrame = Mathf.FloorToInt(contador * cuadrosPorSegundo);

        if(nuevoFrame >= frames.Length)
        {
            Destroy(gameObject);
            return;
        }

        if(nuevoFrame != frameActual)
        {
            frameActual = nuevoFrame;
            spriteRenderer.sprite = frames[frameActual];
        }
    }
}
