using UnityEngine;

// Maneja estados alterados del jugador, como veneno.
// No decide animaciones ni movimiento: solo expone el estado actual.
public class PlayerStatusEffects : MonoBehaviour
{
    // Duracion por defecto del veneno si no se especifica una duracion desde fuera.
    [SerializeField] private float duracionVenenoPorDefecto = 5f;

    // Contador interno del veneno activo.
    private float contadorVeneno;

    public bool EstaEnvenenado => contadorVeneno > 0;

    void Update()
    {
        if(contadorVeneno > 0)
        {
            contadorVeneno -= Time.deltaTime;
        }
    }

    public void AplicarVeneno()
    {
        AplicarVeneno(duracionVenenoPorDefecto);
    }

    public void AplicarVeneno(float duracion)
    {
        contadorVeneno = Mathf.Max(contadorVeneno, duracion);
    }

    public void CurarVeneno()
    {
        contadorVeneno = 0;
    }
}
