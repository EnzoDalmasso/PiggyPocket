using UnityEngine;

// Maneja la vida del jugador, el estado de golpe y la muerte.
// Implementa IDamageable para que enemigos, trampas u otros peligros puedan hacerle dano.
public class PlayerHealth : MonoBehaviour, IDamageable
{
    // Vida inicial y maxima del jugador.
    [SerializeField] private int vidaMaxima = 3;

    // Cuando la vida actual es menor o igual a este valor, el jugador se considera herido.
    [SerializeField] private int vidaHerida = 1;

    // Tiempo durante el cual se reproduce el estado Hit.
    [SerializeField] private float duracionHit = 0.35f;

    // Tiempo de invulnerabilidad despues de recibir dano.
    [SerializeField] private float duracionInvulnerabilidad = 0.8f;

    // Fuerza horizontal del empujon al recibir dano.
    [SerializeField] private float fuerzaKnockbackHorizontal = 4f;

    // Fuerza vertical del empujon al recibir dano.
    [SerializeField] private float fuerzaKnockbackVertical = 2f;

    // Tiempo durante el cual el empujon controla el movimiento.
    [SerializeField] private float duracionKnockback = 0.2f;

    private PlayerMovement playerMovement;
    private int vidaActual;
    private float contadorHit;
    private float contadorInvulnerabilidad;

    public int VidaActual => vidaActual;
    public int VidaMaxima => vidaMaxima;
    public bool VidaCompleta => vidaActual >= vidaMaxima;
    public bool EstaHerido => vidaActual <= vidaHerida && vidaActual > 0;
    public bool EstaMuerto { get; private set; }
    public bool EstaRecibiendoDano => contadorHit > 0 && !EstaMuerto;

    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        vidaActual = vidaMaxima;
    }

    void Update()
    {
        if(contadorHit > 0)
        {
            contadorHit -= Time.deltaTime;
        }

        if(contadorInvulnerabilidad > 0)
        {
            contadorInvulnerabilidad -= Time.deltaTime;
        }
    }

    public void RecibirDano(int cantidad, Vector2 puntoGolpe, GameObject atacante)
    {
        if(EstaMuerto || contadorInvulnerabilidad > 0)
        {
            return;
        }

        vidaActual = Mathf.Max(vidaActual - cantidad, 0);
        Debug.Log(name + " recibio " + cantidad + " de dano. Vida: " + vidaActual, this);

        if(vidaActual <= 0)
        {
            Morir();
            return;
        }

        contadorHit = duracionHit;
        contadorInvulnerabilidad = duracionInvulnerabilidad;

        AplicarKnockback(puntoGolpe);
    }

    public bool Curar(int cantidad)
    {
        if(EstaMuerto || cantidad <= 0 || VidaCompleta)
        {
            return false;
        }

        vidaActual = Mathf.Min(vidaActual + cantidad, vidaMaxima);
        Debug.Log(name + " recupero " + cantidad + " de vida. Vida: " + vidaActual, this);

        return true;
    }

    private void AplicarKnockback(Vector2 puntoGolpe)
    {
        if(playerMovement == null)
        {
            return;
        }

        Vector2 direccionGolpe = (Vector2)transform.position - puntoGolpe;
        playerMovement.AplicarKnockback(direccionGolpe, fuerzaKnockbackHorizontal, fuerzaKnockbackVertical, duracionKnockback);
    }

    private void Morir()
    {
        EstaMuerto = true;
        contadorHit = 0;
        contadorInvulnerabilidad = 0;
        Debug.Log(name + " murio.", this);
    }
}
