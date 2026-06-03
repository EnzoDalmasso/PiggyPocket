using UnityEngine;

// Estado compartido de los controles tactiles.
// PlayerInput lo lee como una fuente mas, junto al teclado.
public static class MobileInputState
{
    private static bool izquierdaPresionada;
    private static bool derechaPresionada;
    private static bool saltoPresionado;
    private static bool ataquePresionado;

    public static float MovimientoHorizontal
    {
        get
        {
            int direccion = 0;

            if(izquierdaPresionada)
            {
                direccion--;
            }

            if(derechaPresionada)
            {
                direccion++;
            }

            return Mathf.Clamp(direccion, -1, 1);
        }
    }

    public static void Presionar(MobileInputAction accion)
    {
        switch(accion)
        {
            case MobileInputAction.Izquierda:
                izquierdaPresionada = true;
                break;

            case MobileInputAction.Derecha:
                derechaPresionada = true;
                break;

            case MobileInputAction.Salto:
                saltoPresionado = true;
                break;

            case MobileInputAction.Ataque:
                ataquePresionado = true;
                break;
        }
    }

    public static void Soltar(MobileInputAction accion)
    {
        switch(accion)
        {
            case MobileInputAction.Izquierda:
                izquierdaPresionada = false;
                break;

            case MobileInputAction.Derecha:
                derechaPresionada = false;
                break;
        }
    }

    public static bool ConsumirSalto()
    {
        if(!saltoPresionado)
        {
            return false;
        }

        saltoPresionado = false;
        return true;
    }

    public static bool ConsumirAtaque()
    {
        if(!ataquePresionado)
        {
            return false;
        }

        ataquePresionado = false;
        return true;
    }

    public static void Reiniciar()
    {
        izquierdaPresionada = false;
        derechaPresionada = false;
        saltoPresionado = false;
        ataquePresionado = false;
    }
}

public enum MobileInputAction
{
    Izquierda,
    Derecha,
    Salto,
    Ataque
}
