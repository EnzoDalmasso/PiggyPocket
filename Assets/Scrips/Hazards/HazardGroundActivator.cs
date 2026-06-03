using UnityEngine;

// Activa un hazard despues de que el objeto toca el suelo.
// Sirve para objetos que primero caen con fisica, como una bomba que sale de un barril.
[RequireComponent(typeof(Rigidbody2D))]
public class HazardGroundActivator : MonoBehaviour
{
    [SerializeField] private AreaEffectHazard hazard;
    [SerializeField] private LayerMask capasSuelo;
    [SerializeField] private float demoraDespuesDeTocarSuelo = 0.45f;

    private Rigidbody2D rb;
    private Collider2D[] colliders;
    private bool activacionProgramada;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        colliders = GetComponents<Collider2D>();

        if(hazard == null)
        {
            hazard = GetComponent<AreaEffectHazard>();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if(activacionProgramada || !PerteneceACapaSuelo(collision.gameObject.layer))
        {
            return;
        }

        activacionProgramada = true;
        DetenerFisica();
        Invoke(nameof(ActivarHazard), demoraDespuesDeTocarSuelo);
    }

    private bool PerteneceACapaSuelo(int layer)
    {
        return capasSuelo.value == 0 || (capasSuelo.value & (1 << layer)) != 0;
    }

    private void DetenerFisica()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void ActivarHazard()
    {
        if(hazard != null)
        {
            hazard.Activar();
        }

        DesactivarColisionesFisicas();
    }

    private void DesactivarColisionesFisicas()
    {
        foreach(Collider2D colliderFisico in colliders)
        {
            if(colliderFisico != null && !colliderFisico.isTrigger)
            {
                colliderFisico.enabled = false;
            }
        }

        rb.simulated = false;
    }
}
