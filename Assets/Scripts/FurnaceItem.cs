// <copyright file="FurnaceItem.cs" company="Mewzor Holdings Inc.">
//     Copyright (c) Mewzor Holdings Inc. All rights reserved.
// </copyright>

[System.Serializable]
public class FurnaceItem
{
    public FurnaceItem(Item item)
    {
        Item = item;
        Cooked = 0;
    }

    public Item Item { get; private set; }
    public float Cooked { get; private set; }

    public void Cook()
    {
        Cooked++;
    }

    public void Destroy()
    {
        Item = null;
        Cooked = -1;
    }
}
