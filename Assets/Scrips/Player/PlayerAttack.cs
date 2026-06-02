using UnityEngine;

// Este script controla la logica basica del ataque del jugador.
// No reproduce animaciones ni aplica dano: solo decide cuando el jugador esta atacando.
[RequireComponent(typeof(PlayerInput))]
public class PlayerAttack : MonoBehaviour
{
    // Tiempo que dura activo el primer golpe del combo.
    [SerializeField] private float duracionAtaque1 = 0.35f;

    // Tiempo que dura activo el segundo golpe del combo.
    [SerializeField] private float duracionAtaque2 = 0.4f;

    // Tiempo minimo para volver a iniciar otro combo despues de terminar.
    [SerializeField] private float tiempoEntreAtaques = 0.45f;

    // Espera antes de activar la colision del primer golpe.
    [SerializeField] private float demoraHitboxAtaque1 = 0.12f;

    // Espera antes de activar la colision del segundo golpe.
    [SerializeField] private float demoraHitboxAtaque2 = 0.14f;

    // Tiempo real durante el que el primer golpe puede hacer dano.
    [SerializeField] private float duracionHitboxAtaque1 = 0.12f;

    // Tiempo real durante el que el segundo golpe puede hacer dano.
    [SerializeField] private float duracionHitboxAtaque2 = 0.14f;

    // Zona de colision que detecta enemigos durante el ataque.
    [SerializeField] private PlayerAttackHitbox attackHitbox;

    // Referencia al script que lee los controles del jugador.
    private PlayerInput playerInput;

    // Referencia a la vida del jugador para bloquear ataques en Hit o Death.
    private PlayerHealth playerHealth;

    // Contador interno de la duracion actual del ataque.
    private float contadorDuracionAtaque;

    // Contador interno del cooldown entre ataques.
    private float contadorCooldown;

    // Contador para esperar al frame correcto del golpe antes de activar el hitbox.
    private float contadorDemoraHitbox;

    // Contador para apagar el hitbox despues de la ventana real de dano.
    private float contadorDuracionHitbox;

    // Guarda si el jugador pidio encadenar el segundo golpe.
    private bool ataqueComboPendiente;

    // Indica si la zona de dano ya esta activa dentro del ataque actual.
    private bool hitboxActiva;

    // Indica si el jugador esta en medio de un ataque.
    public bool EstaAtacando { get; private set; }

    // Indica que golpe del combo esta activo: 0 ninguno, 1 Attack 1, 2 Attack 2.
    public int NumeroAtaqueActual { get; private set; }

    // Se activa solo durante el frame en el que comienza un ataque.
    public bool AtaqueIniciadoEsteFrame { get; private set; }

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        playerHealth = GetComponent<PlayerHealth>();

        if(attackHitbox == null)
        {
            attackHitbox = GetComponentInChildren<PlayerAttackHitbox>();
        }
    }

    void Update()
    {
        AtaqueIniciadoEsteFrame = false;

        if(playerHealth != null && (playerHealth.EstaMuerto || playerHealth.EstaRecibiendoDano))
        {
            CancelarAtaque();
            return;
        }

        ActualizarCooldown();

        if(playerInput.AtaquePresionado)
        {
            ProcesarInputAtaque();
        }

        ActualizarDuracionAtaque();
    }

    private void ActualizarCooldown()
    {
        if(contadorCooldown > 0)
        {
            contadorCooldown -= Time.deltaTime;
        }
    }

    private void ActualizarDuracionAtaque()
    {
        if(!EstaAtacando)
        {
            return;
        }

        ActualizarVentanaHitbox();

        contadorDuracionAtaque -= Time.deltaTime;

        if(contadorDuracionAtaque <= 0)
        {
            if(ataqueComboPendiente && NumeroAtaqueActual == 1)
            {
                ataqueComboPendiente = false;
                IniciarAtaque(2);
            }
            else
            {
                FinalizarAtaque();
            }
        }
    }

    private void ProcesarInputAtaque()
    {
        if(PuedeAtacar())
        {
            IniciarAtaque(1);
        }
        else if(PuedeEncadenarAtaque())
        {
            ataqueComboPendiente = true;
        }
    }

    private bool PuedeAtacar()
    {
        return !EstaAtacando && contadorCooldown <= 0;
    }

    private bool PuedeEncadenarAtaque()
    {
        return EstaAtacando && NumeroAtaqueActual == 1;
    }

    private void IniciarAtaque(int numeroAtaque)
    {
        EstaAtacando = true;
        AtaqueIniciadoEsteFrame = true;
        NumeroAtaqueActual = numeroAtaque;
        contadorDuracionAtaque = numeroAtaque == 1 ? duracionAtaque1 : duracionAtaque2;
        contadorDemoraHitbox = numeroAtaque == 1 ? demoraHitboxAtaque1 : demoraHitboxAtaque2;
        contadorDuracionHitbox = numeroAtaque == 1 ? duracionHitboxAtaque1 : duracionHitboxAtaque2;
        hitboxActiva = false;

        if(attackHitbox != null)
        {
            attackHitbox.Desactivar();
        }

        if(contadorDemoraHitbox <= 0)
        {
            ActivarHitbox();
        }
    }

    private void ActualizarVentanaHitbox()
    {
        if(hitboxActiva)
        {
            contadorDuracionHitbox -= Time.deltaTime;

            if(contadorDuracionHitbox <= 0)
            {
                DesactivarHitbox();
            }

            return;
        }

        if(contadorDemoraHitbox > 0)
        {
            contadorDemoraHitbox -= Time.deltaTime;
        }

        if(contadorDemoraHitbox <= 0 && contadorDuracionHitbox > 0)
        {
            ActivarHitbox();
        }
    }

    private void ActivarHitbox()
    {
        if(attackHitbox == null || hitboxActiva)
        {
            return;
        }

        hitboxActiva = true;
        attackHitbox.Activar(NumeroAtaqueActual);
    }

    private void DesactivarHitbox()
    {
        if(attackHitbox != null)
        {
            attackHitbox.Desactivar();
        }

        hitboxActiva = false;
        contadorDuracionHitbox = 0;
    }

    private void FinalizarAtaque()
    {
        DesactivarHitbox();

        EstaAtacando = false;
        NumeroAtaqueActual = 0;
        ataqueComboPendiente = false;
        contadorCooldown = tiempoEntreAtaques;
    }

    private void CancelarAtaque()
    {
        DesactivarHitbox();

        EstaAtacando = false;
        NumeroAtaqueActual = 0;
        ataqueComboPendiente = false;
        contadorDuracionAtaque = 0;
        contadorDemoraHitbox = 0;
    }

    void OnDisable()
    {
        DesactivarHitbox();
    }
}
