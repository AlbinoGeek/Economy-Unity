// <copyright file="ItemAttribute.cs" company="Mewzor Holdings Inc.">
//     Copyright (c) Mewzor Holdings Inc. All rights reserved.
// </copyright>
using SQLite4Unity3d;

/// <summary>
/// 
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed.  We want to have public methods.")]
public class ItemAttribute
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int ItemId { get; set; }

    public string Key { get; set; }
    public string Value { get; set; }

    public override string ToString()
    {
        return string.Format("[ItemAttribute Id={0}, ItemId={1}, Key={2}, Value={3}]", Id, ItemId, Key, Value);
    }
}
