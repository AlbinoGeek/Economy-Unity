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
public class Agent : MonoBehaviour
{
    /// <summary>
    /// represents our forward moving speed, also decides how fast we consume resources
    /// </summary>
    public float moveSpeed = 3f;

    /// <summary>
    /// food energy
    /// </summary>
    private float calories;

    /// <summary>
    /// water energy
    /// </summary>
    private float hydration;

    /// <summary>
    /// backs property \ref Health
    /// </summary>
    private int health;

    /// <summary>
    /// reference to activity log
    /// </summary>
    private ActivityLog log;
    
    private MapController map;
    
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
    internal Inventory inventory { get; private set; }

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

    private PlayerList players;

    private new Rigidbody rigidbody;

    #region Unity
    /// <summary>
    /// Sets default parameters
    /// </summary>
    private void Awake()
    {
        BirthTime = Time.timeSinceLevelLoad;
        inventory = new Inventory();
        calories = 100;
        hydration = 100;
        health = 5;

        log = GameObject.Find("Activity").GetComponent<ActivityLog>();
        map = GameObject.Find("Map").GetComponent<MapController>();
        players = GameObject.Find("Player List").GetComponent<PlayerList>();
    }

    /// <summary>
    /// Sets our \ref Destination where we get a new random pont
    /// </summary>
    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        Destination = transform.position;
    }

    /// <summary>
    /// takes a step in the simulation
    /// </summary>
    private void FixedUpdate()
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
                log.Append(string.Format("{0} has died of {1}", name, cause));
            }

            return;
        }

        // If we have met our destination, set a new one
        if (Vector3.Distance(transform.position, Destination) < 1f)
        {
            Destination = map.GetRandomPoint();
        }

        // Consume food to compensate
        if (calories < 50)
        {
            if (inventory.Remove("Bread", 1))
            {
                calories += 50;
            }
        }

        // Consume water to compensate
        if (hydration < 50)
        {
            if (inventory.Remove("Water", 1))
            {
                hydration += 50;
            }
        }

        // We lose health if we are losing calories or hydration
        if (calories < 0 || hydration < 0)
        {
            health--;
        }

        int actionsTaken = 0;
        if (Alive && LastActionTime < Time.timeSinceLevelLoad)
        {
            LastActionTime = Time.timeSinceLevelLoad + 1;

            var nearbyAgents = GetNeightbors(2);

            // Remove ourselves from the list
            nearbyAgents = nearbyAgents.Where(agent => agent != this);
            
            var dead = nearbyAgents.Where(agent => !agent.Alive);
            actionsTaken += Loot(dead);

            // You can only loot OR trade ...
            if (actionsTaken == 0)
            {
                var alive = nearbyAgents.Where(agent => agent.Alive);
                actionsTaken += Trade(alive);
            }

            // tradee OR "resource action" per turn
            if (actionsTaken == 0)
            {
                var nearbyProviders = GetNearbyProviders(2);
                CollectFrom(nearbyProviders);
            }
        }

        // If we didn't take an action, move.
        if (actionsTaken == 0)
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
            transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.fixedDeltaTime * 6);

            //move us towards the destination in question, if we're facing it.
            transform.position += transform.TransformDirection(Vector3.forward * Time.fixedDeltaTime * moveSpeed);
            
            // Consume standard resources
            calories -= 5f * Time.fixedDeltaTime * moveSpeed;
            hydration -= 10f * Time.fixedDeltaTime * moveSpeed;
        }
    }

    private int CollectFrom(IEnumerable<Provider> nearbyProviders)
    {
        int count = 0;
        for (int i = 0; i < nearbyProviders.Count(); i++)
        {
            Provider provider = nearbyProviders.ElementAt(i);

            // If the object isn't interactable, then ... go next
            if (!provider)
            {
                continue;
            }

            // Takes item from provider as per its rules
            inventory.Add(provider.ItemName, provider.StockPerUse);
            provider.ItemStock -= provider.StockPerUse;

            log.Append(string.Format("{0} collected {1}x {2} from {3}", name, provider.ItemName, provider.StockPerUse, provider.gameObject.name));
            provider.ReconsiderLife();

            return count;
        }
        return count;
    }

    private int Loot(IEnumerable<Agent> nearbyAgents)
    {
        int count = 0;
        string[] lootable = { "Money", "Bread", "Water" };
        for (int i = 0; i < nearbyAgents.Count(); i++)
        {
            Agent other = nearbyAgents.ElementAt(i);

            // Check if body has already been looted
            if (other.inventory.Find("Money") == null &&
                other.inventory.Find("Bread") == null &&
                other.inventory.Find("Water") == null)
            {
                continue;
            }

            // Take everything from their body!
            for (int j = 0; j < lootable.Length; j++)
            {
                Item theirs = other.inventory.Find(lootable[j]);
                if (theirs != null)
                {
                    inventory.Add(lootable[j], theirs.Quantity);
                    other.inventory.Remove(lootable[j], theirs.Quantity);
                }
            }
            count++;

            // Remove body from game
            map.RemoveAgent(other);
            Destroy(other.gameObject);

            log.Append(string.Format("{0} looted the dead body of {1}", name, other.name));

            // You are only allowed to loot one body per turn
            return count;
        }

        return count;
    }

    private int Trade(IEnumerable<Agent> nearbyAgents)
    {
        int count = 0;

        // If we have run out of money stop trading
        if (inventory.Find("Money") == null)
        {
            return count;
        }

        string[] tradable = { "Bread", "Water" };
        for (int i = 0; i < nearbyAgents.Count(); i++)
        {
            Agent other = nearbyAgents.ElementAt(i);

            // Try to trade each item
            for (int j = 0; j < tradable.Length; j++)
            {
                // If we have run out of money stop trading
                if (inventory.Find("Money") == null)
                {
                    return count;
                }
                
                Item ours = inventory.Find(tradable[j]);
                Item theirs = other.inventory.Find(tradable[j]);

                if (ours != null &&
                    theirs != null &&
                    ours.Quantity < 5 &&
                    theirs.Quantity > 10)
                {
                    if (inventory.Find("Money").Quantity < ours.Value)
                    {
                        continue;
                    }

                    inventory.Find("Money").Quantity -= ours.Value;
                    other.inventory.Find("Money").Quantity += ours.Value;

                    inventory.Add(tradable[j], 1);
                    other.inventory.Remove(tradable[j], 1);
                    count++;

                    log.Append(string.Format("{0} bought {1} from {2}", name, tradable[j], other.name));
                }
            }
        }

        return count;
    }
    
    public override string ToString()
    {
        var money = inventory.Find("Money");
        var food = inventory.Find("Bread");
        var water = inventory.Find("Water");
        
        return string.Format("$: {1} F: {2} | W: {3} | {0}",
            name,
            money != null ? money.Quantity : 0,
            food != null ? food.Quantity : 0,
            water != null ? water.Quantity : 0);
    }
}
