// <copyright file="TradeOffer.cs" company="Mewzor Holdings Inc.">
//     Copyright (c) Mewzor Holdings Inc. All rights reserved.
// </copyright>
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

/// <summary>
/// represents an offer of trade between two parties
/// </summary>
[System.Serializable]
public struct TradeOffer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TradeOffer" /> struct.
    /// </summary>
    /// <param name="buyer">giving party</param>
    /// <param name="seller">receiving party</param>
    public TradeOffer(Agent buyer, Agent seller)
    {
        Buyer = buyer;
        Seller = seller;

        BuyEntry = new TradeOfferItem();
        SellEntries = new List<TradeOfferItem>();
    }

    /// <summary>
    /// Gets person with a want (initiator)
    /// </summary>
    public Agent Buyer { get; private set; }

    /// <summary>
    /// Gets or sets what \ref Buyer wants
    /// </summary>
    public TradeOfferItem BuyEntry { get; set; }

    /// <summary>
    /// Gets second party in a trade (offering \ref BuyEntry )
    /// </summary>
    public Agent Seller { get; private set; }

    /// <summary>
    /// Gets what \ref Buyer is willing to exchange for \ref BuyEntry
    /// </summary>
    public List<TradeOfferItem> SellEntries { get; private set; }

    /// <summary>
    /// Gets total perceived value of all items in \ref SellEntries
    /// </summary>
    public float Value
    {
        get
        {
            var self = this;
            return SellEntries.Sum(x => x.Quantity * x.Value * (self.Seller.inventory.Count(x.ItemName) / Mathf.Max(self.Buyer.inventory.Count(x.ItemName), .5f)));
        }
    }

    /// <summary>
    /// adds an item or increments quantity of item found in \ref SellEntries
    /// - used to increase the \ref Value for both parties
    /// - does not take the item, only references it (see \ref Commit )
    /// </summary>
    /// <param name="itemName">name of item to add</param>
    /// <param name="quantity">amount to add</param>
    public void Add(string itemName, int quantity)
    {
        for (int i = 0; i < SellEntries.Count; i++)
        {
            if (SellEntries[i].ItemName == itemName)
            {
                SellEntries[i].Quantity += quantity;
                return;
            }
        }

        SellEntries.Add(new TradeOfferItem(new Item(itemName), quantity));
    }

    /// <summary>
    /// called to make a trade happen
    /// </summary>
    /// <returns>true always; ERROR ON FAILURE</returns>
    public bool Commit()
    {
        // Transfer payment for the item
        foreach (TradeOfferItem offer in SellEntries)
        {
            if (!Inventory.Transfer(Buyer.inventory, Seller.inventory, offer.ItemName, offer.Quantity))
            {
                Debug.LogError("Unable to complete a trade, items went missing!");
                return false;
            }
        }

        // Transfer the purchased item
        return Inventory.Transfer(Seller.inventory, Buyer.inventory, BuyEntry.ItemName, BuyEntry.Quantity);
    }

    /// <summary>
    /// comma separated list of \ref SellEntries with quantities
    /// </summary>
    /// <returns>string representation</returns>
    public override string ToString()
    {
        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        for (int i = 0; i < SellEntries.Count; i++)
        {
            builder.Append($"{SellEntries[i].Quantity}x {SellEntries[i].ItemName}, ");
        }

        return builder.ToString().TrimEnd(' ').TrimEnd(',');
    }
}

/// <summary>
/// represents an entry in a <see cref="TradeOffer" /> 
/// </summary>
public class TradeOfferItem
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TradeOfferItem" /> class, used internally by <see cref="TradeOffer(Agent, Agent)"/>
    /// </summary>
    public TradeOfferItem() {}

    /// <summary>
    /// Initializes a new instance of the <see cref="TradeOfferItem" /> class (recommended)
    /// </summary>
    /// <param name="item">name of item</param>
    /// <param name="quantity">amount trading</param>
    public TradeOfferItem(Item item, int quantity)
    {
        this.item = item;
        Quantity = quantity;
    }

    private Item item;
    
    /// <summary>
    /// Gets \ref item 's Id
    /// </summary>
    public int ItemId
    {
        get
        {
            return item.Id;
        }
    }

    /// <summary>
    /// Gets \ref item 's Name
    /// </summary>
    public string ItemName
    {
        get
        {
            return item.Name;
        }
    }
    
    /// <summary>
    /// Gets or sets amount of item
    /// </summary>
    public int Quantity { get; set; }
    
    /// <summary>
    /// Gets \ref item 's Value given \ref Quantity
    /// </summary>
    public float Value
    {
        get
        {
            return item.Value * Quantity;
        }
    }

    /// <summary>
    /// Gets \ref item 's Weight given \ref Quantity
    /// </summary>
    public float Weight
    {
        get
        {
            return item.Weight * Quantity;
        }
    }
}
