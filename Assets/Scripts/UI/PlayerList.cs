// <copyright file="PlayerList.cs" company="Mewzor Holdings Inc.">
//     Copyright (c) Mewzor Holdings Inc. All rights reserved.
// </copyright>
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public Text Text;

    private List<Agent> players;

    private string text;

    public bool Add(Agent agent)
    {
        if (!players.Contains(agent))
        {
            players.Add(agent);
            UpdatePlayers();
            return true;
        }
        return false;
    }

    public bool Remove(Agent agent)
    {
        if (players.Contains(agent))
        {
            players.Remove(agent);
            UpdatePlayers();
            return true;
        }
        return false;
    }

    #region Unity
    private void Awake()
    {
        players = new List<Agent>();
    }

    private void LateUpdate()
    {
        UpdatePlayers();
    }
    #endregion

    private void UpdatePlayers()
    {
        System.Text.StringBuilder builder = new System.Text.StringBuilder();

        // Put them into the string Builder
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].Alive)
            {
                builder.AppendLine(players[i].ToString());
            }
        }

        Text.text = builder.ToString().TrimEnd('\n');
    }
}
