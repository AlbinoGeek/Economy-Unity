// <copyright file="Inventory.cs" company="Mewzor Holdings Inc.">
//     Copyright (c) Mewzor Holdings Inc. All rights reserved.
// </copyright>
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed.  We want to have public methods.")]
public class Inventory
{
    /// <summary>
    /// Initializes Inventory
    /// </summary>
    public Inventory()
    {
        Items = new List<Item>();
    }

    public List<Item> Items { get; private set; }

    /// <summary>
    /// shortcut to add one
    /// </summary>
    /// <param name="name">item to add</param>
    public void Add(string name)
    {
        Add(name, 1);
    }
    
    /// <summary>
    /// adds item by name
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
    /// finds item by name
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
}
