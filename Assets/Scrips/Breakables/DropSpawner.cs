using System;
using System.Collections.Generic;
using UnityEngine;

// Spawnea objetos al romperse un contenedor.
// El objeto rompible no necesita saber si suelta vida, monedas, bombas u otra cosa.
public class DropSpawner : MonoBehaviour
{
    [Serializable]
    private class DropEntry
    {
        public GameObject prefab = null;
        [Min(1)] public int cantidad = 1;
        [Range(0f, 1f)] public float probabilidad = 1f;
        public Vector2 offset = Vector2.zero;
        public bool usarOffsetAleatorio = false;
        public Vector2 rangoOffsetAleatorio = Vector2.zero;
    }

    [SerializeField] private Transform puntoSpawn;
    [SerializeField] private List<DropEntry> drops = new List<DropEntry>();

    public void Spawnear()
    {
        Vector3 posicionBase = puntoSpawn != null ? puntoSpawn.position : transform.position;

        foreach(DropEntry drop in drops)
        {
            if(drop == null || drop.prefab == null)
            {
                continue;
            }

            for(int i = 0; i < drop.cantidad; i++)
            {
                if(UnityEngine.Random.value > drop.probabilidad)
                {
                    continue;
                }

                Vector2 offset = drop.offset + ObtenerOffsetAleatorio(drop);
                Instantiate(drop.prefab, posicionBase + (Vector3)offset, Quaternion.identity);
            }
        }
    }

    private Vector2 ObtenerOffsetAleatorio(DropEntry drop)
    {
        if(!drop.usarOffsetAleatorio)
        {
            return Vector2.zero;
        }

        float x = UnityEngine.Random.Range(-drop.rangoOffsetAleatorio.x, drop.rangoOffsetAleatorio.x);
        float y = UnityEngine.Random.Range(-drop.rangoOffsetAleatorio.y, drop.rangoOffsetAleatorio.y);

        return new Vector2(x, y);
    }
}
