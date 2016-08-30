// <copyright file="ItemBlueprint.cs" company="Mewzor Holdings Inc.">
//     Copyright (c) Mewzor Holdings Inc. All rights reserved.
// </copyright>
using SQLite4Unity3d;

/// <summary>
/// 
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed.  We want to have public methods.")]
public class ItemBlueprint
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets relative worth of this item
    /// </summary>
    public int Value { get; private set; } = 0;

    /// <summary>
    /// Gets mass of object in pounds, detracts from observed value
    /// </summary>
    public float Weight { get; private set; } = 0;
    
    /// <summary>
    /// exists for debugging
    /// </summary>
    /// <returns>string representation</returns>
    public override string ToString()
    {
        return $"[ItemBlueprint Id={Id}, Name={Name}, Value={Value}, Weight={Weight}]";
    }
}
