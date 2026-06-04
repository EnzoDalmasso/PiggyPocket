using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

// Pantalla de derrota del nivel. Vive dentro de HUDCanvas y se muestra cuando PlayerHealth avisa la muerte.
public class GameOverScreen : MonoBehaviour
{
    private const string OverlayPath = "GameOverOverlay";
    private const string CardPath = "GameOverOverlay/GameOverCard";

    [Header("Referencias")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private GameObject panelDerrota;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text textoTitulo;
    [SerializeField] private TMP_Text textoDetalle;
    [SerializeField] private Button botonReintentar;
    [SerializeField] private Button botonMenuPrincipal;

    [Header("Contenido")]
    [SerializeField] private string titulo = "Derrota";
    [SerializeField] private string detalle = "Intentalo otra vez.";

    [Header("Comportamiento")]
    [SerializeField] private float delayMostrar = 1.2f;
    [SerializeField] private bool pausarJuegoAlMostrar = true;
    [SerializeField] private string nombreEscenaMenuPrincipal = "MainMenu";
    [SerializeField] private string rutaEscenaMenuPrincipalEditor = "Assets/Scenes/MainMenu.unity";

    private bool visible;
    private bool eventosSuscritos;
    private float timeScaleAnterior = 1f;
    private Coroutine rutinaMostrar;

    void Awake()
    {
        AsegurarEventSystem();
        AsegurarReferencias();
        ConfigurarBotones();
        OcultarInstantaneo();
    }

    void OnEnable()
    {
        SuscribirEventos();
    }

    void Start()
    {
        AsegurarReferencias();
        SuscribirEventos();

        if(playerHealth != null && playerHealth.EstaMuerto)
        {
            IniciarGameOver();
        }
    }

    void OnDisable()
    {
        DesuscribirEventos();
    }

    void OnDestroy()
    {
        if(visible && pausarJuegoAlMostrar)
        {
            RestaurarTiempo();
        }
    }

    public void IniciarGameOver()
    {
        if(visible || rutinaMostrar != null)
        {
            return;
        }

        rutinaMostrar = StartCoroutine(MostrarConDelay());
    }

    public void Mostrar()
    {
        if(visible)
        {
            return;
        }

        visible = true;
        GameAudioManager.DetenerMusicaFondo();
        GameAudioManager.ReproducirDerrota();

        if(textoTitulo != null)
        {
            textoTitulo.text = titulo;
        }

        if(textoDetalle != null)
        {
            textoDetalle.text = detalle;
        }

        if(panelDerrota != null)
        {
            panelDerrota.SetActive(true);
        }

        if(canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        if(pausarJuegoAlMostrar)
        {
            timeScaleAnterior = Time.timeScale;
            Time.timeScale = 0f;
        }
    }

    public void ReiniciarNivel()
    {
        RestaurarTiempo();

        Scene escenaActual = SceneManager.GetActiveScene();

        if(escenaActual.buildIndex >= 0)
        {
            SceneManager.LoadScene(escenaActual.buildIndex);
        }
        else
        {
#if UNITY_EDITOR
            EditorSceneManager.LoadScene(escenaActual.path);
#else
            SceneManager.LoadScene(escenaActual.name);
#endif
        }
    }

    public void CargarMenuPrincipal()
    {
        RestaurarTiempo();

#if UNITY_EDITOR
        if(!string.IsNullOrEmpty(rutaEscenaMenuPrincipalEditor))
        {
            EditorSceneManager.LoadScene(rutaEscenaMenuPrincipalEditor);
            return;
        }
#endif

        SceneManager.LoadScene(nombreEscenaMenuPrincipal);
    }

    private IEnumerator MostrarConDelay()
    {
        if(delayMostrar > 0f)
        {
            yield return new WaitForSecondsRealtime(delayMostrar);
        }

        rutinaMostrar = null;
        Mostrar();
    }

    private void AsegurarReferencias()
    {
        if(!EsReferenciaDeEscena(playerHealth))
        {
            playerHealth = Object.FindFirstObjectByType<PlayerHealth>();
        }

        if(panelDerrota == null)
        {
            Transform overlay = transform.Find(OverlayPath);
            panelDerrota = overlay != null ? overlay.gameObject : null;
        }

        if(panelDerrota == null)
        {
            CrearPanelPorDefecto();
        }

        if(canvasGroup == null && panelDerrota != null)
        {
            canvasGroup = panelDerrota.GetComponent<CanvasGroup>();
        }

        if(textoTitulo == null)
        {
            textoTitulo = BuscarComponente<TMP_Text>(CardPath + "/Titulo");
        }

        if(textoDetalle == null)
        {
            textoDetalle = BuscarComponente<TMP_Text>(CardPath + "/Detalle");
        }

        if(botonReintentar == null)
        {
            botonReintentar = BuscarComponente<Button>(CardPath + "/BotonReintentar");
        }

        if(botonMenuPrincipal == null)
        {
            botonMenuPrincipal = BuscarComponente<Button>(CardPath + "/BotonMenuPrincipal");
        }

        AsegurarBotonesPorDefecto();
    }

    private void CrearPanelPorDefecto()
    {
        GameObject overlayGO = CrearUIObject("GameOverOverlay", transform);
        RectTransform overlay = overlayGO.GetComponent<RectTransform>();
        StretchCompleto(overlay);
        overlay.SetAsLastSibling();

        Image overlayImage = overlayGO.AddComponent<Image>();
        overlayImage.color = new Color(0.03f, 0.05f, 0.07f, 0.78f);
        overlayImage.raycastTarget = true;

        canvasGroup = overlayGO.AddComponent<CanvasGroup>();

        GameObject cardGO = CrearCard("GameOverCard", overlay, new Vector2(560f, 340f));
        RectTransform card = cardGO.GetComponent<RectTransform>();

        textoTitulo = CrearTexto("Titulo", card, titulo, 46f, new Vector2(0f, 104f), new Vector2(500f, 64f));
        textoDetalle = CrearTexto("Detalle", card, detalle, 26f, new Vector2(0f, 28f), new Vector2(470f, 54f));
        botonReintentar = CrearBoton("BotonReintentar", card, "Reintentar", new Vector2(-132f, -98f), new Vector2(220f, 54f));
        botonMenuPrincipal = CrearBoton("BotonMenuPrincipal", card, "Menu principal", new Vector2(132f, -98f), new Vector2(220f, 54f));

        panelDerrota = overlayGO;
        overlayGO.SetActive(false);
    }

    private void AsegurarBotonesPorDefecto()
    {
        if(botonReintentar != null)
        {
            ActualizarTextoBoton(botonReintentar, "Reintentar");
        }

        Transform card = transform.Find(CardPath);

        if(card == null || botonMenuPrincipal != null)
        {
            return;
        }

        RectTransform botonReintentarRect = botonReintentar != null ? botonReintentar.GetComponent<RectTransform>() : null;

        if(botonReintentarRect != null)
        {
            botonReintentarRect.anchoredPosition = new Vector2(-132f, -98f);
            botonReintentarRect.sizeDelta = new Vector2(220f, 54f);
        }

        botonMenuPrincipal = CrearBoton("BotonMenuPrincipal", card, "Menu principal", new Vector2(132f, -98f), new Vector2(220f, 54f), botonReintentar);
    }

    private void ConfigurarBotones()
    {
        ConfigurarBoton(botonReintentar, ReiniciarNivel);
        ConfigurarBoton(botonMenuPrincipal, CargarMenuPrincipal);
    }

    private void ConfigurarBoton(Button boton, UnityEngine.Events.UnityAction accion)
    {
        if(boton == null)
        {
            return;
        }

        boton.onClick.RemoveListener(GameAudioManager.ReproducirClickUI);
        boton.onClick.AddListener(GameAudioManager.ReproducirClickUI);
        boton.onClick.RemoveListener(accion);
        boton.onClick.AddListener(accion);
    }

    private void SuscribirEventos()
    {
        if(eventosSuscritos || playerHealth == null)
        {
            return;
        }

        playerHealth.Muerte += IniciarGameOver;
        eventosSuscritos = true;
    }

    private void DesuscribirEventos()
    {
        if(!eventosSuscritos || playerHealth == null)
        {
            return;
        }

        playerHealth.Muerte -= IniciarGameOver;
        eventosSuscritos = false;
    }

    private void OcultarInstantaneo()
    {
        visible = false;

        if(canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if(panelDerrota != null)
        {
            panelDerrota.SetActive(false);
        }
    }

    private void RestaurarTiempo()
    {
        if(!pausarJuegoAlMostrar)
        {
            return;
        }

        Time.timeScale = timeScaleAnterior > 0f ? timeScaleAnterior : 1f;
    }

    private bool EsReferenciaDeEscena(Component referencia)
    {
        return referencia != null && referencia.gameObject.scene.IsValid();
    }

    private T BuscarComponente<T>(string ruta) where T : Component
    {
        Transform encontrado = transform.Find(ruta);
        return encontrado != null ? encontrado.GetComponent<T>() : null;
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

    private static GameObject CrearCard(string nombre, Transform padre, Vector2 tamano)
    {
        GameObject cardGO = CrearUIObject(nombre, padre);
        RectTransform card = cardGO.GetComponent<RectTransform>();
        card.anchorMin = new Vector2(0.5f, 0.5f);
        card.anchorMax = new Vector2(0.5f, 0.5f);
        card.pivot = new Vector2(0.5f, 0.5f);
        card.anchoredPosition = Vector2.zero;
        card.sizeDelta = tamano;

        Image cardImage = cardGO.AddComponent<Image>();
        cardImage.color = new Color(0.96f, 0.91f, 0.72f, 1f);
        cardImage.raycastTarget = true;

        return cardGO;
    }

    private static TMP_Text CrearTexto(string nombre, Transform padre, string contenido, float fontSize, Vector2 posicion, Vector2 tamano)
    {
        GameObject textGO = CrearUIObject(nombre, padre);
        RectTransform rectTransform = textGO.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = posicion;
        rectTransform.sizeDelta = tamano;

        TMP_Text texto = textGO.AddComponent<TextMeshProUGUI>();
        texto.text = contenido;
        texto.fontSize = fontSize;
        texto.color = new Color(0.12f, 0.11f, 0.09f, 1f);
        texto.alignment = TextAlignmentOptions.Center;
        texto.textWrappingMode = TextWrappingModes.NoWrap;
        texto.raycastTarget = false;

        return texto;
    }

    private static Button CrearBoton(string nombre, Transform padre, string contenido, Vector2 posicion, Vector2 tamano)
    {
        GameObject buttonGO = CrearUIObject(nombre, padre);
        RectTransform rectTransform = buttonGO.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = posicion;
        rectTransform.sizeDelta = tamano;

        Image image = buttonGO.AddComponent<Image>();
        image.color = new Color(0.93f, 0.55f, 0.18f, 1f);

        Button boton = buttonGO.AddComponent<Button>();
        boton.targetGraphic = image;

        ColorBlock colors = boton.colors;
        colors.highlightedColor = new Color(1f, 0.66f, 0.26f, 1f);
        colors.pressedColor = new Color(0.78f, 0.38f, 0.11f, 1f);
        boton.colors = colors;

        GameObject labelGO = CrearUIObject("Texto", buttonGO.transform);
        RectTransform labelRect = labelGO.GetComponent<RectTransform>();
        StretchCompleto(labelRect);

        TMP_Text label = labelGO.AddComponent<TextMeshProUGUI>();
        label.text = contenido;
        label.fontSize = 24f;
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.raycastTarget = false;

        return boton;
    }

    private static Button CrearBoton(string nombre, Transform padre, string contenido, Vector2 posicion, Vector2 tamano, Button botonBase)
    {
        Button boton = CrearBoton(nombre, padre, contenido, posicion, tamano);

        if(botonBase != null)
        {
            boton.colors = botonBase.colors;

            Image imagen = boton.targetGraphic as Image;
            Image imagenBase = botonBase.targetGraphic as Image;

            if(imagen != null && imagenBase != null)
            {
                imagen.color = imagenBase.color;
            }
        }

        return boton;
    }

    private static void ActualizarTextoBoton(Button boton, string contenido)
    {
        TMP_Text texto = boton != null ? boton.GetComponentInChildren<TMP_Text>() : null;

        if(texto != null)
        {
            texto.text = contenido;
        }
    }

    private static void StretchCompleto(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}
