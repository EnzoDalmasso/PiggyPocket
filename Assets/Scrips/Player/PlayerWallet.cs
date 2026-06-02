using UnityEngine;

// Guarda las monedas del Player para compras, mejoras u otros sistemas futuros.
// Cualquier collectible de moneda deberia sumar aca, sin conocer la UI ni tiendas.
public class PlayerWallet : MonoBehaviour
{
    // Total actual de monedas acumuladas por el Player.
    [SerializeField] private int monedas;

    public int Monedas => monedas;

    public void AgregarMonedas(int cantidad)
    {
        if(cantidad <= 0)
        {
            return;
        }

        monedas += cantidad;
        Debug.Log(name + " sumo " + cantidad + " moneda(s). Total: " + monedas, this);
    }

    public bool PuedeGastar(int cantidad)
    {
        return cantidad > 0 && monedas >= cantidad;
    }

    public bool GastarMonedas(int cantidad)
    {
        if(!PuedeGastar(cantidad))
        {
            return false;
        }

        monedas -= cantidad;
        Debug.Log(name + " gasto " + cantidad + " moneda(s). Total: " + monedas, this);

        return true;
    }
}
