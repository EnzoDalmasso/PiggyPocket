using TMPro;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class HUDCanvasPrefabBuilder
{
    private const string HUDCanvasPath = "Assets/Prefabs/UI/HUDCanvas.prefab";
    private const string RebuildFlagPath = "ProjectSettings/PiggyPocket_RebuildHUDCanvas.flag";
    private const string SpriteLeftPath = "Assets/Sprites/Mobile/Sprites/Style C/Default/direction_left.png";
    private const string SpriteRightPath = "Assets/Sprites/Mobile/Sprites/Style C/Default/direction_right.png";
    private const string SpriteJumpButtonPath = "Assets/Sprites/Mobile/Sprites/Style C/Default/button_circle.png";
    private const string SpriteAttackButtonPath = "Assets/Sprites/Mobile/Sprites/Style C/Default/button_diamond.png";
    private const string SpritePauseButtonPath = "Assets/Sprites/Mobile/Sprites/Style C/Default/button_circle.png";
    private const string SpriteJumpIconPath = "Assets/Sprites/Mobile/Sprites/Icons/Default/icon_jump.png";
    private const string SpriteAttackIconPath = "Assets/Sprites/Mobile/Sprites/Icons/Default/icon_sword.png";
    private const string SpritePauseIconPath = "Assets/Sprites/Mobile/Sprites/Icons/Default/icon_pause.png";
    private const string MusicPath = "Assets/Sound/Music/POL-king-of-coins-short.wav";
    private const string SoundJumpPath = "Assets/Sound/Effects/Salto.ogg";
    private const string SoundAttack1Path = "Assets/Sound/Effects/Ataque1.ogg";
    private const string SoundAttack2Path = "Assets/Sound/Effects/Ataque2.ogg";
    private const string SoundPlayerDamagePath = "Assets/Sound/Effects/DanoPlayer.aiff";
    private const string SoundPlayerDeathPath = "Assets/Sound/Effects/MuertePlayer.mp3";
    private const string SoundCoinFallbackPath = "Assets/Sound/Effects/Coin1.ogg";
    private const string SoundLifePath = "Assets/Sound/Effects/LifeCollectible.ogg";
    private const string SoundBreakablePath = "Assets/Sound/Effects/BarrelSound.ogg";
    private const string SoundExplosionPath = "Assets/Sound/Effects/SonidoExplosion.wav";
    private const string SoundPoisonPath = "Assets/Sound/Effects/SonidoVeneno.ogg";
    private const string SoundVictoryPath = "Assets/Sound/Effects/SonidoVictoria.mp3";
    private const string SoundDefeatPath = "Assets/Sound/Effects/SonidoDerrota.wav";
    private const string SoundUIClickPath = "Assets/Sound/Effects/SonidoclickUI.mp3";

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
            RebuildMobileControls(hudCanvas);
            ConfigureGameAudioManager(hudCanvas);

            PrefabUtility.SaveAsPrefabAsset(hudCanvas, HUDCanvasPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(HUDCanvasPath);
            Debug.Log("HUDCanvas actualizado con menus y controles mobile en " + HUDCanvasPath);
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
        card.sizeDelta = new Vector2(600f, 360f);

        Image cardImage = cardGO.AddComponent<Image>();
        cardImage.color = new Color(0.96f, 0.91f, 0.72f, 1f);
        cardImage.raycastTarget = true;

        TMP_Text titleText = CreateText("Titulo", card, "Nivel completado", 44f, new Vector2(0f, 112f), new Vector2(540f, 64f));
        TMP_Text coinsText = CreateText("Monedas", card, "Monedas recolectadas: 0", 30f, new Vector2(0f, 34f), new Vector2(540f, 48f));
        TMP_Text detailText = CreateText("Detalle", card, "Llegaste a la meta final.", 22f, new Vector2(0f, -24f), new Vector2(540f, 44f));
        Button restartButton = CreateButton("BotonReiniciar", card, "Reintentar", new Vector2(-132f, -116f), new Vector2(220f, 54f));
        Button mainMenuButton = CreateButton("BotonMenuPrincipal", card, "Menu principal", new Vector2(132f, -116f), new Vector2(220f, 54f));

        LevelVictoryScreen victoryScreen = hudCanvas.AddComponent<LevelVictoryScreen>();
        SerializedObject serializedScreen = new SerializedObject(victoryScreen);
        serializedScreen.FindProperty("panelVictoria").objectReferenceValue = overlayGO;
        serializedScreen.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
        serializedScreen.FindProperty("textoTitulo").objectReferenceValue = titleText;
        serializedScreen.FindProperty("textoMonedas").objectReferenceValue = coinsText;
        serializedScreen.FindProperty("textoDetalle").objectReferenceValue = detailText;
        serializedScreen.FindProperty("botonReiniciar").objectReferenceValue = restartButton;
        serializedScreen.FindProperty("botonMenuPrincipal").objectReferenceValue = mainMenuButton;
        serializedScreen.FindProperty("titulo").stringValue = "Nivel completado";
        serializedScreen.FindProperty("formatoMonedas").stringValue = "Monedas recolectadas: {0}";
        serializedScreen.FindProperty("detalle").stringValue = "Llegaste a la meta final.";
        serializedScreen.FindProperty("pausarJuegoAlGanar").boolValue = true;
        serializedScreen.FindProperty("nombreEscenaMenuPrincipal").stringValue = "MainMenu";
        serializedScreen.FindProperty("rutaEscenaMenuPrincipalEditor").stringValue = "Assets/Scenes/MainMenu.unity";
        serializedScreen.ApplyModifiedPropertiesWithoutUndo();

        overlayGO.SetActive(false);
    }

    private static void ConfigureGameAudioManager(GameObject hudCanvas)
    {
        GameAudioManager audioManager = hudCanvas.GetComponent<GameAudioManager>();

        if(audioManager == null)
        {
            audioManager = hudCanvas.AddComponent<GameAudioManager>();
        }

        SerializedObject serializedAudio = new SerializedObject(audioManager);
        serializedAudio.FindProperty("volumenSfx").floatValue = 1f;
        serializedAudio.FindProperty("volumenMusica").floatValue = 0.45f;
        serializedAudio.FindProperty("musicaFondo").objectReferenceValue = LoadAudioClip(MusicPath);
        serializedAudio.FindProperty("reproducirMusicaAlIniciar").boolValue = true;
        serializedAudio.FindProperty("sonidoSalto").objectReferenceValue = LoadAudioClip(SoundJumpPath);
        serializedAudio.FindProperty("sonidoAtaque1").objectReferenceValue = LoadAudioClip(SoundAttack1Path);
        serializedAudio.FindProperty("sonidoAtaque2").objectReferenceValue = LoadAudioClip(SoundAttack2Path);
        serializedAudio.FindProperty("sonidoDanoJugador").objectReferenceValue = LoadAudioClip(SoundPlayerDamagePath);
        serializedAudio.FindProperty("sonidoMuerteJugador").objectReferenceValue = LoadAudioClip(SoundPlayerDeathPath);
        serializedAudio.FindProperty("sonidoMoneda").objectReferenceValue = LoadAudioClip(SoundCoinFallbackPath);
        serializedAudio.FindProperty("sonidoVida").objectReferenceValue = LoadAudioClip(SoundLifePath);
        serializedAudio.FindProperty("sonidoRompible").objectReferenceValue = LoadAudioClip(SoundBreakablePath);
        serializedAudio.FindProperty("sonidoExplosion").objectReferenceValue = LoadAudioClip(SoundExplosionPath);
        serializedAudio.FindProperty("sonidoVeneno").objectReferenceValue = LoadAudioClip(SoundPoisonPath);
        serializedAudio.FindProperty("sonidoVictoria").objectReferenceValue = LoadAudioClip(SoundVictoryPath);
        serializedAudio.FindProperty("sonidoDerrota").objectReferenceValue = LoadAudioClip(SoundDefeatPath);
        serializedAudio.FindProperty("sonidoClickUI").objectReferenceValue = LoadAudioClip(SoundUIClickPath);
        serializedAudio.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void RebuildMobileControls(GameObject hudCanvas)
    {
        Transform existingControls = hudCanvas.transform.Find("MobileControls");

        if(existingControls != null)
        {
            Object.DestroyImmediate(existingControls.gameObject);
        }

        foreach(MobileControlsUI existingControlsUI in hudCanvas.GetComponents<MobileControlsUI>())
        {
            Object.DestroyImmediate(existingControlsUI);
        }

        Sprite leftSprite = LoadSprite(SpriteLeftPath);
        Sprite rightSprite = LoadSprite(SpriteRightPath);
        Sprite jumpButtonSprite = LoadSprite(SpriteJumpButtonPath);
        Sprite attackButtonSprite = LoadSprite(SpriteAttackButtonPath);
        Sprite pauseButtonSprite = LoadSprite(SpritePauseButtonPath);
        Sprite jumpIconSprite = LoadSprite(SpriteJumpIconPath);
        Sprite attackIconSprite = LoadSprite(SpriteAttackIconPath);
        Sprite pauseIconSprite = null;

        GameObject controlsGO = CreateUIObject("MobileControls", hudCanvas.transform);
        RectTransform controls = controlsGO.GetComponent<RectTransform>();
        StretchFull(controls);
        controls.SetSiblingIndex(Mathf.Min(1, hudCanvas.transform.childCount - 1));

        GameObject movementGO = CreateUIObject("Movimiento", controlsGO.transform);
        RectTransform movement = movementGO.GetComponent<RectTransform>();
        movement.anchorMin = Vector2.zero;
        movement.anchorMax = Vector2.zero;
        movement.pivot = Vector2.zero;
        movement.anchoredPosition = new Vector2(42f, 38f);
        movement.sizeDelta = new Vector2(236f, 116f);

        CreateMobileButton("BotonIzquierda", movementGO.transform, MobileInputAction.Izquierda, leftSprite, null, new Vector2(52f, 52f), new Vector2(104f, 104f), Vector2.zero);
        CreateMobileButton("BotonDerecha", movementGO.transform, MobileInputAction.Derecha, rightSprite, null, new Vector2(168f, 52f), new Vector2(104f, 104f), Vector2.zero);

        GameObject actionsGO = CreateUIObject("Acciones", controlsGO.transform);
        RectTransform actions = actionsGO.GetComponent<RectTransform>();
        actions.anchorMin = new Vector2(1f, 0f);
        actions.anchorMax = new Vector2(1f, 0f);
        actions.pivot = new Vector2(1f, 0f);
        actions.anchoredPosition = new Vector2(-42f, 38f);
        actions.sizeDelta = new Vector2(278f, 178f);

        CreateMobileButton("BotonAtaque", actionsGO.transform, MobileInputAction.Ataque, attackButtonSprite, attackIconSprite, new Vector2(-56f, 56f), new Vector2(112f, 112f), new Vector2(54f, 54f));
        CreateMobileButton("BotonSalto", actionsGO.transform, MobileInputAction.Salto, jumpButtonSprite, jumpIconSprite, new Vector2(-170f, 104f), new Vector2(112f, 112f), new Vector2(54f, 54f));
        CreatePauseButton("BotonPausaTouch", controlsGO.transform, pauseButtonSprite, pauseIconSprite, new Vector2(-42f, -38f), new Vector2(88f, 88f), new Vector2(42f, 42f));

        MobileControlsUI controlsUI = hudCanvas.AddComponent<MobileControlsUI>();
        SerializedObject serializedControls = new SerializedObject(controlsUI);
        serializedControls.FindProperty("mostrarEnEditor").boolValue = true;
        serializedControls.FindProperty("mostrarEnStandalone").boolValue = false;
        serializedControls.FindProperty("mostrarEnWebGLConTouch").boolValue = true;
        serializedControls.FindProperty("spriteIzquierda").objectReferenceValue = leftSprite;
        serializedControls.FindProperty("spriteDerecha").objectReferenceValue = rightSprite;
        serializedControls.FindProperty("spriteBotonSalto").objectReferenceValue = jumpButtonSprite;
        serializedControls.FindProperty("spriteBotonAtaque").objectReferenceValue = attackButtonSprite;
        serializedControls.FindProperty("spriteBotonPausa").objectReferenceValue = pauseButtonSprite;
        serializedControls.FindProperty("spriteIconoSalto").objectReferenceValue = jumpIconSprite;
        serializedControls.FindProperty("spriteIconoAtaque").objectReferenceValue = attackIconSprite;
        serializedControls.FindProperty("spriteIconoPausa").objectReferenceValue = pauseIconSprite;
        serializedControls.FindProperty("tamanoBotonMovimiento").vector2Value = new Vector2(104f, 104f);
        serializedControls.FindProperty("tamanoBotonAccion").vector2Value = new Vector2(112f, 112f);
        serializedControls.FindProperty("tamanoBotonPausa").vector2Value = new Vector2(88f, 88f);
        serializedControls.FindProperty("tamanoIconoAccion").vector2Value = new Vector2(54f, 54f);
        serializedControls.FindProperty("tamanoIconoPausa").vector2Value = new Vector2(42f, 42f);
        serializedControls.FindProperty("margenHorizontal").floatValue = 42f;
        serializedControls.FindProperty("margenVertical").floatValue = 38f;
        serializedControls.ApplyModifiedPropertiesWithoutUndo();
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

        GameObject cardGO = CreateCard("PauseCard", overlay, new Vector2(520f, 500f));
        RectTransform card = cardGO.GetComponent<RectTransform>();

        CreateText("Titulo", card, "Pausa", 46f, new Vector2(0f, 182f), new Vector2(460f, 64f));
        Button continueButton = CreateButton("BotonContinuar", card, "Continuar", new Vector2(0f, 102f), new Vector2(260f, 54f));
        Button settingsButton = CreateButton("BotonAjustes", card, "Ajustes", new Vector2(0f, 34f), new Vector2(260f, 54f));
        Button controlsButton = CreateButton("BotonControles", card, "Controles", new Vector2(0f, -34f), new Vector2(260f, 54f));
        Button restartButton = CreateButton("BotonReiniciar", card, "Reiniciar", new Vector2(0f, -102f), new Vector2(260f, 54f));
        Button mainMenuButton = CreateButton("BotonMenuPrincipal", card, "Salir al menu", new Vector2(0f, -170f), new Vector2(260f, 54f));

        GameObject settingsGO = CreateCard("SettingsCard", overlay, new Vector2(520f, 380f));
        RectTransform settingsCard = settingsGO.GetComponent<RectTransform>();
        CreateText("Titulo", settingsCard, "Ajustes", 42f, new Vector2(0f, 128f), new Vector2(460f, 58f));
        CreateText("TextoVolumen", settingsCard, "Volumen general", 24f, new Vector2(-92f, 62f), new Vector2(260f, 38f));
        TMP_Text volumeValueText = CreateText("TextoValorVolumen", settingsCard, "100%", 24f, new Vector2(180f, 62f), new Vector2(90f, 38f));
        Slider volumeSlider = CreateSlider("SliderVolumen", settingsCard, new Vector2(0f, 16f), new Vector2(360f, 36f));
        Toggle muteToggle = CreateToggle("ToggleSilencio", settingsCard, "Silenciar audio", new Vector2(0f, -56f), new Vector2(300f, 42f));
        Button backButton = CreateButton("BotonVolver", settingsCard, "Volver", new Vector2(0f, -128f), new Vector2(220f, 54f));
        settingsGO.SetActive(false);

        GameObject controlsGO = ControlsHelpPanelFactory.CrearPanelControles("ControlsCard", overlay, new Vector2(760f, 440f), out Button controlsBackButton);

        PauseMenuScreen pauseScreen = hudCanvas.AddComponent<PauseMenuScreen>();
        SerializedObject serializedScreen = new SerializedObject(pauseScreen);
        serializedScreen.FindProperty("panelPausa").objectReferenceValue = overlayGO;
        serializedScreen.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
        serializedScreen.FindProperty("menuPrincipal").objectReferenceValue = cardGO;
        serializedScreen.FindProperty("panelAjustes").objectReferenceValue = settingsGO;
        serializedScreen.FindProperty("panelControles").objectReferenceValue = controlsGO;
        serializedScreen.FindProperty("botonContinuar").objectReferenceValue = continueButton;
        serializedScreen.FindProperty("botonAjustes").objectReferenceValue = settingsButton;
        serializedScreen.FindProperty("botonControles").objectReferenceValue = controlsButton;
        serializedScreen.FindProperty("botonReiniciar").objectReferenceValue = restartButton;
        serializedScreen.FindProperty("botonMenuPrincipal").objectReferenceValue = mainMenuButton;
        serializedScreen.FindProperty("botonVolverAjustes").objectReferenceValue = backButton;
        serializedScreen.FindProperty("botonVolverControles").objectReferenceValue = controlsBackButton;
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
        serializedScreen.FindProperty("nombreEscenaMenuPrincipal").stringValue = "MainMenu";
        serializedScreen.FindProperty("rutaEscenaMenuPrincipalEditor").stringValue = "Assets/Scenes/MainMenu.unity";
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

        GameObject cardGO = CreateCard("GameOverCard", overlay, new Vector2(560f, 340f));
        RectTransform card = cardGO.GetComponent<RectTransform>();

        TMP_Text titleText = CreateText("Titulo", card, "Derrota", 46f, new Vector2(0f, 104f), new Vector2(500f, 64f));
        TMP_Text detailText = CreateText("Detalle", card, "Intentalo otra vez.", 26f, new Vector2(0f, 28f), new Vector2(470f, 54f));
        Button retryButton = CreateButton("BotonReintentar", card, "Reintentar", new Vector2(-132f, -98f), new Vector2(220f, 54f));
        Button mainMenuButton = CreateButton("BotonMenuPrincipal", card, "Menu principal", new Vector2(132f, -98f), new Vector2(220f, 54f));

        GameOverScreen gameOverScreen = hudCanvas.AddComponent<GameOverScreen>();
        SerializedObject serializedScreen = new SerializedObject(gameOverScreen);
        serializedScreen.FindProperty("panelDerrota").objectReferenceValue = overlayGO;
        serializedScreen.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
        serializedScreen.FindProperty("textoTitulo").objectReferenceValue = titleText;
        serializedScreen.FindProperty("textoDetalle").objectReferenceValue = detailText;
        serializedScreen.FindProperty("botonReintentar").objectReferenceValue = retryButton;
        serializedScreen.FindProperty("botonMenuPrincipal").objectReferenceValue = mainMenuButton;
        serializedScreen.FindProperty("titulo").stringValue = "Derrota";
        serializedScreen.FindProperty("detalle").stringValue = "Intentalo otra vez.";
        serializedScreen.FindProperty("delayMostrar").floatValue = 1.2f;
        serializedScreen.FindProperty("pausarJuegoAlMostrar").boolValue = true;
        serializedScreen.FindProperty("nombreEscenaMenuPrincipal").stringValue = "MainMenu";
        serializedScreen.FindProperty("rutaEscenaMenuPrincipalEditor").stringValue = "Assets/Scenes/MainMenu.unity";
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

    private static void CreateMobileButton(
        string name,
        Transform parent,
        MobileInputAction action,
        Sprite backgroundSprite,
        Sprite iconSprite,
        Vector2 position,
        Vector2 size,
        Vector2 iconSize)
    {
        GameObject buttonGO = CreateUIObject(name, parent);
        RectTransform rectTransform = buttonGO.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;

        Image background = buttonGO.AddComponent<Image>();
        background.sprite = backgroundSprite;
        background.preserveAspect = true;
        background.raycastTarget = true;
        background.color = new Color(1f, 1f, 1f, 0.72f);

        MobileInputButton inputButton = buttonGO.AddComponent<MobileInputButton>();
        SerializedObject serializedButton = new SerializedObject(inputButton);
        serializedButton.FindProperty("accion").intValue = (int)action;
        serializedButton.FindProperty("imagenObjetivo").objectReferenceValue = background;
        serializedButton.ApplyModifiedPropertiesWithoutUndo();

        if(iconSprite == null)
        {
            CreatePauseBars(buttonGO.transform);
            return;
        }

        GameObject iconGO = CreateUIObject("Icono", buttonGO.transform);
        RectTransform icon = iconGO.GetComponent<RectTransform>();
        icon.anchorMin = new Vector2(0.5f, 0.5f);
        icon.anchorMax = new Vector2(0.5f, 0.5f);
        icon.pivot = new Vector2(0.5f, 0.5f);
        icon.anchoredPosition = Vector2.zero;
        icon.sizeDelta = iconSize;

        Image iconImage = iconGO.AddComponent<Image>();
        iconImage.sprite = iconSprite;
        iconImage.preserveAspect = true;
        iconImage.raycastTarget = false;
    }

    private static void CreatePauseBars(Transform parent)
    {
        CreatePauseBar("BarraIzquierda", parent, new Vector2(-8f, 0f));
        CreatePauseBar("BarraDerecha", parent, new Vector2(8f, 0f));
    }

    private static void CreatePauseBar(string name, Transform parent, Vector2 position)
    {
        GameObject barGO = CreateUIObject(name, parent);
        RectTransform bar = barGO.GetComponent<RectTransform>();
        bar.anchorMin = new Vector2(0.5f, 0.5f);
        bar.anchorMax = new Vector2(0.5f, 0.5f);
        bar.pivot = new Vector2(0.5f, 0.5f);
        bar.anchoredPosition = position;
        bar.sizeDelta = new Vector2(10f, 34f);

        Image image = barGO.AddComponent<Image>();
        image.color = Color.white;
        image.raycastTarget = false;
    }

    private static void CreatePauseButton(
        string name,
        Transform parent,
        Sprite backgroundSprite,
        Sprite iconSprite,
        Vector2 position,
        Vector2 size,
        Vector2 iconSize)
    {
        GameObject buttonGO = CreateUIObject(name, parent);
        RectTransform rectTransform = buttonGO.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(1f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(1f, 1f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;

        Image background = buttonGO.AddComponent<Image>();
        background.sprite = backgroundSprite;
        background.preserveAspect = true;
        background.raycastTarget = true;
        background.color = new Color(1f, 1f, 1f, 0.72f);

        Button button = buttonGO.AddComponent<Button>();
        button.targetGraphic = background;

        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.9f);
        colors.pressedColor = new Color(1f, 1f, 1f, 0.55f);
        button.colors = colors;

        if(iconSprite == null)
        {
            return;
        }

        GameObject iconGO = CreateUIObject("Icono", buttonGO.transform);
        RectTransform icon = iconGO.GetComponent<RectTransform>();
        icon.anchorMin = new Vector2(0.5f, 0.5f);
        icon.anchorMax = new Vector2(0.5f, 0.5f);
        icon.pivot = new Vector2(0.5f, 0.5f);
        icon.anchoredPosition = Vector2.zero;
        icon.sizeDelta = iconSize;

        Image iconImage = iconGO.AddComponent<Image>();
        iconImage.sprite = iconSprite;
        iconImage.preserveAspect = true;
        iconImage.raycastTarget = false;
    }

    private static void StretchFull(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    private static Sprite LoadSprite(string path)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

        if(sprite == null)
        {
            Debug.LogWarning("No se encontro el sprite de UI mobile: " + path);
        }

        return sprite;
    }

    private static AudioClip LoadAudioClip(string path)
    {
        AudioClip audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);

        if(audioClip == null)
        {
            Debug.LogWarning("No se encontro el audio: " + path);
        }

        return audioClip;
    }
}

public static class ControlsHelpPanelApplier
{
    private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
    private const string HUDCanvasPath = "Assets/Prefabs/UI/HUDCanvas.prefab";

    [MenuItem("PiggyPocket/UI/Aplicar Panel Controles A Todo")]
    public static void ApplyControlsToAll()
    {
        ApplyControlsToMainMenu();
        ApplyControlsToHUDCanvas();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Panel de controles aplicado en MainMenu y HUDCanvas.");
    }

    [MenuItem("PiggyPocket/UI/Aplicar Panel Controles A MainMenu")]
    public static void ApplyControlsToMainMenu()
    {
        if(!File.Exists(MainMenuScenePath))
        {
            Debug.LogError("No se encontro la escena MainMenu en " + MainMenuScenePath);
            return;
        }

        EditorSceneManager.OpenScene(MainMenuScenePath);

        MainMenuScreen mainMenu = Object.FindFirstObjectByType<MainMenuScreen>(FindObjectsInactive.Include);

        if(mainMenu == null)
        {
            Debug.LogError("No se encontro MainMenuScreen en MainMenu.");
            return;
        }

        Transform menuRoot = mainMenu.transform.Find("MenuRoot");
        Transform mainPanel = menuRoot != null ? menuRoot.Find("MainPanel") : null;

        if(menuRoot == null || mainPanel == null)
        {
            Debug.LogError("MainMenu no tiene MenuRoot/MainPanel. No se puede aplicar el panel de controles sin reconstruir el menu.");
            return;
        }

        Button controlsButton = EnsureButton(mainPanel, "BotonControles", "Controles", new Vector2(0f, -56f), new Vector2(270f, 56f), mainPanel.Find("BotonAjustes")?.GetComponent<Button>());
        LayoutMainMenuButtons(mainPanel, controlsButton);

        GameObject controlsPanel = EnsureControlsPanel(menuRoot, "ControlsPanel", new Vector2(760f, 440f), out Button backButton);
        controlsPanel.SetActive(false);

        SerializedObject serializedMenu = new SerializedObject(mainMenu);
        SetReference(serializedMenu, "panelControles", controlsPanel);
        SetReference(serializedMenu, "botonControles", controlsButton);
        SetReference(serializedMenu, "botonVolverControles", backButton);
        serializedMenu.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(mainMenu);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log("Panel de controles materializado en MainMenu.");
    }

    [MenuItem("PiggyPocket/UI/Aplicar Panel Controles A HUDCanvas")]
    public static void ApplyControlsToHUDCanvas()
    {
        GameObject hudCanvas = PrefabUtility.LoadPrefabContents(HUDCanvasPath);

        try
        {
            Transform overlay = hudCanvas.transform.Find("PauseOverlay");
            Transform pauseCard = hudCanvas.transform.Find("PauseOverlay/PauseCard");

            if(overlay == null || pauseCard == null)
            {
                Debug.LogError("HUDCanvas no tiene PauseOverlay/PauseCard. No se puede aplicar el panel de controles.");
                return;
            }

            Button controlsButton = EnsureButton(pauseCard, "BotonControles", "Controles", new Vector2(0f, -34f), new Vector2(260f, 54f), pauseCard.Find("BotonAjustes")?.GetComponent<Button>());
            LayoutPauseButtons(pauseCard, controlsButton);

            GameObject controlsPanel = EnsureControlsPanel(overlay, "ControlsCard", new Vector2(760f, 440f), out Button backButton);
            controlsPanel.SetActive(false);

            PauseMenuScreen pauseScreen = hudCanvas.GetComponent<PauseMenuScreen>();

            if(pauseScreen == null)
            {
                pauseScreen = hudCanvas.AddComponent<PauseMenuScreen>();
            }

            SerializedObject serializedPause = new SerializedObject(pauseScreen);
            SetReference(serializedPause, "panelControles", controlsPanel);
            SetReference(serializedPause, "botonControles", controlsButton);
            SetReference(serializedPause, "botonVolverControles", backButton);
            serializedPause.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(hudCanvas, HUDCanvasPath);
            Debug.Log("Panel de controles materializado en HUDCanvas.");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(hudCanvas);
        }
    }

    private static GameObject EnsureControlsPanel(Transform parent, string name, Vector2 size, out Button backButton)
    {
        Transform existing = parent.Find(name);

        if(existing != null)
        {
            backButton = existing.Find("BotonVolver")?.GetComponent<Button>();

            if(backButton != null)
            {
                ClearButtonEvents(backButton);
            }

            return existing.gameObject;
        }

        return ControlsHelpPanelFactory.CrearPanelControles(name, parent, size, out backButton);
    }

    private static Button EnsureButton(Transform parent, string name, string text, Vector2 position, Vector2 size, Button template)
    {
        Transform existing = parent.Find(name);

        if(existing != null)
        {
            Button existingButton = existing.GetComponent<Button>();
            UpdateButton(existingButton, text, position, size);
            return existingButton;
        }

        Button button;

        if(template != null)
        {
            GameObject clone = Object.Instantiate(template.gameObject, parent, false);
            clone.name = name;
            button = clone.GetComponent<Button>();
        }
        else
        {
            button = CreateButton(name, parent);
        }

        UpdateButton(button, text, position, size);
        ClearButtonEvents(button);
        return button;
    }

    private static Button CreateButton(string name, Transform parent)
    {
        GameObject buttonGO = new GameObject(name, typeof(RectTransform));
        buttonGO.layer = LayerMask.NameToLayer("UI");
        buttonGO.transform.SetParent(parent, false);

        Image image = buttonGO.AddComponent<Image>();
        image.color = new Color(0.93f, 0.55f, 0.18f, 1f);

        Button button = buttonGO.AddComponent<Button>();
        button.targetGraphic = image;

        GameObject labelGO = new GameObject("Texto", typeof(RectTransform));
        labelGO.layer = LayerMask.NameToLayer("UI");
        labelGO.transform.SetParent(buttonGO.transform, false);

        RectTransform labelRect = labelGO.GetComponent<RectTransform>();
        StretchFull(labelRect);

        TMP_Text label = labelGO.AddComponent<TextMeshProUGUI>();
        label.fontSize = 24f;
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.raycastTarget = false;

        return button;
    }

    private static void UpdateButton(Button button, string text, Vector2 position, Vector2 size)
    {
        if(button == null)
        {
            return;
        }

        RectTransform rect = button.GetComponent<RectTransform>();

        if(rect != null)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        TMP_Text label = button.GetComponentInChildren<TMP_Text>();

        if(label != null)
        {
            label.text = text;
        }
    }

    private static void LayoutMainMenuButtons(Transform mainPanel, Button controlsButton)
    {
        RectTransform panelRect = mainPanel.GetComponent<RectTransform>();

        if(panelRect != null)
        {
            panelRect.sizeDelta = new Vector2(Mathf.Max(panelRect.sizeDelta.x, 620f), Mathf.Max(panelRect.sizeDelta.y, 590f));
        }

        SetButtonPosition(mainPanel, "BotonJugar", new Vector2(0f, 84f));
        SetButtonPosition(mainPanel, "BotonAjustes", new Vector2(0f, 14f));
        UpdateButton(controlsButton, "Controles", new Vector2(0f, -56f), new Vector2(270f, 56f));
        SetButtonPosition(mainPanel, "BotonCreditos", new Vector2(0f, -126f));
        SetButtonPosition(mainPanel, "BotonSalir", new Vector2(0f, -196f));
    }

    private static void LayoutPauseButtons(Transform pauseCard, Button controlsButton)
    {
        RectTransform cardRect = pauseCard.GetComponent<RectTransform>();

        if(cardRect != null)
        {
            cardRect.sizeDelta = new Vector2(Mathf.Max(cardRect.sizeDelta.x, 520f), Mathf.Max(cardRect.sizeDelta.y, 500f));
        }

        SetButtonPosition(pauseCard, "BotonContinuar", new Vector2(0f, 102f));
        SetButtonPosition(pauseCard, "BotonAjustes", new Vector2(0f, 34f));
        UpdateButton(controlsButton, "Controles", new Vector2(0f, -34f), new Vector2(260f, 54f));
        SetButtonPosition(pauseCard, "BotonReiniciar", new Vector2(0f, -102f));
        SetButtonPosition(pauseCard, "BotonMenuPrincipal", new Vector2(0f, -170f));
    }

    private static void SetButtonPosition(Transform parent, string name, Vector2 position)
    {
        Transform buttonTransform = parent.Find(name);

        if(buttonTransform == null)
        {
            return;
        }

        RectTransform rect = buttonTransform.GetComponent<RectTransform>();

        if(rect != null)
        {
            rect.anchoredPosition = position;
        }
    }

    private static void ClearButtonEvents(Button button)
    {
        if(button == null)
        {
            return;
        }

        SerializedObject serializedButton = new SerializedObject(button);
        SerializedProperty calls = serializedButton.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");

        if(calls != null)
        {
            calls.ClearArray();
            serializedButton.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void SetReference(SerializedObject serializedObject, string propertyName, Object value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);

        if(property != null)
        {
            property.objectReferenceValue = value;
        }
    }

    private static void StretchFull(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}
