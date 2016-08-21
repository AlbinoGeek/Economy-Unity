// <copyright file="PlayerListEntry.cs" company="Mewzor Holdings Inc.">
//     Copyright (c) Mewzor Holdings Inc. All rights reserved.
// </copyright>
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// represents a player in the player list
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed.  We want to have public methods.")]
public struct PlayerListEntry
{
    public Agent Agent;
    public int Position;
    public Text CanvasElement;

    public void CreateCanvasElement(GameObject prefab, Transform parent)
    {
        var go = MonoBehaviour.Instantiate(prefab, parent) as GameObject;
        CanvasElement = go.GetComponent<Text>();

        go.name = Agent.name;
        go.transform.parent = parent;
        go.transform.position = new Vector2(10, Position * 15 + 20);
    }
}