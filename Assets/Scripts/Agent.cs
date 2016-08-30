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
    private static string[] tradable = {
        "Bread", "Berry", "Apple", "Coconut", "Mango", "Water", 
        "Log", "Plank", "Leaf", "Stick",
        "Stone (Flat)", "Stone (Round)",
        "Fish (Raw)", "Fish (Cooked)",
        //// "Knife", "Stone (Chipped)",
    };
    private static string[] cookable =
    {
        "Fish (Raw)",
    };

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

    private new Rigidbody rigidbody;

    #region Unity
    /// <summary>
    /// Sets default parameters
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        BirthTime = Time.timeSinceLevelLoad;
        
        if (Random.Range(0f, 1f) >= .5f)
        {
            MoveSpeed = 3.2f;
            CarryCapacity = 30f;
        }
        else
        {
            MoveSpeed = 2.8f;
            CarryCapacity = 50f;
        }
    }

    /// <summary>
    /// Sets our \ref Destination where we get a new random pont
    /// </summary>
    private void Start()
    {
        Destination = transform.position;
        rigidbody = GetComponent<Rigidbody>();

        transform.Find("Head").GetComponent<MeshRenderer>().material.color = color;

        InvokeRepeating("Live", .1f, .1f);
    }

    private int actionsTaken = 0;

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
                players.UpdateCanvas(this);
            }

            return;
        }

        // If we have met our destination, set a new one
        if (Vector3.Distance(transform.position, Destination) < 1.5f)
        {
            while (true)
            {
                Vector2 position = Random.insideUnitCircle * 10f;
                Destination = new Vector3(position.x + transform.position.x, 0, position.y + transform.position.z);

                if (map.IsInMapBounds(Destination))
                {
                    break;
                }
                else
                {
                    var provider = map.Providers.Where(x => Vector3.Distance(x.transform.position, transform.position) < 3f).FirstOrDefault();
                    if (provider != null)
                    {
                        Destination = provider.transform.position;
                        break;
                    }
                }
            }
        }

        // Consume food to compensate
        int totalfoods = 0;
        if (calories < 50)
        {
            foreach (string food in foods)
            {
                var foodCount = inventory.Count(food);
                totalfoods += foodCount;
                if (foodCount > 0)
                {
                    Item item = inventory.Find(food);
                    calories += int.Parse(item.GetAttribute("Calories"));
                    inventory.Remove(item.Name, 1);
                }
            }
        }

        // Consume water to compensate
        if (hydration < 50)
        {
            if (inventory.Remove("Water", 1))
            {
                hydration += 150;
            }
        }

        // Count cookable items
        actionsTaken = 0;

        if (Alive && LastActionTime < Time.timeSinceLevelLoad)
        {
            Cook(totalfoods);

            var nearbyAgents = GetNeightbors(2);

            // Remove ourselves from the list
            nearbyAgents = nearbyAgents.Where(agent => agent != this);

            // 1) Try to loot dead bodies
            var dead = nearbyAgents.Where(agent => !agent.Alive);
            actionsTaken += Loot(dead);

            // 2) Try to collect resources from the environment
            if (actionsTaken == 0)
            {
                var nearbyProviders = GetNearbyProviders(2);
                actionsTaken += CollectFrom(nearbyProviders);
            }

            // 3) Try to trade with people near us
            if (actionsTaken == 0)
            {
                var alive = nearbyAgents.Where(agent => agent.Alive);
                actionsTaken += TradeWith(alive);

                // 4) If we are getting desperate, try to steal...
                if (actionsTaken == 0 &&
                    inventory.Count("Money") < 4 &&
                    (inventory.Count("Bread") < 3 || inventory.Count("Water") < 5))
                {
                    actionsTaken += StealFrom(alive);
                }
            }

            if (actionsTaken > 0)
            {
                LastActionTime = Time.timeSinceLevelLoad + 6;
            }
        }
    }

    private void FixedUpdate()
    {
        // Dead things don't consume resources
        if (!Alive)
        {
            return;
        }

        // We lose health if we are losing calories or hydration
        if (calories < 0 || hydration < 0)
        {
            health--;
        }

        // Consume standard resources
        calories -= 6f * Time.fixedDeltaTime;
        hydration -= 4f * Time.fixedDeltaTime;

        // If we didn't take an action, move.
        if (actionsTaken == 0 &&
            LastActionTime - 3f < Time.timeSinceLevelLoad)
        {
            // Point us towards our destination
            MoveTowardsDestination();
        }
    }
    #endregion

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
        if (Vector3.Distance(transform.position, Destination) > .5f)
        {
            //find the vector pointing from our position to the target
            Vector3 direction = (Destination - transform.position).normalized / 4;

            //create the rotation we need to be in to look at the target
            Quaternion lookRotation = Quaternion.LookRotation(direction);

            //rotate us over time according to speed until we are in the required rotation
            transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.fixedDeltaTime * 4);

            //move us towards the destination in question, if we're facing it.
            rigidbody.velocity += 5f * transform.TransformDirection(Vector3.forward * Time.fixedDeltaTime * MoveSpeed);

            // If there's something in front of us we can't travel on, strafe a bit
            if (Physics.Raycast(new Ray(transform.position, transform.TransformDirection(Vector3.forward)), .5f))
            {
                // Try to go left
                if (!Physics.Raycast(new Ray(transform.position, transform.TransformDirection(Vector3.left)), .5f))
                {
                    transform.position += transform.TransformDirection(Vector3.left) / 5f;
                }

                // Try to go right
                if (!Physics.Raycast(new Ray(transform.position, transform.TransformDirection(Vector3.right)), .5f))
                {
                    transform.position += transform.TransformDirection(Vector3.right) / 5f;
                }
            }

            // Consume standard resources
            calories -= 6f * Time.fixedDeltaTime * rigidbody.velocity.magnitude;
            hydration -= 4f * Time.fixedDeltaTime * rigidbody.velocity.magnitude;
        }
    }

    // ACTIONS BELOW THIS POINT

    private void Cook(int totalfoods)
    {
        var cooklist = inventory.Items.Where(x => cookable.Contains(x.Name));
        int countCookable = cooklist.Sum(x => x.Quantity);

        // Find nearest fire
        Provider fire = map.Providers.Where(x => x.gameObject.name == "Tile_Fire")
                                     .OrderBy(x => Vector3.Distance(x.transform.position, transform.position))
                                     .FirstOrDefault();

        // We're low on food, and have something to cook
        if (totalfoods < 10 && countCookable > 0)
        {
            // If we couldn't find a fire, we should make one
            if (fire == null)
            {
                bool haveSticks = inventory.Count("Stick") > 1;
                bool haveBranch = inventory.Count("Branch") > 0;

                // If we need to create sticks ...
                if (!haveSticks && haveBranch)
                {
                    // Crafting counts as two actions
                    actionsTaken += 2;

                    // Consume a branch and create 1-2 sticks
                    int count = Random.Range(1, 3);
                    inventory.Remove("Branch", 1);
                    inventory.Add("Stick", count);
                    log.Append($"{name} CRAFTED {count}x Sticks", "green");
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
                        log.Append($"{name} CRAFTED A FIRE", "orange");
                    }
                }
            }
        }

        // If we have something to cook
        Item toCook = cooklist.FirstOrDefault();

        // We also need something to cook with
        Item cookWith = inventory.Find("Stick") ?? inventory.Find("Branch");
        if (cookWith == null)
        {
            cookWith = inventory.Find("Log");
        }

        Furnace furnace = null;
        if (fire != null)
        {
            furnace = fire.GetComponent<Furnace>();
        }

        if (furnace != null && actionsTaken == 0 &&
            (toCook != null || cookWith != null))
        {
            // Re-set our destination to the fire
            Destination = fire.transform.position;

            if (Vector3.Distance(transform.position, fire.transform.position) < 1f)
            {
                // Cooking counts as 4 actions
                actionsTaken += 2;

                // If there is space in the furnace, add our item
                if (toCook != null && furnace.Contents.Count <= 2)
                {
                    furnace.Add(Item.Clone(toCook));
                    inventory.Remove(toCook.Name, 1);
                    log.Append($"{name} started cooking {toCook.Name} in {fire.name}.", "cyan");
                }

                // Add more fuel to continue cooking
                if (cookWith != null && furnace.Fuel < 10 && furnace.Contents.Count > 0)
                {
                    furnace.Fuel += (5 * int.Parse(cookWith.GetAttribute("Fuel")));
                    inventory.Remove(cookWith.Name, 1);
                    log.Append($"{name} added {cookWith.Name} as fuel to {fire.name}.", "cyan");
                }
            }
        }
    }

    private int CollectFrom(IEnumerable<Provider> nearbyProviders)
    {
        int count = 0;
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

            count++;

            // Takes item from provider as per its rules
            Provider.DropEntry drop = provider.GetDrop();
            if (drop != null)
            {
                inventory.Add(drop.ItemName, drop.StockPerUse);
                log.Append($"{name} collected {drop.ItemName} x{drop.StockPerUse} from {provider.gameObject.name}", "#66CC66");
            }

            return count;
        }
        return count;
    }

    private int Loot(IEnumerable<Agent> nearbyAgents)
    {
        int count = 0;
        for (int i = 0; i < nearbyAgents.Count(); i++)
        {
            Agent other = nearbyAgents.ElementAt(i);

            // Check if body has already been looted
            if (other.inventory.Items.Count == 0)
            {
                continue;
            }

            count++;

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

            // Remove body from game
            map.Agents.Remove(other);
            Destroy(other.gameObject);

            log.Append($"{name} looted the dead body of {other.name}; gaining {stuff}", "yellow");

            // You are only allowed to loot one body per turn
            return count;
        }

        return count;
    }

    private int TradeWith(IEnumerable<Agent> nearbyAgents)
    {
        int count = 0;

        // If we have run out of money stop trading
        if (inventory.Count("Money") <= 1)
        {
            return count;
        }

        for (int i = 0; i < nearbyAgents.Count(); i++)
        {
            Agent other = nearbyAgents.ElementAt(i);

            // Try to trade each item
            for (int j = 0; j < tradable.Length; j++)
            {
                // If we have run out of money stop trading
                if (inventory.Count("Money") <= 1)
                {
                    return count;
                }

                Item ours = inventory.Find(tradable[j]);
                Item theirs = other.inventory.Find(tradable[j]);

                if (ours != null &&
                    theirs != null &&
                    ours.Quantity < 5)
                {
                    // Don't trade away your last X
                    if ((tradable[j] == "Bread" ||
                        tradable[j] == "Water") &&
                        theirs.Quantity < 5)
                    {
                        continue;
                    }

                    float cost = ours.Value * 10;

                    // Scale it based on supply vs demand + random
                    cost *= Mathf.Clamp(1f + (ours.Quantity / theirs.Quantity), .1f, 2.1f);

                    // Don't trade when over 200% cost
                    if (cost > (ours.Value * 10 * 2))
                    {
                        continue;
                    }

                    // Don't trade where we can't afford it
                    if (inventory.Count("Money") < cost)
                    {
                        continue;
                    }
                    
                    count++;

                    inventory.Find("Money").Quantity -= (int) cost;
                    other.inventory.Find("Money").Quantity += (int) cost;

                    inventory.Add(tradable[j], 1);
                    other.inventory.Remove(tradable[j], 1);

                    log.Append($"{name} bought {tradable[j]} from {other.name}", "#9999FF");

                    //move us towards the destination in question, if we're facing it.
                    rigidbody.velocity += 20f * transform.TransformDirection(Vector3.forward * Time.fixedDeltaTime * MoveSpeed);
                }
            }
        }

        return count;
    }
    
    private int StealFrom(IEnumerable<Agent> nearbyAgents)
    {
        if (nearbyAgents.Count() == 0)
        {
            return 0;
        }

        Agent other = nearbyAgents.ElementAt(0);

        // Attempt to steal food or water, whatever we need most.
        string thingToSteal = string.Empty;
        if (inventory.Count("Bread") < inventory.Count("Water"))
        {
            thingToSteal = "Bread";
        }
        else
        {
            thingToSteal = "Water";
        }

        Item theirs = other.inventory.Find(thingToSteal);
        if (theirs == null)
        {
            // They didn't have what we wanted to steal.
            return 0;
        }

        // Did we succeed?
        if (Random.Range(0, 1f) > .7f)
        {
            // We succeeded!
            inventory.Add(thingToSteal, 1);
            other.inventory.Remove(thingToSteal, 1);

            log.Append($"{name} STOLE {thingToSteal} from {other.name}", "yellow");
        }
        else
        {
            // We failed, and they hurt us.
            log.Append($"{name} FAILED TO STEAL from {other.name}", "orange");

            Item weapon = other.inventory.Find("Knife");
            if (weapon != null)
            {
                var str = weapon.GetAttribute("Damage");
                var dmg = str.Split('~');
                int damage = Random.Range(int.Parse(dmg[0]), int.Parse(dmg[1]));
                
                log.Append($"{name} TOOK {damage} damage from {other.name}'s {weapon.Name}!", "#99FFF");
                health -= damage;
            }
            else
            {
                health--;
            }
        }

        //move us towards the destination in question, if we're facing it.
        rigidbody.velocity += 20f * transform.TransformDirection(Vector3.forward * Time.fixedDeltaTime * MoveSpeed);

        return 1;
    }

    public override string ToString()
    {
        return $"$: {inventory.Count("Money")} | F: {inventory.Count("Berry") + inventory.Count("Bread")} | W: {inventory.Count("Water")} | {name}";
    }
}
