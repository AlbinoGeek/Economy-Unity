﻿// <copyright file="Provider.cs" company="Mewzor Holdings Inc.">
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
        public string ItemName { get; private set; }

        /// <summary>
        /// how many items can be taken before depleted
        /// </summary>
        public int ItemStock { get; set; }

        /// <summary>
        /// how many items are collected per use
        /// </summary>
        public int StockPerUse { get; private set; }
    }

    public List<DropEntry> DropEntries = new List<DropEntry>();

    /// <summary>
    /// destroy the parent object once we have been depleted
    /// </summary>
    public bool DestroyOnEmpty = false;

    public bool ScaleWithContents = false;

    private int currentDrops = 0;
    private int totalDrops = 0;
    private Vector3 originalScale;

    /// <summary>
    /// when destroying the parent object, replace it with this
    /// </summary>
    public GameObject OnDestroyReplace = null;

    public void Add(string name, int stock, int use)
    {
        for (int i = 0; i < DropEntries.Count; i++)
        {
            if (DropEntries[i].ItemName == name)
            {
                DropEntries[i].ItemStock += stock;
                totalDrops += stock;
                currentDrops += stock;
                return;
            }
        }

        totalDrops += stock;
        currentDrops += stock;
        DropEntries.Add(new DropEntry(name, stock, use));
    }

    public DropEntry GetDrop()
    {
        for (int i = 0; i < DropEntries.Count; i++)
        {
            if (DropEntries[i].ItemStock <= 0)
            {
                DropEntries.RemoveAt(i);
                i--;
                continue;
            }

            currentDrops -= (DropEntries[i].StockPerUse > DropEntries[i].ItemStock ? DropEntries[i].StockPerUse : DropEntries[i].ItemStock);
            return DropEntries[i];
        }

        return null;
    }
    
    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void Start()
    {
        InvokeRepeating("CheckDead", 1f, .1f);
    }

    private void CheckDead()
    {
        if (DropEntries.Count == 0)
        {
            if (DestroyOnEmpty)
            {
                DestroySelf();
            }
        }
        
        if (ScaleWithContents)
        {
            Vector3 scale = transform.localScale;
            scale.y = Mathf.Clamp(Mathf.Max(currentDrops, .1f) / totalDrops, 0f, 1f);
            transform.localScale = scale;
        }
    }
    
    private void DestroySelf()
    {
        // If we have something to replace ourselves with...
        if (OnDestroyReplace)
        {
            GameObject go = Instantiate(OnDestroyReplace, transform.position - Vector3.up, transform.rotation) as GameObject;
            go.transform.parent = transform.parent;
            go.name = OnDestroyReplace.name;
        }

        // Remove ourselves from the map list
        GameObject.Find("Control Objects").GetComponent<MapController>().Providers.Remove(this);

        // Destroy ourselves
        Destroy(gameObject);
        enabled = false;
    }
}
