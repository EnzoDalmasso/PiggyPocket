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

    // Guarda si el jugador pidio encadenar el segundo golpe.
    private bool ataqueComboPendiente;

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

        if(attackHitbox != null)
        {
            attackHitbox.Activar(numeroAtaque);
        }
    }

    private void FinalizarAtaque()
    {
        if(attackHitbox != null)
        {
            attackHitbox.Desactivar();
        }

        EstaAtacando = false;
        NumeroAtaqueActual = 0;
        ataqueComboPendiente = false;
        contadorCooldown = tiempoEntreAtaques;
    }

    private void CancelarAtaque()
    {
        if(attackHitbox != null)
        {
            attackHitbox.Desactivar();
        }

        EstaAtacando = false;
        NumeroAtaqueActual = 0;
        ataqueComboPendiente = false;
        contadorDuracionAtaque = 0;
    }

    void OnDisable()
    {
        if(attackHitbox != null)
        {
            attackHitbox.Desactivar();
        }
    }
}
