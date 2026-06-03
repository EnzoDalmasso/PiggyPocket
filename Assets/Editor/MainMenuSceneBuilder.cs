using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MainMenuSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/MainMenu.unity";

    [MenuItem("PiggyPocket/Scenes/Generate Main Menu")]
    public static void BuildMainMenu()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "MainMenu";

        SetupCamera();
        CreateMainMenuCanvas();
        CreateEventSystem();

        EditorSceneManager.MarkSceneDirty(scene);
        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, ScenePath);
        ConfigureBuildScenes();
        AssetDatabase.Refresh();

        Debug.Log("MainMenu generado en " + ScenePath);
    }

    [MenuItem("PiggyPocket/Scenes/Configure Build Scenes")]
    public static void ConfigureBuildScenes()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(ScenePath, true),
            new EditorBuildSettingsScene("Assets/Scenes/Level_01.unity", true)
        };

        Debug.Log("Build Settings configurado con MainMenu y Level_01.");
    }

    private static void SetupCamera()
    {
        GameObject cameraGO = new GameObject("Main Camera");
        cameraGO.tag = "MainCamera";
        cameraGO.transform.position = new Vector3(0f, 0f, -10f);

        Camera camera = cameraGO.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.28f, 0.76f, 0.9f, 1f);
        camera.orthographic = true;
        camera.orthographicSize = 5f;

        cameraGO.AddComponent<AudioListener>();
    }

    private static void CreateMainMenuCanvas()
    {
        GameObject canvasGO = new GameObject("MainMenuCanvas");
        RectTransform rectTransform = canvasGO.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;
        rectTransform.pivot = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        MainMenuScreen mainMenu = canvasGO.AddComponent<MainMenuScreen>();
        mainMenu.ReconstruirUI();
    }

    private static void CreateEventSystem()
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
