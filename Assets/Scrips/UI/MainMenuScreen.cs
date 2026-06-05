using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

// Pantalla inicial del juego. Maneja jugar, ajustes, creditos y salir.
public class MainMenuScreen : MonoBehaviour
{
    private const string MenuRootPath = "MenuRoot";
    private const string MainPanelPath = "MenuRoot/MainPanel";
    private const string SettingsPanelPath = "MenuRoot/SettingsPanel";
    private const string CreditsPanelPath = "MenuRoot/CreditsPanel";
    private const string ControlsPanelPath = "MenuRoot/ControlsPanel";
    private const string VolumenKey = "PiggyPocket_VolumenGeneral";
    private const string SilencioKey = "PiggyPocket_AudioSilenciado";

    [Header("Escenas")]
    [SerializeField] private string nombreEscenaJuego = "Level_01";
    [SerializeField] private string rutaEscenaJuegoEditor = "Assets/Scenes/Level_01.unity";

    [Header("Referencias")]
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private GameObject panelPrincipal;
    [SerializeField] private GameObject panelAjustes;
    [SerializeField] private GameObject panelCreditos;
    [SerializeField] private GameObject panelControles;
    [SerializeField] private Button botonJugar;
    [SerializeField] private Button botonAjustes;
    [SerializeField] private Button botonControles;
    [SerializeField] private Button botonCreditos;
    [SerializeField] private Button botonSalir;
    [SerializeField] private Button botonVolverAjustes;
    [SerializeField] private Button botonVolverCreditos;
    [SerializeField] private Button botonVolverControles;
    [SerializeField] private Slider sliderVolumen;
    [SerializeField] private TMP_Text textoValorVolumen;
    [SerializeField] private Toggle toggleSilencio;

    [Header("Creditos")]
    [TextArea(5, 12)]
    [SerializeField] private string textoCreditos =
        "Inputs: https://opengameart.org/content/mobile-controls\n" +
        "UI: https://cga-creative-game-assets.itch.io/gold-2d-mobile-ui-for-casual-game\n" +
        "Player, enemigos, barriles, vida y resto: https://crusenho.itch.io/beriesadventureseaside";

    private bool cargandoAjustes;

    void Awake()
    {
        Time.timeScale = 1f;
        AsegurarCanvas();
        AsegurarEventSystem();
        AsegurarReferencias();
        ConfigurarBotones();
        ConfigurarAjustes();
        MostrarPrincipal();
    }

    [ContextMenu("Reconstruir UI")]
    public void ReconstruirUI()
    {
        AsegurarCanvas();
        LimpiarUIExistente();
        CrearUI();
        AsegurarReferencias();
        ConfigurarBotones();
        ConfigurarAjustes();
        MostrarPrincipal();
    }

    public void Jugar()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        if(!string.IsNullOrWhiteSpace(rutaEscenaJuegoEditor))
        {
            EditorSceneManager.LoadScene(rutaEscenaJuegoEditor);
            return;
        }
#endif

        SceneManager.LoadScene(nombreEscenaJuego);
    }

    public void MostrarPrincipal()
    {
        GameAudioManager.ReanudarMusicaFondo();
        ActivarPanel(panelPrincipal);
    }

    public void MostrarAjustes()
    {
        ActivarPanel(panelAjustes);
        RefrescarVistaAjustes();
    }

    public void MostrarCreditos()
    {
        ActivarPanel(panelCreditos);
    }

    public void MostrarControles()
    {
        ActivarPanel(panelControles);
    }

    public void Salir()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void AsegurarReferencias()
    {
        if(menuRoot == null)
        {
            Transform root = transform.Find(MenuRootPath);
            menuRoot = root != null ? root.gameObject : null;
        }

        if(menuRoot == null)
        {
            CrearUI();
        }

        if(panelPrincipal == null)
        {
            panelPrincipal = BuscarGameObject(MainPanelPath);
        }

        if(panelAjustes == null)
        {
            panelAjustes = BuscarGameObject(SettingsPanelPath);
        }

        if(panelCreditos == null)
        {
            panelCreditos = BuscarGameObject(CreditsPanelPath);
        }

        if(panelControles == null)
        {
            panelControles = BuscarGameObject(ControlsPanelPath);
        }

        if(botonJugar == null)
        {
            botonJugar = BuscarComponente<Button>(MainPanelPath + "/BotonJugar");
        }

        if(botonAjustes == null)
        {
            botonAjustes = BuscarComponente<Button>(MainPanelPath + "/BotonAjustes");
        }

        if(botonControles == null)
        {
            botonControles = BuscarComponente<Button>(MainPanelPath + "/BotonControles");
        }

        if(botonCreditos == null)
        {
            botonCreditos = BuscarComponente<Button>(MainPanelPath + "/BotonCreditos");
        }

        if(botonSalir == null)
        {
            botonSalir = BuscarComponente<Button>(MainPanelPath + "/BotonSalir");
        }

        if(botonVolverAjustes == null)
        {
            botonVolverAjustes = BuscarComponente<Button>(SettingsPanelPath + "/BotonVolver");
        }

        if(botonVolverCreditos == null)
        {
            botonVolverCreditos = BuscarComponente<Button>(CreditsPanelPath + "/BotonVolver");
        }

        if(botonVolverControles == null)
        {
            botonVolverControles = BuscarComponente<Button>(ControlsPanelPath + "/BotonVolver");
        }

        if(sliderVolumen == null)
        {
            sliderVolumen = BuscarComponente<Slider>(SettingsPanelPath + "/SliderVolumen");
        }

        if(textoValorVolumen == null)
        {
            textoValorVolumen = BuscarComponente<TMP_Text>(SettingsPanelPath + "/TextoValorVolumen");
        }

        if(toggleSilencio == null)
        {
            toggleSilencio = BuscarComponente<Toggle>(SettingsPanelPath + "/ToggleSilencio");
        }

        AsegurarPanelControles();
        AsegurarBotonControles();
    }

    private void ConfigurarBotones()
    {
        ConfigurarBoton(botonJugar, Jugar);
        ConfigurarBoton(botonAjustes, MostrarAjustes);
        ConfigurarBoton(botonControles, MostrarControles);
        ConfigurarBoton(botonCreditos, MostrarCreditos);
        ConfigurarBoton(botonSalir, Salir);
        ConfigurarBoton(botonVolverAjustes, MostrarPrincipal);
        ConfigurarBoton(botonVolverCreditos, MostrarPrincipal);
        ConfigurarBoton(botonVolverControles, MostrarPrincipal);
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

        boton.onClick.RemoveListener(GameAudioManager.ReproducirClickUI);
        boton.onClick.AddListener(GameAudioManager.ReproducirClickUI);
        boton.onClick.RemoveListener(accion);
        boton.onClick.AddListener(accion);
    }

    private void ActivarPanel(GameObject panelActivo)
    {
        if(panelPrincipal != null)
        {
            panelPrincipal.SetActive(panelActivo == panelPrincipal);
        }

        if(panelAjustes != null)
        {
            panelAjustes.SetActive(panelActivo == panelAjustes);
        }

        if(panelCreditos != null)
        {
            panelCreditos.SetActive(panelActivo == panelCreditos);
        }

        if(panelControles != null)
        {
            panelControles.SetActive(panelActivo == panelControles);
        }
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

    private void CrearUI()
    {
        menuRoot = CrearUIObject("MenuRoot", transform);
        RectTransform root = menuRoot.GetComponent<RectTransform>();
        StretchCompleto(root);

        Image background = menuRoot.AddComponent<Image>();
        background.color = new Color(0.28f, 0.76f, 0.9f, 1f);

        panelPrincipal = CrearCard("MainPanel", menuRoot.transform, new Vector2(620f, 590f));
        RectTransform mainCard = panelPrincipal.GetComponent<RectTransform>();

        CrearTexto("Titulo", mainCard, "PiggyPocket", 58f, new Vector2(0f, 210f), new Vector2(540f, 72f));
        CrearTexto("Subtitulo", mainCard, "Recolecta monedas y llega a la meta", 24f, new Vector2(0f, 154f), new Vector2(520f, 40f));
        botonJugar = CrearBoton("BotonJugar", mainCard, "Jugar", new Vector2(0f, 84f), new Vector2(270f, 56f));
        botonAjustes = CrearBoton("BotonAjustes", mainCard, "Ajustes", new Vector2(0f, 14f), new Vector2(270f, 56f));
        botonControles = CrearBoton("BotonControles", mainCard, "Controles", new Vector2(0f, -56f), new Vector2(270f, 56f));
        botonCreditos = CrearBoton("BotonCreditos", mainCard, "Creditos", new Vector2(0f, -126f), new Vector2(270f, 56f));
        botonSalir = CrearBoton("BotonSalir", mainCard, "Salir", new Vector2(0f, -196f), new Vector2(270f, 56f));

        panelAjustes = CrearCard("SettingsPanel", menuRoot.transform, new Vector2(560f, 380f));
        RectTransform settingsCard = panelAjustes.GetComponent<RectTransform>();

        CrearTexto("Titulo", settingsCard, "Ajustes", 44f, new Vector2(0f, 128f), new Vector2(500f, 60f));
        CrearTexto("TextoVolumen", settingsCard, "Volumen general", 24f, new Vector2(-92f, 62f), new Vector2(270f, 38f));
        textoValorVolumen = CrearTexto("TextoValorVolumen", settingsCard, "100%", 24f, new Vector2(185f, 62f), new Vector2(90f, 38f));
        sliderVolumen = CrearSlider("SliderVolumen", settingsCard, new Vector2(0f, 16f), new Vector2(380f, 36f));
        toggleSilencio = CrearToggle("ToggleSilencio", settingsCard, "Silenciar audio", new Vector2(0f, -56f), new Vector2(310f, 42f));
        botonVolverAjustes = CrearBoton("BotonVolver", settingsCard, "Volver", new Vector2(0f, -128f), new Vector2(220f, 54f));

        panelCreditos = CrearCard("CreditsPanel", menuRoot.transform, new Vector2(760f, 460f));
        RectTransform creditsCard = panelCreditos.GetComponent<RectTransform>();

        CrearTexto("Titulo", creditsCard, "Creditos", 44f, new Vector2(0f, 166f), new Vector2(620f, 60f));
        TMP_Text creditsText = CrearTexto("TextoCreditos", creditsCard, textoCreditos, 22f, new Vector2(0f, 22f), new Vector2(650f, 220f));
        creditsText.textWrappingMode = TextWrappingModes.Normal;
        botonVolverCreditos = CrearBoton("BotonVolver", creditsCard, "Volver", new Vector2(0f, -166f), new Vector2(220f, 54f));

        panelControles = ControlsHelpPanelFactory.CrearPanelControles("ControlsPanel", menuRoot.transform, new Vector2(760f, 440f), out botonVolverControles);
    }

    private void LimpiarUIExistente()
    {
        Transform existente = transform.Find(MenuRootPath);

        if(existente == null)
        {
            return;
        }

        if(Application.isPlaying)
        {
            Destroy(existente.gameObject);
        }
        else
        {
            DestroyImmediate(existente.gameObject);
        }

        menuRoot = null;
        panelPrincipal = null;
        panelAjustes = null;
        panelCreditos = null;
        panelControles = null;
        botonJugar = null;
        botonAjustes = null;
        botonControles = null;
        botonCreditos = null;
        botonSalir = null;
        botonVolverAjustes = null;
        botonVolverCreditos = null;
        botonVolverControles = null;
        sliderVolumen = null;
        textoValorVolumen = null;
        toggleSilencio = null;
    }

    private void AsegurarCanvas()
    {
        if(GetComponentInParent<Canvas>() != null)
        {
            return;
        }

        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();
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

    private GameObject BuscarGameObject(string ruta)
    {
        Transform encontrado = transform.Find(ruta);
        return encontrado != null ? encontrado.gameObject : null;
    }

    private T BuscarComponente<T>(string ruta) where T : Component
    {
        Transform encontrado = transform.Find(ruta);
        return encontrado != null ? encontrado.GetComponent<T>() : null;
    }

    private void AsegurarPanelControles()
    {
        if(panelControles != null || menuRoot == null)
        {
            return;
        }

        panelControles = ControlsHelpPanelFactory.CrearPanelControles("ControlsPanel", menuRoot.transform, new Vector2(760f, 440f), out botonVolverControles);
    }

    private void AsegurarBotonControles()
    {
        if(panelPrincipal == null)
        {
            return;
        }

        if(botonControles == null)
        {
            RectTransform mainCard = panelPrincipal.GetComponent<RectTransform>();

            if(mainCard != null)
            {
                mainCard.sizeDelta = new Vector2(620f, 590f);
            }

            ReubicarBoton(botonJugar, new Vector2(0f, 84f));
            ReubicarBoton(botonAjustes, new Vector2(0f, 14f));
            ReubicarBoton(botonCreditos, new Vector2(0f, -126f));
            ReubicarBoton(botonSalir, new Vector2(0f, -196f));
            botonControles = CrearBoton("BotonControles", panelPrincipal.transform, "Controles", new Vector2(0f, -56f), new Vector2(270f, 56f));
        }
        else
        {
            ActualizarTextoBoton(botonControles, "Controles");
        }
    }

    private static void ReubicarBoton(Button boton, Vector2 posicion)
    {
        RectTransform rectTransform = boton != null ? boton.GetComponent<RectTransform>() : null;

        if(rectTransform != null)
        {
            rectTransform.anchoredPosition = posicion;
        }
    }

    private static void ActualizarTextoBoton(Button boton, string contenido)
    {
        TMP_Text texto = boton != null ? boton.GetComponentInChildren<TMP_Text>() : null;

        if(texto != null)
        {
            texto.text = contenido;
        }
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
        cardImage.color = new Color(0.96f, 0.91f, 0.72f, 0.96f);
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
        box.anchoredPosition = Vector2.zero;
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

// Crea un panel simple de controles reutilizable por MainMenu y HUDCanvas.
// El diseno puede modificarse despues desde Unity; esto asegura que la navegacion funcione.
public static class ControlsHelpPanelFactory
{
    private const string TextoPCWeb =
        "A / D o Flechas: moverse\n" +
        "Space: saltar\n" +
        "J o Click: atacar\n" +
        "Esc o P: pausa";

    private const string TextoMobile =
        "Flechas tactiles: moverse\n" +
        "Boton salto: saltar\n" +
        "Boton ataque: atacar\n" +
        "Boton pausa: pausa";

    public static GameObject CrearPanelControles(string nombre, Transform padre, Vector2 tamano, out Button botonVolver)
    {
        GameObject panelGO = CrearCard(nombre, padre, tamano);
        RectTransform panel = panelGO.GetComponent<RectTransform>();

        CrearTexto("Titulo", panel, "Controles", 44f, new Vector2(0f, 150f), new Vector2(tamano.x - 80f, 58f), TextAlignmentOptions.Center);

        CrearSeccion(
            "PCWeb",
            panel,
            "PC / Web",
            TextoPCWeb,
            new Vector2(-tamano.x * 0.24f, 24f),
            new Vector2(tamano.x * 0.38f, 210f));

        CrearSeccion(
            "Mobile",
            panel,
            "Celular",
            TextoMobile,
            new Vector2(tamano.x * 0.24f, 24f),
            new Vector2(tamano.x * 0.38f, 210f));

        botonVolver = CrearBoton("BotonVolver", panel, "Volver", new Vector2(0f, -154f), new Vector2(220f, 54f));
        panelGO.SetActive(false);

        return panelGO;
    }

    private static void CrearSeccion(string nombre, Transform padre, string titulo, string contenido, Vector2 posicion, Vector2 tamano)
    {
        GameObject sectionGO = CrearUIObject(nombre, padre);
        RectTransform section = sectionGO.GetComponent<RectTransform>();
        section.anchorMin = new Vector2(0.5f, 0.5f);
        section.anchorMax = new Vector2(0.5f, 0.5f);
        section.pivot = new Vector2(0.5f, 0.5f);
        section.anchoredPosition = posicion;
        section.sizeDelta = tamano;

        Image sectionImage = sectionGO.AddComponent<Image>();
        sectionImage.color = new Color(1f, 1f, 1f, 0.24f);
        sectionImage.raycastTarget = false;

        CrearTexto("Titulo", section, titulo, 28f, new Vector2(0f, 72f), new Vector2(tamano.x - 28f, 38f), TextAlignmentOptions.Center);
        TMP_Text body = CrearTexto("Texto", section, contenido, 23f, new Vector2(0f, -20f), new Vector2(tamano.x - 34f, 126f), TextAlignmentOptions.Left);
        body.textWrappingMode = TextWrappingModes.Normal;
        body.lineSpacing = 12f;
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

    private static TMP_Text CrearTexto(string nombre, Transform padre, string contenido, float fontSize, Vector2 posicion, Vector2 tamano, TextAlignmentOptions alineacion)
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
        texto.alignment = alineacion;
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
