using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Clase base para cualquier enemigo del juego.
// Aca vive lo comun: vida, recibir dano, morir y dano por contacto.
public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("Vida")]
    [SerializeField] private int vidaMaxima = 3;
    [SerializeField] private bool destruirAlMorir = true;
    [SerializeField] private float tiempoAntesDeDestruir = 0.6f;
    [SerializeField] private bool desactivarColisionesAlMorir = true;
    [SerializeField] private bool detenerFisicaAlMorir = true;

    [Header("Dano por contacto")]
    [SerializeField] private bool haceDanoPorContacto = true;
    [SerializeField] private int danoContacto = 1;
    [SerializeField] private float tiempoEntreGolpesContacto = 1f;
    [SerializeField] private float demoraDanoContacto = 0.15f;
    [SerializeField] private bool danoContactoSoloLateral = true;
    [SerializeField] private float margenContactoSuperior = 0.03f;
    [SerializeField] private float velocidadMinimaCaidaContactoSuperior = -0.05f;

    [Header("Estados opcionales")]
    [SerializeField] private bool aplicaVeneno;
    [SerializeField] private float duracionVeneno = 5f;

    [Header("Feedback visual")]
    [SerializeField] private bool crearFeedbackVisualAutomatico = true;
    [SerializeField] private EnemyHitFeedback hitFeedback;

    private float contadorCooldownContacto;
    private float contadorDemoraContacto;
    private float contadorIgnorarContacto;
    private Collider2D objetivoContactoActual;
    private Collider2D objetivoContactoIgnorado;
    private readonly Dictionary<Collider2D, IDamageable> damageablesPorCollider = new Dictionary<Collider2D, IDamageable>();
    private Rigidbody2D rbPropio;
    private Collider2D colliderPrincipal;

    protected int vidaActual;

    public int VidaActual => vidaActual;
    public int VidaMaxima => vidaMaxima;
    public bool EstaMuerto { get; private set; }

    public event Action MuerteIniciada;

    protected virtual void Awake()
    {
        vidaActual = vidaMaxima;
        rbPropio = GetComponent<Rigidbody2D>();
        colliderPrincipal = GetComponent<Collider2D>();
        AsegurarHitFeedback();
    }

    protected virtual void Update()
    {
        ActualizarCooldownContacto();
        ActualizarEnemigo();
    }

    // Hook para que cada enemigo agregue su comportamiento propio sin tocar la base.
    protected virtual void ActualizarEnemigo()
    {
    }

    public virtual void RecibirDano(int cantidad, Vector2 puntoGolpe, GameObject atacante)
    {
        if(EstaMuerto || cantidad <= 0)
        {
            return;
        }

        vidaActual = Mathf.Max(vidaActual - cantidad, 0);

        ReproducirFeedbackGolpe();
        AlRecibirDano(cantidad, puntoGolpe, atacante);

        if(vidaActual <= 0)
        {
            Morir();
        }
    }

    // Hook para reacciones propias: animacion de hit, sonido, particulas, retroceso, etc.
    protected virtual void AlRecibirDano(int cantidad, Vector2 puntoGolpe, GameObject atacante)
    {
    }

    private void AsegurarHitFeedback()
    {
        if(hitFeedback == null)
        {
            hitFeedback = GetComponentInChildren<EnemyHitFeedback>();
        }

        if(hitFeedback == null && crearFeedbackVisualAutomatico)
        {
            hitFeedback = gameObject.AddComponent<EnemyHitFeedback>();
        }
    }

    private void ReproducirFeedbackGolpe()
    {
        if(hitFeedback == null)
        {
            return;
        }

        hitFeedback.Reproducir();
    }

    protected virtual void Morir()
    {
        if(EstaMuerto)
        {
            return;
        }

        EstaMuerto = true;
        MuerteIniciada?.Invoke();
        AlMorir();
        DetenerFisicaSiCorresponde();
        DesactivarColisionesSiCorresponde();

        if(destruirAlMorir)
        {
            Destroy(gameObject, tiempoAntesDeDestruir);
        }
        else
        {
            Invoke(nameof(DesactivarObjeto), tiempoAntesDeDestruir);
        }
    }

    // Hook para que un enemigo concreto pueda reaccionar al morir.
    protected virtual void AlMorir()
    {
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        IntentarDanarPorContacto(collision);
    }

    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        IntentarDanarPorContacto(collision);
    }

    protected virtual void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.collider == objetivoContactoActual)
        {
            objetivoContactoActual = null;
            contadorDemoraContacto = 0;
        }

        damageablesPorCollider.Remove(collision.collider);
    }

    protected virtual void IntentarDanarPorContacto(Collision2D collision)
    {
        if(collision == null || (danoContactoSoloLateral && EsContactoSuperior(collision)))
        {
            return;
        }

        IntentarDanarPorContacto(collision.collider);
    }

    protected virtual void IntentarDanarPorContacto(Collider2D other)
    {
        if(!haceDanoPorContacto || EstaMuerto || contadorCooldownContacto > 0 || EsMismoObjeto(other) || EsContactoIgnorado(other))
        {
            return;
        }

        IDamageable damageable = BuscarDamageable(other);

        if(damageable == null)
        {
            return;
        }

        if(!ContactoListoParaDanar(other))
        {
            return;
        }

        damageable.RecibirDano(danoContacto, transform.position, gameObject);
        AplicarEstadoAlObjetivo(other);
        contadorCooldownContacto = tiempoEntreGolpesContacto;
    }

    protected virtual void AplicarEstadoAlObjetivo(Collider2D other)
    {
        if(!aplicaVeneno)
        {
            return;
        }

        PlayerStatusEffects statusEffects = other.GetComponentInParent<PlayerStatusEffects>();

        if(statusEffects != null)
        {
            statusEffects.AplicarVeneno(duracionVeneno);
        }
    }

    public void IgnorarDanoContactoTemporal(Collider2D objetivo, float duracion)
    {
        objetivoContactoIgnorado = objetivo;
        contadorIgnorarContacto = duracion;
    }

    protected IDamageable BuscarDamageable(Collider2D objetivo)
    {
        if(objetivo == null)
        {
            return null;
        }

        if(damageablesPorCollider.TryGetValue(objetivo, out IDamageable damageableCacheado))
        {
            return damageableCacheado;
        }

        IDamageable damageable = objetivo.GetComponentInParent<IDamageable>();

        if(damageable is MonoBehaviour componenteDamageable && componenteDamageable == this)
        {
            damageable = null;
        }

        damageablesPorCollider[objetivo] = damageable;
        return damageable;
    }

    private void ActualizarCooldownContacto()
    {
        if(contadorCooldownContacto > 0)
        {
            contadorCooldownContacto -= Time.deltaTime;
        }

        if(contadorIgnorarContacto > 0)
        {
            contadorIgnorarContacto -= Time.deltaTime;

            if(contadorIgnorarContacto <= 0)
            {
                objetivoContactoIgnorado = null;
            }
        }
    }

    private bool EsMismoObjeto(Collider2D other)
    {
        if(other == null)
        {
            return true;
        }

        if(other.transform == transform || other.transform.IsChildOf(transform))
        {
            return true;
        }

        if(rbPropio == null)
        {
            rbPropio = GetComponent<Rigidbody2D>();
        }

        return rbPropio != null && other.attachedRigidbody == rbPropio;
    }

    private bool ContactoListoParaDanar(Collider2D other)
    {
        if(demoraDanoContacto <= 0)
        {
            return true;
        }

        if(objetivoContactoActual != other)
        {
            objetivoContactoActual = other;
            contadorDemoraContacto = demoraDanoContacto;
            return false;
        }

        if(contadorDemoraContacto > 0)
        {
            contadorDemoraContacto -= Time.deltaTime;
            return false;
        }

        return true;
    }

    private bool EsContactoIgnorado(Collider2D other)
    {
        return objetivoContactoIgnorado != null && other == objetivoContactoIgnorado && contadorIgnorarContacto > 0;
    }

    private bool EsContactoSuperior(Collision2D collision)
    {
        if(collision.collider == null)
        {
            return false;
        }

        Rigidbody2D rbObjetivo = collision.collider.attachedRigidbody;

        if(rbObjetivo == null || rbObjetivo.linearVelocity.y > velocidadMinimaCaidaContactoSuperior)
        {
            return false;
        }

        Collider2D colliderEnemigo = collision.otherCollider;

        if(colliderEnemigo == null)
        {
            if(colliderPrincipal == null)
            {
                colliderPrincipal = GetComponent<Collider2D>();
            }

            colliderEnemigo = colliderPrincipal;
        }

        if(colliderEnemigo == null)
        {
            return false;
        }

        float parteBajaObjetivo = collision.collider.bounds.min.y;
        float parteAltaEnemigo = colliderEnemigo.bounds.max.y;

        if(parteBajaObjetivo < parteAltaEnemigo - margenContactoSuperior)
        {
            return false;
        }

        for(int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contacto = collision.GetContact(i);

            if(contacto.point.y >= parteAltaEnemigo - margenContactoSuperior)
            {
                return true;
            }
        }

        return false;
    }

    private void DesactivarColisionesSiCorresponde()
    {
        if(!desactivarColisionesAlMorir)
        {
            return;
        }

        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();

        foreach(Collider2D colliderEnemigo in colliders)
        {
            colliderEnemigo.enabled = false;
        }
    }

    private void DetenerFisicaSiCorresponde()
    {
        if(!detenerFisicaAlMorir)
        {
            return;
        }

        if(rbPropio == null)
        {
            rbPropio = GetComponent<Rigidbody2D>();
        }

        if(rbPropio == null)
        {
            return;
        }

        rbPropio.linearVelocity = Vector2.zero;
        rbPropio.angularVelocity = 0;
        rbPropio.gravityScale = 0;
        rbPropio.bodyType = RigidbodyType2D.Kinematic;
    }

    private void DesactivarObjeto()
    {
        gameObject.SetActive(false);
    }
}

// Feedback visual generico para enemigos al recibir dano.
// Se puede reutilizar en cualquier enemigo futuro sin tocar su IA.
public class EnemyHitFeedback : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] spriteRenderers;
    [SerializeField] private Color colorFlash = Color.white;
    [SerializeField] private float duracionFlash = 0.06f;
    [SerializeField] private Color colorGolpe = new Color(1f, 0.18f, 0.18f, 1f);
    [SerializeField] private float duracionColorGolpe = 0.12f;
    [SerializeField] private float escalaGolpe = 1.16f;
    [SerializeField] private float duracionEscala = 0.16f;

    private Color[] coloresOriginales;
    private Vector3 escalaBase;
    private Coroutine rutinaFeedback;

    void Awake()
    {
        AsegurarReferencias();
    }

    public void Reproducir()
    {
        AsegurarReferencias();

        if(rutinaFeedback != null)
        {
            StopCoroutine(rutinaFeedback);
            RestaurarColores();
            transform.localScale = escalaBase;
        }

        GuardarColoresOriginales();
        escalaBase = transform.localScale;
        rutinaFeedback = StartCoroutine(ReproducirRutina());
    }

    private IEnumerator ReproducirRutina()
    {
        AplicarColor(colorFlash);
        transform.localScale = escalaBase * escalaGolpe;

        float duracionColorTotal = duracionFlash + duracionColorGolpe;
        float duracionTotal = Mathf.Max(duracionColorTotal, duracionEscala);
        float contador = 0f;
        bool colorGolpeAplicado = false;
        bool coloresRestaurados = false;

        while(contador < duracionTotal)
        {
            contador += Time.deltaTime;

            if(!colorGolpeAplicado && contador >= duracionFlash)
            {
                AplicarColor(colorGolpe);
                colorGolpeAplicado = true;
            }

            if(!coloresRestaurados && contador >= duracionColorTotal)
            {
                RestaurarColores();
                coloresRestaurados = true;
            }

            if(duracionEscala > 0)
            {
                float progresoEscala = Mathf.Clamp01(contador / duracionEscala);
                transform.localScale = Vector3.Lerp(escalaBase * escalaGolpe, escalaBase, progresoEscala);
            }

            yield return null;
        }

        RestaurarColores();
        transform.localScale = escalaBase;
        rutinaFeedback = null;
    }

    private void AsegurarReferencias()
    {
        if(spriteRenderers == null || spriteRenderers.Length == 0)
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        }

        if(coloresOriginales == null || coloresOriginales.Length != spriteRenderers.Length)
        {
            coloresOriginales = new Color[spriteRenderers.Length];
        }
    }

    private void GuardarColoresOriginales()
    {
        for(int i = 0; i < spriteRenderers.Length; i++)
        {
            coloresOriginales[i] = spriteRenderers[i] != null ? spriteRenderers[i].color : Color.white;
        }
    }

    private void AplicarColor(Color color)
    {
        foreach(SpriteRenderer spriteRenderer in spriteRenderers)
        {
            if(spriteRenderer != null)
            {
                spriteRenderer.color = color;
            }
        }
    }

    private void RestaurarColores()
    {
        for(int i = 0; i < spriteRenderers.Length; i++)
        {
            if(spriteRenderers[i] != null && i < coloresOriginales.Length)
            {
                spriteRenderers[i].color = coloresOriginales[i];
            }
        }
    }
}
