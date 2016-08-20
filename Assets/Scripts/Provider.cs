// <copyright file="Provider.cs" company="Mewzor Holdings Inc.">
//     Copyright (c) Mewzor Holdings Inc. All rights reserved.
// </copyright>
using UnityEngine;

/// <summary>
/// 
/// </summary>
[DisallowMultipleComponent]
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed.  We want to have public methods.")]
public class Provider : MonoBehaviour
{
    /// <summary>
    /// creates a representation of this item
    /// </summary>
    public string ItemName;

    /// <summary>
    /// how many items can be taken before depleted
    /// </summary>
    public int ItemStock = 1;

    /// <summary>
    /// how many items are collected per use
    /// </summary>
    public int StockPerUse = 1;

    /// <summary>
    /// destroy the parent object once we have been depleted
    /// </summary>
    public bool DestroyOnEmpty = true;

    /// <summary>
    /// when destroying the parent object, replace it with this
    /// </summary>
    public GameObject OnDestroyReplace = null;

    /// <summary>
    /// makes the Provider face an existential question of its own existence
    /// (but really, it just sees if we should be destroyed yet or not)
    /// </summary>
    public void ReconsiderLife()
    {
        // If we have run out of stock, and were told to destroy on empty ...
        if (ItemStock <= 0 && DestroyOnEmpty)
        {
            // If we have something to replace ourselves with...
            if (OnDestroyReplace)
            {
                GameObject go = Instantiate(OnDestroyReplace, transform.position - Vector3.up, transform.rotation) as GameObject;
                go.transform.parent = transform.parent;
            }

            // Remove ourselves from the map list
            GameObject.Find("Map").GetComponent<MapController>().Providers.Remove(this);

            // Destroy ourselves
            Destroy(gameObject);
            enabled = false;
        }
    }
}
