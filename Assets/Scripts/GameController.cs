// <copyright file="GameController.cs" company="Mewzor Holdings Inc.">
//     Copyright (c) Mewzor Holdings Inc. All rights reserved.
// </copyright>
using UnityEngine;

/// <summary>
/// handles the flow logic of the game
/// </summary>
[DisallowMultipleComponent]
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed.  We want to have public methods.")]
public class GameController : MonoBehaviour
{
    /// <summary>
    /// list of players to be created at the start of the game
    /// </summary>
    private static string[] population =
    {
        "AngryAlbino",
        "JohnGeese",
        "StabbyGaming",
        "Big Hoss",
        "deccer",
        "E.B.",
        "human_supremacist",
        "Le Chat",
        "Malscythe",
        "Prxy",
        "RobbieW",
        "SadCloud123",
        "sean",
        "Sense",
        "Westermin",
        "wubbalubbadubdub",
        "vassvik",
    };
    
    private ActivityLog log;

    private MapController map;

    #region Unity
    /// <summary>
    /// gets references to map
    /// </summary>
    private void Awake()
    {
        log = GameObject.Find("Activity").GetComponent<ActivityLog>();
        map = GameObject.Find("Map").GetComponent<MapController>();
    }

    /// <summary>
    /// beings a game, initializing things as required
    /// </summary>
    private void Start()
    {
        GameObject agentContainer = new GameObject("Agents");
        foreach (string name in population)
        {
            GameObject go = Agent.Create(name);
            Agent agent = go.GetComponent<Agent>();
            go.transform.parent = agentContainer.transform;

            // Position Agents randomly on the map
            go.transform.position = map.GetRandomPoint();

            // Give them a random quality starting kit
            agent.inventory.Add("Money", Random.Range(0, 10));
            agent.inventory.Add("Bread", Random.Range(5, 15));
            agent.inventory.Add("Water", Random.Range(10, 30));

            // Register our Agent as ready
            map.Agents.Add(agent);
        }

        log.Append(string.Format("{0} Agents created, Simulating Economy...", map.Agents.Count));
    }
    #endregion
}
