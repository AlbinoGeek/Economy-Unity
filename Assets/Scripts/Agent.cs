// <copyright file="Agent.cs" company="Mewzor Holdings Inc.">
//     Copyright (c) Mewzor Holdings Inc. All rights reserved.
// </copyright>
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

/// <summary>
/// represents a member of the economy
/// </summary>
[DisallowMultipleComponent]
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed.  We want to have public methods.")]
public class Agent : GlobalBehaviour
{
    private static string[] foods = { "Berry", "Bread", "Apple", "Coconut", "Mango", "Fish (Cooked)" };
    private static string[] cookable =
    {
        "Fish (Raw)",
    };
    
    public Alignment Alignment;

    public House House;

    /// <summary>
    /// represents our forward moving speed, also decides how fast we consume resources
    /// </summary>
    public float MoveSpeed = 3f;

    public float CarryCapacity = 40f;
    
    public Color color = Color.white;

    /// <summary>
    /// food energy
    /// </summary>
    private float calories = 100;

    /// <summary>
    /// water energy
    /// </summary>
    private float hydration = 100;

    /// <summary>
    /// backs property \ref Health
    /// </summary>
    private int health = 10;
    
    /// <summary>
    /// Gets a value indicating whether we will trade or loot
    /// </summary>
    public bool Alive
    {
        get
        {
            return health > 0;
        }
    }

    /// <summary>
    /// Gets our current status, if less than 1 ! \ref Alive
    /// </summary>
    public int Health
    {
        get
        {
            return health;
        }
    }

    public Vector3 Destination { get; private set; }

    /// <summary>
    /// Gets Time.timeSinceLevelLoad when we were created
    /// </summary>
    public float BirthTime { get; private set; }

    /// <summary>
    /// Gets Time.timeSinceLevelLoad when we died
    /// </summary>
    public float DeathTime { get; private set; }

    /// <summary>
    /// Gets Time.timeSinceLevelLoad of our last action
    /// </summary>
    public float LastActionTime { get; private set; }

    /// <summary>
    /// Gets reference to the items we hold
    /// </summary>
    internal Inventory inventory { get; private set; } = new Inventory();

    /// <summary>
    /// helper method to create new Agents
    /// </summary>
    /// <param name="name">name of Agent</param>
    /// <returns>newly created GameObject with Agent</returns>
    public static GameObject Create(string name)
    {
        var prefab = Resources.Load("Player", typeof(GameObject));
        GameObject go = Instantiate(prefab) as GameObject;
        Agent agent = go.AddComponent<Agent>();
        agent.name = name;
        return go;
    }

    /// <summary>
    /// number of actions taken in the last .1 seconds
    /// </summary>
    private int actionsTaken = 0;

    private int loiterTimer = 0;

    private new Rigidbody rigidbody;

    private string lastAction = string.Empty;

    #region Unity
    /// <summary>
    /// Sets default parameters
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        Alignment = (Alignment)Random.Range(0, 9);
        BirthTime = Time.timeSinceLevelLoad;
        actionsTaken = 1;
        
        if (Random.Range(0f, 1f) >= .5f)
        {
            MoveSpeed = 3.2f;
            CarryCapacity = 50f;
        }
        else
        {
            MoveSpeed = 2.9f;
            CarryCapacity = 80f;
        }
    }

    /// <summary>
    /// Sets our \ref Destination where we get a new random pont
    /// </summary>
    private void Start()
    {
        Destination = transform.position;
        rigidbody = GetComponent<Rigidbody>();
        LastActionTime = Time.timeSinceLevelLoad;

        transform.Find("Head").GetComponent<MeshRenderer>().material.color = color;

        InvokeRepeating("Live", .1f, .1f);
    }

    private void FixedUpdate()
    {
        // Dead things don't consume resources
        if (!Alive)
        {
            return;
        }

        if (actionsTaken == 0)
        {
            loiterTimer++;
        }
        else
        {
            loiterTimer = 0;
        }

        // ANTI-STUCK MECHANISM
        if (loiterTimer > 4000)
        {
            log.Append($"Detected that {name} isn't engaging in the Economy, so we gave them hope!", "orange");
            Destination = transform.position;
            MoveTowardsDestination();
            loiterTimer = 0;
        }

        // We lose health if we are losing calories or hydration
        if (calories < 0 || hydration < 0)
        {
            health--;
        }

        // If we currently need water, go to a place near the center of the map.
        if (hydration < 100 && inventory.Count("Water") < 5)
        {
            Destination = map.GetRandomPoint(15);
        }

        // Consume standard resources
        calories -= 2f * Time.fixedDeltaTime;
        hydration -= 3f * Time.fixedDeltaTime;

        if (actionsTaken == 0 && LastActionTime < Time.timeSinceLevelLoad)
        {
            // Reset our last action time to now (moving)
            LastActionTime = Time.timeSinceLevelLoad - 1;

            // Point us towards our destination
            MoveTowardsDestination();
        }

        // Reduce actions taken into busytime
        else if (actionsTaken > 0)
        {
            actionsTaken--;
            LastActionTime += 6f;
        }
    }
    #endregion

    /// <summary>
    /// takes a step in the simulation
    /// </summary>
    private void Live()
    {
        // Dead things don't take turns.
        if (!Alive)
        {
            // We JUST died, let's do something about it.
            if (DeathTime == 0)
            {
                rigidbody.isKinematic = true;
                rigidbody.velocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;

                // We shrivel up and die
                DeathTime = Time.timeSinceLevelLoad;
                transform.localScale /= 2;

                // Show a message when an Agent dies
                string cause = calories < 0 ? "Starvation" : hydration < 0 ? "Dehydration" : "Unknown";
                log.Append($"{name} has died of {cause}", "red");

                color.a = .3f;
                players.UpdateAllCanvas();
            }

            return;
        }

        // Consume food to compensate
        int totalfoods = 0;
        foreach (string food in foods)
        {
            var foodCount = inventory.Count(food);
            totalfoods += foodCount;

            if (calories < 500)
            {
                if (foodCount > 0)
                {
                    Item item = inventory.Find(food);
                    calories += int.Parse(item.GetAttribute("Calories"));
                    inventory.Remove(item.Name, 1);
                }

                actionsTaken++;
            }
        }

        // Consume water to compensate
        if (hydration < 500)
        {
            if (inventory.Remove("Water", 1))
            {
                hydration += 300;
            }
        }

        // Go home if we need water or food, and the base has it
        Inventory baseInventory = House.Base.GetComponent<Base>().Inventory;
        if ((hydration < 500 && inventory.Count("Water") < 5 && baseInventory.Count("Water") > 0) ||
            (calories < 500 && (inventory.Count("Bread") < 5 && baseInventory.Count("Bread") > 0) ||
                               (inventory.Count("Fish (Cooked)") < 1) && baseInventory.Count("Fish (Cooked)") > 0))
        {
            log.Append($"{name} ran to Base in search of" + (hydration < calories ? "Water" : "Food"), "violet");
            Destination = House.BuildArea;
        }

        if (LastActionTime < Time.timeSinceLevelLoad)
        {
            Cook(totalfoods);

            // Remove ourselves from the list
            var nearbyAgents = GetNeightbors(1);
            nearbyAgents = nearbyAgents.Where(agent => agent != this);

            // 1) Try to loot dead bodies
            Loot(nearbyAgents.Where(agent => !agent.Alive));

            // 2) We try to improve our base
            if (actionsTaken == 0)
            {
                if (Vector3.Distance(transform.position, House.BuildArea) < 1f)
                {
                    if (inventory.Count("Log") > 0)
                    {
                        log.Append($"{name} USED 1x Log to improve Base!", "violet", LogEntry.MessageType.Construction);
                        Inventory.Transfer(inventory, House.Base.GetComponent<Base>().Inventory, "Log", 1);
                        actionsTaken++;
                        return;
                    }

                    // 3) If we need something from the base, take it.
                    if (calories < 500)
                    {
                        foreach (string food in foods)
                        {
                            if (baseInventory.Count(food) > 0)
                            {
                                log.Append($"{name} TOOK 1x {food} from Base to survive!", "violet", LogEntry.MessageType.Gather);
                                Inventory.Transfer(baseInventory, inventory, food, 1);
                                actionsTaken++;
                                return;
                            }
                        }
                    }

                    if (hydration < 500)
                    {
                        if (baseInventory.Count("Water") > 1)
                        {
                            log.Append($"{name} TOOK 1x Water from Base to survive!", "violet", LogEntry.MessageType.Gather);
                            Inventory.Transfer(baseInventory, inventory, "Water", Mathf.Min(5, baseInventory.Count("Water")));
                            actionsTaken++;
                            return;
                        }
                    }

                    // 4) Or contribute extra resources to the Stockpile
                    for (int i = 0; i < inventory.Items.Count; i++)
                    {
                        if (inventory.Items[i].Quantity > 20 || (inventory.Items[i].Weight * inventory.Items[i].Quantity) > 30)
                        {
                            int count = Mathf.Min(inventory.Items[i].Quantity, 5);
                            Inventory.Transfer(inventory, House.Base.GetComponent<Base>().Inventory, inventory.Items[i].Name, count);
                            log.Append($"{name} STORED {count}x {inventory.Items[i].Name} in Base!", "violet", LogEntry.MessageType.Construction);
                            actionsTaken++;
                            return;
                        }
                    }
                }
                else if (inventory.Count("Log") > 0 || inventory.Weight >= CarryCapacity)
                {
                    // Don't override a need for food or water
                    if (calories > 100 && hydration > 100)
                    {
                        Destination = House.BuildArea;
                    }
                }
            }

            // 5) Try to collect resources from the environment
            if (actionsTaken == 0)
            {
                CollectFrom(GetNearbyProviders(2));
            }

            if (actionsTaken > 0)
            {
                lastAction = "Gather";
            }

            // 6) Try to trade with people near us
            if (actionsTaken == 0)
            {
                var alive = nearbyAgents.Where(agent => agent.Alive);
                if (lastAction != "Trade")
                {
                    TradeWith(alive);
                }

                if (actionsTaken > 0)
                {
                    //move us towards the destination in question, if we're facing it.
                    rigidbody.velocity += 8f * transform.TransformDirection(Vector3.forward * Time.fixedDeltaTime * MoveSpeed);
                    return;
                }

                // 7) If we are getting desperate; and our alignment allows it, try to steal...
                if (Alignment != Alignment.LawfulGood && Alignment != Alignment.LawfulNeutral)
                {
                    if ((totalfoods < 5 && calories < 500) ||
                        (inventory.Count("Water") < 5 && hydration < 500))
                    {
                        StealFrom(alive);
                    }
                }
            }
        }
    }

    private IEnumerable<Agent> GetNeightbors(float range)
    {
        return map.Agents.Where(agent => Vector3.Distance(agent.transform.position, transform.position) < range);
    }

    private IEnumerable<Provider> GetNearbyProviders(float range)
    {
        return map.Providers.Where(@object => Vector3.Distance(@object.transform.position, transform.position) < range);
    }

    private void MoveTowardsDestination()
    {
        if (Vector3.Distance(transform.position, Destination) > 1f)
        {
            //find the vector pointing from our position to the target
            Vector3 direction = (Destination - transform.position).normalized / 4;

            //create the rotation we need to be in to look at the target
            Quaternion lookRotation = Quaternion.LookRotation(direction);

            //rotate us over time according to speed until we are in the required rotation
            transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.fixedDeltaTime * 4);

            //move us towards the destination in question, if we're facing it.
            rigidbody.velocity += 5f * transform.TransformDirection(Vector3.forward * Time.fixedDeltaTime * MoveSpeed);

            // Consume standard resources
            calories -= 4f * Time.fixedDeltaTime * rigidbody.velocity.magnitude;
            hydration -= 6f * Time.fixedDeltaTime * rigidbody.velocity.magnitude;
        }

        // If we have met our destination, set a new one
        else
        {
            float radius = 20;
            while (true)
            {
                radius--;

                if (radius < 3)
                {
                    radius = 20;
                }

                Vector2 position = Random.insideUnitCircle * radius;
                Destination = new Vector3(position.x + transform.position.x, 0, position.y + transform.position.z);

                if (map.IsInMapBounds(Destination) && map.IsValidPosition(Destination + (Vector3.up / 2f)))
                {
                    break;
                }
                else
                {
                    var provider = map.Providers.Where(x => Vector3.Distance(x.transform.position, transform.position) < 2f).FirstOrDefault();
                    if (provider != null)
                    {
                        Destination = provider.transform.position;
                        break;
                    }
                }
            }
        }
    }

    // ACTIONS BELOW THIS POINT

    private void Cook(int totalfoods)
    {
        Item toCook = inventory.Items.FirstOrDefault(x => cookable.Contains(x.Name));

        // If we don't want to cook; don't.
        if (totalfoods > 10 || toCook == null)
        {
            return;
        }

        // Find nearest fire
        Provider fire = map.Providers.Where(x => x.gameObject.name == "Tile_Fire")
                                     .Where(x => Vector3.Distance(x.transform.position, transform.position) < 8f)
                                     .OrderBy(x => Vector3.Distance(x.transform.position, transform.position))
                                     .FirstOrDefault();
        
        // If we couldn't find a fire, we should make one
        if (fire == null)
        {
            bool haveSticks = inventory.Count("Stick") > 1;
            bool haveBranch = inventory.Count("Branch") > 0;

            // If we need to create sticks ...
            if (!haveSticks && haveBranch)
            {
                // Crafting counts as two actions
                actionsTaken++;

                // Consume a branch and create 1-2 sticks
                int count = Random.Range(1, 3);
                inventory.Remove("Branch", 1);
                inventory.Add("Stick", count);
                log.Append($"{name} CRAFTED {count}x Sticks", "violet");
            }

            // If we can make a fire, and we still haven't acted
            if (haveSticks && actionsTaken == 0)
            {
                actionsTaken++;

                // One stick is consume per success or failure
                inventory.Remove("Stick", 1);

                // 10% success rate
                if (Random.value > .9f)
                {
                    // Create a fire in front of us
                    GameObject go = CreateResource("Fire", transform.position + transform.TransformDirection(Vector3.forward), transform.rotation);
                    map.Providers.Add(go.GetComponent<Provider>());
                    log.Append($"{name} CRAFTED A FIRE", "violet");
                }
            }
            
            return;
        }
        
        // We also need something to cook with
        Furnace furnace = fire.GetComponent<Furnace>();
        if (furnace != null && actionsTaken == 0)
        {
            if (Vector3.Distance(transform.position, fire.transform.position) < 1f)
            {
                // Add more fuel to continue cooking
                while (furnace.Fuel <= 5 && furnace.Fuel * 10 <= furnace.Inventory.Items.Count)
                {
                    Item cookWith = inventory.Find("Log") ?? inventory.Find("Branch") ?? inventory.Find("Stick") ?? inventory.Find("Twig");
                    if (cookWith == null)
                    {
                        break;
                    }

                    log.Append($"{name} added {cookWith.Name} as fuel to {fire.name}.", "cyan");
                    furnace.Fuel += int.Parse(cookWith.GetAttribute("Fuel"));
                    inventory.Remove(cookWith.Name, 1);
                    actionsTaken++;
                }

                // If there is space in the furnace, add our item
                actionsTaken += 2;
                log.Append($"{name} started cooking {toCook.Name} in {fire.name}.", "cyan");
                Inventory.Transfer(inventory, furnace.Inventory, toCook.Name, 1);
                inventory.Remove(toCook.Name, 1);
            }
            else
            {
                // Re-set our destination to the fire
                Destination = fire.transform.position;
            }
        }
    }

    private void CollectFrom(IEnumerable<Provider> nearbyProviders)
    {
        for (int i = 0; i < nearbyProviders.Count(); i++)
        {
            Provider provider = nearbyProviders.ElementAt(i);

            // Don't collect if inventory full
            if (inventory.Weight > CarryCapacity)
            {
                continue;
            }

            // If the object isn't interactable, then ... go next
            if (!provider)
            {
                continue;
            }
            
            // Takes item from provider as per its rules
            var drop = provider.GetDrop();
            if (drop != null)
            {
                int amount = drop.StockPerUse > drop.ItemStock ? drop.StockPerUse : drop.ItemStock;
                inventory.Add(drop.ItemName, amount);
                drop.ItemStock -= amount;
                actionsTaken++;

                log.Append($"{name} collected {drop.ItemName} x{drop.StockPerUse} from {provider.gameObject.name}", "#66CC66", LogEntry.MessageType.Gather);
                return;
            }
        }
    }

    private void Loot(IEnumerable<Agent> nearbyAgents)
    {
        Agent other = nearbyAgents.FirstOrDefault();
        if (other == null)
        {
            return;
        }

        string stuff = other.inventory.ToString();

        // Take everything from their body!
        for (int j = 0; j < other.inventory.Items.Count; j++)
        {
            Item theirs = other.inventory.Items[j];
            if (theirs != null)
            {
                inventory.Add(theirs.Name, theirs.Quantity);
                other.inventory.Remove(theirs.Name, theirs.Quantity);
            }
        }

        actionsTaken++;

        // Remove body from game
        map.Agents.Remove(other);
        Destroy(other.gameObject);

        log.Append($"{name} looted the dead body of {other.name}; gaining {stuff}", "orange", LogEntry.MessageType.Gather);
    }

    private void TradeWith(IEnumerable<Agent> nearbyAgents)
    {
        for (int i = 0; i < nearbyAgents.Count(); i++)
        {
            Agent other = nearbyAgents.ElementAt(i);

            // Try to find an item we want
            Item theirs = other.inventory.Items.OrderBy(x => System.Guid.NewGuid())
                                               .FirstOrDefault();
            
            if (theirs != null)
            {
                // Don't buy their last food or water
                if (theirs.Quantity < 10 &&
                   (theirs.Name == "Bread" ||
                    theirs.Name == "Water"))
                {
                    continue;
                }
                
                // We don't want something we have too much of
                if (inventory.Count(theirs.Name) > 10)
                {
                    continue;
                }
                
                // Item Base Cost * appraised value modifier (Seller Supply vs Buyer Supply)
                float cost = theirs.Value * (theirs.Quantity / Mathf.Max(inventory.Count(theirs.Name), .5f));

                // Don't buy something that's over 250% marked up
                if (cost > theirs.Value * 2.5f)
                {
                    continue;
                }

                // We trade at 45% off for our own family members
                if (House == other.House)
                {
                    cost *= .55f;
                }

                TradeOffer trade = new TradeOffer(this, other);
                trade.BuyEntry = new TradeOfferItem(theirs, 1);
                
                // Add items until we hit expected value
                foreach (Item item in inventory.Items)
                {
                    // Don't trade the item we want
                    if (item.Name == theirs.Name)
                    {
                        continue;
                    }

                    // Add items one by one until the value is acceptable
                    int trading = 0;
                    int total = item.Quantity;

                    // Account for the special case that this is Food or Water
                    // - We won't trade away our last 5 of these things
                    if (item.Name == "Bread" || item.Name == "Water")
                    {
                        total -= 5;
                    }
                    
                    if (total > 0)
                    {
                        while (trade.Value <= cost)
                        {
                            if (trading >= total)
                            {
                                break;
                            }

                            trade.Add(item.Name, 1);
                            trading++;
                        }
                    }

                    // We can stop adding items
                    if (trade.Value >= cost)
                    {
                        break;
                    }
                }

                if (trade.Value <= cost || trade.SellEntries.Count == 0)
                {
                    // We can't trade!
                    return;
                }

                if (trade.Commit())
                {
                    string tmp = House.Name == other.House.Name ? "HOUSEMATE" : "STRANGER";
                    log.Append($"{name} bought 1x {trade.BuyEntry.ItemName} for {trade.ToString()} from {tmp} {other.name}", "#9999FF", LogEntry.MessageType.Trade);

                    lastAction = "Trade";
                    actionsTaken++;
                    return;
                }
            }
        }
    }
    
    private void StealFrom(IEnumerable<Agent> nearbyAgents)
    {
        // Attempt to steal food or water, whatever we need most.
        string thingToSteal = inventory.Count("Bread") < inventory.Count("Water") ? "Bread" : "Water";

        Agent other = nearbyAgents.FirstOrDefault();
        if (other == null)
        {
            // We didn't find anyone to steal from
            return;
        }

        Item theirs = other.inventory.Find(thingToSteal);
        if (theirs == null)
        {
            // We didn't find what we wanted to steal
            return;
        }

        // Did we succeed? (30%)
        if (Random.value > .7f)
        {
            inventory.Add(thingToSteal, 1);
            other.inventory.Remove(thingToSteal, 1);

            log.Append($"{name} STOLE {thingToSteal} from {other.name}", "yellow");
        }
        else
        {
            // We failed, and they hurt us.
            Item weapon = other.inventory.Find("Knife");
            int damage = 2;
            if (weapon != null)
            {
                var str = weapon.GetAttribute("Damage");
                var dmg = str.Split('~');
                damage *= Random.Range(int.Parse(dmg[0]), int.Parse(dmg[1]));
            }

            string weaponName = (weapon == null ? "Fist" : weapon.Name);
            log.Append($"{name} TOOK {damage} damage from {other.name}'s {weaponName} by FAILING to steal!", "#FF3333", LogEntry.MessageType.Combat);
            health -= damage;
        }

        //move us towards the destination in question, if we're facing it.
        rigidbody.velocity += 10f * transform.TransformDirection(Vector3.forward * Time.fixedDeltaTime * MoveSpeed);
        actionsTaken++;
    }

    public override string ToString()
    {
        return $"F: {inventory.Count("Berry") + inventory.Count("Bread")} | W: {inventory.Count("Water")} | {name}";
    }
}
