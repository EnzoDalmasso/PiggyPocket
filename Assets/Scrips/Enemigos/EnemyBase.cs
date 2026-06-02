using System;
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

    private float contadorCooldownContacto;
    private float contadorDemoraContacto;
    private float contadorIgnorarContacto;
    private Collider2D objetivoContactoActual;
    private Collider2D objetivoContactoIgnorado;

    protected int vidaActual;

    public int VidaActual => vidaActual;
    public int VidaMaxima => vidaMaxima;
    public bool EstaMuerto { get; private set; }

    public event Action MuerteIniciada;

    protected virtual void Awake()
    {
        vidaActual = vidaMaxima;
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
        Debug.Log(name + " recibio " + cantidad + " de dano. Vida: " + vidaActual, this);

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

    protected virtual void Morir()
    {
        if(EstaMuerto)
        {
            return;
        }

        EstaMuerto = true;
        Debug.Log(name + " fue derrotado.", this);
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
        MonoBehaviour[] componentes = objetivo.GetComponentsInParent<MonoBehaviour>();

        foreach(MonoBehaviour componente in componentes)
        {
            if(componente != this && componente is IDamageable damageable)
            {
                return damageable;
            }
        }

        return null;
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
        return other == null || other.transform.root == transform.root;
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
            colliderEnemigo = GetComponent<Collider2D>();
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

        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if(rb == null)
        {
            return;
        }

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0;
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void DesactivarObjeto()
    {
        gameObject.SetActive(false);
    }
}
