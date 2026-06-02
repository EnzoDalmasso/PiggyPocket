using UnityEngine;

// Este script conecta el estado real del jugador con el Animator.
// No mueve, no salta y no lee input: solo actualiza parametros de animacion.
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerAnimator : MonoBehaviour
{
    // Nombres de estados que deben existir en el Animator Controller.
    private const string Idle = "Idle";
    private const string Run = "Run";
    private const string Jump = "Jump";
    private const string Fall = "Fall";
    private const string Grounded = "Grounded";
    private const string ShowOff = "Show Off";
    private const string Injured = "Injured";
    private const string InjuredPoisoned = "Injured Poisoned";
    private const string Hit = "Hit";
    private const string Death = "Death";

    // Nombres de parametros que deben existir en el Animator Controller.
    // Los seguimos actualizando para poder depurar desde la ventana Animator.
    private static readonly int VelocidadX = Animator.StringToHash("VelocidadX");
    private static readonly int VelocidadY = Animator.StringToHash("VelocidadY");
    private static readonly int EstaSuelo = Animator.StringToHash("EstaSuelo");
    private static readonly int EstaCayendo = Animator.StringToHash("EstaCayendo");

    // Referencia al Animator que reproduce las animaciones.
    private Animator animator;

    // Referencia al Rigidbody2D para leer la velocidad real del jugador.
    private Rigidbody2D rb;

    // Referencia al detector de suelo, normalmente ubicado en un hijo llamado CheckSuelo.
    [SerializeField] private GroundCheck groundCheck;

    // Referencia a la logica de ataque del jugador.
    [SerializeField] private PlayerAttack playerAttack;

    // Referencia a la vida del jugador para reproducir Hit o Death.
    [SerializeField] private PlayerHealth playerHealth;

    // Referencia a los estados alterados del jugador.
    [SerializeField] private PlayerStatusEffects playerStatusEffects;

    // Nombre del estado del primer ataque dentro del Animator Controller.
    [SerializeField] private string nombreAnimacionAtaque1 = "Attack 1";

    // Nombre del estado del segundo ataque dentro del Animator Controller.
    [SerializeField] private string nombreAnimacionAtaque2 = "Attack 2";

    // Duracion suave del cambio entre animaciones.
    [SerializeField] private float duracionTransicion = 0.05f;

    // Tiempo durante el cual se reproduce la animacion de aterrizaje.
    [SerializeField] private float duracionGrounded = 0.14f;

    // Tiempo quieto necesario para reproducir Show Off.
    [SerializeField] private float tiempoParaShowOff = 4f;

    // Tiempo durante el cual se mantiene la animacion Show Off.
    [SerializeField] private float duracionShowOff = 1.4f;

    // Punto normalizado donde se congela Death para evitar que vuelva a empezar si el clip tiene loop.
    [SerializeField] private float tiempoCongelarDeath = 0.98f;

    // Evita reiniciar la misma animacion todos los frames.
    private string animacionActual;

    // Contador interno de la animacion de aterrizaje.
    private float contadorGrounded;

    // Contador del tiempo que el jugador lleva quieto en Idle.
    private float contadorIdle;

    // Contador interno de la animacion Show Off.
    private float contadorShowOff;

    // Guarda el suelo del frame anterior para detectar cuando aterriza.
    private bool estabaSuelo;

    // Evita disparar Grounded apenas empieza la escena si el Player ya estaba en el piso.
    private bool sueloInicializado;

    // Indican si el Animator Controller tiene estos parametros creados.
    private bool tieneVelocidadX;
    private bool tieneVelocidadY;
    private bool tieneEstaSuelo;
    private bool tieneEstaCayendo;

    void Start()
    {
        // Se obtienen las referencias principales del mismo objeto.
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        RevisarParametrosAnimator();

        // Si no se asigno manualmente en el Inspector, busca PlayerAttack en este objeto.
        if(playerAttack == null)
        {
            playerAttack = GetComponent<PlayerAttack>();
        }

        // Si no se asigno manualmente en el Inspector, busca PlayerHealth en este objeto.
        if(playerHealth == null)
        {
            playerHealth = GetComponent<PlayerHealth>();
        }

        // Si no se asigno manualmente en el Inspector, busca PlayerStatusEffects en este objeto.
        if(playerStatusEffects == null)
        {
            playerStatusEffects = GetComponent<PlayerStatusEffects>();
        }

        // Si no se asigno manualmente en el Inspector, busca el GroundCheck en los hijos.
        if(groundCheck == null)
        {
            groundCheck = GetComponentInChildren<GroundCheck>();
        }

        // Si no existe GroundCheck, se desactiva para evitar errores al leer el suelo.
        if(groundCheck == null)
        {
            Debug.LogError("PlayerAnimator necesita una referencia a GroundCheck.", this);
            enabled = false;
        }
    }

    void LateUpdate()
    {
        // LateUpdate corre despues de Update, asi toma el estado ya calculado por PlayerMovement.
        float velocidadX = Mathf.Abs(rb.linearVelocity.x);
        float velocidadY = rb.linearVelocity.y;
        bool estaSuelo = groundCheck.EstaSuelo;

        if(tieneVelocidadX)
        {
            animator.SetFloat(VelocidadX, velocidadX);
        }

        if(tieneVelocidadY)
        {
            animator.SetFloat(VelocidadY, velocidadY);
        }

        if(tieneEstaSuelo)
        {
            animator.SetBool(EstaSuelo, estaSuelo);
        }

        if(tieneEstaCayendo)
        {
            animator.SetBool(EstaCayendo, !estaSuelo && velocidadY < 0);
        }

        ActualizarGrounded(estaSuelo);

        // Death tiene maxima prioridad y queda fija hasta reiniciar o cambiar de escena.
        if(playerHealth != null && playerHealth.EstaMuerto)
        {
            CambiarAnimacion(Death);
            CongelarDeathAlFinal();
            ReiniciarShowOff();
            return;
        }

        animator.speed = 1;

        // Hit tiene prioridad sobre ataque y movimiento.
        if(playerHealth != null && playerHealth.EstaRecibiendoDano)
        {
            CambiarAnimacion(Hit);
            ReiniciarShowOff();
            return;
        }

        // El ataque tiene prioridad visual sobre el resto de animaciones.
        if(playerAttack != null && playerAttack.EstaAtacando)
        {
            string nombreAtaque = playerAttack.NumeroAtaqueActual == 2
                ? nombreAnimacionAtaque2
                : nombreAnimacionAtaque1;

            CambiarAnimacion(nombreAtaque);
            ReiniciarShowOff();
            return;
        }

        // Grounded se reproduce brevemente al pasar de estar en el aire a tocar suelo.
        if(contadorGrounded > 0 && estaSuelo)
        {
            CambiarAnimacion(Grounded);
            ReiniciarShowOff();
            return;
        }

        ActualizarShowOff(estaSuelo, velocidadX, velocidadY);

        if(contadorShowOff > 0)
        {
            CambiarAnimacion(ShowOff);
            return;
        }

        if(!estaSuelo)
        {
            ReiniciarShowOff();
            CambiarAnimacion(velocidadY < 0 ? Fall : Jump);
        }
        else if(velocidadX > 0.01f)
        {
            ReiniciarShowOff();
            CambiarAnimacion(Run);
        }
        else
        {
            CambiarAnimacion(ObtenerAnimacionIdle());
        }
    }

    private string ObtenerAnimacionIdle()
    {
        if(playerHealth != null && playerHealth.EstaHerido)
        {
            if(playerStatusEffects != null && playerStatusEffects.EstaEnvenenado)
            {
                return InjuredPoisoned;
            }

            return Injured;
        }

        return Idle;
    }

    private void CambiarAnimacion(string nombreAnimacion)
    {
        if(animacionActual == nombreAnimacion)
        {
            return;
        }

        int estadoHash = Animator.StringToHash("Base Layer." + nombreAnimacion);

        if(!animator.HasState(0, estadoHash))
        {
            Debug.LogWarning("No existe el estado de animacion: " + nombreAnimacion, this);
            return;
        }

        animator.CrossFade(estadoHash, duracionTransicion);
        animacionActual = nombreAnimacion;
    }

    private void ActualizarGrounded(bool estaSuelo)
    {
        if(!sueloInicializado)
        {
            estabaSuelo = estaSuelo;
            sueloInicializado = true;
            return;
        }

        bool aterrizoEsteFrame = estaSuelo && !estabaSuelo;

        if(aterrizoEsteFrame)
        {
            contadorGrounded = duracionGrounded;
        }
        else if(contadorGrounded > 0)
        {
            contadorGrounded -= Time.deltaTime;
        }

        if(!estaSuelo)
        {
            contadorGrounded = 0;
        }

        estabaSuelo = estaSuelo;
    }

    private void ActualizarShowOff(bool estaSuelo, float velocidadX, float velocidadY)
    {
        bool puedeMostrarShowOff = estaSuelo
            && velocidadX <= 0.01f
            && Mathf.Abs(velocidadY) <= 0.01f;

        if(!puedeMostrarShowOff)
        {
            ReiniciarShowOff();
            return;
        }

        if(contadorShowOff > 0)
        {
            contadorShowOff -= Time.deltaTime;
            return;
        }

        contadorIdle += Time.deltaTime;

        if(contadorIdle >= tiempoParaShowOff)
        {
            contadorShowOff = duracionShowOff;
            contadorIdle = 0;
        }
    }

    private void ReiniciarShowOff()
    {
        contadorIdle = 0;
        contadorShowOff = 0;
    }

    private void CongelarDeathAlFinal()
    {
        AnimatorStateInfo estadoActual = animator.GetCurrentAnimatorStateInfo(0);

        if(!estadoActual.IsName("Base Layer." + Death) || estadoActual.normalizedTime < tiempoCongelarDeath)
        {
            return;
        }

        int estadoHash = Animator.StringToHash("Base Layer." + Death);
        animator.Play(estadoHash, 0, tiempoCongelarDeath);
        animator.speed = 0;
    }

    private void RevisarParametrosAnimator()
    {
        foreach(AnimatorControllerParameter parametro in animator.parameters)
        {
            if(parametro.name == "VelocidadX")
            {
                tieneVelocidadX = true;
            }
            else if(parametro.name == "VelocidadY")
            {
                tieneVelocidadY = true;
            }
            else if(parametro.name == "EstaSuelo")
            {
                tieneEstaSuelo = true;
            }
            else if(parametro.name == "EstaCayendo")
            {
                tieneEstaCayendo = true;
            }
        }
    }
}
