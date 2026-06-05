using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Herramienta de editor para armar rapidamente el fondo parallax del nivel.
public static class ParallaxBackgroundBuilder
{
    private const string RutaSprites = "Assets/Sprites/Berie's_Adventure_Seaside_Asset_Pack_Free/PNG/";
    private const string RutaLevel01 = "Assets/Scenes/Level_01.unity";
    private const string NombreFondo = "ParallaxBackground";
    private const float AltoDiseno = 1.8f;
    private const float EscalaExtra = 1.05f;
    private const float TamanoCamaraNivel = 1.8f;

    private readonly struct LayerData
    {
        public readonly string nombre;
        public readonly string archivo;
        public readonly float factorX;
        public readonly int sortingOrder;

        public LayerData(string nombre, string archivo, float factorX, int sortingOrder)
        {
            this.nombre = nombre;
            this.archivo = archivo;
            this.factorX = factorX;
            this.sortingOrder = sortingOrder;
        }
    }

    private static readonly LayerData[] Layers =
    {
        new LayerData("Sky", "background_sky.png", 0.02f, -100),
        new LayerData("Clouds_Big", "background_clouds_big.png", 0.08f, -90),
        new LayerData("Clouds_Medium", "background_clouds_medium.png", 0.14f, -89),
        new LayerData("Clouds_Small", "background_clouds_small.png", 0.2f, -88),
        new LayerData("Ocean", "background_ocean.png", 0.05f, -80),
        new LayerData("Sand", "background_sand.png", 0.12f, -70)
    };

    [MenuItem("PiggyPocket/Level/Crear Fondo Parallax")]
    public static void CrearFondoParallax()
    {
        Camera camara = Camera.main;

        if(camara == null)
        {
            Debug.LogError("No se encontro una camara con tag MainCamera en la escena.");
            return;
        }

        PrepararCamara(camara);
        RebuildFondoParallax(camara);
    }

    [MenuItem("PiggyPocket/Level/Reparar Level_01 Fondo Parallax")]
    public static void RepararLevel01FondoParallax()
    {
        EditorSceneManager.OpenScene(RutaLevel01);

        Camera camara = BuscarOCrearCamara();
        PrepararCamara(camara);
        RebuildFondoParallax(camara);

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log("Level_01 reparado: camara y fondo parallax guardados.");
    }

    private static void RebuildFondoParallax(Camera camara)
    {
        GameObject existente = GameObject.Find(NombreFondo);

        if(existente != null)
        {
            Undo.DestroyObjectImmediate(existente);
        }

        GameObject fondo = new GameObject(NombreFondo);
        Undo.RegisterCreatedObjectUndo(fondo, "Crear Fondo Parallax");

        foreach(LayerData layer in Layers)
        {
            CrearLayer(fondo.transform, camara, layer);
        }

        Selection.activeGameObject = fondo;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Fondo parallax creado. Revisalo en la jerarquia como ParallaxBackground.");
    }

    private static Camera BuscarOCrearCamara()
    {
        Camera camara = Camera.main;

        if(camara != null)
        {
            return camara;
        }

        GameObject cameraObject = new GameObject("Main Camera");
        Undo.RegisterCreatedObjectUndo(cameraObject, "Crear Main Camera");
        cameraObject.tag = "MainCamera";

        camara = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<CameraFollow2D>();

        return camara;
    }

    private static void PrepararCamara(Camera camara)
    {
        if(camara == null)
        {
            return;
        }

        camara.orthographic = true;
        camara.orthographicSize = TamanoCamaraNivel;
        camara.transform.SetParent(null);

        Transform player = BuscarPlayer();

        if(player != null)
        {
            camara.transform.position = player.position + new Vector3(0f, 1.2f, -10f);
            CameraFollow2D follow = camara.GetComponent<CameraFollow2D>();

            if(follow == null)
            {
                follow = camara.gameObject.AddComponent<CameraFollow2D>();
            }

            follow.AsignarObjetivo(player);
        }
    }

    private static Transform BuscarPlayer()
    {
        PlayerMovement playerMovement = Object.FindFirstObjectByType<PlayerMovement>();

        if(playerMovement != null)
        {
            return playerMovement.transform;
        }

        GameObject player = GameObject.Find("Player");
        return player != null ? player.transform : null;
    }

    private static void CrearLayer(Transform parent, Camera camara, LayerData data)
    {
        Sprite sprite = CargarSprite(RutaSprites + data.archivo);

        if(sprite == null)
        {
            Debug.LogWarning("No se encontro el sprite para la capa: " + data.archivo);
            return;
        }

        GameObject layer = new GameObject(data.nombre);
        Undo.RegisterCreatedObjectUndo(layer, "Crear Layer Parallax");
        layer.transform.SetParent(parent, true);

        SpriteRenderer spriteRenderer = layer.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = data.sortingOrder;

        float altoCamara = camara.orthographicSize * 2f;
        float anchoCamara = altoCamara * camara.aspect;
        float escala = altoCamara / AltoDiseno * EscalaExtra;
        float bordeExtraX = anchoCamara * 0.05f;
        float bordeExtraY = altoCamara * 0.025f;
        float origenX = camara.transform.position.x - anchoCamara * 0.5f - bordeExtraX;
        float origenY = camara.transform.position.y - altoCamara * 0.5f - bordeExtraY;
        float offsetY = sprite.rect.y / sprite.pixelsPerUnit * escala;

        layer.transform.position = new Vector3(origenX, origenY + offsetY, 5f);
        layer.transform.localScale = new Vector3(escala, escala, 1f);

        ParallaxBackgroundLayer parallax = layer.AddComponent<ParallaxBackgroundLayer>();
        parallax.Configurar(camara.transform, new Vector2(data.factorX, 1f), true, 2, true, AltoDiseno, EscalaExtra);
    }

    private static Sprite CargarSprite(string ruta)
    {
        return AssetDatabase.LoadAllAssetsAtPath(ruta).OfType<Sprite>().FirstOrDefault();
    }
}
