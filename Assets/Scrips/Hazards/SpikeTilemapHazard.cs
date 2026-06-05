using System.Collections.Generic;
using UnityEngine;

// Aplica dano al Player cuando toca un Tilemap de pinchos.
// Se usa en un Tilemap separado de hazards para poder pintar trampas con Tile Palette.
[RequireComponent(typeof(Collider2D))]
public class SpikeTilemapHazard : MonoBehaviour
{
    [SerializeField] private int dano = 1;
    [SerializeField] private float tiempoEntreDano = 0.8f;
    [SerializeField] private bool usarColliderComoTrigger = true;

    private readonly Dictionary<PlayerHealth, float> proximoDanoPorPlayer = new Dictionary<PlayerHealth, float>();
    private readonly Dictionary<Collider2D, PlayerHealth> playersPorCollider = new Dictionary<Collider2D, PlayerHealth>();
    private Collider2D hazardCollider;

    void Awake()
    {
        hazardCollider = GetComponent<Collider2D>();

        if(usarColliderComoTrigger)
        {
            hazardCollider.isTrigger = true;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        ProcesarContacto(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        ProcesarContacto(other);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        playersPorCollider.Remove(other);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        ProcesarContacto(collision.collider);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        ProcesarContacto(collision.collider);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        playersPorCollider.Remove(collision.collider);
    }

    private void ProcesarContacto(Collider2D other)
    {
        if(other == null || dano <= 0)
        {
            return;
        }

        PlayerHealth playerHealth = BuscarPlayerHealth(other);

        if(playerHealth == null || playerHealth.EstaMuerto || !PuedeDaniar(playerHealth))
        {
            return;
        }

        proximoDanoPorPlayer[playerHealth] = Time.time + tiempoEntreDano;
        playerHealth.RecibirDano(dano, ObtenerPuntoGolpe(other), gameObject);
    }

    private bool PuedeDaniar(PlayerHealth playerHealth)
    {
        return !proximoDanoPorPlayer.TryGetValue(playerHealth, out float proximoDano) || Time.time >= proximoDano;
    }

    private PlayerHealth BuscarPlayerHealth(Collider2D other)
    {
        if(playersPorCollider.TryGetValue(other, out PlayerHealth playerHealthCacheado))
        {
            return playerHealthCacheado;
        }

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
        playersPorCollider[other] = playerHealth;

        return playerHealth;
    }

    private Vector2 ObtenerPuntoGolpe(Collider2D playerCollider)
    {
        Bounds bounds = playerCollider.bounds;
        return new Vector2(bounds.center.x, bounds.min.y - 0.05f);
    }
}
