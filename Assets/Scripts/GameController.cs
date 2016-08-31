// <copyright file="GameController.cs" company="Mewzor Holdings Inc.">
//     Copyright (c) Mewzor Holdings Inc. All rights reserved.
// </copyright>
using System.Collections;
using UnityEngine;

/// <summary>
/// handles the flow logic of the game
/// </summary>
[DisallowMultipleComponent]
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed.  We want to have public methods.")]
public class GameController : GlobalBehaviour
{
    private static House[] Houses;

    /// <summary>
    /// list of players with a color to be created at the start of the game
    /// </summary>
    private static readonly string[] HeroicPopulation =
    {
        "GoodGuyWill",
        "Malscythe",
        "RobbieW",
        "vassvik",
    };

    /// <summary>
    /// list of NPCs to be created at the start of the game
    /// </summary>
    private static readonly string[] RegularPopulation =
    {
        "b4u7",
        "Big Hoss",
        "divideddarko",
        "HD",
        "Le Chat",
        "Mirage",
        "Mobilpadde",
        "Movariant",
        "Prxy",
        "romgerman",
        "sean",
        "SadCloud123",
        "WildNoob",
    };

    public static GameObject AgentContainer { get; private set; }

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
        AgentContainer = new GameObject("Agents");
        Houses = new House[] {
            //new House("The LightHouse", "E.B."),

            new House(
                new HSBColor(.08f, .7f, 1f, 1f),
                "Quellar No'Quar", "deccer",
                new string[] { "wubbalubbadubdub" }),

            new House(
                new HSBColor(.94f, .7f, 1f, 1f),
                "Bravery minister", "Sense",
                new string[] { "Westermin" }),

            new House(
                new HSBColor(0f, 0f, .85f, 1f),
                "Atreides", "human_supremacist",
                new string[] { "Leia", "Luke", "Han", "Chewie" }),

            new House(
                new HSBColor(.55f, .7f, 1f, 1f),
                "The Horsemen", "matt",
                new string[] { "Death", "striking", "Conquest", "War" }),

            new House(
                new HSBColor(.78f, .7f, 1f, 1f),
                "", "AngryAlbino",
                new string[] { "StabbyGaming", "MewzorMewMew", "JohnGeese", "Raiden", "HappyGiant" }),
        };

        StartCoroutine("CreateHouses");
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
                    log.Append("Nature is regrowing...", "lightblue", LogEntry.MessageType.World);
                    MapEventGrowth();
                    break;
                
                case EventType.Flood:
                    log.Append("It has started raining...", "lightblue", LogEntry.MessageType.World);
                    GameObject go = GameObject.Find("Tile_Water");
                    if (go)
                    {
                        Provider provider = go.GetComponent<Provider>();

                        // Replenish the primary stock of Water (Water)
                        if (provider != null && provider.DropEntries.Count == 1)
                        {
                            provider.DropEntries[0].ItemStock += 3;

                            // If there is a second item, replenish some of it too
                            if (provider.DropEntries.Count == 2)
                            {
                                provider.DropEntries[1].ItemStock += 1;
                            }
                        }
                    }

                    break;

                case EventType.Fire:
                    go = CreateResource("Fire", map.GetRandomPoint(), Quaternion.identity);
                    map.Providers.Add(go.GetComponent<Provider>());
                    log.Append($"A fire has broken out!", "orange", LogEntry.MessageType.World);
                    break;
            }
        }
    }
    #endregion

    private IEnumerator CreateHouses()
    {
        // For each house, create all agents
        for (int i = 0; i < Houses.Length; i++)
        {
            Houses[i].CreateAgents(map);
            for (int j = Houses[i].Agents.Count; j > 0; j--)
            {
                players.Add(Houses[i].Agents[j - 1]);
            }

            log.Append($"House of {Houses[i].Name} has been founded by {Houses[i].Leader}", "green");
        }

        yield return new WaitForSeconds(.5f);

        log.Append($"{map.Agents.Count} Agents created, Simulating Economy...", "green");
        Time.timeScale = 3f;
        yield return null;
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
