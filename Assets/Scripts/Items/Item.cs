// <copyright file="Item.cs" company="Mewzor Holdings Inc.">
//     Copyright (c) Mewzor Holdings Inc. All rights reserved.
// </copyright>
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// represents a thing in the world that can be traded or looted by \ref Agent
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed.  We want to have public methods.")]
[System.Serializable]
public class Item
{
    private static List<ItemBlueprint> blueprints = new List<ItemBlueprint>();
    private static List<KeyValuePair<int, ItemAttribute>> blueprintAttributes = new List<KeyValuePair<int, ItemAttribute>>();

    public static Item Clone(Item source)
    {
        Item item = new Item(source.Name);
        item.Quantity = 1;
        return item;
    }

    public static ItemBlueprint GetBlueprint(string name)
    {
        // First, check our cache
        ItemBlueprint blueprint = blueprints.Where(x => x.Name == name).FirstOrDefault();

        // Next, load it if we have to
        if (blueprint == null)
        {
            blueprint = Database.GetConnection("Items.db").Table<ItemBlueprint>().Where(x => x.Name == name).FirstOrDefault();

            if (blueprint != null)
            {
                // Add it to our cache
                blueprints.Add(blueprint);

                // Optimization: Load our attributes once on load
                var query = Database.GetConnection("Items.db").Table<ItemAttribute>().Where(x => x.ItemId == blueprint.Id);
                foreach (var result in query)
                {
                    blueprintAttributes.Add(new KeyValuePair<int, ItemAttribute>(blueprint.Id, result));
                }
            }
        }

        return blueprint;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Item" /> class and load from \ref ItemBlueprint. 
    /// </summary>
    /// <param name="name">name of \ref ItemBlueprint to load from</param>
    public Item(string name)
    {
        ItemBlueprint blueprint = GetBlueprint(name);
        
        // If we could not load the item from database
        if (blueprint == null)
        {
            // Throw an exception (corrupt database detected)
            UnityEngine.Debug.LogError(string.Format("Item with name {0} could not be found.", name));
            return;
        }

        Id = blueprint.Id;
        Name = name;
        Value = blueprint.Value;
        Weight = blueprint.Weight;
    }

    public int Id { get; private set; } = -1;

    /// <summary>
    /// Gets name, unique, same as file
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets amount in stack
    /// </summary>
    public int Quantity { get; internal set; } = 1;

    /// <summary>
    /// Gets relative trading value
    /// </summary>
    public int Value { get; private set; } = 0;

    /// <summary>
    /// Gets carrying mass
    /// </summary>
    public float Weight { get; private set; } = 0;

    public string GetAttribute(string key)
    {
        var list = blueprintAttributes.Where(x => x.Key == Id);
        foreach (KeyValuePair<int, ItemAttribute> attribute in list)
        {
            if (attribute.Value.Key == key)
            {
                return attribute.Value.Value;
            }
        }

        return null;
    }

    /// <summary>
    /// TEMPORARY DATA stored about an item, such as Temperature, Wear, etc
    /// </summary>
    public Dictionary<string, int> KeyValueData { get; private set; } = new Dictionary<string, int>();

    public int Temperature
    {
        get
        {
            int temperature;
            if (!KeyValueData.TryGetValue("temperature", out temperature))
            {
                Temperature = temperature;
            }
            return temperature;
        }
        set
        {
            KeyValueData["temperature"] = value;
        }
    }
}
