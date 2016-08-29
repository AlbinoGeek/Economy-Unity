// <copyright file="Inventory.cs" company="Mewzor Holdings Inc.">
//     Copyright (c) Mewzor Holdings Inc. All rights reserved.
// </copyright>
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// represents a collection of \ref Item held by an \ref Agent
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed.  We want to have public methods.")]
[System.Serializable]
public class Inventory
{
    /// <summary>
    /// Gets list of items we hold
    /// </summary>
    public List<Item> Items { get; private set; } = new List<Item>();

    /// <summary>
    /// shortcut to add one by name
    /// </summary>
    /// <param name="name">item to add</param>
    public void Add(string name)
    {
        Add(name, 1);
    }
    
    /// <summary>
    /// adds one or many by name
    /// </summary>
    /// <param name="name">item to add</param>
    /// <param name="quantity">amount to add</param>
    public void Add(string name, int quantity)
    {
        Item stack = Find(name);
        if (stack != null)
        {
            stack.Quantity += quantity;
        }
        else
        {
            stack = new Item(name);
            stack.Quantity = quantity;
            Items.Add(stack);
        }
    }

    /// <summary>
    /// count by name
    /// </summary>
    /// <param name="name">item to find</param>
    /// <returns>amount found, or 0</returns>
    public int Count(string name)
    {
        Item item = Find(name);
        return item != null ? item.Quantity : 0;
    }

    /// <summary>
    /// search by name
    /// </summary>
    /// <param name="name">item to find</param>
    /// <returns>found item on success ; null on failure</returns>
    public Item Find(string name)
    {
        return Items.FirstOrDefault(item => item.Name == name);
    }

    /// <summary>
    /// shortcut to remove one
    /// </summary>
    /// <param name="name">item to remove</param>
    /// <returns>true on success ; false on failure</returns>
    public bool Remove(string name)
    {
        return Remove(name, 1);
    }

    /// <summary>
    /// removes item by name
    /// </summary>
    /// <param name="name">item to remove</param>
    /// <param name="quantity">amount to remove</param>
    /// <returns>true on success ; false on failure</returns>
    public bool Remove(string name, int quantity)
    {
        Item stack = Find(name);
        if (stack != null && stack.Quantity >= quantity)
        {
            stack.Quantity -= quantity;

            // If we no longer have any, remove this item
            if (stack.Quantity == 0)
            {
                Items.Remove(stack);
            }

            return true;
        }

        return false;
    }
    
    public float Weight
    {
        get
        {
            return Items.Sum(x => x.Weight);
        }
    }

    /// <summary>
    /// string representation
    /// </summary>
    public override string ToString()
    {
        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        
        // Put them into the string Builder
        for (int i = 0; i < Items.Count; i++)
        {
            builder.Append($"{Items[i].Quantity}x {Items[i].Name}, ");
        }

        return builder.ToString().TrimEnd(' ').TrimEnd(',');
    }
}
