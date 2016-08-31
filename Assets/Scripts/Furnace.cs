// <copyright file="Furnace.cs" company="Mewzor Holdings Inc.">
//     Copyright (c) Mewzor Holdings Inc. All rights reserved.
// </copyright>
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// represents an object that applies heat to items inside until they are cooked
/// </summary>
[DisallowMultipleComponent]
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed.  We want to have public methods.")]
public class Furnace : MonoBehaviour
{
    public Inventory Inventory = new Inventory();

    public int Fuel = 100;

    public float LastFueledTime;

    private Provider provider;
    
    private void Awake()
    {
        provider = GetComponent<Provider>();
    }

    /// <summary>
    /// moves finished items to Provider
    /// </summary>
    private void FixedUpdate()
    {
        if (Fuel <= 0)
        {
            // Mark ourselves to destroy if we've been 60 seconds without fuel
            if (LastFueledTime < Time.timeSinceLevelLoad - 60)
            {
                GetComponent<Provider>().DestroyOnEmpty = true;
            }
        }
        else
        {
            // A fueled fire doesn't peter out
            LastFueledTime = Time.timeSinceLevelLoad;
            Fuel--;
        }

        for (int i = 0; i < Inventory.Items.Count; i++)
        {
            if (Inventory.Items[i].Temperature > 200)
            {
                string baseName = Inventory.Items[i].Name.Split('(')[0].TrimEnd(' ');
                if (Inventory.Remove(baseName + " (Raw)", 1))
                {
                    provider.Add(baseName + " (Cooked)", 1, 1);
                }

                i--;
                continue;
            }

            if (Fuel > 0)
            {
                Inventory.Items[i].Temperature++;
                Fuel--;
            }
        }
    }
}
