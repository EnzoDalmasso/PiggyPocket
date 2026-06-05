using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

// Herramienta de editor para agregar vegetacion decorativa a la Tile Palette.
// Los tiles se crean sin collider porque sirven para fondo/parallax/decoracion.
[InitializeOnLoad]
public static class VegetationTilePaletteBuilder
{
    private const string RutaSpritesheets = "Assets/Sprites/Berie's_Adventure_Seaside_Asset_Pack_Free/Spritesheet";
    private const string RutaTilesVegetacion = "Assets/Tilemap/Vegetation";
    private const string RutaPaleta = "Assets/Tilemap/Tile Pallete 1/New Tile Palette.prefab";
    private const int ColumnasPorGrupo = 8;

    private static readonly GrupoVegetacion[] Grupos =
    {
        new GrupoVegetacion("vegetation_grass_rock_trunk.png", "vegetation_grass_rock_trunk"),
        new GrupoVegetacion("vegetation_grass_small.png", "vegetation_grass_small"),
        new GrupoVegetacion("vegetation_tree_palm.png", "vegetation_tree_palm")
    };

    static VegetationTilePaletteBuilder()
    {
        EditorApplication.delayCall += AplicarAutomaticamenteSiFaltaVegetacion;
    }

    [MenuItem("PiggyPocket/Level/Agregar Vegetacion A Tile Palette")]
    public static void AgregarVegetacionATilePalette()
    {
        TileBase[][] tilesPorGrupo = CrearTilesVegetacionPorGrupo();
        AgregarTilesAPaleta(tilesPorGrupo);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Vegetacion agregada a la Tile Palette: grass rock trunk, grass small y tree palm.");
    }

    private static void AplicarAutomaticamenteSiFaltaVegetacion()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        TileBase[][] tilesPorGrupo = CrearTilesVegetacionPorGrupo();

        if(tilesPorGrupo.All(grupo => grupo.Length == 0) || PaletaContieneTodos(tilesPorGrupo.SelectMany(grupo => grupo)))
        {
            return;
        }

        AgregarTilesAPaleta(tilesPorGrupo);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static TileBase[][] CrearTilesVegetacionPorGrupo()
    {
        AsegurarCarpetaTiles();

        List<TileBase[]> gruposCreados = new List<TileBase[]>();

        foreach(GrupoVegetacion grupo in Grupos)
        {
            string rutaSpritesheet = RutaSpritesheets + "/" + grupo.Archivo;
            Sprite[] sprites = CargarSpritesOrdenados(rutaSpritesheet, grupo.Prefijo);
            List<TileBase> tiles = new List<TileBase>();

            foreach(Sprite sprite in sprites)
            {
                string rutaTile = RutaTilesVegetacion + "/" + sprite.name + ".asset";
                Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(rutaTile);

                if(tile == null)
                {
                    tile = ScriptableObject.CreateInstance<Tile>();
                    AssetDatabase.CreateAsset(tile, rutaTile);
                }

                tile.sprite = sprite;
                tile.colliderType = Tile.ColliderType.None;
                tile.color = Color.white;
                EditorUtility.SetDirty(tile);

                tiles.Add(tile);
            }

            gruposCreados.Add(tiles.ToArray());
        }

        return gruposCreados.ToArray();
    }

    private static Sprite[] CargarSpritesOrdenados(string rutaSpritesheet, string prefijo)
    {
        Object[] assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(rutaSpritesheet);

        return assets
            .OfType<Sprite>()
            .Where(sprite => sprite.name.StartsWith(prefijo))
            .OrderBy(ObtenerIndiceSprite)
            .ToArray();
    }

    private static int ObtenerIndiceSprite(Sprite sprite)
    {
        string nombre = sprite.name;
        int separador = nombre.LastIndexOf('_');

        if(separador < 0 || separador >= nombre.Length - 1)
        {
            return 0;
        }

        return int.TryParse(nombre.Substring(separador + 1), out int indice) ? indice : 0;
    }

    private static void AgregarTilesAPaleta(TileBase[][] tilesPorGrupo)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RutaPaleta);

        if(prefab == null)
        {
            Debug.LogWarning("No se encontro la Tile Palette. Los tiles quedaron creados en " + RutaTilesVegetacion + ".");
            return;
        }

        GameObject contenidoPaleta = PrefabUtility.LoadPrefabContents(RutaPaleta);

        try
        {
            Tilemap tilemap = contenidoPaleta.GetComponentInChildren<Tilemap>();

            if(tilemap == null)
            {
                Debug.LogWarning("La Tile Palette no tiene Tilemap. Los tiles quedaron creados en " + RutaTilesVegetacion + ".");
                return;
            }

            if(PaletaContieneTodos(tilemap, tilesPorGrupo.SelectMany(grupo => grupo)))
            {
                return;
            }

            RemoverVegetacionExistente(tilemap);
            Vector3Int inicio = ObtenerInicioNuevaSeccion(tilemap);

            for(int grupoIndex = 0; grupoIndex < tilesPorGrupo.Length; grupoIndex++)
            {
                TileBase[] grupo = tilesPorGrupo[grupoIndex];

                for(int i = 0; i < grupo.Length; i++)
                {
                    int columna = i % ColumnasPorGrupo;
                    int fila = i / ColumnasPorGrupo;
                    Vector3Int posicion = inicio + new Vector3Int(columna, -(grupoIndex * 2 + fila), 0);
                    tilemap.SetTile(posicion, grupo[i]);
                }
            }

            tilemap.CompressBounds();
            PrefabUtility.SaveAsPrefabAsset(contenidoPaleta, RutaPaleta);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contenidoPaleta);
        }
    }

    private static bool PaletaContieneTodos(IEnumerable<TileBase> tiles)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RutaPaleta);

        if(prefab == null)
        {
            return false;
        }

        GameObject contenidoPaleta = PrefabUtility.LoadPrefabContents(RutaPaleta);

        try
        {
            Tilemap tilemap = contenidoPaleta.GetComponentInChildren<Tilemap>();
            return tilemap != null && PaletaContieneTodos(tilemap, tiles);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contenidoPaleta);
        }
    }

    private static bool PaletaContieneTodos(Tilemap tilemap, IEnumerable<TileBase> tiles)
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

        return tiles.Where(tile => tile != null).All(tilesExistentes.Contains);
    }

    private static void RemoverVegetacionExistente(Tilemap tilemap)
    {
        BoundsInt bounds = tilemap.cellBounds;

        foreach(Vector3Int posicion in bounds.allPositionsWithin)
        {
            TileBase tile = tilemap.GetTile(posicion);

            if(tile != null && EsTileVegetacion(tile))
            {
                tilemap.SetTile(posicion, null);
            }
        }
    }

    private static bool EsTileVegetacion(TileBase tile)
    {
        return Grupos.Any(grupo => tile.name.StartsWith(grupo.Prefijo));
    }

    private static Vector3Int ObtenerInicioNuevaSeccion(Tilemap tilemap)
    {
        tilemap.CompressBounds();
        BoundsInt bounds = tilemap.cellBounds;

        if(bounds.size.x == 0 || bounds.size.y == 0)
        {
            return Vector3Int.zero;
        }

        return new Vector3Int(bounds.xMin, bounds.yMin - 2, 0);
    }

    private static void AsegurarCarpetaTiles()
    {
        if(!AssetDatabase.IsValidFolder("Assets/Tilemap"))
        {
            AssetDatabase.CreateFolder("Assets", "Tilemap");
        }

        if(!AssetDatabase.IsValidFolder(RutaTilesVegetacion))
        {
            AssetDatabase.CreateFolder("Assets/Tilemap", Path.GetFileName(RutaTilesVegetacion));
        }
    }

    private readonly struct GrupoVegetacion
    {
        public readonly string Archivo;
        public readonly string Prefijo;

        public GrupoVegetacion(string archivo, string prefijo)
        {
            Archivo = archivo;
            Prefijo = prefijo;
        }
    }
}
