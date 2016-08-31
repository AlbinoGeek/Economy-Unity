// <copyright file="Houses.cs" company="Mewzor Holdings Inc.">
//     Copyright (c) Mewzor Holdings Inc. All rights reserved.
// </copyright>
using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// represents a collection of agents that work together
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed.  We want to have public methods.")]
[System.Serializable]
public class House
{
    public HSBColor Tint { get; private set; } = HSBColor.FromColor(Color.white);

    public string Name { get; private set; } = string.Empty;
    public string Leader { get; private set; } = string.Empty;
    public string[] Members { get; private set; }

    public List<Agent> Agents { get; private set; } = new List<Agent>();
    public Alignment Alignment { get; private set; } = Alignment.Neutral;

    public GameObject Base { get; private set; }
    public Vector3 BuildArea { get; private set; }
    
    private bool created = false;

    private static Agent CreateAgent(string name, Color color, MapController map)
    {
        GameObject go = Agent.Create(name);
        Agent agent = go.GetComponent<Agent>();

        go.transform.parent = GameController.AgentContainer.transform;

        // Position Agents randomly on the map
        go.transform.position = map.GetRandomPoint(2);

        // Give them a fair amount of resources
        agent.inventory.Add("Bread", Random.Range(10, 10));
        agent.inventory.Add("Water", Random.Range(10, 10));

        agent.color = color;

        map.Agents.Add(agent);
        return agent;
    }

    public House(HSBColor tint, string name, string leader, string[] members)
    {
        Name = name;
        Tint = tint;
        Leader = leader;
        Members = members;
    }

    public void CreateAgents(MapController map)
    {
        if (created)
        {
            Debug.LogError("Attempting to create house twice!");
            return;
        }

        created = true;

        // Create leader
        Agent leader = CreateAgent(Leader, Tint.ToColor(), map);
        Alignment = leader.Alignment;
        leader.House = this;
        Agents.Add(leader);

        // Create a duplicate duller color
        HSBColor minorColor = new HSBColor(Tint.ToColor());
        minorColor.b -= .2f;

        // Create the mcguffin (base)
        BuildArea = leader.transform.position;

        Base = GlobalBehaviour.CreateResource("Base", BuildArea - new Vector3(0, .59f, 0), Quaternion.Euler(0, Random.Range(-89, 89), 0));
        Base.transform.Find("Head").GetComponent<MeshRenderer>().material.color = Tint.ToColor();

        // Create all the Members
        for (int j = 0; j < Members.Length; j++)
        {
            Agent member = CreateAgent(Members[j], minorColor.ToColor(), map);
            member.transform.position = leader.transform.position;
            member.Alignment = Alignment;
            member.House = this;
            Agents.Add(member);
        }
    }
}
