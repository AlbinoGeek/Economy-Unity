// <copyright file="MapController.cs" company="Mewzor Holdings Inc.">
//     Copyright (c) Mewzor Holdings Inc. All rights reserved.
// </copyright>
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
[DisallowMultipleComponent]
[ExecuteInEditMode]
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
    public Vector3 GetRandomPoint()
    {
        // We MUST find a valid point, this can't fail.
        Vector3 point;
        while (true)
        {
            point = new Vector3(Random.Range(-XSize / 2f, XSize / 2f), .1f, Random.Range(-YSize / 2f, YSize / 2f));

            Collider[] col = Physics.OverlapSphere(point + Vector3.up, .9f);
            if (col.Length == 0)
            {
                return point;
            }
        }
    }

    #region Unity
    /// <summary>
    /// if not \ref initialized then \ref Generate map 
    /// </summary>
    private void Awake()
    {
        Agents = new List<Agent>();
        if (!initialized)
        {
            Generate();
        }
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
        DrawHorizontalLine(0, 0, XSize);
        DrawHorizontalLine(0, YSize - 1, XSize);

        // Draw vertical borders
        DrawVerticalLine(0, 0, YSize);
        DrawVerticalLine(XSize - 1, 0, YSize);
    }

    /// <summary>
    /// adds walls in a horizontal line
    /// </summary>
    /// <param name="x">starting horizontal position</param>
    /// <param name="y">starting vertical position</param>
    /// <param name="length">distance to draw</param>
    private void DrawHorizontalLine(int x, int y, int length)
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
    private void DrawVerticalLine(int x, int y, int length)
    {
        for (int i = y; i < length + y; i++)
        {
            map[x][i] = MapTile.Wall;
        }
    }
}
