using UnityEngine;

// Punto final del nivel.
// Detecta al Player vivo y dispara la pantalla de victoria una sola vez.
[RequireComponent(typeof(Collider2D))]
public class LevelGoal : MonoBehaviour
{
    [SerializeField] private LevelVictoryScreen pantallaVictoria;

    private bool completado;

    void Awake()
    {
        Collider2D trigger = GetComponent<Collider2D>();
        trigger.isTrigger = true;

        if(pantallaVictoria == null)
        {
            pantallaVictoria = Object.FindFirstObjectByType<LevelVictoryScreen>(FindObjectsInactive.Include);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(completado)
        {
            return;
        }

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();

        if(playerHealth == null || playerHealth.EstaMuerto)
        {
            return;
        }

        completado = true;

        PlayerWallet playerWallet = other.GetComponentInParent<PlayerWallet>();

        if(pantallaVictoria != null)
        {
            pantallaVictoria.Mostrar(playerWallet);
        }
        else
        {
            Debug.LogWarning("LevelGoal no encontro LevelVictoryScreen. Agrega HUDCanvas a la escena.", this);
        }
    }
}
