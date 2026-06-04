using UnityEngine;

// Pickup de vida para el Player.
// Vive en un objeto con Collider2D en Trigger y se consume al curar.
[RequireComponent(typeof(Collider2D))]
public class LifeCollectible : MonoBehaviour
{
    // Cantidad de vida que recupera el Player al tocar este collectible.
    [SerializeField] private int cantidadVida = 1;

    // Si esta activo, el pickup se consume aunque el Player ya tenga vida completa.
    [SerializeField] private bool consumirSiVidaCompleta;

    [Header("Audio")]
    [SerializeField] private AudioClip sonidoRecolectar;

    private Collider2D trigger;
    private bool recolectado;

    void Awake()
    {
        trigger = GetComponent<Collider2D>();
        trigger.isTrigger = true;
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

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();

        if(playerHealth == null || playerHealth.EstaMuerto)
        {
            return;
        }

        bool pudoCurar = playerHealth.Curar(cantidadVida);

        if(!pudoCurar && !consumirSiVidaCompleta)
        {
            return;
        }

        recolectado = true;
        trigger.enabled = false;
        GameAudioManager.ReproducirVida(sonidoRecolectar);
        Destroy(gameObject);
    }
}
