// <copyright file="Provider.cs" company="Mewzor Holdings Inc.">
//     Copyright (c) Mewzor Holdings Inc. All rights reserved.
// </copyright>
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 
/// </summary>
[DisallowMultipleComponent]
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed.  We want to have public methods.")]
public class Provider : MonoBehaviour
{
    [System.Serializable]
    public class DropEntry
    {
        public DropEntry(string name, int stock, int perUse)
        {
            ItemName = name;
            ItemStock = stock;
            StockPerUse = perUse;
        }

        /// <summary>
        /// creates a representation of this item
        /// </summary>
        public string ItemName;

        /// <summary>
        /// how many items can be taken before depleted
        /// </summary>
        public int ItemStock;

        /// <summary>
        /// how many items are collected per use
        /// </summary>
        public int StockPerUse;
    }

    public List<DropEntry> DropEntries;

    /// <summary>
    /// destroy the parent object once we have been depleted
    /// </summary>
    public bool DestroyOnEmpty = true;

    /// <summary>
    /// when destroying the parent object, replace it with this
    /// </summary>
    public GameObject OnDestroyReplace = null;

    public DropEntry GetDrop()
    {
        for (int i = 0; i < DropEntries.Count; i++)
        {
            if (DropEntries[i].ItemStock <= 0)
            {
                continue;
            }

            DropEntries[i].ItemStock -= DropEntries[i].StockPerUse;
            return DropEntries[i];
        }

        if (DestroyOnEmpty)
        {
            DestroySelf();
        }
        return null;
    }
    
    private void DestroySelf()
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
