using UnityEngine;

// Movimiento reutilizable para enemigos terrestres simples.
// Camina en una direccion y gira si detecta una pared o un borde.
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPatrolMovement : MonoBehaviour
{
    [SerializeField] private float velocidad = 0.6f;
    [SerializeField] private bool mirandoDerecha;

    [Header("Deteccion")]
    [SerializeField] private LayerMask capaSuelo;
    [SerializeField] private LayerMask capaObstaculos;
    [SerializeField] private bool usarDeteccionBordes = true;
    [SerializeField] private bool usarDeteccionParedes = true;

    [Header("Puntos de deteccion")]
    [SerializeField] private Vector2 offsetDeteccionPared = new Vector2(0.11f, 0f);
    [SerializeField] private float distanciaDeteccionPared = 0.08f;
    [SerializeField] private Vector2 offsetDeteccionBorde = new Vector2(0.1f, -0.08f);
    [SerializeField] private float distanciaDeteccionSuelo = 0.14f;

    private Rigidbody2D rb;
    private EnemyBase enemyBase;
    private int direccion;
    private bool movimientoDetenido;

    void Awake()
    {
        AsegurarReferencias();
        CompletarCapasPorDefecto();
        direccion = mirandoDerecha ? 1 : -1;
        ActualizarEscalaVisual();
    }

    void OnEnable()
    {
        AsegurarReferencias();

        if(enemyBase != null)
        {
            enemyBase.MuerteIniciada += DetenerMovimiento;
        }
    }

    void OnDisable()
    {
        if(enemyBase != null)
        {
            enemyBase.MuerteIniciada -= DetenerMovimiento;
        }
    }

    void FixedUpdate()
    {
        if(movimientoDetenido || (enemyBase != null && enemyBase.EstaMuerto))
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if(DebeGirar())
        {
            Girar();
        }

        rb.linearVelocity = new Vector2(direccion * velocidad, rb.linearVelocity.y);
    }

    private bool DebeGirar()
    {
        bool hayPared = usarDeteccionParedes && DetectaPared();
        bool haySueloAdelante = !usarDeteccionBordes || DetectaSueloAdelante();

        return hayPared || !haySueloAdelante;
    }

    private bool DetectaPared()
    {
        Vector2 origen = (Vector2)transform.position + new Vector2(offsetDeteccionPared.x * direccion, offsetDeteccionPared.y);
        RaycastHit2D hit = Physics2D.Raycast(origen, Vector2.right * direccion, distanciaDeteccionPared, capaObstaculos);

        return hit.collider != null;
    }

    private bool DetectaSueloAdelante()
    {
        Vector2 origen = (Vector2)transform.position + new Vector2(offsetDeteccionBorde.x * direccion, offsetDeteccionBorde.y);
        RaycastHit2D hit = Physics2D.Raycast(origen, Vector2.down, distanciaDeteccionSuelo, capaSuelo);

        return hit.collider != null;
    }

    private void Girar()
    {
        direccion *= -1;
        ActualizarEscalaVisual();
    }

    private void ActualizarEscalaVisual()
    {
        Vector3 escala = transform.localScale;
        escala.x = Mathf.Abs(escala.x) * direccion;
        transform.localScale = escala;
    }

    private void CompletarCapasPorDefecto()
    {
        if(capaSuelo.value == 0)
        {
            capaSuelo = LayerMask.GetMask("Floor");
        }

        if(capaObstaculos.value == 0)
        {
            capaObstaculos = capaSuelo;
        }
    }

    private void DetenerMovimiento()
    {
        movimientoDetenido = true;

        if(rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0;
    }

    private void AsegurarReferencias()
    {
        if(rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if(enemyBase == null)
        {
            enemyBase = GetComponent<EnemyBase>();
        }
    }

    void OnDrawGizmosSelected()
    {
        int direccionGizmo = mirandoDerecha ? 1 : -1;

        if(Application.isPlaying)
        {
            direccionGizmo = direccion;
        }

        Vector2 origenPared = (Vector2)transform.position + new Vector2(offsetDeteccionPared.x * direccionGizmo, offsetDeteccionPared.y);
        Vector2 destinoPared = origenPared + Vector2.right * direccionGizmo * distanciaDeteccionPared;

        Vector2 origenSuelo = (Vector2)transform.position + new Vector2(offsetDeteccionBorde.x * direccionGizmo, offsetDeteccionBorde.y);
        Vector2 destinoSuelo = origenSuelo + Vector2.down * distanciaDeteccionSuelo;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(origenPared, destinoPared);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(origenSuelo, destinoSuelo);
    }
}
