// <copyright file="Item.cs" company="Mewzor Holdings Inc.">
//     Copyright (c) Mewzor Holdings Inc. All rights reserved.
// </copyright>

/// <summary>
/// represents a thing in the world that can be traded or looted by \ref Agent
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed.  We want to have public methods.")]
public class Item
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Item" /> class and load from \ref ItemBlueprint. 
    /// </summary>
    /// <param name="name">name of \ref ItemBlueprint to load from</param>
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
    
    /// <summary>
    ///  Gets relative trading cost in Money
    /// </summary>
    public int Value { get; private set; }

    /// <summary>
    /// Gets carrying mass
    /// </summary>
    public float Weight { get; private set; }
}
