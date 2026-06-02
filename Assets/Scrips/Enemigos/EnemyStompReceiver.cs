using UnityEngine;

// Permite que un enemigo reciba dano cuando el Player cae encima.
// Si el contacto viene desde arriba, el Player rebota y no recibe dano lateral.
[RequireComponent(typeof(EnemyBase))]
public class EnemyStompReceiver : MonoBehaviour
{
    [SerializeField] private int danoPisoton = 1;
    [SerializeField] private float fuerzaRebote = 3.6f;
    [SerializeField] private float velocidadMinimaCaida = -0.05f;
    [SerializeField] private float margenAlturaPisoton = 0.03f;
    [SerializeField] private float cooldownPisoton = 0.12f;
    [SerializeField] private float tiempoIgnorarContactoDespuesPisoton = 0.2f;

    private EnemyBase enemyBase;
    private Collider2D colliderEnemigo;
    private float contadorCooldownPisoton;

    void Awake()
    {
        enemyBase = GetComponent<EnemyBase>();
        colliderEnemigo = GetComponent<Collider2D>();
    }

    void Update()
    {
        if(contadorCooldownPisoton > 0)
        {
            contadorCooldownPisoton -= Time.deltaTime;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        IntentarPisoton(collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        IntentarPisoton(collision);
    }

    private void IntentarPisoton(Collision2D collision)
    {
        if(enemyBase == null || enemyBase.EstaMuerto || contadorCooldownPisoton > 0)
        {
            return;
        }

        PlayerMovement playerMovement = collision.collider.GetComponentInParent<PlayerMovement>();

        if(playerMovement == null)
        {
            return;
        }

        PlayerHealth playerHealth = playerMovement.GetComponent<PlayerHealth>();

        if(playerHealth != null && (playerHealth.EstaMuerto || playerHealth.EstaRecibiendoDano))
        {
            return;
        }

        Rigidbody2D playerRb = playerMovement.GetComponent<Rigidbody2D>();

        if(playerRb == null || playerRb.linearVelocity.y > velocidadMinimaCaida)
        {
            return;
        }

        if(!ContactoDesdeArriba(collision))
        {
            return;
        }

        contadorCooldownPisoton = cooldownPisoton;
        enemyBase.IgnorarDanoContactoTemporal(collision.collider, tiempoIgnorarContactoDespuesPisoton);
        enemyBase.RecibirDano(danoPisoton, playerMovement.transform.position, playerMovement.gameObject);
        playerMovement.RebotarPorPisoton(fuerzaRebote);
    }

    private bool ContactoDesdeArriba(Collision2D collision)
    {
        Collider2D colliderPlayer = collision.collider;

        if(colliderEnemigo == null)
        {
            colliderEnemigo = collision.otherCollider != null ? collision.otherCollider : GetComponent<Collider2D>();
        }

        if(colliderEnemigo == null)
        {
            return false;
        }

        float parteBajaPlayer = colliderPlayer.bounds.min.y;
        float parteAltaEnemigo = colliderEnemigo.bounds.max.y;

        if(parteBajaPlayer < parteAltaEnemigo - margenAlturaPisoton)
        {
            return false;
        }

        for(int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contacto = collision.GetContact(i);

            if(contacto.point.y >= parteAltaEnemigo - margenAlturaPisoton)
            {
                return true;
            }
        }

        return false;
    }
}
