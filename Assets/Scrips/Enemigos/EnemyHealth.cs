using UnityEngine;

// Enemigo simple para probar el sistema de dano del Player.
// Implementa IDamageable para poder recibir golpes desde PlayerAttackHitbox.
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int vidaMaxima = 3;
    [SerializeField] private bool destruirAlMorir = true;

    private int vidaActual;

    void Awake()
    {
        vidaActual = vidaMaxima;
    }

    public void RecibirDano(int cantidad, Vector2 puntoGolpe, GameObject atacante)
    {
        vidaActual = Mathf.Max(vidaActual - cantidad, 0);
        Debug.Log(name + " recibio " + cantidad + " de dano. Vida: " + vidaActual, this);

        if(vidaActual <= 0)
        {
            Morir();
        }
    }

    private void Morir()
    {
        Debug.Log(name + " fue derrotado.", this);

        if(destruirAlMorir)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
