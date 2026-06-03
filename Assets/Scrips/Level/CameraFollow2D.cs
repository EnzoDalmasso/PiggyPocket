using UnityEngine;

// Camara simple para niveles 2D.
// Sigue al objetivo con suavizado y limita la posicion dentro de los bordes del nivel.
public class CameraFollow2D : MonoBehaviour
{
    [SerializeField] private Transform objetivo;
    [SerializeField] private Vector3 offset = new Vector3(0f, 1.2f, -10f);
    [SerializeField] private float suavizado = 6f;
    [SerializeField] private Vector2 limiteMinimo = new Vector2(-8f, -1f);
    [SerializeField] private Vector2 limiteMaximo = new Vector2(20f, 5f);

    void LateUpdate()
    {
        if(objetivo == null)
        {
            return;
        }

        Vector3 posicionDeseada = objetivo.position + offset;
        posicionDeseada.x = Mathf.Clamp(posicionDeseada.x, limiteMinimo.x, limiteMaximo.x);
        posicionDeseada.y = Mathf.Clamp(posicionDeseada.y, limiteMinimo.y, limiteMaximo.y);

        transform.position = Vector3.Lerp(transform.position, posicionDeseada, suavizado * Time.deltaTime);
    }

    public void AsignarObjetivo(Transform nuevoObjetivo)
    {
        objetivo = nuevoObjetivo;
    }
}
