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
    public int Id { get; set; }

    public string Name { get; set; }

    public int Value { get; set; }

    public float Weight { get; set; }
    
    public override string ToString()
    {
        return string.Format("[ItemBlueprint Id={0}, Name={1}, Value={2}, Weight={3}]", Id, Name, Value, Weight);
    }
}
