// <copyright file="TradeOff.cs" company="Mewzor Holdings Inc.">
//     Copyright (c) Mewzor Holdings Inc. All rights reserved.
// </copyright>
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

[System.Serializable]
public struct TradeOffer
{
    public TradeOffer(Agent buyer, Agent seller)
    {
        Buyer = buyer;
        Seller = seller;

        BuyEntry = new TradeOfferItem();
        SellEntries = new List<TradeOfferItem>();
    }

    public Agent Buyer { get; private set; }
    public Agent Seller { get; private set; }
    
    public TradeOfferItem BuyEntry { get; set; }
    public List<TradeOfferItem> SellEntries { get; private set; }

    public float Value
    {
        get
        {
            var self = this;
            return SellEntries.Sum(x => x.Quantity * x.Value * (self.Seller.inventory.Count(x.ItemName) / Mathf.Max(self.Buyer.inventory.Count(x.ItemName), .5f)));
        }
    }

    public void Add(string itemName, int quantity)
    {
        for (int i = 0; i < SellEntries.Count; i++)
        {
            if (SellEntries[i].ItemName == itemName)
            {
                SellEntries[i].Add(quantity);
                return;
            }
        }

        SellEntries.Add(new TradeOfferItem(new Item(itemName), quantity));
    }

    /// <summary>
    /// called to make a trade happen
    /// </summary>
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

public class TradeOfferItem
{
    public TradeOfferItem() {}

    public TradeOfferItem(Item item, int quantity)
    {
        Quantity = quantity;
        CloneFrom(item);
    }

    public void CloneFrom(Item item)
    {
        ItemId = item.Id;
        ItemName = item.Name;
        Weight = item.Weight;
        Value = item.Value;
    }
    
    public int ItemId { get; private set; }
    public string ItemName { get; private set; }
    public int Quantity { get; private set; }
    public float Weight { get; private set; }
    public int Value { get; private set; }

    public void Add(int amount)
    {
        Quantity += amount;
    }
}
