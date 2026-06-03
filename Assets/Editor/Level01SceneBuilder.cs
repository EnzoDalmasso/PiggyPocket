using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public static class Level01SceneBuilder
{
    private const string ScenePath = "Assets/Scenes/Level_01.unity";
    private const float CellSize = 0.32f;

    [MenuItem("PiggyPocket/Levels/Generate Level 01")]
    public static void BuildLevel01()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Level_01";

        SetupLighting();

        GameObject player = InstantiatePrefab("Assets/Prefabs/Player/Player.prefab", new Vector3(-5.4f, 0.72f, 0f), "Player");

        CreateCamera(player.transform);
        InstantiatePrefab("Assets/Prefabs/UI/HUDCanvas.prefab", Vector3.zero, "HUDCanvas");

        Tilemap tilemap = CreateGameplayTilemap();
        BuildTerrain(tilemap);

        CreateCollectibles();
        CreateBreakables();
        CreateEnemies();
        CreateGoal();

        EditorSceneManager.MarkSceneDirty(scene);
        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.Refresh();

        Debug.Log("Level_01 generado en " + ScenePath);
    }

    private static void SetupLighting()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = Color.white;

        GameObject lightGO = new GameObject("Global Light 2D");
        Light2D light2D = lightGO.AddComponent<Light2D>();
        light2D.lightType = Light2D.LightType.Global;
        light2D.intensity = 1f;
    }

    private static void CreateCamera(Transform player)
    {
        GameObject cameraGO = new GameObject("Main Camera");
        cameraGO.tag = "MainCamera";
        cameraGO.transform.position = new Vector3(-4f, 1.8f, -10f);

        Camera camera = cameraGO.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.36f, 0.82f, 0.95f, 1f);
        camera.orthographic = true;
        camera.orthographicSize = 3.1f;

        cameraGO.AddComponent<AudioListener>();

        CameraFollow2D follow = cameraGO.AddComponent<CameraFollow2D>();
        follow.AsignarObjetivo(player);
    }

    private static Tilemap CreateGameplayTilemap()
    {
        GameObject gridGO = new GameObject("Grid");
        Grid grid = gridGO.AddComponent<Grid>();
        grid.cellSize = new Vector3(CellSize, CellSize, 0f);

        GameObject tilemapGO = new GameObject("Tilemap");
        tilemapGO.tag = "Floor";
        tilemapGO.layer = LayerMask.NameToLayer("Floor");
        tilemapGO.transform.SetParent(gridGO.transform);

        Tilemap tilemap = tilemapGO.AddComponent<Tilemap>();
        TilemapRenderer renderer = tilemapGO.AddComponent<TilemapRenderer>();
        renderer.sortingOrder = 0;

        Rigidbody2D rb = tilemapGO.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;

        tilemapGO.AddComponent<TilemapCollider2D>();

        return tilemap;
    }

    private static void BuildTerrain(Tilemap tilemap)
    {
        TileBase topTile = LoadAsset<TileBase>("Assets/Tilemap/Tilemap 1/tilemap_0.asset");
        TileBase fillTile = LoadAsset<TileBase>("Assets/Tilemap/Tilemap 1/tilemap_3.asset");
        TileBase edgeTile = LoadAsset<TileBase>("Assets/Tilemap/Tilemap 1/tilemap_1.asset");

        PaintGround(tilemap, topTile, fillTile, -22, 12, -3, 1);
        PaintGround(tilemap, topTile, fillTile, 16, 39, -3, 1);
        PaintGround(tilemap, topTile, fillTile, 44, 72, -3, 1);

        PaintPlatform(tilemap, edgeTile, 2, 12, 6);
        PaintPlatform(tilemap, edgeTile, 25, 35, 9);
        PaintPlatform(tilemap, edgeTile, 48, 58, 6);
    }

    private static void PaintGround(Tilemap tilemap, TileBase topTile, TileBase fillTile, int xMin, int xMax, int yMin, int yMax)
    {
        for(int x = xMin; x <= xMax; x++)
        {
            for(int y = yMin; y <= yMax; y++)
            {
                tilemap.SetTile(new Vector3Int(x, y, 0), y == yMax ? topTile : fillTile);
            }
        }
    }

    private static void PaintPlatform(Tilemap tilemap, TileBase tile, int xMin, int xMax, int y)
    {
        for(int x = xMin; x <= xMax; x++)
        {
            tilemap.SetTile(new Vector3Int(x, y, 0), tile);
        }
    }

    private static void CreateCollectibles()
    {
        Vector3[] bronzeCoins =
        {
            new Vector3(-3.4f, 1.35f, 0f),
            new Vector3(-2.9f, 1.65f, 0f),
            new Vector3(-2.4f, 1.35f, 0f),
            new Vector3(1.0f, 2.55f, 0f),
            new Vector3(1.5f, 2.8f, 0f),
            new Vector3(2.0f, 2.55f, 0f),
            new Vector3(8.0f, 1.35f, 0f),
            new Vector3(8.6f, 1.35f, 0f),
            new Vector3(14.6f, 2.55f, 0f),
            new Vector3(15.2f, 2.8f, 0f),
            new Vector3(15.8f, 2.55f, 0f)
        };

        foreach(Vector3 position in bronzeCoins)
        {
            InstantiatePrefab("Assets/Prefabs/Collectibles/BronzeCoinCollectible.prefab", position, "BronzeCoinCollectible");
        }

        InstantiatePrefab("Assets/Prefabs/Collectibles/GoldCoinCollectible.prefab", new Vector3(9.6f, 1.55f, 0f), "GoldCoinCollectible");
        InstantiatePrefab("Assets/Prefabs/Collectibles/GoldCoinCollectible.prefab", new Vector3(17.4f, 1.55f, 0f), "GoldCoinCollectible");
    }

    private static void CreateBreakables()
    {
        InstantiatePrefab("Assets/Prefabs/Breakables/LifeBarrel.prefab", new Vector3(-0.6f, 0.75f, 0f), "LifeBarrel");
        InstantiatePrefab("Assets/Prefabs/Breakables/BombBarrel.prefab", new Vector3(6.4f, 0.75f, 0f), "BombBarrel");
        InstantiatePrefab("Assets/Prefabs/Breakables/PoisonBarrel.prefab", new Vector3(11.2f, 0.75f, 0f), "PoisonBarrel");
    }

    private static void CreateEnemies()
    {
        InstantiatePrefab("Assets/Prefabs/Enemy/Enemigo.prefab", new Vector3(3.7f, 0.76f, 0f), "Oruga_01");
        InstantiatePrefab("Assets/Prefabs/Enemy/Enemigo.prefab", new Vector3(14.0f, 0.76f, 0f), "Oruga_02");
    }

    private static void CreateGoal()
    {
        GameObject goal = new GameObject("MetaFinal");
        goal.transform.position = new Vector3(20.2f, 0.95f, 0f);

        SpriteRenderer renderer = goal.AddComponent<SpriteRenderer>();
        renderer.sprite = LoadAsset<Sprite>("Assets/Sprites/Berie's_Adventure_Seaside_Asset_Pack_Free/PNG/collectibles_treasure_ruby_static.png");
        renderer.sortingOrder = 2;

        BoxCollider2D collider = goal.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(0.5f, 0.7f);
        collider.offset = new Vector2(0f, 0.2f);
        collider.isTrigger = true;

        goal.AddComponent<LevelGoal>();
    }

    private static GameObject InstantiatePrefab(string path, Vector3 position, string name)
    {
        GameObject prefab = LoadAsset<GameObject>(path);
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.transform.position = position;
        instance.name = name;
        return instance;
    }

    private static T LoadAsset<T>(string path) where T : Object
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);

        if(asset == null)
        {
            throw new FileNotFoundException("No se encontro el asset requerido: " + path);
        }

        return asset;
    }
}
