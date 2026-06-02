using UnityEngine;

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
        MovimientoHorizontal = Input.GetAxisRaw("Horizontal");
        SaltoPresionado = Input.GetButtonDown("Jump");
        AtaquePresionado = Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.J);
    }
}
