using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

// Herramienta de editor para preparar los pinchos como tiles pintables.
public static class SpikeTrapTilemapBuilder
{
    private const string RutaLevel01 = "Assets/Scenes/Level_01.unity";
    private const string RutaSpritesPng = "Assets/Sprites/Berie's_Adventure_Seaside_Asset_Pack_Free/PNG";
    private const string RutaTilesSpike = "Assets/Tilemap/Traps";
    private const string RutaPaleta = "Assets/Tilemap/Tile Pallete 1/New Tile Palette.prefab";
    private const string NombreTilemapSpikes = "Hazards_Spikes";

    private static readonly string[] ArchivosSpike =
    {
        "trap_spike_1.png",
        "trap_spike_2.png",
        "trap_spike_3.png",
        "trap_spike_4.png"
    };

    [MenuItem("PiggyPocket/Level/Preparar Trampas Spike")]
    public static void PrepararTrampasSpike()
    {
        LimpiarTilesAnimadosPrevios();

        TileBase[] tiles = CrearTilesSpike();
        AgregarTilesAPaleta(tiles);
        CrearTilemapSpikeEnEscenaActiva();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log("Trampas spike listas: tiles creados, palette actualizada y Tilemap Hazards_Spikes preparado.");
    }

    [MenuItem("PiggyPocket/Level/Reparar Level_01 Trampas Spike")]
    public static void RepararLevel01TrampasSpike()
    {
        EditorSceneManager.OpenScene(RutaLevel01);
        PrepararTrampasSpike();
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

        Debug.Log("Level_01 reparado: Tilemap de spikes guardado en la escena.");
    }

    private static TileBase[] CrearTilesSpike()
    {
        AsegurarCarpetaTiles();

        List<TileBase> tiles = new List<TileBase>();

        foreach(string archivo in ArchivosSpike)
        {
            string rutaSprite = RutaSpritesPng + "/" + archivo;
            Sprite sprite = PrepararYCargarSprite(rutaSprite);

            if(sprite == null)
            {
                Debug.LogWarning("No se encontro el sprite de spike: " + rutaSprite);
                continue;
            }

            string nombreTile = System.IO.Path.GetFileNameWithoutExtension(archivo);
            string rutaTile = RutaTilesSpike + "/" + nombreTile + ".asset";
            Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(rutaTile);

            if(tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                AssetDatabase.CreateAsset(tile, rutaTile);
            }

            tile.sprite = sprite;
            tile.colliderType = Tile.ColliderType.Sprite;
            EditorUtility.SetDirty(tile);

            tiles.Add(tile);
        }

        return tiles.ToArray();
    }

    private static Sprite PrepararYCargarSprite(string rutaSprite)
    {
        TextureImporter importer = AssetImporter.GetAtPath(rutaSprite) as TextureImporter;

        if(importer != null)
        {
            bool requiereReimport = importer.textureType != TextureImporterType.Sprite ||
                                     importer.spriteImportMode != SpriteImportMode.Single ||
                                     !Mathf.Approximately(importer.spritePixelsPerUnit, 100f);

            if(requiereReimport)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 100f;
                importer.spritePivot = new Vector2(0.5f, 0.5f);
                importer.SaveAndReimport();
            }
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(rutaSprite);
    }

    [MenuItem("PiggyPocket/Level/Limpiar Trampas Spike Animadas")]
    public static void LimpiarTilesAnimadosPrevios()
    {
        RemoverTilesAnimadosDePaleta();
        BorrarAssetsAnimadosPrevios();
    }

    private static void AgregarTilesAPaleta(TileBase[] tiles)
    {
        if(tiles == null || tiles.Length == 0)
        {
            return;
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RutaPaleta);

        if(prefab == null)
        {
            Debug.LogWarning("No se encontro la palette existente. Los tiles quedaron creados en " + RutaTilesSpike + ".");
            return;
        }

        GameObject contenidoPaleta = PrefabUtility.LoadPrefabContents(RutaPaleta);

        try
        {
            Tilemap tilemap = contenidoPaleta.GetComponentInChildren<Tilemap>();

            if(tilemap == null)
            {
                Debug.LogWarning("La palette no tiene Tilemap. Los tiles quedaron creados en " + RutaTilesSpike + ".");
                return;
            }

            if(PaletaContieneTodos(tilemap, tiles))
            {
                return;
            }

            Vector3Int inicio = ObtenerInicioFilaNueva(tilemap);

            for(int i = 0; i < tiles.Length; i++)
            {
                tilemap.SetTile(inicio + new Vector3Int(i, 0, 0), tiles[i]);
            }

            PrefabUtility.SaveAsPrefabAsset(contenidoPaleta, RutaPaleta);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contenidoPaleta);
        }
    }

    private static bool PaletaContieneTodos(Tilemap tilemap, TileBase[] tiles)
    {
        HashSet<TileBase> tilesExistentes = new HashSet<TileBase>();
        BoundsInt bounds = tilemap.cellBounds;

        foreach(Vector3Int posicion in bounds.allPositionsWithin)
        {
            TileBase tile = tilemap.GetTile(posicion);

            if(tile != null)
            {
                tilesExistentes.Add(tile);
            }
        }

        return tiles.All(tilesExistentes.Contains);
    }

    private static void RemoverTilesAnimadosDePaleta()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RutaPaleta);

        if(prefab == null)
        {
            return;
        }

        GameObject contenidoPaleta = PrefabUtility.LoadPrefabContents(RutaPaleta);

        try
        {
            Tilemap tilemap = contenidoPaleta.GetComponentInChildren<Tilemap>();

            if(tilemap == null)
            {
                return;
            }

            bool modificada = false;
            BoundsInt bounds = tilemap.cellBounds;

            foreach(Vector3Int posicion in bounds.allPositionsWithin)
            {
                TileBase tile = tilemap.GetTile(posicion);

                if(tile != null && tile.name.StartsWith("trap_spike_animated_"))
                {
                    tilemap.SetTile(posicion, null);
                    modificada = true;
                }
            }

            if(modificada)
            {
                tilemap.CompressBounds();
                PrefabUtility.SaveAsPrefabAsset(contenidoPaleta, RutaPaleta);
            }
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contenidoPaleta);
        }
    }

    private static void BorrarAssetsAnimadosPrevios()
    {
        string[] rutasAnimadas = AssetDatabase.FindAssets("trap_spike_animated_", new[] { RutaTilesSpike })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(ruta => !string.IsNullOrEmpty(ruta))
            .ToArray();

        foreach(string ruta in rutasAnimadas)
        {
            AssetDatabase.DeleteAsset(ruta);
        }
    }

    private static Vector3Int ObtenerInicioFilaNueva(Tilemap tilemap)
    {
        tilemap.CompressBounds();
        BoundsInt bounds = tilemap.cellBounds;

        if(bounds.size.x == 0 || bounds.size.y == 0)
        {
            return Vector3Int.zero;
        }

        return new Vector3Int(bounds.xMin, bounds.yMin - 2, 0);
    }

    private static void CrearTilemapSpikeEnEscenaActiva()
    {
        Grid grid = Object.FindFirstObjectByType<Grid>();

        if(grid == null)
        {
            GameObject gridObject = new GameObject("Grid");
            Undo.RegisterCreatedObjectUndo(gridObject, "Crear Grid");
            grid = gridObject.AddComponent<Grid>();
            grid.cellSize = new Vector3(0.32f, 0.32f, 0f);
        }

        Transform existente = grid.transform.Find(NombreTilemapSpikes);
        GameObject spikesObject = existente != null ? existente.gameObject : new GameObject(NombreTilemapSpikes);

        if(existente == null)
        {
            Undo.RegisterCreatedObjectUndo(spikesObject, "Crear Tilemap Spikes");
        }

        spikesObject.transform.SetParent(grid.transform, false);
        spikesObject.layer = LayerMask.NameToLayer("Default");

        GetOrAdd<Tilemap>(spikesObject);

        TilemapRenderer renderer = GetOrAdd<TilemapRenderer>(spikesObject);
        renderer.sortingOrder = 2;

        TilemapCollider2D collider = GetOrAdd<TilemapCollider2D>(spikesObject);
        collider.isTrigger = true;

        Rigidbody2D rb = GetOrAdd<Rigidbody2D>(spikesObject);
        rb.bodyType = RigidbodyType2D.Static;

        GetOrAdd<SpikeTilemapHazard>(spikesObject);
        Selection.activeGameObject = spikesObject;
    }

    private static void AsegurarCarpetaTiles()
    {
        if(!AssetDatabase.IsValidFolder("Assets/Tilemap"))
        {
            AssetDatabase.CreateFolder("Assets", "Tilemap");
        }

        if(!AssetDatabase.IsValidFolder(RutaTilesSpike))
        {
            AssetDatabase.CreateFolder("Assets/Tilemap", "Traps");
        }
    }

    private static T GetOrAdd<T>(GameObject gameObject) where T : Component
    {
        T componente = gameObject.GetComponent<T>();
        return componente != null ? componente : gameObject.AddComponent<T>();
    }
}
