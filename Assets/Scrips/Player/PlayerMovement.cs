using UnityEngine;

// Estos atributos aseguran que el objeto tenga los componentes necesarios.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
// Este script se encarga de mover al jugador y aplicar el salto.
// No lee teclas directamente: recibe el input desde PlayerInput.
public class PlayerMovement : MonoBehaviour
{
    // Referencia al Rigidbody2D, usado para mover al personaje con fisica.
    private Rigidbody2D rb;

    // Referencia al script que lee los controles del jugador.
    private PlayerInput playerInput;

    // Referencia a la vida del jugador para bloquear movimiento en Hit o Death.
    private PlayerHealth playerHealth;

    // Referencia al detector de suelo. Normalmente esta en un hijo llamado CheckSuelo.
    [SerializeField] private GroundCheck groundCheck;

    // Velocidad horizontal del jugador.
    [SerializeField] public float velocidad = 5;

    // Guarda el input horizontal del frame actual.
    private float movimientoH;

    // Fuerza vertical aplicada al saltar.
    public float fuerzaSalto = 4;

    // Tiempo extra permitido para saltar despues de dejar el suelo.
    public float tiempoCoyote = 0.2f;

    // Contador interno del coyote time.
    private float contadorCoyote;

    // Velocidad horizontal que se aplica durante el knockback.
    private float velocidadKnockbackX;

    // Tiempo restante del knockback.
    private float contadorKnockback;

    // Guarda el pedido de salto hasta el siguiente paso de fisica.
    private bool saltoPendiente;


    void Start()
    {
        // Se obtienen las referencias necesarias al iniciar el objeto.
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        playerHealth = GetComponent<PlayerHealth>();

        // Si no se asigno manualmente en el Inspector, lo busca en los hijos.
        if(groundCheck == null)
        {
            groundCheck = GetComponentInChildren<GroundCheck>();
        }

        // Si no existe GroundCheck, se desactiva este script para evitar errores.
        if(groundCheck == null)
        {
            Debug.LogError("PlayerMovement necesita una referencia a GroundCheck.", this);
            enabled = false;
        }
    }

    void Update()
    {
        if(playerHealth != null && playerHealth.EstaMuerto)
        {
            return;
        }

        // Primero actualizamos si el jugador esta tocando el suelo.
        groundCheck.RevisarSuelo();

        // Si esta en suelo, se recarga el coyote time.
        // Si no, el tiempo disponible para saltar va disminuyendo.
        if(groundCheck.EstaSuelo)
        {
            contadorCoyote = tiempoCoyote;
        }
        else
        {
            contadorCoyote -= Time.deltaTime;
        }

        // Se toma el input horizontal y se aplica al Rigidbody2D.
        movimientoH = playerInput.MovimientoHorizontal;

        // Invierte visualmente al jugador segun la direccion en la que se mueve.
        if(movimientoH != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(movimientoH), 1, 1);
        }

        // Permite saltar si el jugador presiono salto y aun tiene coyote time disponible.
        if(playerInput.SaltoPresionado && contadorCoyote > 0)
        {
            saltoPendiente = true;
            GameAudioManager.ReproducirSalto();

            // Se consume el coyote time para evitar multiples saltos en el aire.
            contadorCoyote = 0;
        }
    }

    void FixedUpdate()
    {
        if(playerHealth != null && playerHealth.EstaMuerto)
        {
            saltoPendiente = false;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        if(contadorKnockback > 0)
        {
            contadorKnockback -= Time.fixedDeltaTime;
            saltoPendiente = false;
            rb.linearVelocity = new Vector2(velocidadKnockbackX, rb.linearVelocity.y);
            return;
        }

        rb.linearVelocity = new Vector2(movimientoH * velocidad, rb.linearVelocity.y);

        if(saltoPendiente)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
            saltoPendiente = false;
        }
    }

    public void AplicarKnockback(Vector2 direccion, float fuerzaHorizontal, float fuerzaVertical, float duracion)
    {
        if(rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        float direccionX = direccion.x;

        if(Mathf.Approximately(direccionX, 0))
        {
            direccionX = -Mathf.Sign(transform.localScale.x);
        }

        velocidadKnockbackX = Mathf.Sign(direccionX) * fuerzaHorizontal;
        contadorKnockback = duracion;
        saltoPendiente = false;
        rb.linearVelocity = new Vector2(velocidadKnockbackX, fuerzaVertical);
    }

    public void RebotarPorPisoton(float fuerzaRebote)
    {
        if(rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        contadorKnockback = 0;
        contadorCoyote = 0;
        saltoPendiente = false;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaRebote);
    }
}
