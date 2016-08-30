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
            return SellEntries.Sum(x => x.Value * (self.Seller.inventory.Count(self.BuyEntry.ItemName) / Mathf.Max(self.Buyer.inventory.Count(self.BuyEntry.ItemName), .1f)));
        }
    }

    public void Add(string itemName, int quantity)
    {
        for (int i = 0; i < SellEntries.Count; i++)
        {
            if (SellEntries[i].ItemName == itemName)
            {
                SellEntries[i].Increase();
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

public struct TradeOfferItem
{
    public TradeOfferItem(Item item, int quantity)
    {
        ItemId = item.Id;
        ItemName = item.Name;
        Quantity = quantity;
        Weight = item.Weight;
        Value = item.Value;
    }
    
    public int ItemId;
    public string ItemName;
    public int Quantity;
    public float Weight;
    public int Value;

    public void Increase()
    {
        Quantity++;
    }
}
