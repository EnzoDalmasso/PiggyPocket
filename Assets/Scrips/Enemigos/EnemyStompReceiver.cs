using System.Collections.Generic;
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
    private readonly Dictionary<Collider2D, PlayerMovement> playersPorCollider = new Dictionary<Collider2D, PlayerMovement>();
    private readonly Dictionary<PlayerMovement, PlayerHealth> healthPorPlayer = new Dictionary<PlayerMovement, PlayerHealth>();
    private readonly Dictionary<PlayerMovement, Rigidbody2D> rbPorPlayer = new Dictionary<PlayerMovement, Rigidbody2D>();

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

    void OnCollisionExit2D(Collision2D collision)
    {
        playersPorCollider.Remove(collision.collider);
    }

    private void IntentarPisoton(Collision2D collision)
    {
        if(enemyBase == null || enemyBase.EstaMuerto || contadorCooldownPisoton > 0)
        {
            return;
        }

        PlayerMovement playerMovement = BuscarPlayerMovement(collision.collider);

        if(playerMovement == null)
        {
            return;
        }

        PlayerHealth playerHealth = BuscarPlayerHealth(playerMovement);

        if(playerHealth != null && (playerHealth.EstaMuerto || playerHealth.EstaRecibiendoDano))
        {
            return;
        }

        Rigidbody2D playerRb = BuscarPlayerRigidbody(playerMovement);

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

    private PlayerMovement BuscarPlayerMovement(Collider2D colliderPlayer)
    {
        if(colliderPlayer == null)
        {
            return null;
        }

        if(playersPorCollider.TryGetValue(colliderPlayer, out PlayerMovement playerMovementCacheado))
        {
            return playerMovementCacheado;
        }

        PlayerMovement playerMovement = colliderPlayer.GetComponentInParent<PlayerMovement>();
        playersPorCollider[colliderPlayer] = playerMovement;

        return playerMovement;
    }

    private PlayerHealth BuscarPlayerHealth(PlayerMovement playerMovement)
    {
        if(playerMovement == null)
        {
            return null;
        }

        if(healthPorPlayer.TryGetValue(playerMovement, out PlayerHealth playerHealthCacheado))
        {
            return playerHealthCacheado;
        }

        PlayerHealth playerHealth = playerMovement.GetComponent<PlayerHealth>();
        healthPorPlayer[playerMovement] = playerHealth;

        return playerHealth;
    }

    private Rigidbody2D BuscarPlayerRigidbody(PlayerMovement playerMovement)
    {
        if(playerMovement == null)
        {
            return null;
        }

        if(rbPorPlayer.TryGetValue(playerMovement, out Rigidbody2D rbCacheado))
        {
            return rbCacheado;
        }

        Rigidbody2D rb = playerMovement.GetComponent<Rigidbody2D>();
        rbPorPlayer[playerMovement] = rb;

        return rb;
    }
}
