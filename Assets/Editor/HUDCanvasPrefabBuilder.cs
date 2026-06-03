using TMPro;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class HUDCanvasPrefabBuilder
{
    private const string HUDCanvasPath = "Assets/Prefabs/UI/HUDCanvas.prefab";
    private const string RebuildFlagPath = "ProjectSettings/PiggyPocket_RebuildHUDCanvas.flag";

    [InitializeOnLoadMethod]
    private static void RebuildHUDCanvasIfRequested()
    {
        if(!File.Exists(RebuildFlagPath))
        {
            return;
        }

        EditorApplication.delayCall += () =>
        {
            if(!File.Exists(RebuildFlagPath))
            {
                return;
            }

            RebuildHUDCanvas();
            File.Delete(RebuildFlagPath);
        };
    }

    [MenuItem("PiggyPocket/UI/Rebuild HUD Canvas")]
    public static void RebuildHUDCanvas()
    {
        GameObject hudCanvas = PrefabUtility.LoadPrefabContents(HUDCanvasPath);

        try
        {
            RebuildVictoryPanel(hudCanvas);
            RebuildGameOverPanel(hudCanvas);
            RebuildPausePanel(hudCanvas);

            PrefabUtility.SaveAsPrefabAsset(hudCanvas, HUDCanvasPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(HUDCanvasPath);
            Debug.Log("HUDCanvas actualizado con VictoryOverlay, PauseOverlay y GameOverOverlay en " + HUDCanvasPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(hudCanvas);
        }
    }

    private static void RebuildVictoryPanel(GameObject hudCanvas)
    {
        Transform existingOverlay = hudCanvas.transform.Find("VictoryOverlay");

        if(existingOverlay != null)
        {
            Object.DestroyImmediate(existingOverlay.gameObject);
        }

        foreach(LevelVictoryScreen existingScreen in hudCanvas.GetComponents<LevelVictoryScreen>())
        {
            Object.DestroyImmediate(existingScreen);
        }

        GameObject overlayGO = CreateUIObject("VictoryOverlay", hudCanvas.transform);
        RectTransform overlay = overlayGO.GetComponent<RectTransform>();
        StretchFull(overlay);
        overlay.SetAsLastSibling();

        Image overlayImage = overlayGO.AddComponent<Image>();
        overlayImage.color = new Color(0.03f, 0.05f, 0.07f, 0.78f);
        overlayImage.raycastTarget = true;

        CanvasGroup canvasGroup = overlayGO.AddComponent<CanvasGroup>();

        GameObject cardGO = CreateUIObject("VictoryCard", overlay);
        RectTransform card = cardGO.GetComponent<RectTransform>();
        card.anchorMin = new Vector2(0.5f, 0.5f);
        card.anchorMax = new Vector2(0.5f, 0.5f);
        card.pivot = new Vector2(0.5f, 0.5f);
        card.anchoredPosition = Vector2.zero;
        card.sizeDelta = new Vector2(560f, 320f);

        Image cardImage = cardGO.AddComponent<Image>();
        cardImage.color = new Color(0.96f, 0.91f, 0.72f, 1f);
        cardImage.raycastTarget = true;

        TMP_Text titleText = CreateText("Titulo", card, "Nivel completado", 44f, new Vector2(0f, 92f), new Vector2(500f, 64f));
        TMP_Text coinsText = CreateText("Monedas", card, "Monedas recolectadas: 0", 30f, new Vector2(0f, 22f), new Vector2(500f, 48f));
        TMP_Text detailText = CreateText("Detalle", card, "Llegaste a la meta final.", 22f, new Vector2(0f, -34f), new Vector2(500f, 44f));
        Button restartButton = CreateButton("BotonReiniciar", card, "Reiniciar", new Vector2(0f, -112f), new Vector2(220f, 54f));

        LevelVictoryScreen victoryScreen = hudCanvas.AddComponent<LevelVictoryScreen>();
        SerializedObject serializedScreen = new SerializedObject(victoryScreen);
        serializedScreen.FindProperty("panelVictoria").objectReferenceValue = overlayGO;
        serializedScreen.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
        serializedScreen.FindProperty("textoTitulo").objectReferenceValue = titleText;
        serializedScreen.FindProperty("textoMonedas").objectReferenceValue = coinsText;
        serializedScreen.FindProperty("textoDetalle").objectReferenceValue = detailText;
        serializedScreen.FindProperty("botonReiniciar").objectReferenceValue = restartButton;
        serializedScreen.FindProperty("titulo").stringValue = "Nivel completado";
        serializedScreen.FindProperty("formatoMonedas").stringValue = "Monedas recolectadas: {0}";
        serializedScreen.FindProperty("detalle").stringValue = "Llegaste a la meta final.";
        serializedScreen.FindProperty("pausarJuegoAlGanar").boolValue = true;
        serializedScreen.ApplyModifiedPropertiesWithoutUndo();

        overlayGO.SetActive(false);
    }

    private static void RebuildPausePanel(GameObject hudCanvas)
    {
        Transform existingOverlay = hudCanvas.transform.Find("PauseOverlay");

        if(existingOverlay != null)
        {
            Object.DestroyImmediate(existingOverlay.gameObject);
        }

        foreach(PauseMenuScreen existingScreen in hudCanvas.GetComponents<PauseMenuScreen>())
        {
            Object.DestroyImmediate(existingScreen);
        }

        GameObject overlayGO = CreateUIObject("PauseOverlay", hudCanvas.transform);
        RectTransform overlay = overlayGO.GetComponent<RectTransform>();
        StretchFull(overlay);
        overlay.SetAsLastSibling();

        Image overlayImage = overlayGO.AddComponent<Image>();
        overlayImage.color = new Color(0.03f, 0.05f, 0.07f, 0.72f);
        overlayImage.raycastTarget = true;

        CanvasGroup canvasGroup = overlayGO.AddComponent<CanvasGroup>();

        GameObject cardGO = CreateCard("PauseCard", overlay, new Vector2(520f, 360f));
        RectTransform card = cardGO.GetComponent<RectTransform>();

        CreateText("Titulo", card, "Pausa", 46f, new Vector2(0f, 116f), new Vector2(460f, 64f));
        Button continueButton = CreateButton("BotonContinuar", card, "Continuar", new Vector2(0f, 42f), new Vector2(260f, 54f));
        Button settingsButton = CreateButton("BotonAjustes", card, "Ajustes", new Vector2(0f, -26f), new Vector2(260f, 54f));
        Button restartButton = CreateButton("BotonReiniciar", card, "Reiniciar", new Vector2(0f, -94f), new Vector2(260f, 54f));

        GameObject settingsGO = CreateCard("SettingsCard", overlay, new Vector2(520f, 380f));
        RectTransform settingsCard = settingsGO.GetComponent<RectTransform>();
        CreateText("Titulo", settingsCard, "Ajustes", 42f, new Vector2(0f, 128f), new Vector2(460f, 58f));
        CreateText("TextoVolumen", settingsCard, "Volumen general", 24f, new Vector2(-92f, 62f), new Vector2(260f, 38f));
        TMP_Text volumeValueText = CreateText("TextoValorVolumen", settingsCard, "100%", 24f, new Vector2(180f, 62f), new Vector2(90f, 38f));
        Slider volumeSlider = CreateSlider("SliderVolumen", settingsCard, new Vector2(0f, 16f), new Vector2(360f, 36f));
        Toggle muteToggle = CreateToggle("ToggleSilencio", settingsCard, "Silenciar audio", new Vector2(0f, -56f), new Vector2(300f, 42f));
        Button backButton = CreateButton("BotonVolver", settingsCard, "Volver", new Vector2(0f, -128f), new Vector2(220f, 54f));
        settingsGO.SetActive(false);

        PauseMenuScreen pauseScreen = hudCanvas.AddComponent<PauseMenuScreen>();
        SerializedObject serializedScreen = new SerializedObject(pauseScreen);
        serializedScreen.FindProperty("panelPausa").objectReferenceValue = overlayGO;
        serializedScreen.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
        serializedScreen.FindProperty("menuPrincipal").objectReferenceValue = cardGO;
        serializedScreen.FindProperty("panelAjustes").objectReferenceValue = settingsGO;
        serializedScreen.FindProperty("botonContinuar").objectReferenceValue = continueButton;
        serializedScreen.FindProperty("botonAjustes").objectReferenceValue = settingsButton;
        serializedScreen.FindProperty("botonReiniciar").objectReferenceValue = restartButton;
        serializedScreen.FindProperty("botonVolverAjustes").objectReferenceValue = backButton;
        serializedScreen.FindProperty("sliderVolumen").objectReferenceValue = volumeSlider;
        serializedScreen.FindProperty("textoValorVolumen").objectReferenceValue = volumeValueText;
        serializedScreen.FindProperty("toggleSilencio").objectReferenceValue = muteToggle;

        SerializedProperty overlays = serializedScreen.FindProperty("overlaysBloqueantes");
        overlays.arraySize = 2;
        overlays.GetArrayElementAtIndex(0).objectReferenceValue = hudCanvas.transform.Find("VictoryOverlay")?.gameObject;
        overlays.GetArrayElementAtIndex(1).objectReferenceValue = hudCanvas.transform.Find("GameOverOverlay")?.gameObject;

        serializedScreen.FindProperty("pausarConTeclado").boolValue = true;
        serializedScreen.FindProperty("teclaPausa").intValue = (int)KeyCode.Escape;
        serializedScreen.FindProperty("teclaPausaAlternativa").intValue = (int)KeyCode.P;
        serializedScreen.FindProperty("pausarTiempo").boolValue = true;
        serializedScreen.ApplyModifiedPropertiesWithoutUndo();

        overlayGO.SetActive(false);
    }

    private static void RebuildGameOverPanel(GameObject hudCanvas)
    {
        Transform existingOverlay = hudCanvas.transform.Find("GameOverOverlay");

        if(existingOverlay != null)
        {
            Object.DestroyImmediate(existingOverlay.gameObject);
        }

        foreach(GameOverScreen existingScreen in hudCanvas.GetComponents<GameOverScreen>())
        {
            Object.DestroyImmediate(existingScreen);
        }

        GameObject overlayGO = CreateUIObject("GameOverOverlay", hudCanvas.transform);
        RectTransform overlay = overlayGO.GetComponent<RectTransform>();
        StretchFull(overlay);
        overlay.SetAsLastSibling();

        Image overlayImage = overlayGO.AddComponent<Image>();
        overlayImage.color = new Color(0.03f, 0.05f, 0.07f, 0.78f);
        overlayImage.raycastTarget = true;

        CanvasGroup canvasGroup = overlayGO.AddComponent<CanvasGroup>();

        GameObject cardGO = CreateCard("GameOverCard", overlay, new Vector2(520f, 300f));
        RectTransform card = cardGO.GetComponent<RectTransform>();

        TMP_Text titleText = CreateText("Titulo", card, "Derrota", 46f, new Vector2(0f, 84f), new Vector2(460f, 64f));
        TMP_Text detailText = CreateText("Detalle", card, "Intentalo otra vez.", 26f, new Vector2(0f, 18f), new Vector2(430f, 54f));
        Button retryButton = CreateButton("BotonReintentar", card, "Reintentar", new Vector2(0f, -88f), new Vector2(240f, 54f));

        GameOverScreen gameOverScreen = hudCanvas.AddComponent<GameOverScreen>();
        SerializedObject serializedScreen = new SerializedObject(gameOverScreen);
        serializedScreen.FindProperty("panelDerrota").objectReferenceValue = overlayGO;
        serializedScreen.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
        serializedScreen.FindProperty("textoTitulo").objectReferenceValue = titleText;
        serializedScreen.FindProperty("textoDetalle").objectReferenceValue = detailText;
        serializedScreen.FindProperty("botonReintentar").objectReferenceValue = retryButton;
        serializedScreen.FindProperty("titulo").stringValue = "Derrota";
        serializedScreen.FindProperty("detalle").stringValue = "Intentalo otra vez.";
        serializedScreen.FindProperty("delayMostrar").floatValue = 1.2f;
        serializedScreen.FindProperty("pausarJuegoAlMostrar").boolValue = true;
        serializedScreen.ApplyModifiedPropertiesWithoutUndo();

        overlayGO.SetActive(false);
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        go.transform.SetParent(parent, false);
        return go;
    }

    private static TMP_Text CreateText(string name, Transform parent, string text, float fontSize, Vector2 position, Vector2 size)
    {
        GameObject textGO = CreateUIObject(name, parent);
        RectTransform rectTransform = textGO.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;

        TMP_Text textComponent = textGO.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.color = new Color(0.12f, 0.11f, 0.09f, 1f);
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.textWrappingMode = TextWrappingModes.NoWrap;
        textComponent.raycastTarget = false;

        return textComponent;
    }

    private static GameObject CreateCard(string name, Transform parent, Vector2 size)
    {
        GameObject cardGO = CreateUIObject(name, parent);
        RectTransform card = cardGO.GetComponent<RectTransform>();
        card.anchorMin = new Vector2(0.5f, 0.5f);
        card.anchorMax = new Vector2(0.5f, 0.5f);
        card.pivot = new Vector2(0.5f, 0.5f);
        card.anchoredPosition = Vector2.zero;
        card.sizeDelta = size;

        Image cardImage = cardGO.AddComponent<Image>();
        cardImage.color = new Color(0.96f, 0.91f, 0.72f, 1f);
        cardImage.raycastTarget = true;

        return cardGO;
    }

    private static Button CreateButton(string name, Transform parent, string text, Vector2 position, Vector2 size)
    {
        GameObject buttonGO = CreateUIObject(name, parent);
        RectTransform rectTransform = buttonGO.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;

        Image image = buttonGO.AddComponent<Image>();
        image.color = new Color(0.93f, 0.55f, 0.18f, 1f);

        Button button = buttonGO.AddComponent<Button>();
        button.targetGraphic = image;

        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(1f, 0.66f, 0.26f, 1f);
        colors.pressedColor = new Color(0.78f, 0.38f, 0.11f, 1f);
        button.colors = colors;

        GameObject labelGO = CreateUIObject("Texto", buttonGO.transform);
        RectTransform labelRect = labelGO.GetComponent<RectTransform>();
        StretchFull(labelRect);

        TMP_Text label = labelGO.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = 24f;
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.raycastTarget = false;

        return button;
    }

    private static Slider CreateSlider(string name, Transform parent, Vector2 position, Vector2 size)
    {
        GameObject sliderGO = CreateUIObject(name, parent);
        RectTransform rectTransform = sliderGO.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;

        Slider slider = sliderGO.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        slider.direction = Slider.Direction.LeftToRight;

        GameObject backgroundGO = CreateUIObject("Background", sliderGO.transform);
        RectTransform background = backgroundGO.GetComponent<RectTransform>();
        background.anchorMin = new Vector2(0f, 0.5f);
        background.anchorMax = new Vector2(1f, 0.5f);
        background.anchoredPosition = Vector2.zero;
        background.sizeDelta = new Vector2(0f, 10f);

        Image backgroundImage = backgroundGO.AddComponent<Image>();
        backgroundImage.color = new Color(0.28f, 0.23f, 0.18f, 0.35f);

        GameObject fillAreaGO = CreateUIObject("Fill Area", sliderGO.transform);
        RectTransform fillArea = fillAreaGO.GetComponent<RectTransform>();
        fillArea.anchorMin = new Vector2(0f, 0.5f);
        fillArea.anchorMax = new Vector2(1f, 0.5f);
        fillArea.anchoredPosition = Vector2.zero;
        fillArea.sizeDelta = new Vector2(-18f, 10f);

        GameObject fillGO = CreateUIObject("Fill", fillAreaGO.transform);
        RectTransform fill = fillGO.GetComponent<RectTransform>();
        StretchFull(fill);

        Image fillImage = fillGO.AddComponent<Image>();
        fillImage.color = new Color(0.93f, 0.55f, 0.18f, 1f);

        GameObject handleAreaGO = CreateUIObject("Handle Slide Area", sliderGO.transform);
        RectTransform handleArea = handleAreaGO.GetComponent<RectTransform>();
        StretchFull(handleArea);
        handleArea.offsetMin = new Vector2(8f, 0f);
        handleArea.offsetMax = new Vector2(-8f, 0f);

        GameObject handleGO = CreateUIObject("Handle", handleAreaGO.transform);
        RectTransform handle = handleGO.GetComponent<RectTransform>();
        handle.sizeDelta = new Vector2(28f, 28f);

        Image handleImage = handleGO.AddComponent<Image>();
        handleImage.color = Color.white;

        slider.fillRect = fill;
        slider.handleRect = handle;
        slider.targetGraphic = handleImage;

        return slider;
    }

    private static Toggle CreateToggle(string name, Transform parent, string text, Vector2 position, Vector2 size)
    {
        GameObject toggleGO = CreateUIObject(name, parent);
        RectTransform rectTransform = toggleGO.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;

        Toggle toggle = toggleGO.AddComponent<Toggle>();

        GameObject boxGO = CreateUIObject("Caja", toggleGO.transform);
        RectTransform box = boxGO.GetComponent<RectTransform>();
        box.anchorMin = new Vector2(0f, 0.5f);
        box.anchorMax = new Vector2(0f, 0.5f);
        box.pivot = new Vector2(0f, 0.5f);
        box.anchoredPosition = Vector2.zero;
        box.sizeDelta = new Vector2(28f, 28f);

        Image boxImage = boxGO.AddComponent<Image>();
        boxImage.color = Color.white;

        GameObject checkGO = CreateUIObject("Marca", boxGO.transform);
        RectTransform check = checkGO.GetComponent<RectTransform>();
        check.anchorMin = new Vector2(0.5f, 0.5f);
        check.anchorMax = new Vector2(0.5f, 0.5f);
        check.pivot = new Vector2(0.5f, 0.5f);
        check.anchoredPosition = Vector2.zero;
        check.sizeDelta = new Vector2(16f, 16f);

        Image checkImage = checkGO.AddComponent<Image>();
        checkImage.color = new Color(0.93f, 0.55f, 0.18f, 1f);

        CreateText("Texto", toggleGO.transform, text, 24f, new Vector2(82f, 0f), new Vector2(250f, 36f));

        toggle.targetGraphic = boxImage;
        toggle.graphic = checkImage;
        toggle.isOn = false;

        return toggle;
    }

    private static void StretchFull(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}
