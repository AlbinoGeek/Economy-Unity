// <copyright file="Item.cs" company="Mewzor Holdings Inc.">
//     Copyright (c) Mewzor Holdings Inc. All rights reserved.
// </copyright>

/// <summary>
/// 
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed.  We want to have public methods.")]
public class Item
{
    public Item(string name)
    {
        Name = name;
        Quantity = 1;
        
        // Load base properties from the database
        ItemBlueprint blueprint = Database.GetConnection("Items.db").Table<ItemBlueprint>().Where(x => x.Name == name).FirstOrDefault();
        Value = blueprint.Value;
        Weight = blueprint.Weight;
    }

    /// <summary>
    /// Gets name, unique, same as file
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets amount in stack
    /// </summary>
    public int Quantity { get; internal set; }
    
    public int Value { get; private set; }

    public float Weight { get; private set; }
}
