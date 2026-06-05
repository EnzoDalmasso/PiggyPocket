using System.Collections.Generic;
using UnityEngine;

// Hazard reutilizable para bombas, pociones u objetos que aplican efectos en area.
// Puede activarse solo despues de una demora o al tocar al Player.
[RequireComponent(typeof(Collider2D))]
public class AreaEffectHazard : MonoBehaviour
{
    [Header("Activacion")]
    [SerializeField] private bool activarAutomaticamente = true;
    [SerializeField] private bool activarAlTocarPlayer = true;
    [SerializeField] private float demoraActivacion = 1f;
    [SerializeField] private float tiempoAntesDeDestruir = 0.6f;

    [Header("Area")]
    [SerializeField] private float radioArea = 0.45f;
    [SerializeField] private LayerMask capasObjetivos;

    [Header("Efectos")]
    [SerializeField] private int dano;
    [SerializeField] private bool aplicaVeneno;
    [SerializeField] private float duracionVeneno = 5f;

    [Header("Animacion")]
    [SerializeField] private Animator animator;
    [SerializeField] private string nombreAnimacionActivacion = "explode";

    private Collider2D trigger;
    private float contadorActivacion;
    private bool activado;

    void Awake()
    {
        trigger = GetComponent<Collider2D>();
        trigger.isTrigger = true;
        contadorActivacion = demoraActivacion;

        if(animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    void Update()
    {
        if(!activarAutomaticamente || activado)
        {
            return;
        }

        contadorActivacion -= Time.deltaTime;

        if(contadorActivacion <= 0)
        {
            Activar();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(!activarAlTocarPlayer || activado || other.GetComponentInParent<PlayerHealth>() == null)
        {
            return;
        }

        Activar();
    }

    public void Activar()
    {
        if(activado)
        {
            return;
        }

        activado = true;
        ReproducirSonidoActivacion();
        ReproducirAnimacionActivacion();
        AplicarEfectosEnArea();

        if(trigger != null)
        {
            trigger.enabled = false;
        }

        Destroy(gameObject, tiempoAntesDeDestruir);
    }

    private void ReproducirSonidoActivacion()
    {
        if(aplicaVeneno)
        {
            GameAudioManager.ReproducirVeneno();
            return;
        }

        GameAudioManager.ReproducirExplosion();
    }

    private void AplicarEfectosEnArea()
    {
        Collider2D[] objetivos = BuscarObjetivos();
        HashSet<MonoBehaviour> damageablesProcesados = new HashSet<MonoBehaviour>();
        HashSet<PlayerStatusEffects> estadosProcesados = new HashSet<PlayerStatusEffects>();

        foreach(Collider2D objetivo in objetivos)
        {
            if(objetivo == null)
            {
                continue;
            }

            if(dano > 0)
            {
                IDamageable damageable = BuscarDamageable(objetivo);

                if(damageable is MonoBehaviour componenteDamageable && damageablesProcesados.Add(componenteDamageable))
                {
                    damageable.RecibirDano(dano, transform.position, gameObject);
                }
            }

            if(aplicaVeneno)
            {
                PlayerStatusEffects statusEffects = objetivo.GetComponentInParent<PlayerStatusEffects>();

                if(statusEffects != null && estadosProcesados.Add(statusEffects))
                {
                    statusEffects.AplicarVeneno(duracionVeneno);
                }
            }
        }
    }

    private Collider2D[] BuscarObjetivos()
    {
        if(capasObjetivos.value == 0)
        {
            return Physics2D.OverlapCircleAll(transform.position, radioArea);
        }

        return Physics2D.OverlapCircleAll(transform.position, radioArea, capasObjetivos);
    }

    private IDamageable BuscarDamageable(Collider2D objetivo)
    {
        MonoBehaviour[] componentes = objetivo.GetComponentsInParent<MonoBehaviour>();

        foreach(MonoBehaviour componente in componentes)
        {
            if(componente is IDamageable damageable)
            {
                return damageable;
            }
        }

        return null;
    }

    private void ReproducirAnimacionActivacion()
    {
        if(animator == null || string.IsNullOrWhiteSpace(nombreAnimacionActivacion))
        {
            return;
        }

        int hash = Animator.StringToHash(nombreAnimacionActivacion);

        if(animator.HasState(0, hash))
        {
            animator.Play(hash, 0, 0f);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioArea);
    }
}
