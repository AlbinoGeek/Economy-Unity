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
    /// list of players with a color to be created at the start of the game
    /// </summary>
    private static readonly string[] HeroicPopulation =
    {
        "AngryAlbino",
        "deccer",
        "E.B.",
        "GoodGuyWill",
        "JohnGeese",
        "Malscythe",
        "matt",
        "MewzorMewMew",
        "Raiden",
        "RobbieW",
        "StabbyGaming",
        "Westermin",
        "wubbalubbadubdub",
        "vassvik",
    };

    private static readonly float[] Hues =
    {
        .09f,
        .00f,
        .16f,
        .25f,
        .80f,
        .63f,
        .50f,
        .90f,
        .40f,
        .58f,
        .02f,
        .54f,
        .54f,
        .58f
    };

    /// <summary>
    /// list of NPCs to be created at the start of the game
    /// </summary>
    private static readonly string[] RegularPopulation =
    {
        "b4u7",
        "Big Hoss",
        "divideddarko",
        "HappyGiant",
        "HD",
        "human_supremacist",
        "Le Chat",
        "Mirage",
        "Mobilpadde",
        "Movariant",
        "Prxy",
        "romgerman",
        "sean",
        "SadCloud123",
        "Sense",
        "WildNoob",
    };

    private GameObject agentContainer;

    private float timeSinceLastEvent;

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

    private void FixedUpdate()
    {
        // If it has been 20 rounds since our last event, trigger one
        if (Time.timeSinceLevelLoad > timeSinceLastEvent + 120f)
        {
            timeSinceLastEvent = Time.timeSinceLevelLoad;
            EventType worldEvent = (EventType)Random.Range(0, 3);
            switch (worldEvent)
            {
                case EventType.Growth:
                    log.Append("Nature is regrowing...", "lightblue");
                    MapEventGrowth();
                    break;
                
                case EventType.Flood:
                    log.Append("It has started raining...", "lightblue");
                    GameObject go = GameObject.Find("Tile_Water");
                    if (go)
                    {
                        Provider provider = go.GetComponent<Provider>();

                        // Replenish the primary stock of Water (Water)
                        if (provider != null && provider.DropEntries.Count > 0)
                        {
                            provider.DropEntries[0].Increase(3);

                            // If there is a second item, replenish some of it too
                            if (provider.DropEntries.Count == 2)
                            {
                                provider.DropEntries[1].Increase(1);
                            }
                        }
                    }

                    break;

                case EventType.Fire:
                    go = CreateResource("Fire", map.GetRandomPoint(), Quaternion.identity);
                    map.Providers.Add(go.GetComponent<Provider>());
                    log.Append($"A fire has broken out in the world!", "orange");
                    break;
            }
        }
    }
    #endregion

    private IEnumerator CreateAgents()
    {
        agentContainer = new GameObject("Agents");

        // Create heroic characters (Players) with a colour
        for (int i = HeroicPopulation.Length - 1; i >= 0; i--)
        {
            CreateAgent(HeroicPopulation[i], new HSBColor(Hues[i], .75f, .75f, 1f).ToColor());
            yield return new WaitForSeconds(.1f);
        }

        // Create NPCs without a colour for now
        // TODO(Albino) Find a way to make unique colours so I don't have to have two classes of people
        var pop = RegularPopulation.OrderByDescending(x => x.Length);
        foreach (string name in pop)
        {
            CreateAgent(name, Color.gray);
            yield return new WaitForSeconds(.05f);
        }

        yield return new WaitForSeconds(.1f);

        Time.timeScale = 3f;
        log.Append($"{map.Agents.Count} Agents created, Simulating Economy...", "green");
        yield return null;
    }

    private void CreateAgent(string name, Color color)
    {
        GameObject go = Agent.Create(name);
        Agent agent = go.GetComponent<Agent>();
        go.transform.parent = agentContainer.transform;

        // Position Agents randomly on the map
        go.transform.position = map.GetRandomPoint();

        // Give them a fair amount of resources
        agent.inventory.Add("Bread", Random.Range(10, 10));
        agent.inventory.Add("Water", Random.Range(10, 10));

        // Generate a random color for this agent
        // TODO(Albino) should be visually unique from all previous colors
        agent.color = color;

        players.Add(agent);
        map.Agents.Add(agent);
    }

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
}
