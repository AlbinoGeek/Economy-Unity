// <copyright file="Base.cs" company="Mewzor Holdings Inc.">
//     Copyright (c) Mewzor Holdings Inc. All rights reserved.
// </copyright>
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

[DisallowMultipleComponent]
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed.  We want to have public methods.")]
public class Base : MonoBehaviour
{
    public Inventory Inventory { get; private set; } = new Inventory();

    private Vector3 startingScale;
    
    #region Unity
    /// <summary>
    /// called once at first frame
    /// </summary>
    private void Start()
    {
        startingScale = transform.localScale;

        // once per second check our upgrades
        InvokeRepeating("CheckUpgrades", 1f, 1f);
    }
    #endregion

    private void CheckUpgrades()
    {
        // We currently use only logs to improve
        if (Inventory.Count("Log") <= 0)
        {
            return;
        }

        // 1) Build the floor if it hasn't been
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (!meshRenderer.enabled)
        {
            meshRenderer.enabled = true;
            Inventory.Remove("Log", 1);
            return;
        }

        // 2) Expand the floor
        if (transform.localScale.y == startingScale.y)
        {
            transform.localScale *= 2;
            Inventory.Remove("Log", 1);
            return;
        }
    }
}
