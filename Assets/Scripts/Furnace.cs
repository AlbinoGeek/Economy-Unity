// <copyright file="Furnace.cs" company="Mewzor Holdings Inc.">
//     Copyright (c) Mewzor Holdings Inc. All rights reserved.
// </copyright>
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
[DisallowMultipleComponent]
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed.  We want to have public methods.")]
public class Furnace : MonoBehaviour
{
    [System.Serializable]
    public class FurnaceItem
    {
        public Item Item;
        public float Cooked;
    }

    public int Fuel = 100;

    public List<FurnaceItem> Contents = new List<FurnaceItem>();

    public float LastFueledTime;

    public void Add(Item item)
    {
        var fi = new FurnaceItem();
        fi.Item = item;
        Contents.Add(fi);
    }
    
    /// <summary>
    /// moves finished items to Provider
    /// </summary>
    private void FixedUpdate()
    {
        if (Fuel <= 0)
        {
            if (LastFueledTime < Time.timeSinceLevelLoad - 30)
            {
                GetComponent<Provider>().DestroyOnEmpty = true;
            }

            return;
        }

        // A fueled fire doesn't peter out
        LastFueledTime = Time.timeSinceLevelLoad;

        for (int i = 0; i < Contents.Count; i++)
        {
            // It takes at least two rounds to cook
            Contents[i].Cooked += 1;
            Fuel -= 1;

            if (Contents[i].Cooked > 240)
            {
                string name = Contents[i].Item.Name.Split('(')[0].TrimEnd(' ');
                
                GetComponent<Provider>().Add(name + " (Cooked)", 1, 1);
                Contents.RemoveAt(i);
                i--;
            }
        }

        if (Contents.Count == 0)
        {
            Fuel -= 1;
        }
    }
}
