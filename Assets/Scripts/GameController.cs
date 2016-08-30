// <copyright file="GameController.cs" company="Mewzor Holdings Inc.">
//     Copyright (c) Mewzor Holdings Inc. All rights reserved.
// </copyright>
using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// handles the flow logic of the game
/// </summary>
[DisallowMultipleComponent]
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed.  We want to have public methods.")]
public class GameController : GlobalBehaviour
{
    /// <summary>
    /// list of players to be created at the start of the game
    /// </summary>
    private static string[] population =
    {
        "AngryAlbino",
        "HappyGiant",
        "JohnGeese",
        "MewzorMewMew",
        "StabbyGaming",
        "Raiden",
        "b4u7",
        "Big Hoss",
        "divideddarko",
        "deccer",
        "E.B.",
        "GoodGuyWill",
        "HD",
        "human_supremacist",
        "Le Chat",
        "Malscythe",
        "matt",
        "Mirage",
        "Mobilpadde",
        "Movariant",
        "Prxy",
        "RobbieW",
        "romgerman",
        "SadCloud123",
        "sean",
        "Sense",
        "Westermin",
        "WildNoob",
        "wubbalubbadubdub",
        "vassvik",
    };
    
    private float timeSinceLastEvent;
    
    #region Unity
    protected override void Awake()
    {
        base.Awake();

        timeSinceLastEvent = Time.timeSinceLevelLoad;
    }

    /// <summary>
    /// begins a game, initializing things as required
    /// </summary>
    private void Start()
    {
        StartCoroutine("CreateAgents");
    }

    private IEnumerator CreateAgents()
    {
        GameObject agentContainer = new GameObject("Agents");

        float hue = 0f;
        float inc = 1f / population.Length;
        bool even = true;
        var pop = population.OrderBy(x => x.Length).Reverse();
        foreach (string name in pop)
        {
            GameObject go = Agent.Create(name);
            Agent agent = go.GetComponent<Agent>();
            go.transform.parent = agentContainer.transform;

            // Position Agents randomly on the map
            go.transform.position = map.GetRandomPoint();

            // Give them a fair amount of resources
            agent.inventory.Add("Money", Random.Range(50, 50));
            agent.inventory.Add("Bread", Random.Range(10, 10));
            agent.inventory.Add("Water", Random.Range(20, 20));

            // Generate a random color for this agent
            // TODO(Albino) should be visually unique from all previous colors
            agent.color = new HSBColor(hue, (even ? .75f : .5f), .75f, 1f).ToColor();

            players.Add(agent);
            map.Agents.Add(agent);
            even = !even;
            hue += inc;

            yield return new WaitForSeconds(.1f);
        }
        yield return new WaitForSeconds(.1f);

        Time.timeScale = 3f;
        log.Append($"{map.Agents.Count} Agents created, Simulating Economy...", "green");
        yield return null;
    }

    private void FixedUpdate()
    {
        // If it has been 10 rounds since our last event, trigger one
        if (Time.timeSinceLevelLoad > timeSinceLastEvent + 60f)
        {
            timeSinceLastEvent = Time.timeSinceLevelLoad;
            EventType worldEvent = (EventType)Random.Range(0, 3);
            switch (worldEvent)
            {
                case (EventType.Growth):
                    log.Append("Nature is regrowing...", "lightblue");
                    MapEventGrowth();
                    break;
                
                case (EventType.Flood):
                    log.Append("It has started raining...", "lightblue");
                    GameObject go = GameObject.Find("Tile_Water");
                    if (go)
                    {
                        Provider provider = go.GetComponent<Provider>();

                        // Replenish the primary stock of Water (Water)
                        provider.DropEntries[0].ItemStock += 3;

                        // If there is a second item, replenish some of it too
                        if (provider.DropEntries.Count == 2)
                        {
                            provider.DropEntries[1].ItemStock += 1;
                        }
                    }
                    break;

                case (EventType.Fire):
                    go = CreateResource("Fire", map.GetRandomPoint(), Quaternion.identity);
                    map.Providers.Add(go.GetComponent<Provider>());
                    log.Append($"A fire has broken out in the world!", "orange");
                    break;
            }
        }
    }
    #endregion

    private void MapEventGrowth()
    {
        int treesToCreate = Random.Range(1, 2);
        for (int j = 0; j < treesToCreate; j++)
        {
            GameObject go = GameObject.Find("Tile_Stump");
            if (go)
            {
                // Replace stump with tree
                GameObject go2 = CreateResource("Tree", go.transform.position + Vector3.up, go.transform.parent);
                map.Providers.Add(go2.GetComponent<Provider>());

                // Create 0-2 bushes per tree
                int bushesToCreate = Random.Range(0, 2);
                for (int i = 0; i < bushesToCreate; i++)
                {
                    Vector3 position = Vector3.zero;

                    if (map.IsValidPosition(go.transform.position + new Vector3(1, 0, -1)))
                    {
                        position = go.transform.position + new Vector3(1, 0, -1);
                    }
                    else if (map.IsValidPosition(go.transform.position + new Vector3(-1, 0, 1)))
                    {
                        position = go.transform.position + new Vector3(-1, 0, 1);
                    }

                    if (position != Vector3.zero)
                    {
                        go2 = CreateResource("Bush", position, go.transform.parent);
                        map.Providers.Add(go2.GetComponent<Provider>());
                    }
                }

                Destroy(go);
            }
        }
    }

    public enum EventType
    {
        /// <summary>
        /// causes a tree to grow, or bushes to regrow
        /// </summary>
        Growth, 

        /// <summary>
        /// causes water bodies to refill and expand
        /// </summary>
        Flood,

        /// <summary>
        /// causes tree/bush to catch fire
        /// </summary>
        Fire,
    }
}
