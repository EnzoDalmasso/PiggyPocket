using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Controles tactiles del HUD. Crea botones de movimiento, salto y ataque dentro de HUDCanvas.
public class MobileControlsUI : MonoBehaviour
{
    private const string ControlsPath = "MobileControls";
    private const string PauseButtonPath = "BotonPausaTouch";

    [Header("Visibilidad")]
    [SerializeField] private bool mostrarEnEditor = true;
    [SerializeField] private bool mostrarEnStandalone = false;
    [SerializeField] private bool mostrarEnWebGLConTouch = true;

    [Header("Sprites")]
    [SerializeField] private Sprite spriteIzquierda;
    [SerializeField] private Sprite spriteDerecha;
    [SerializeField] private Sprite spriteBotonSalto;
    [SerializeField] private Sprite spriteBotonAtaque;
    [SerializeField] private Sprite spriteBotonPausa;
    [SerializeField] private Sprite spriteIconoSalto;
    [SerializeField] private Sprite spriteIconoAtaque;
    [SerializeField] private Sprite spriteIconoPausa;

    [Header("Layout")]
    [SerializeField] private Vector2 tamanoBotonMovimiento = new Vector2(104f, 104f);
    [SerializeField] private Vector2 tamanoBotonAccion = new Vector2(112f, 112f);
    [SerializeField] private Vector2 tamanoBotonPausa = new Vector2(88f, 88f);
    [SerializeField] private Vector2 tamanoIconoAccion = new Vector2(54f, 54f);
    [SerializeField] private Vector2 tamanoIconoPausa = new Vector2(42f, 42f);
    [SerializeField] private float margenHorizontal = 42f;
    [SerializeField] private float margenVertical = 38f;

    private GameObject panelControles;
    private Button botonPausaTouch;

    void Awake()
    {
        AsegurarEventSystem();
        AsegurarReferencias();
        AplicarVisibilidad();
    }

    void OnEnable()
    {
        AplicarVisibilidad();
    }

    void OnDisable()
    {
        MobileInputState.Reiniciar();
    }

    private void AsegurarReferencias()
    {
        Transform existente = transform.Find(ControlsPath);
        panelControles = existente != null ? existente.gameObject : null;

        if(panelControles == null)
        {
            CrearControles();
        }

        AsegurarBotonPausa();
    }

    private void CrearControles()
    {
        panelControles = CrearUIObject(ControlsPath, transform);
        RectTransform root = panelControles.GetComponent<RectTransform>();
        StretchCompleto(root);
        root.SetSiblingIndex(Mathf.Min(1, transform.childCount - 1));

        GameObject grupoMovimiento = CrearUIObject("Movimiento", panelControles.transform);
        RectTransform movimientoRect = grupoMovimiento.GetComponent<RectTransform>();
        movimientoRect.anchorMin = new Vector2(0f, 0f);
        movimientoRect.anchorMax = new Vector2(0f, 0f);
        movimientoRect.pivot = new Vector2(0f, 0f);
        movimientoRect.anchoredPosition = new Vector2(margenHorizontal, margenVertical);
        movimientoRect.sizeDelta = new Vector2(236f, 116f);

        CrearBotonControl(
            "BotonIzquierda",
            grupoMovimiento.transform,
            MobileInputAction.Izquierda,
            spriteIzquierda,
            null,
            new Vector2(52f, 52f),
            tamanoBotonMovimiento,
            Vector2.zero);

        CrearBotonControl(
            "BotonDerecha",
            grupoMovimiento.transform,
            MobileInputAction.Derecha,
            spriteDerecha,
            null,
            new Vector2(168f, 52f),
            tamanoBotonMovimiento,
            Vector2.zero);

        GameObject grupoAcciones = CrearUIObject("Acciones", panelControles.transform);
        RectTransform accionesRect = grupoAcciones.GetComponent<RectTransform>();
        accionesRect.anchorMin = new Vector2(1f, 0f);
        accionesRect.anchorMax = new Vector2(1f, 0f);
        accionesRect.pivot = new Vector2(1f, 0f);
        accionesRect.anchoredPosition = new Vector2(-margenHorizontal, margenVertical);
        accionesRect.sizeDelta = new Vector2(278f, 178f);

        CrearBotonControl(
            "BotonAtaque",
            grupoAcciones.transform,
            MobileInputAction.Ataque,
            spriteBotonAtaque,
            spriteIconoAtaque,
            new Vector2(-56f, 56f),
            tamanoBotonAccion,
            tamanoIconoAccion);

        CrearBotonControl(
            "BotonSalto",
            grupoAcciones.transform,
            MobileInputAction.Salto,
            spriteBotonSalto,
            spriteIconoSalto,
            new Vector2(-170f, 104f),
            tamanoBotonAccion,
            tamanoIconoAccion);

        AsegurarBotonPausa();
    }

    private void AsegurarBotonPausa()
    {
        if(panelControles == null)
        {
            return;
        }

        Transform existente = panelControles.transform.Find(PauseButtonPath);

        if(existente != null)
        {
            botonPausaTouch = existente.GetComponent<Button>();
            AsegurarIconoPausa(existente);
        }
        else
        {
            botonPausaTouch = CrearBotonPausa(panelControles.transform);
        }

        ConfigurarBotonPausa();
    }

    private void AsegurarIconoPausa(Transform boton)
    {
        if(boton.Find("Icono") != null || boton.Find("BarraIzquierda") != null)
        {
            return;
        }

        if(spriteIconoPausa != null)
        {
            CrearIcono("Icono", boton, spriteIconoPausa, tamanoIconoPausa);
        }
        else
        {
            CrearIconoPausaPorDefecto(boton);
        }
    }

    private Button CrearBotonPausa(Transform padre)
    {
        GameObject botonGO = CrearUIObject(PauseButtonPath, padre);
        RectTransform rectTransform = botonGO.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(1f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(1f, 1f);
        rectTransform.anchoredPosition = new Vector2(-margenHorizontal, -margenVertical);
        rectTransform.sizeDelta = tamanoBotonPausa;

        Image fondo = botonGO.AddComponent<Image>();
        fondo.sprite = spriteBotonPausa;
        fondo.preserveAspect = true;
        fondo.raycastTarget = true;
        fondo.color = new Color(1f, 1f, 1f, 0.72f);

        Button boton = botonGO.AddComponent<Button>();
        boton.targetGraphic = fondo;

        ColorBlock colors = boton.colors;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.9f);
        colors.pressedColor = new Color(1f, 1f, 1f, 0.55f);
        boton.colors = colors;

        if(spriteIconoPausa != null)
        {
            CrearIcono(PauseButtonPath + "Icono", botonGO.transform, spriteIconoPausa, tamanoIconoPausa);
        }
        else
        {
            CrearIconoPausaPorDefecto(botonGO.transform);
        }

        return boton;
    }

    private void ConfigurarBotonPausa()
    {
        if(botonPausaTouch == null)
        {
            return;
        }

        botonPausaTouch.onClick.RemoveListener(GameAudioManager.ReproducirClickUI);
        botonPausaTouch.onClick.AddListener(GameAudioManager.ReproducirClickUI);
        botonPausaTouch.onClick.RemoveListener(AbrirPausa);
        botonPausaTouch.onClick.AddListener(AbrirPausa);
    }

    private void AbrirPausa()
    {
        PauseMenuScreen pauseMenu = GetComponent<PauseMenuScreen>();

        if(pauseMenu == null)
        {
            pauseMenu = Object.FindFirstObjectByType<PauseMenuScreen>(FindObjectsInactive.Include);
        }

        if(pauseMenu != null)
        {
            pauseMenu.Mostrar();
        }
    }

    private void CrearBotonControl(
        string nombre,
        Transform padre,
        MobileInputAction accion,
        Sprite spriteFondo,
        Sprite spriteIcono,
        Vector2 posicion,
        Vector2 tamano,
        Vector2 tamanoIcono)
    {
        GameObject botonGO = CrearUIObject(nombre, padre);
        RectTransform rectTransform = botonGO.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 0f);
        rectTransform.anchorMax = new Vector2(0f, 0f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = posicion;
        rectTransform.sizeDelta = tamano;

        Image fondo = botonGO.AddComponent<Image>();
        fondo.sprite = spriteFondo;
        fondo.preserveAspect = true;
        fondo.raycastTarget = true;
        fondo.color = new Color(1f, 1f, 1f, 0.72f);

        MobileInputButton inputButton = botonGO.AddComponent<MobileInputButton>();
        inputButton.Configurar(accion, fondo);

        if(spriteIcono == null)
        {
            return;
        }

        CrearIcono("Icono", botonGO.transform, spriteIcono, tamanoIcono);
    }

    private void CrearIcono(string nombre, Transform padre, Sprite spriteIcono, Vector2 tamanoIcono)
    {
        GameObject iconoGO = CrearUIObject(nombre, padre);
        RectTransform iconoRect = iconoGO.GetComponent<RectTransform>();
        iconoRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconoRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconoRect.pivot = new Vector2(0.5f, 0.5f);
        iconoRect.anchoredPosition = Vector2.zero;
        iconoRect.sizeDelta = tamanoIcono;

        Image icono = iconoGO.AddComponent<Image>();
        icono.sprite = spriteIcono;
        icono.preserveAspect = true;
        icono.raycastTarget = false;
        icono.color = Color.white;
    }

    private void CrearIconoPausaPorDefecto(Transform padre)
    {
        CrearBarraPausa("BarraIzquierda", padre, new Vector2(-8f, 0f));
        CrearBarraPausa("BarraDerecha", padre, new Vector2(8f, 0f));
    }

    private void CrearBarraPausa(string nombre, Transform padre, Vector2 posicion)
    {
        GameObject barraGO = CrearUIObject(nombre, padre);
        RectTransform barraRect = barraGO.GetComponent<RectTransform>();
        barraRect.anchorMin = new Vector2(0.5f, 0.5f);
        barraRect.anchorMax = new Vector2(0.5f, 0.5f);
        barraRect.pivot = new Vector2(0.5f, 0.5f);
        barraRect.anchoredPosition = posicion;
        barraRect.sizeDelta = new Vector2(10f, 34f);

        Image barra = barraGO.AddComponent<Image>();
        barra.color = Color.white;
        barra.raycastTarget = false;
    }

    private void AplicarVisibilidad()
    {
        if(panelControles == null)
        {
            return;
        }

        bool visible = DebeMostrarControles();
        panelControles.SetActive(visible);

        if(!visible)
        {
            MobileInputState.Reiniciar();
        }
    }

    private bool DebeMostrarControles()
    {
        if(Application.isEditor)
        {
            return mostrarEnEditor;
        }

        if(Application.platform == RuntimePlatform.WebGLPlayer)
        {
            return mostrarEnWebGLConTouch && Input.touchSupported;
        }

        return Application.isMobilePlatform || mostrarEnStandalone;
    }

    private static void AsegurarEventSystem()
    {
        if(Object.FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemGO = new GameObject("EventSystem");
        eventSystemGO.AddComponent<EventSystem>();
        eventSystemGO.AddComponent<StandaloneInputModule>();
    }

    private static GameObject CrearUIObject(string nombre, Transform padre)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        go.transform.SetParent(padre, false);
        return go;
    }

    private static void StretchCompleto(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}
