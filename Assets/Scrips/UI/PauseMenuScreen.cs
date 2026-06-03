using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

// Menu de pausa del nivel. Vive dentro de HUDCanvas y se abre con Escape o P.
public class PauseMenuScreen : MonoBehaviour
{
    private const string OverlayPath = "PauseOverlay";
    private const string CardPath = "PauseOverlay/PauseCard";
    private const string SettingsPath = "PauseOverlay/SettingsCard";
    private const string VolumenKey = "PiggyPocket_VolumenGeneral";
    private const string SilencioKey = "PiggyPocket_AudioSilenciado";

    [Header("Referencias")]
    [SerializeField] private GameObject panelPausa;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject menuPrincipal;
    [SerializeField] private GameObject panelAjustes;
    [SerializeField] private Button botonContinuar;
    [SerializeField] private Button botonAjustes;
    [SerializeField] private Button botonReiniciar;
    [SerializeField] private Button botonVolverAjustes;
    [SerializeField] private GameObject[] overlaysBloqueantes;

    [Header("Ajustes")]
    [SerializeField] private Slider sliderVolumen;
    [SerializeField] private TMP_Text textoValorVolumen;
    [SerializeField] private Toggle toggleSilencio;

    [Header("Input")]
    [SerializeField] private bool pausarConTeclado = true;
    [SerializeField] private KeyCode teclaPausa = KeyCode.Escape;
    [SerializeField] private KeyCode teclaPausaAlternativa = KeyCode.P;

    [Header("Comportamiento")]
    [SerializeField] private bool pausarTiempo = true;

    private bool visible;
    private float timeScaleAnterior = 1f;
    private bool cargandoAjustes;

    void Awake()
    {
        AsegurarEventSystem();
        AsegurarReferencias();
        ConfigurarBotones();
        ConfigurarAjustes();
        OcultarInstantaneo();
    }

    void Update()
    {
        if(!pausarConTeclado || !SePresionoPausa())
        {
            return;
        }

        if(visible)
        {
            Ocultar();
            return;
        }

        if(!HayOverlayBloqueanteActivo())
        {
            Mostrar();
        }
    }

    void OnDestroy()
    {
        if(visible && pausarTiempo)
        {
            RestaurarTiempo();
        }
    }

    public void Mostrar()
    {
        if(visible)
        {
            return;
        }

        visible = true;

        if(panelPausa != null)
        {
            panelPausa.SetActive(true);
        }

        MostrarMenuPrincipal();

        if(canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        if(pausarTiempo)
        {
            timeScaleAnterior = Time.timeScale;
            Time.timeScale = 0f;
        }
    }

    public void Ocultar()
    {
        if(!visible)
        {
            return;
        }

        visible = false;
        RestaurarTiempo();
        OcultarInstantaneo();
    }

    public void MostrarAjustes()
    {
        if(menuPrincipal != null)
        {
            menuPrincipal.SetActive(false);
        }

        if(panelAjustes != null)
        {
            panelAjustes.SetActive(true);
        }

        RefrescarVistaAjustes();
    }

    public void MostrarMenuPrincipal()
    {
        if(menuPrincipal != null)
        {
            menuPrincipal.SetActive(true);
        }

        if(panelAjustes != null)
        {
            panelAjustes.SetActive(false);
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

    private void AsegurarReferencias()
    {
        if(panelPausa == null)
        {
            Transform overlay = transform.Find(OverlayPath);
            panelPausa = overlay != null ? overlay.gameObject : null;
        }

        if(panelPausa == null)
        {
            CrearPanelPorDefecto();
        }

        if(canvasGroup == null && panelPausa != null)
        {
            canvasGroup = panelPausa.GetComponent<CanvasGroup>();
        }

        if(menuPrincipal == null)
        {
            Transform card = transform.Find(CardPath);
            menuPrincipal = card != null ? card.gameObject : null;
        }

        if(panelAjustes == null)
        {
            Transform settings = transform.Find(SettingsPath);
            panelAjustes = settings != null ? settings.gameObject : null;
        }

        if(botonContinuar == null)
        {
            botonContinuar = BuscarComponente<Button>(CardPath + "/BotonContinuar");
        }

        if(botonAjustes == null)
        {
            botonAjustes = BuscarComponente<Button>(CardPath + "/BotonAjustes");
        }

        if(botonReiniciar == null)
        {
            botonReiniciar = BuscarComponente<Button>(CardPath + "/BotonReiniciar");
        }

        if(botonVolverAjustes == null)
        {
            botonVolverAjustes = BuscarComponente<Button>(SettingsPath + "/BotonVolver");
        }

        if(sliderVolumen == null)
        {
            sliderVolumen = BuscarComponente<Slider>(SettingsPath + "/SliderVolumen");
        }

        if(textoValorVolumen == null)
        {
            textoValorVolumen = BuscarComponente<TMP_Text>(SettingsPath + "/TextoValorVolumen");
        }

        if(toggleSilencio == null)
        {
            toggleSilencio = BuscarComponente<Toggle>(SettingsPath + "/ToggleSilencio");
        }

        if(overlaysBloqueantes == null || overlaysBloqueantes.Length == 0)
        {
            Transform victoryOverlay = transform.Find("VictoryOverlay");
            Transform gameOverOverlay = transform.Find("GameOverOverlay");
            overlaysBloqueantes = CrearListaOverlaysBloqueantes(victoryOverlay, gameOverOverlay);
        }

        if(panelPausa == null)
        {
            Debug.LogWarning("PauseMenuScreen no encontro PauseOverlay dentro de HUDCanvas.", this);
        }
    }

    private void CrearPanelPorDefecto()
    {
        GameObject overlayGO = CrearUIObject("PauseOverlay", transform);
        RectTransform overlay = overlayGO.GetComponent<RectTransform>();
        StretchCompleto(overlay);
        overlay.SetAsLastSibling();

        Image overlayImage = overlayGO.AddComponent<Image>();
        overlayImage.color = new Color(0.03f, 0.05f, 0.07f, 0.72f);
        overlayImage.raycastTarget = true;

        canvasGroup = overlayGO.AddComponent<CanvasGroup>();

        GameObject cardGO = CrearCard("PauseCard", overlay, new Vector2(520f, 360f));
        RectTransform card = cardGO.GetComponent<RectTransform>();

        CrearTexto("Titulo", card, "Pausa", 46f, new Vector2(0f, 116f), new Vector2(460f, 64f));
        botonContinuar = CrearBoton("BotonContinuar", card, "Continuar", new Vector2(0f, 42f), new Vector2(260f, 54f));
        botonAjustes = CrearBoton("BotonAjustes", card, "Ajustes", new Vector2(0f, -26f), new Vector2(260f, 54f));
        botonReiniciar = CrearBoton("BotonReiniciar", card, "Reiniciar", new Vector2(0f, -94f), new Vector2(260f, 54f));

        GameObject settingsGO = CrearCard("SettingsCard", overlay, new Vector2(520f, 380f));
        RectTransform settingsCard = settingsGO.GetComponent<RectTransform>();
        CrearTexto("Titulo", settingsCard, "Ajustes", 42f, new Vector2(0f, 128f), new Vector2(460f, 58f));
        CrearTexto("TextoVolumen", settingsCard, "Volumen general", 24f, new Vector2(-92f, 62f), new Vector2(260f, 38f));
        textoValorVolumen = CrearTexto("TextoValorVolumen", settingsCard, "100%", 24f, new Vector2(180f, 62f), new Vector2(90f, 38f));
        sliderVolumen = CrearSlider("SliderVolumen", settingsCard, new Vector2(0f, 16f), new Vector2(360f, 36f));
        toggleSilencio = CrearToggle("ToggleSilencio", settingsCard, "Silenciar audio", new Vector2(0f, -56f), new Vector2(300f, 42f));
        botonVolverAjustes = CrearBoton("BotonVolver", settingsCard, "Volver", new Vector2(0f, -128f), new Vector2(220f, 54f));
        settingsGO.SetActive(false);

        panelPausa = overlayGO;
        menuPrincipal = cardGO;
        panelAjustes = settingsGO;

        Transform victoryOverlay = transform.Find("VictoryOverlay");
        Transform gameOverOverlay = transform.Find("GameOverOverlay");
        overlaysBloqueantes = CrearListaOverlaysBloqueantes(victoryOverlay, gameOverOverlay);
        overlayGO.SetActive(false);
    }

    private void ConfigurarBotones()
    {
        ConfigurarBoton(botonContinuar, Ocultar);
        ConfigurarBoton(botonAjustes, MostrarAjustes);
        ConfigurarBoton(botonReiniciar, ReiniciarNivel);
        ConfigurarBoton(botonVolverAjustes, MostrarMenuPrincipal);
    }

    private void ConfigurarAjustes()
    {
        float volumenGuardado = Mathf.Clamp01(PlayerPrefs.GetFloat(VolumenKey, 1f));
        bool audioSilenciado = PlayerPrefs.GetInt(SilencioKey, 0) == 1;

        cargandoAjustes = true;

        if(sliderVolumen != null)
        {
            sliderVolumen.minValue = 0f;
            sliderVolumen.maxValue = 1f;
            sliderVolumen.wholeNumbers = false;
            sliderVolumen.value = volumenGuardado;
            sliderVolumen.onValueChanged.RemoveListener(CambiarVolumen);
            sliderVolumen.onValueChanged.AddListener(CambiarVolumen);
        }

        if(toggleSilencio != null)
        {
            toggleSilencio.isOn = audioSilenciado;
            toggleSilencio.onValueChanged.RemoveListener(CambiarSilencio);
            toggleSilencio.onValueChanged.AddListener(CambiarSilencio);
        }

        cargandoAjustes = false;

        AplicarAudio(volumenGuardado, audioSilenciado);
        ActualizarTextoVolumen(volumenGuardado, audioSilenciado);
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

        if(panelAjustes != null)
        {
            panelAjustes.SetActive(false);
        }

        if(menuPrincipal != null)
        {
            menuPrincipal.SetActive(true);
        }

        if(canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if(panelPausa != null)
        {
            panelPausa.SetActive(false);
        }
    }

    private void RestaurarTiempo()
    {
        if(!pausarTiempo)
        {
            return;
        }

        Time.timeScale = timeScaleAnterior > 0f ? timeScaleAnterior : 1f;
    }

    private void CambiarVolumen(float volumen)
    {
        if(cargandoAjustes)
        {
            return;
        }

        volumen = Mathf.Clamp01(volumen);
        PlayerPrefs.SetFloat(VolumenKey, volumen);
        PlayerPrefs.Save();

        bool audioSilenciado = EstaSilenciado();
        AplicarAudio(volumen, audioSilenciado);
        ActualizarTextoVolumen(volumen, audioSilenciado);
    }

    private void CambiarSilencio(bool audioSilenciado)
    {
        if(cargandoAjustes)
        {
            return;
        }

        PlayerPrefs.SetInt(SilencioKey, audioSilenciado ? 1 : 0);
        PlayerPrefs.Save();

        float volumen = VolumenActual();
        AplicarAudio(volumen, audioSilenciado);
        ActualizarTextoVolumen(volumen, audioSilenciado);
    }

    private void RefrescarVistaAjustes()
    {
        float volumen = VolumenActual();
        bool audioSilenciado = EstaSilenciado();

        AplicarAudio(volumen, audioSilenciado);
        ActualizarTextoVolumen(volumen, audioSilenciado);
    }

    private float VolumenActual()
    {
        return sliderVolumen != null ? sliderVolumen.value : Mathf.Clamp01(PlayerPrefs.GetFloat(VolumenKey, 1f));
    }

    private bool EstaSilenciado()
    {
        return toggleSilencio != null ? toggleSilencio.isOn : PlayerPrefs.GetInt(SilencioKey, 0) == 1;
    }

    private void AplicarAudio(float volumen, bool audioSilenciado)
    {
        AudioListener.volume = audioSilenciado ? 0f : Mathf.Clamp01(volumen);
    }

    private void ActualizarTextoVolumen(float volumen, bool audioSilenciado)
    {
        if(textoValorVolumen == null)
        {
            return;
        }

        textoValorVolumen.text = audioSilenciado ? "Mute" : Mathf.RoundToInt(volumen * 100f) + "%";
    }

    private bool SePresionoPausa()
    {
        return Input.GetKeyDown(teclaPausa) || Input.GetKeyDown(teclaPausaAlternativa);
    }

    private bool HayOverlayBloqueanteActivo()
    {
        if(OverlayActivo("VictoryOverlay") || OverlayActivo("GameOverOverlay"))
        {
            return true;
        }

        if(overlaysBloqueantes == null)
        {
            return false;
        }

        foreach(GameObject overlay in overlaysBloqueantes)
        {
            if(overlay != null && overlay.activeInHierarchy)
            {
                return true;
            }
        }

        return false;
    }

    private bool OverlayActivo(string nombre)
    {
        Transform overlay = transform.Find(nombre);
        return overlay != null && overlay.gameObject.activeInHierarchy;
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

    private static GameObject[] CrearListaOverlaysBloqueantes(params Transform[] overlays)
    {
        int cantidad = 0;

        foreach(Transform overlay in overlays)
        {
            if(overlay != null)
            {
                cantidad++;
            }
        }

        GameObject[] resultado = new GameObject[cantidad];
        int indice = 0;

        foreach(Transform overlay in overlays)
        {
            if(overlay == null)
            {
                continue;
            }

            resultado[indice] = overlay.gameObject;
            indice++;
        }

        return resultado;
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

    private static Slider CrearSlider(string nombre, Transform padre, Vector2 posicion, Vector2 tamano)
    {
        GameObject sliderGO = CrearUIObject(nombre, padre);
        RectTransform rectTransform = sliderGO.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = posicion;
        rectTransform.sizeDelta = tamano;

        Slider slider = sliderGO.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        slider.direction = Slider.Direction.LeftToRight;

        GameObject backgroundGO = CrearUIObject("Background", sliderGO.transform);
        RectTransform background = backgroundGO.GetComponent<RectTransform>();
        background.anchorMin = new Vector2(0f, 0.5f);
        background.anchorMax = new Vector2(1f, 0.5f);
        background.anchoredPosition = Vector2.zero;
        background.sizeDelta = new Vector2(0f, 10f);

        Image backgroundImage = backgroundGO.AddComponent<Image>();
        backgroundImage.color = new Color(0.28f, 0.23f, 0.18f, 0.35f);

        GameObject fillAreaGO = CrearUIObject("Fill Area", sliderGO.transform);
        RectTransform fillArea = fillAreaGO.GetComponent<RectTransform>();
        fillArea.anchorMin = new Vector2(0f, 0.5f);
        fillArea.anchorMax = new Vector2(1f, 0.5f);
        fillArea.anchoredPosition = Vector2.zero;
        fillArea.sizeDelta = new Vector2(-18f, 10f);

        GameObject fillGO = CrearUIObject("Fill", fillAreaGO.transform);
        RectTransform fill = fillGO.GetComponent<RectTransform>();
        StretchCompleto(fill);

        Image fillImage = fillGO.AddComponent<Image>();
        fillImage.color = new Color(0.93f, 0.55f, 0.18f, 1f);

        GameObject handleAreaGO = CrearUIObject("Handle Slide Area", sliderGO.transform);
        RectTransform handleArea = handleAreaGO.GetComponent<RectTransform>();
        StretchCompleto(handleArea);
        handleArea.offsetMin = new Vector2(8f, 0f);
        handleArea.offsetMax = new Vector2(-8f, 0f);

        GameObject handleGO = CrearUIObject("Handle", handleAreaGO.transform);
        RectTransform handle = handleGO.GetComponent<RectTransform>();
        handle.sizeDelta = new Vector2(28f, 28f);

        Image handleImage = handleGO.AddComponent<Image>();
        handleImage.color = Color.white;

        slider.fillRect = fill;
        slider.handleRect = handle;
        slider.targetGraphic = handleImage;

        return slider;
    }

    private static Toggle CrearToggle(string nombre, Transform padre, string contenido, Vector2 posicion, Vector2 tamano)
    {
        GameObject toggleGO = CrearUIObject(nombre, padre);
        RectTransform rectTransform = toggleGO.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = posicion;
        rectTransform.sizeDelta = tamano;

        Toggle toggle = toggleGO.AddComponent<Toggle>();

        GameObject boxGO = CrearUIObject("Caja", toggleGO.transform);
        RectTransform box = boxGO.GetComponent<RectTransform>();
        box.anchorMin = new Vector2(0f, 0.5f);
        box.anchorMax = new Vector2(0f, 0.5f);
        box.pivot = new Vector2(0f, 0.5f);
        box.anchoredPosition = new Vector2(0f, 0f);
        box.sizeDelta = new Vector2(28f, 28f);

        Image boxImage = boxGO.AddComponent<Image>();
        boxImage.color = Color.white;

        GameObject checkGO = CrearUIObject("Marca", boxGO.transform);
        RectTransform check = checkGO.GetComponent<RectTransform>();
        check.anchorMin = new Vector2(0.5f, 0.5f);
        check.anchorMax = new Vector2(0.5f, 0.5f);
        check.pivot = new Vector2(0.5f, 0.5f);
        check.anchoredPosition = Vector2.zero;
        check.sizeDelta = new Vector2(16f, 16f);

        Image checkImage = checkGO.AddComponent<Image>();
        checkImage.color = new Color(0.93f, 0.55f, 0.18f, 1f);

        CrearTexto("Texto", toggleGO.transform, contenido, 24f, new Vector2(82f, 0f), new Vector2(250f, 36f));

        toggle.targetGraphic = boxImage;
        toggle.graphic = checkImage;
        toggle.isOn = false;

        return toggle;
    }

    private static void StretchCompleto(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}
