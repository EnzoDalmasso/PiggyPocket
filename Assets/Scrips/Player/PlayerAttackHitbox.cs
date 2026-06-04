using System.Collections.Generic;
using UnityEngine;

// Este script vive en un objeto hijo del Player con un Collider2D en modo Trigger.
// Cuando PlayerAttack lo activa, detecta objetos golpeados y les envia dano si implementan IDamageable.
[RequireComponent(typeof(Collider2D))]
public class PlayerAttackHitbox : MonoBehaviour
{
    // Dano aplicado por el primer golpe del combo.
    [SerializeField] private int danoAtaque1 = 1;

    // Dano aplicado por el segundo golpe del combo.
    [SerializeField] private int danoAtaque2 = 2;

    // Capas que pueden recibir golpes. Si queda en Nothing, no filtra por layer.
    [SerializeField] private LayerMask capasObjetivos;

    // Collider usado como zona de ataque.
    private Collider2D hitbox;

    // Evita golpear varias veces al mismo collider durante un mismo golpe.
    private readonly HashSet<Collider2D> collidersGolpeados = new HashSet<Collider2D>();

    // Dano del golpe que esta activo ahora.
    private int danoActual;

    // Objeto que origina el ataque.
    private GameObject atacante;

    // Transform real del Player, usado para ignorar sus propios colliders.
    private Transform atacanteTransform;

    private bool EstaActiva => hitbox != null && hitbox.enabled;

    void Awake()
    {
        hitbox = GetComponent<Collider2D>();
        hitbox.isTrigger = true;
        hitbox.enabled = false;
        AsignarAtacante();
    }

    public void Activar(int numeroAtaque)
    {
        danoActual = numeroAtaque == 2 ? danoAtaque2 : danoAtaque1;
        collidersGolpeados.Clear();
        hitbox.enabled = true;
    }

    public void Desactivar()
    {
        if(hitbox == null)
        {
            return;
        }

        hitbox.enabled = false;
        collidersGolpeados.Clear();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        IntentarGolpear(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        IntentarGolpear(other);
    }

    private void IntentarGolpear(Collider2D other)
    {
        if(!EstaActiva || EsColliderPropio(other))
        {
            return;
        }

        if(!PerteneceACapaObjetivo(other.gameObject.layer))
        {
            return;
        }

        if(!collidersGolpeados.Add(other))
        {
            return;
        }

        IDamageable damageable = BuscarDamageable(other);

        if(damageable == null)
        {
            return;
        }

        damageable.RecibirDano(danoActual, transform.position, atacante);
    }

    private bool PerteneceACapaObjetivo(int layer)
    {
        return capasObjetivos.value == 0 || (capasObjetivos.value & (1 << layer)) != 0;
    }

    private bool EsColliderPropio(Collider2D other)
    {
        if(other == null)
        {
            return true;
        }

        if(atacanteTransform == null)
        {
            return false;
        }

        if(other.attachedRigidbody != null && other.attachedRigidbody.transform == atacanteTransform)
        {
            return true;
        }

        return other.transform == atacanteTransform || other.transform.IsChildOf(atacanteTransform);
    }

    private void AsignarAtacante()
    {
        PlayerAttack playerAttack = GetComponentInParent<PlayerAttack>();

        if(playerAttack != null)
        {
            atacante = playerAttack.gameObject;
            atacanteTransform = playerAttack.transform;
            return;
        }

        atacanteTransform = transform.parent != null ? transform.parent : transform;
        atacante = atacanteTransform.gameObject;
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

    void OnDrawGizmosSelected()
    {
        CircleCollider2D circle = GetComponent<CircleCollider2D>();

        if(circle == null)
        {
            return;
        }

        Gizmos.color = Color.red;
        Vector3 posicion = transform.TransformPoint(circle.offset);
        float escala = Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y));
        Gizmos.DrawWireSphere(posicion, circle.radius * escala);
    }
}
