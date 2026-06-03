using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

// Pantalla final del nivel. Vive dentro de HUDCanvas y se activa cuando LevelGoal confirma la victoria.
public class LevelVictoryScreen : MonoBehaviour
{
    private const string OverlayPath = "VictoryOverlay";
    private const string CardPath = "VictoryOverlay/VictoryCard";

    [Header("Referencias")]
    [SerializeField] private GameObject panelVictoria;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text textoTitulo;
    [SerializeField] private TMP_Text textoMonedas;
    [SerializeField] private TMP_Text textoDetalle;
    [SerializeField] private Button botonReiniciar;
    [SerializeField] private Button botonMenuPrincipal;

    [Header("Contenido")]
    [SerializeField] private string titulo = "Nivel completado";
    [SerializeField] private string formatoMonedas = "Monedas recolectadas: {0}";
    [SerializeField] private string detalle = "Llegaste a la meta final.";

    [Header("Comportamiento")]
    [SerializeField] private bool pausarJuegoAlGanar = true;
    [SerializeField] private string nombreEscenaMenuPrincipal = "MainMenu";
    [SerializeField] private string rutaEscenaMenuPrincipalEditor = "Assets/Scenes/MainMenu.unity";

    private bool visible;
    private float timeScaleAnterior = 1f;

    void Awake()
    {
        AsegurarEventSystem();
        AsegurarReferencias();
        ConfigurarBotones();
        OcultarInstantaneo();
    }

    void OnDestroy()
    {
        if(visible && pausarJuegoAlGanar)
        {
            RestaurarTiempo();
        }
    }

    public void Mostrar(PlayerWallet playerWallet)
    {
        int monedas = playerWallet != null ? playerWallet.Monedas : 0;
        Mostrar(monedas);
    }

    public void Mostrar(int monedasRecolectadas)
    {
        if(visible)
        {
            return;
        }

        visible = true;

        if(textoTitulo != null)
        {
            textoTitulo.text = titulo;
        }

        if(textoMonedas != null)
        {
            textoMonedas.text = string.Format(formatoMonedas, monedasRecolectadas);
        }

        if(textoDetalle != null)
        {
            textoDetalle.text = detalle;
        }

        if(panelVictoria != null)
        {
            panelVictoria.SetActive(true);
        }

        if(canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        if(pausarJuegoAlGanar)
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

    private void AsegurarReferencias()
    {
        if(panelVictoria == null)
        {
            Transform overlay = transform.Find(OverlayPath);
            panelVictoria = overlay != null ? overlay.gameObject : null;
        }

        if(canvasGroup == null && panelVictoria != null)
        {
            canvasGroup = panelVictoria.GetComponent<CanvasGroup>();
        }

        if(textoTitulo == null)
        {
            textoTitulo = BuscarTexto(CardPath + "/Titulo");
        }

        if(textoMonedas == null)
        {
            textoMonedas = BuscarTexto(CardPath + "/Monedas");
        }

        if(textoDetalle == null)
        {
            textoDetalle = BuscarTexto(CardPath + "/Detalle");
        }

        if(botonReiniciar == null)
        {
            botonReiniciar = BuscarComponente<Button>(CardPath + "/BotonReiniciar");
        }

        if(botonMenuPrincipal == null)
        {
            botonMenuPrincipal = BuscarComponente<Button>(CardPath + "/BotonMenuPrincipal");
        }

        AsegurarBotonesPorDefecto();

        if(panelVictoria == null)
        {
            Debug.LogWarning("LevelVictoryScreen no encontro VictoryOverlay dentro de HUDCanvas.", this);
        }
    }

    private void AsegurarBotonesPorDefecto()
    {
        if(botonReiniciar != null)
        {
            ActualizarTextoBoton(botonReiniciar, "Reintentar");
        }

        Transform card = transform.Find(CardPath);

        if(card == null || botonMenuPrincipal != null)
        {
            return;
        }

        RectTransform botonReiniciarRect = botonReiniciar != null ? botonReiniciar.GetComponent<RectTransform>() : null;

        if(botonReiniciarRect != null)
        {
            botonReiniciarRect.anchoredPosition = new Vector2(-132f, -116f);
            botonReiniciarRect.sizeDelta = new Vector2(220f, 54f);
        }

        botonMenuPrincipal = CrearBoton("BotonMenuPrincipal", card, "Menu principal", new Vector2(132f, -116f), new Vector2(220f, 54f), botonReiniciar);
    }

    private void ConfigurarBotones()
    {
        ConfigurarBoton(botonReiniciar, ReiniciarNivel);
        ConfigurarBoton(botonMenuPrincipal, CargarMenuPrincipal);
    }

    private void ConfigurarBoton(Button boton, UnityEngine.Events.UnityAction accion)
    {
        if(boton == null)
        {
            return;
        }

        boton.onClick.RemoveListener(accion);
        boton.onClick.AddListener(accion);
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

        if(panelVictoria != null)
        {
            panelVictoria.SetActive(false);
        }
    }

    private void RestaurarTiempo()
    {
        visible = false;
        Time.timeScale = timeScaleAnterior > 0f ? timeScaleAnterior : 1f;
    }

    private TMP_Text BuscarTexto(string ruta)
    {
        return BuscarComponente<TMP_Text>(ruta);
    }

    private T BuscarComponente<T>(string ruta) where T : Component
    {
        Transform encontrado = transform.Find(ruta);
        return encontrado != null ? encontrado.GetComponent<T>() : null;
    }

    private static Button CrearBoton(string nombre, Transform padre, string contenido, Vector2 posicion, Vector2 tamano, Button botonBase)
    {
        GameObject buttonGO = new GameObject(nombre, typeof(RectTransform));
        buttonGO.layer = LayerMask.NameToLayer("UI");
        buttonGO.transform.SetParent(padre, false);

        RectTransform rectTransform = buttonGO.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = posicion;
        rectTransform.sizeDelta = tamano;

        Image image = buttonGO.AddComponent<Image>();
        Image imagenBase = botonBase != null ? botonBase.targetGraphic as Image : null;
        image.color = imagenBase != null ? imagenBase.color : new Color(0.93f, 0.55f, 0.18f, 1f);

        Button boton = buttonGO.AddComponent<Button>();
        boton.targetGraphic = image;

        if(botonBase != null)
        {
            boton.colors = botonBase.colors;
        }

        GameObject labelGO = new GameObject("Texto", typeof(RectTransform));
        labelGO.layer = LayerMask.NameToLayer("UI");
        labelGO.transform.SetParent(buttonGO.transform, false);

        RectTransform labelRect = labelGO.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TMP_Text label = labelGO.AddComponent<TextMeshProUGUI>();
        label.text = contenido;
        label.fontSize = 24f;
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.raycastTarget = false;

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

}
