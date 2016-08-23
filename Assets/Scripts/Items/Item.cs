// <copyright file="Item.cs" company="Mewzor Holdings Inc.">
//     Copyright (c) Mewzor Holdings Inc. All rights reserved.
// </copyright>
using System.Collections.Generic;

/// <summary>
/// represents a thing in the world that can be traded or looted by \ref Agent
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed.  We want to have public methods.")]
[System.Serializable]
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

        Attributes = new List<ItemAttribute>();
        
        // Load base properties from the database
        ItemBlueprint blueprint = Database.GetConnection("Items.db").Table<ItemBlueprint>().Where(x => x.Name == name).FirstOrDefault();

        // If we could not load the item from database
        if (blueprint == null)
        {
            // Throw an exception (corrupt database detected)
            UnityEngine.Debug.LogError(string.Format("Item with name {0} could not be found.", name));
        }

        Value = blueprint.Value;
        Weight = blueprint.Weight;

        // Load Attributes from the database
        var query = Database.GetConnection("Items.db").Table<ItemAttribute>().Where(x => x.ItemId == blueprint.Id);
        foreach (var result in query)
        {
            Attributes.Add(result);
        }
    }

    public List<ItemAttribute> Attributes { get; private set; }

    public string GetAttribute(string Key)
    {
        for (int i = 0; i < Attributes.Count; i++)
        {
            if (Attributes[i].Key == Key)
            {
                return Attributes[i].Value;
            }
        }

        return null;
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
