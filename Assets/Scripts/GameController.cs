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

    private PlayerList players;

    #region Unity
    /// <summary>
    /// gets references to map
    /// </summary>
    private void Awake()
    {
        log = GameObject.Find("Activity").GetComponent<ActivityLog>();
        map = GameObject.Find("Map").GetComponent<MapController>();
        players = GameObject.Find("Player List").GetComponent<PlayerList>();
    }

    /// <summary>
    /// begins a game, initializing things as required
    /// </summary>
    private void Start()
    {
        GameObject agentContainer = new GameObject("Agents");

        float hue = 0f;
        float inc = 1f / population.Length;
        bool even = true;
        foreach (string name in population)
        {
            GameObject go = Agent.Create(name);
            Agent agent = go.GetComponent<Agent>();
            go.transform.parent = agentContainer.transform;

            // Position Agents randomly on the map
            go.transform.position = map.GetRandomPoint();

            // Give them a random quality starting kit
            agent.inventory.Add("Money", Random.Range(0, 10));
            agent.inventory.Add("Bread", Random.Range(10, 15));
            agent.inventory.Add("Water", Random.Range(10, 15));

            // Generate a random color for this agent
            // TODO(Albino) should be visually unique from all previous colors
            agent.color = new HSBColor(hue, (even ? .75f : .5f), .75f,  1f).ToColor();
            
            players.Add(agent);
            map.AddAgent(agent);
            even = !even;
            hue += inc;
        }

        log.Append(string.Format("{0} Agents created, Simulating Economy...", map.Agents.Count));
    }
    #endregion
}
