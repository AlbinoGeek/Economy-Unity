// <copyright file="MapController.cs" company="Mewzor Holdings Inc.">
//     Copyright (c) Mewzor Holdings Inc. All rights reserved.
// </copyright>
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
[DisallowMultipleComponent]
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed.  We want to have public methods.")]
public class MapController : MonoBehaviour
{
    /// <summary>
    /// horizontal (width) length
    /// </summary>
    public int XSize;

    /// <summary>
    /// vertical (depth) length
    /// </summary>
    public int YSize;
    
    /// <summary>
    /// whether we have run \ref Generate yet
    /// </summary>
    private bool initialized;
    
    // TODO(Albino) This really should be private, but we need it elsewhere
    public List<Agent> Agents { get; private set; }

    internal List<Provider> Providers { get; private set; }
    
    private MapTile[][] map;
    
    public bool AddAgent(Agent agent)
    {
        if (!Agents.Contains(agent))
        {
            Agents.Add(agent);
            return true;
        }
        return false;
    }

    public bool RemoveAgent(Agent agent)
    {
        if (Agents.Contains(agent))
        {
            Agents.Remove(agent);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Gets a random valid point on the map
    /// - not ocupied at the time of acquiring
    /// </summary>
    /// <returns>valid coordinate on map</returns>
    public Vector3 GetRandomPoint(int offset = 0)
    {
        // We MUST find a valid point, this can't fail.
        Vector3 point;
        while (true)
        {
            point = new Vector3(Random.Range(-XSize / 2f + offset, XSize / 2f - offset),
                                .1f,
                                Random.Range(-YSize / 2f + offset, YSize / 2f - offset));

            Collider[] col = Physics.OverlapSphere(point + Vector3.up, .9f);
            if (col.Length == 0)
            {
                return point;
            }
        }
    }

    /// <summary>
    /// helper method to create new Agents
    /// </summary>
    /// <param name="prefabName">name of tile to spawn</param>
    /// <returns>newly created GameObject tile</returns>
    private static GameObject CreateTile(string prefabName, Vector3 position, Transform transform)
    {
        var prefab = Resources.Load("Tile_" + prefabName, typeof(GameObject));
        GameObject go = Instantiate(prefab, position, Quaternion.identity) as GameObject;
        go.transform.parent = transform;
        go.name = prefab.name;
        return go;
    }

    #region Unity
    /// <summary>
    /// if not \ref initialized then \ref Generate map 
    /// </summary>
    private void Awake()
    {
        Agents = new List<Agent>();
        Providers = new List<Provider>();
        if (!initialized)
        {
            Generate();
        }
    }
    
    /// <summary>
    /// builds the physical map based on \ref this.map
    /// </summary>
    private void Start()
    {
        // Draw the structure and shape of the map
        for (int y = 0; y < YSize; y++)
        {
            for (int x = 0; x < XSize; x++)
            {
                switch (this.map[x][y])
                {
                    case MapTile.Wall:
                        CreateTile("Wall", new Vector3(x - (XSize/2f), 0, y - (YSize/2f)), transform);
                        break;
                }
            }
        }
        
        // Fill map features, generate resources, etc
        int rand = Random.Range(2, 4);
        for (int i = 0; i < rand; i++)
        {
            DrawLake(GetRandomPoint(4), new Vector2(Random.Range(2, 6), Random.Range(2, 6)));
        }
        
        DrawForest(Random.Range(4, 12));
    }
    
    /// <summary>
    /// draw a border helper for \ref XSize and \ref YSize while selected
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(XSize, .25f, YSize));
    }
    #endregion

    private void Generate()
    {
        // Initialize the map array
        this.map = new MapTile[XSize][];
        for (int i = 0; i < XSize; i++)
        {
            this.map[i] = new MapTile[YSize];
        }

        DrawBorder();
        initialized = true;
    }

    /// <summary>
    /// adds a border of walls around \ref this.map
    /// </summary>
    private void DrawBorder()
    {
        // Draw horizontal borders
        DrawLineHorizontal(0, 0, XSize);
        DrawLineHorizontal(0, YSize - 1, XSize);

        // Draw vertical borders
        DrawLineVertical(0, 0, YSize);
        DrawLineVertical(XSize - 1, 0, YSize);
    }

    private void DrawForest(int num)
    {
        // Draw at random, each tree for all trees
        for (int i = 0; i < num; i++)
        {
            GameObject go = CreateTile("Tree", GetRandomPoint(), transform);
            Providers.Add(go.GetComponent<Provider>());

            // Add a series of bushes to each tre.
            float offset = -2;

            go = CreateTile("Bush", go.transform.position + new Vector3(offset, 0, -offset), transform);
            Providers.Add(go.GetComponent<Provider>());

            go = CreateTile("Bush", go.transform.position + new Vector3(-offset, 0, offset), transform);
            Providers.Add(go.GetComponent<Provider>());

            go = CreateTile("Bush", go.transform.position + new Vector3(offset, 0, offset), transform);
            Providers.Add(go.GetComponent<Provider>());

            go = CreateTile("Bush", go.transform.position + new Vector3(-offset, 0, -offset), transform);
            Providers.Add(go.GetComponent<Provider>());
        }
    }

    private void DrawLake(Vector3 position, Vector2 dimensions)
    {
        for (int y = 0; y < (int)dimensions.y; y++)
        {
            for (int x = 0; x < (int)dimensions.x; x++)
            {
                GameObject go = CreateTile("Water", position + new Vector3(x, -1f, y), transform);
                Providers.Add(go.GetComponent<Provider>());
            }
        }
    }

    /// <summary>
    /// adds walls in a horizontal line
    /// </summary>
    /// <param name="x">starting horizontal position</param>
    /// <param name="y">starting vertical position</param>
    /// <param name="length">distance to draw</param>
    private void DrawLineHorizontal(int x, int y, int length)
    {
        for (int i = x; i < length + x; i++)
        {
            map[i][y] = MapTile.Wall;
        }
    }
    
    /// <summary>
    /// adds walls in a vertical line
    /// </summary>
    /// <param name="x">starting horizontal position</param>
    /// <param name="y">starting vertical position</param>
    /// <param name="length">distance to draw</param>
    private void DrawLineVertical(int x, int y, int length)
    {
        for (int i = y; i < length + y; i++)
        {
            map[x][i] = MapTile.Wall;
        }
    }
}
