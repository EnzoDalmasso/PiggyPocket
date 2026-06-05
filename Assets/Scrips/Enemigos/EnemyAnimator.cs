using UnityEngine;

// Controla las animaciones comunes de enemigos terrestres.
// No usa transiciones del Animator: el estado se decide por codigo.
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAnimator : MonoBehaviour
{
    [SerializeField] private string nombreSpawn = "sb_spawn";
    [SerializeField] private string nombreIdle = "sb_idle";
    [SerializeField] private string nombreWalk = "sb_walk";
    [SerializeField] private string nombreDeath = "sb_death";

    [SerializeField] private float duracionTransicion = 0.05f;
    [SerializeField] private float duracionSpawn = 0.58f;
    [SerializeField] private float velocidadMinimaWalk = 0.03f;
    [SerializeField] private float tiempoCongelarDeath = 0.98f;

    private Animator animator;
    private Rigidbody2D rb;
    private EnemyBase enemyBase;

    private int estadoActual;
    private int hashDeath;
    private float contadorSpawn;
    private bool deathIniciada;

    void Awake()
    {
        AsegurarReferencias();

        hashDeath = Animator.StringToHash(nombreDeath);
    }

    void OnEnable()
    {
        AsegurarReferencias();

        if(enemyBase != null)
        {
            enemyBase.MuerteIniciada += ReproducirDeath;
        }
    }

    void OnDisable()
    {
        if(enemyBase != null)
        {
            enemyBase.MuerteIniciada -= ReproducirDeath;
        }
    }

    void Start()
    {
        contadorSpawn = duracionSpawn;
        Reproducir(nombreSpawn, 0f, 0f);
    }

    void Update()
    {
        if(enemyBase != null && enemyBase.EstaMuerto)
        {
            ReproducirDeath();
            return;
        }

        if(contadorSpawn > 0)
        {
            contadorSpawn -= Time.deltaTime;
            return;
        }

        if(Mathf.Abs(rb.linearVelocity.x) > velocidadMinimaWalk)
        {
            Reproducir(nombreWalk);
        }
        else
        {
            Reproducir(nombreIdle);
        }
    }

    private void ReproducirDeath()
    {
        if(!deathIniciada)
        {
            deathIniciada = true;
            animator.speed = 1;
            Reproducir(nombreDeath, 0f, 0f);
            return;
        }

        AnimatorStateInfo estado = animator.GetCurrentAnimatorStateInfo(0);

        if(estado.shortNameHash == hashDeath && estado.normalizedTime >= tiempoCongelarDeath)
        {
            animator.speed = 0;
        }
    }

    private void Reproducir(string nombreEstado)
    {
        Reproducir(nombreEstado, duracionTransicion, 0f);
    }

    private void Reproducir(string nombreEstado, float transicion, float tiempoNormalizado)
    {
        int hash = Animator.StringToHash(nombreEstado);

        if(estadoActual == hash || !animator.HasState(0, hash))
        {
            return;
        }

        estadoActual = hash;
        animator.CrossFade(hash, transicion, 0, tiempoNormalizado);
    }

    private void AsegurarReferencias()
    {
        if(animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if(rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if(enemyBase == null)
        {
            enemyBase = GetComponent<EnemyBase>();
        }
    }
}
