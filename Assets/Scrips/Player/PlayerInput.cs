using UnityEngine;
using UnityEngine.EventSystems;

// Este script solo se encarga de leer los controles del jugador.
// No mueve al personaje: otros scripts usan estos valores para decidir que hacer.
[DefaultExecutionOrder(-100)]
public class PlayerInput : MonoBehaviour
{
    // Valor horizontal del input: -1 izquierda, 0 quieto, 1 derecha.
    public float MovimientoHorizontal { get; private set; }

    // Se activa solo durante el frame exacto en el que se presiona el salto.
    public bool SaltoPresionado { get; private set; }

    // Se activa solo durante el frame exacto en el que se presiona el ataque.
    public bool AtaquePresionado { get; private set; }

    void Update()
    {
        // El input se lee en Update porque depende de cada frame del juego.
        float movimientoTeclado = Input.GetAxisRaw("Horizontal");
        float movimientoMobile = MobileInputState.MovimientoHorizontal;
        bool saltoMobile = MobileInputState.ConsumirSalto();
        bool ataqueMobile = MobileInputState.ConsumirAtaque();

        MovimientoHorizontal = Mathf.Clamp(movimientoTeclado + movimientoMobile, -1f, 1f);
        SaltoPresionado = Input.GetButtonDown("Jump") || saltoMobile;
        AtaquePresionado = ataqueMobile || Input.GetKeyDown(KeyCode.J) || ClickAtaquePresionado();
    }

    private bool ClickAtaquePresionado()
    {
        if(!Input.GetButtonDown("Fire1"))
        {
            return false;
        }

        return !HayPunteroSobreUI();
    }

    private bool HayPunteroSobreUI()
    {
        if(EventSystem.current == null)
        {
            return false;
        }

        for(int i = 0; i < Input.touchCount; i++)
        {
            if(EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
            {
                return true;
            }
        }

        return EventSystem.current.IsPointerOverGameObject();
    }
}
