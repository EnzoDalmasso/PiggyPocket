using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Boton tactil reutilizable para movimiento, salto y ataque.
// Usa eventos de puntero para funcionar con touch y mouse durante pruebas en Editor.
public class MobileInputButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField] private MobileInputAction accion;
    [SerializeField] private Image imagenObjetivo;
    [SerializeField] private Color colorNormal = new Color(1f, 1f, 1f, 0.72f);
    [SerializeField] private Color colorPresionado = new Color(1f, 1f, 1f, 1f);

    private bool presionado;
    private int pointerId;

    void Awake()
    {
        if(imagenObjetivo == null)
        {
            imagenObjetivo = GetComponent<Image>();
        }

        ActualizarVisual(false);
    }

    void OnDisable()
    {
        Soltar();
    }

    public void Configurar(MobileInputAction nuevaAccion, Image nuevaImagenObjetivo = null)
    {
        accion = nuevaAccion;

        if(nuevaImagenObjetivo != null)
        {
            imagenObjetivo = nuevaImagenObjetivo;
        }

        ActualizarVisual(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(presionado)
        {
            return;
        }

        presionado = true;
        pointerId = eventData.pointerId;
        MobileInputState.Presionar(accion);
        ActualizarVisual(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(!presionado || eventData.pointerId != pointerId)
        {
            return;
        }

        Soltar();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(!presionado || eventData.pointerId != pointerId)
        {
            return;
        }

        Soltar();
    }

    private void Soltar()
    {
        if(!presionado)
        {
            return;
        }

        presionado = false;
        MobileInputState.Soltar(accion);
        ActualizarVisual(false);
    }

    private void ActualizarVisual(bool estaPresionado)
    {
        if(imagenObjetivo == null)
        {
            return;
        }

        imagenObjetivo.color = estaPresionado ? colorPresionado : colorNormal;
    }
}
