// <copyright file="PlayerList.cs" company="Mewzor Holdings Inc.">
//     Copyright (c) Mewzor Holdings Inc. All rights reserved.
// </copyright>
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
[DisallowMultipleComponent]
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed.  We want to have public methods.")]
public class PlayerList : MonoBehaviour
{
    /// <summary>
    /// Canvas Text component set in Unity Editor
    /// </summary>
    public Transform panel;

    /// <summary>
    /// represents a single player entry as a prefab to instantiate many times
    /// </summary>
    public GameObject Prefab;

    private List<PlayerListEntry> players = new List<PlayerListEntry>();

    public bool Add(Agent agent)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].Agent == agent)
            {
                return false;
            }
        }

        var ple = new PlayerListEntry();
        ple.Agent = agent;
        ple.Position = players.Count;
        ple.CreateCanvasElement(Prefab, panel);
        ple.CanvasElement.color = agent.color;

        ple.CanvasElement.fontSize = 20;
        if (ple.Agent.name.Length > 14)
        {
            ple.CanvasElement.fontSize = 16;
        }
        else if (ple.Agent.name.Length > 8)
        {
            ple.CanvasElement.fontSize = 18;
        }

        players.Add(ple);
        return true;
    }
    
    public bool Remove(Agent agent)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].Agent == agent)
            {
                players.Remove(players[i]);
                return true;
            }
        }

        return false;
    }
    
    #region Unity
    private void Start()
    {
        InvokeRepeating("UpdateAllCanvas", .5f, .5f);
    }
    #endregion

    public void UpdateAllCanvas()
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].Agent == null)
            {
                players[i].CanvasElement.text = "Dead and Gone";
                continue;
            }

            players[i].CanvasElement.text = players[i].Agent.ToString();
            players[i].CanvasElement.color = players[i].Agent.color;
        }
    }
}
