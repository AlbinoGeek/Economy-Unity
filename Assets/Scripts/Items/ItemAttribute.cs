// <copyright file="ItemAttribute.cs" company="Mewzor Holdings Inc.">
//     Copyright (c) Mewzor Holdings Inc. All rights reserved.
// </copyright>
using SQLite4Unity3d;

/// <summary>
/// represents a single entry in key/value store related to \ref ItemBlueprint
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed.  We want to have public methods.")]
public class ItemAttribute
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets reference to \ref ItemBlueprint by Id
    /// </summary>
    public int ItemId { get; set; }

    public string Key { get; set; }

    public string Value { get; set; }

    /// <summary>
    /// exists for debugging
    /// </summary>
    /// <returns>string representation</returns>
    public override string ToString()
    {
        return $"[ItemAttribute Id={Id}, ItemId={ItemId}, Key={Key}, Value={Value}]";
    }
}
