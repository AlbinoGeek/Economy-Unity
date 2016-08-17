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
    }

    /// <summary>
    /// Sets our \ref Destination where we get a new random pont
    /// </summary>
    private void Start()
    {
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
                Rigidbody rigidbody = GetComponent<Rigidbody>();

                rigidbody.isKinematic = true;
                rigidbody.velocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;

                GetComponent<NavMeshAgent>().enabled = false;
                DeathTime = Time.timeSinceLevelLoad;

                // Show a message when an Agent dies
                string cause = calories < 0 ? "Starvation" : hydration < 0 ? "Dehydration" : "Unknown";
                log.Append(string.Format("{0} has died of {1}", name, cause));
            }

            return;
        }

        // If we have met our destination, set a new one
        if (Vector3.Distance(transform.position, Destination) < .5f)
        {
            Destination = map.GetRandomPoint();
            GetComponent<NavMeshAgent>().SetDestination(Destination);
        }
        
        // Consume standard resources
        calories -= .2f;
        hydration -= .4f;

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

        if (Alive && LastActionTime < Time.timeSinceLevelLoad)
        {
            LastActionTime = Time.timeSinceLevelLoad + 1;

            var nearby = GetNeightbors(2);

            // Remove ourselves from the list
            nearby = nearby.Where(agent => agent != this);

            var dead = nearby.Where(agent => !agent.Alive);
            Loot(dead);

            var alive = nearby.Where(agent => agent.Alive);
            Trade(alive);
        }
    }
    #endregion

    private IEnumerable<Agent> GetNeightbors(float range)
    {
        return map.Agents.Where(agent => Vector3.Distance(agent.transform.position, transform.position) < range);
    }

    private void Loot(IEnumerable<Agent> nearbyAgents)
    {
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

            log.Append(string.Format("{0} looted the dead body of {1}", name, other.name));

            // You are only allowed to loot one body per turn
            return;
        }
    }

    private void Trade(IEnumerable<Agent> nearbyAgents)
    {
        // If we have run out of money stop trading
        if (inventory.Find("Money") == null)
        {
            return;
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
                    return;
                }
                
                Item ours = inventory.Find(tradable[j]);
                Item theirs = other.inventory.Find(tradable[j]);

                if (ours != null &&
                    theirs != null &&
                    ours.Quantity < 10 &&
                    theirs.Quantity > 10)
                {
                    inventory.Find("Money").Quantity -= ours.Value;
                    other.inventory.Find("Money").Quantity += ours.Value;

                    inventory.Add(tradable[j], 1);
                    other.inventory.Remove(tradable[j], 1);

                    log.Append(string.Format("{0} bought {1} from {2}", name, tradable[j], other.name));
                }
            }
        }
    }
}
