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
public class MapController : GlobalBehaviour
{
    private static string[] fruits = { "Apple", "Coconut", "Mango" };

    /// <summary>
    /// horizontal (width) length
    /// </summary>
    public int XSize;

    /// <summary>
    /// vertical (depth) length
    /// </summary>
    public int YSize;

    // TODO(Albino) This really should be private, but we need it elsewhere
    public List<Agent> Agents { get; private set; } = new List<Agent>();

    internal List<Provider> Providers { get; private set; } = new List<Provider>();
    
    private MapTile[][] mapData;
    
    public bool IsInMapBounds(Vector3 position)
    {
        if (position.x > 0 + 3f &&
            position.x < XSize - 3f &&
            position.z > 0 + 3f &&
            position.z < YSize - 3f)
        {
            return true;
        }

        return false;
    }

    public bool IsValidPosition(Vector3 position)
    {
        Collider[] col = Physics.OverlapSphere(position, .5f);
        return col.Length == 0;
    }

    /// <summary>
    /// Gets a random valid point on the map
    /// - not occupied at the time of acquiring
    /// </summary>
    /// <param name="offset">distance from all edges</param>
    /// <returns>valid coordinate on map</returns>
    public Vector3 GetRandomPoint(float offset = 1f)
    {
        while (true)
        {
            Vector3 point = new Vector3(Random.Range(0 + offset, XSize - offset),
                                        .1f,
                                        Random.Range(0 + offset, YSize - offset));

            if (IsInMapBounds(point) && IsValidPosition(point))
            {
                return point;
            }
        }
    }

    #region Unity
    /// <summary>
    /// if not \ref initialized then \ref Generate map 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        // Prevent self access
        map = null;

        Generate();
    }
    
    /// <summary>
    /// draw a border helper for \ref XSize and \ref YSize while selected
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(transform.position + new Vector3(XSize/2f, 0, YSize/2), new Vector3(XSize, .25f, YSize));
    }
    #endregion

    private void Generate()
    {
        // Initialize the map array
        mapData = new MapTile[XSize][];
        for (int i = 0; i < XSize; i++)
        {
            mapData[i] = new MapTile[YSize];
        }
        
        // Place lake near middle
        int originX = Random.Range(20, XSize - 20);
        int originY = Random.Range(20, YSize - 20);
        int width = Random.Range(5, 13);
        int height = Random.Range(5, 13);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x < 1 || x > width - 1 ||
                    y < 1 || y > height - 1)
                {
                    // Randomly (40% of the time), don't draw the lake's outer edges
                    if (Random.value > .6f)
                    {
                        continue;
                    }
                }

                mapData[originX + x][originY + y] = MapTile.Water;
            }
        }

        // Draw at random, each tree for all trees
        int trees = Random.Range(11, 18);
        for (int i = 0; i < trees; i++)
        {
            int x = Random.Range(1, XSize - 1);
            int y = Random.Range(1, YSize - 1);

            if (mapData[x][y] != MapTile.Dirt)
            {
                // We can't place that tree
                continue;
            }

            mapData[x][y] = MapTile.Tree;

            // Add 0-3 bushes per tree
            int bushes = Random.Range(0, 3);
            for (int j = 0; j < bushes; j++)
            {
                x = Mathf.Clamp(Random.Range(x - 2, x + 2), 1, XSize - 1);
                y = Mathf.Clamp(Random.Range(y - 2, y + 2), 1, YSize - 1);

                if (mapData[x][y] != MapTile.Dirt)
                {
                    // We can't place that bush
                    continue;
                }

                mapData[x][y] = MapTile.Bush;
            }
        }
        
        DrawBorder();
        
        // Draw the structure and shape of the map
        for (int y = 0; y < YSize; y++)
        {
            for (int x = 0; x < XSize; x++)
            {
                Vector3 location = new Vector3(x, 0, y);

                Provider provider;
                GameObject tile;

                switch (mapData[x][y])
                {
                    case MapTile.Water:
                        tile = CreateResource("Water", location + Vector3.down, transform);
                        Providers.Add(tile.GetComponent<Provider>());
                        break;

                    case MapTile.Tree:
                        tile = CreateResource("Tree", location + (Vector3.up / 3f), transform);
                        provider = tile.GetComponent<Provider>();

                        // Add 1-4 random fruit to this tree
                        provider.DropEntries.Add(new Provider.DropEntry(fruits[Random.Range(0, fruits.Length)], Random.Range(1, 4), 1));
                        Providers.Add(provider);
                        
                        // Also draw dirt under tree
                        goto case MapTile.Dirt;

                    case MapTile.Bush:
                        tile = CreateResource("Bush", location + (Vector3.down / 2), transform);
                        Providers.Add(tile.GetComponent<Provider>());

                        // Also draw dirt under bush
                        goto case MapTile.Dirt;

                    case MapTile.Dirt:
                        tile = CreateResource("Dirt", location + Vector3.down, transform);
                        
                        // Also add rock (1%)
                        if (Random.value < .01f)
                        {
                            tile = CreateResource("Stone", location + (Vector3.down * .5f), tile.transform);
                            provider = tile.AddComponent<Provider>();
                            string[] choices = { "Stone (Flat)", "Stone (Round)" };
                            int count = Random.Range(1, 2);
                            for (int i = 0; i < count; i++)
                            {
                                string choice = choices[Random.Range(0, 1)];
                                provider.DropEntries.Add(new Provider.DropEntry(choice, 1, 1));
                            }
                            Providers.Add(provider);
                        }
                        break;
                    case MapTile.Wall:
                        CreateResource("Wall", location, transform);
                        break;
                }
            }
        }

        // Create 1-2 loot chests
        int chests = Random.Range(1, 2);
        for (int i = 0; i < chests; i++)
        {
            GameObject go = CreateResource("Chest", GetRandomPoint(), transform);
            Provider provider = go.AddComponent<Provider>();
            provider.DropEntries.Add(new Provider.DropEntry("Knife", 1, 1));

            string[] options = { "Knife", "Money", "Bread" };
            provider.DropEntries.Add(new Provider.DropEntry(options[Random.Range(0, options.Length)], 1, 1));
            Providers.Add(provider);
        }
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
            mapData[i][y] = MapTile.Wall;
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
            mapData[x][i] = MapTile.Wall;
        }
    }
}
