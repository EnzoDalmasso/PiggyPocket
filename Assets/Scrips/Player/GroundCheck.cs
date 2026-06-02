using UnityEngine;

// Este script detecta si el punto donde esta colocado toca una capa de suelo.
// Lo ideal es ponerlo en un objeto hijo ubicado bajo los pies del jugador.
public class GroundCheck : MonoBehaviour
{
    // Radio del circulo invisible que se usa para revisar contacto con el suelo.
    [SerializeField] private float radioSuelo = 0.1f;

    // Capas que cuentan como suelo o plataforma.
    [SerializeField] private LayerMask capaSuelo;

    // Resultado publico de la revision. Otros scripts pueden leerlo, pero no modificarlo.
    public bool EstaSuelo { get; private set; }

    // Actualiza el estado de suelo usando un circulo en la posicion de este objeto.
    public void RevisarSuelo()
    {
        EstaSuelo = Physics2D.OverlapCircle(transform.position, radioSuelo, capaSuelo);
    }

    void OnDrawGizmosSelected()
    {
        // Dibuja el area de deteccion en la escena para poder ajustarla visualmente.
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioSuelo);
    }
}
