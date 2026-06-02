using UnityEngine;

// Pickup de moneda reutilizable.
// El valor permite crear monedas bronze, silver, gold u otras variantes con el mismo script.
[RequireComponent(typeof(Collider2D))]
public class CoinCollectible : MonoBehaviour
{
    // Cantidad de monedas que suma al Player al recolectarla.
    [SerializeField] private int valor = 1;

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
        Destroy(gameObject);
    }
}
