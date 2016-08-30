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
        if (position.x > 1 &&
            position.x < XSize &&
            position.z > 1 &&
            position.z < YSize)
        {
            return true;
        }

        return false;
    }

    public bool IsValidPosition(Vector3 position)
    {
        Collider[] col = Physics.OverlapSphere(position + (Vector3.up * .5f), 1f);
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
            Vector3 point = new Vector3(Random.Range(offset + 1, XSize - offset),
                                        .1f,
                                        Random.Range(offset + 1, YSize - offset));

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
        mapData = new MapTile[XSize + 2][];
        for (int i = 0; i < XSize + 2; i++)
        {
            mapData[i] = new MapTile[YSize + 2];
        }
        
        // Generate three water bodies and fill them
        GenerateLake(XSize / 2, 0);
        GenerateLake(XSize / 2, 0);
        GenerateLake(XSize / 2, 0);
        FillInLakes();
        FillInLakes();

        // Draw at random, each tree for all trees
        int trees = Random.Range(11, 18);
        for (int i = 0; i < trees; i++)
        {
            int x = Random.Range(2, XSize - 3);
            int y = Random.Range(2, YSize - 3);

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
                x = Mathf.Clamp(Random.Range(x - 2, x + 2), 2, XSize - 3);
                y = Mathf.Clamp(Random.Range(y - 2, y + 2), 2, YSize - 3);

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
        for (int y = 0; y < YSize + 2; y++)
        {
            for (int x = 0; x < XSize + 2; x++)
            {
                Vector3 location = new Vector3(x, 0, y);

                Provider provider;
                GameObject tile;

                switch (mapData[x][y])
                {
                    case MapTile.Water:
                        tile = CreateResource("Water", location + Vector3.down, transform);
                        provider = tile.GetComponent<Provider>();
                        provider.Add("Water", 12, 3);
                        provider.Add("Fish (Raw)", Random.Range(1, 4), 1);
                        Providers.Add(provider);
                        break;

                    case MapTile.Tree:
                        tile = CreateResource("Tree", location + (Vector3.up / 3f), transform);
                        provider = tile.GetComponent<Provider>();
                        provider.Add("Log", 1, 1);
                        provider.Add("Branch", Random.Range(1, 4), 2);
                        if (Random.value > .5f)
                        {
                            provider.Add("Stick", 1, 1);
                        }
                        provider.Add(fruits[Random.Range(0, fruits.Length)], Random.Range(1, 4), 1);
                        Providers.Add(provider);
                        
                        // Also draw dirt under tree
                        goto case MapTile.Dirt;

                    case MapTile.Bush:
                        tile = CreateResource("Bush", location + (Vector3.down / 2), transform);
                        provider = tile.GetComponent<Provider>();
                        provider.Add("Twig", Random.Range(1, 4), 2);
                        provider.Add("Berry", Random.Range(3, 8), 4);
                        Providers.Add(provider);
                        
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
                        CreateResource("Wall", location + Vector3.down, transform);
                        break;
                }
            }
        }

        // Create 1-2 loot chests
        int chests = Random.Range(1, 2);
        for (int i = 0; i < chests; i++)
        {
            GameObject go = CreateResource("Chest", GetRandomPoint(), transform);
            Provider provider = go.GetComponent<Provider>();
            provider.Add("Knife", 1, 1);

            string[] options = { "Knife", "Bread", "Fish (Cooked)" };
            provider.Add(options[Random.Range(0, options.Length)], 1, 1);
            Providers.Add(provider);
        }
    }

    /// <summary>
    /// adds a border of walls around \ref this.map
    /// </summary>
    private void DrawBorder()
    {
        // Draw horizontal borders
        DrawLineHorizontal(0, 0, XSize + 2);
        DrawLineHorizontal(0, YSize + 1, XSize + 2);

        // Draw vertical borders
        DrawLineVertical(0, 0, YSize + 2);
        DrawLineVertical(XSize + 1, 0, YSize + 2);
    }
    
    private void GenerateLake(int originX, int originY)
    {
        int x = 0;
        int y = 0;
        int count = 0;
        int lastDir = 0;
        while (count < 30)
        {
            int dir = Random.Range(0, 4);

            // If we generated a wrong direction, try again.
            if (lastDir == 0 && dir == 3 ||
                lastDir == 1 && dir == 2 ||
                lastDir == 2 && dir == 1 ||
                lastDir == 3 && dir == 0)
            {
                continue;
            }

            if (dir == 0)
            {
                y += 1;
            }
            else if (dir == 1)
            {
                x += 1;
            }
            else if (dir == 2)
            {
                x -= 1;
            }
            else if (dir == 3)
            {
                y -= 1;
            }

            if (originX + x < 0 || originY + y < 0 || originX + x > XSize - 1 || originY + y > YSize - 1)
            {
                return;
            }

            if (mapData[originX + x][originY + y] != MapTile.Dirt)
            {
                continue;
            }

            mapData[originX + x][originY + y] = MapTile.Water;
        }
    }

    private int FindNeighbors(int x, int y, MapTile tile)
    {
        int neighbors = 0;

        //if (mapData[x - 1][y - 1] == tile) neighbors++;
        if (mapData[x - 1][y] == tile) neighbors++;
        if (mapData[x - 1][y + 1] == tile) neighbors++;

        if (mapData[x][y - 1] == tile) neighbors++;
        if (mapData[x][y] == tile) neighbors++;
        if (mapData[x][y + 1] == tile) neighbors++;

        if (mapData[x + 1][y - 1] == tile) neighbors++;
        if (mapData[x + 1][y] == tile) neighbors++;
        if (mapData[x + 1][y + 1] == tile) neighbors++;

        return neighbors;
    }

    private void FillInLakes()
    {
        for (int y = 1; y < YSize; ++y)
        {
            for (int x = 1; x < XSize; ++x)
            {
                // Fill in dirt islands with water
                if (mapData[x][y] == MapTile.Dirt && FindNeighbors(x, y, MapTile.Water) >= 4)
                {
                    mapData[x][y] = MapTile.Water;
                }
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
