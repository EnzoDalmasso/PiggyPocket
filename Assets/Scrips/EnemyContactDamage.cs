using UnityEngine;

// Dano simple por contacto para probar Hit y Death del Player.
// Sirve como placeholder hasta tener una IA o ataques enemigos reales.
public class EnemyContactDamage : MonoBehaviour
{
    [SerializeField] private int dano = 1;
    [SerializeField] private float tiempoEntreGolpes = 1f;
    [SerializeField] private bool aplicaVeneno;
    [SerializeField] private float duracionVeneno = 5f;

    private float contadorCooldown;

    void Update()
    {
        if(contadorCooldown > 0)
        {
            contadorCooldown -= Time.deltaTime;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        IntentarDanar(collision.collider);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        IntentarDanar(collision.collider);
    }

    private void IntentarDanar(Collider2D other)
    {
        if(contadorCooldown > 0)
        {
            return;
        }

        IDamageable damageable = BuscarDamageable(other);

        if(damageable == null)
        {
            return;
        }

        damageable.RecibirDano(dano, transform.position, gameObject);
        AplicarVenenoSiCorresponde(other);
        contadorCooldown = tiempoEntreGolpes;
    }

    private void AplicarVenenoSiCorresponde(Collider2D other)
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
}
