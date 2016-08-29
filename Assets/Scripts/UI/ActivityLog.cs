// <copyright file="ActivityLog.cs" company="Mewzor Holdings Inc.">
//     Copyright (c) Mewzor Holdings Inc. All rights reserved.
// </copyright>
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// handles displaying recent activity messages on the screen
/// </summary>
[DisallowMultipleComponent]
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed.  We want to have public methods.")]
public class ActivityLog : MonoBehaviour
{
    /// <summary>
    /// Canvas Text component set in Unity Editor
    /// </summary>
    public Text Text;

    /// <summary>
    /// amount of recent messages to show
    /// </summary>
    [Range(3, 30)] public int MessagesToShow;

    private List<LogEntry> entries = new List<LogEntry>();
    
    /// <summary>
    /// adds a new message to \ref entries
    /// </summary>
    /// <param name="entry">log to add</param>
    public void Append(LogEntry entry)
    {
        entries.Add(entry);
        UpdateLog();
    }

    /// <summary>
    /// helper method creates a LogEntry \see Append
    /// </summary>
    /// <param name="message">description log to create</param>
    public void Append(string message)
    {
        Append(new LogEntry(message));
    }

    /// <summary>
    /// helper method creates a LogEntry with given Color \see Append
    /// </summary>
    /// <param name="message">description log to create</param>
    /// <param name="color">color to flavor entry with</param>
    public void Append(string message, string color)
    {
        Append(new LogEntry(message, color));
    }

    private void UpdateLog()
    {
        int count = 0;

        List<int> picked = new List<int>();
        for (int i = entries.Count - 1; i > 0; i--)
        {
            picked.Add(i);
            count++;

            if (count >= MessagesToShow)
            {
                break;
            }
        }

        // We want only the LAST Nth messages
        picked.Reverse();

        // Put them into the string builder
        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        for (int i = 0; i < picked.Count; i++)
        {
            if (!entries[picked[i]].Color.Equals("white"))
            {
                builder.AppendLine($"<color={entries[picked[i]].Color}>{entries[picked[i]].ToString()}</color>");
            }
            else
            {
                builder.AppendLine(entries[picked[i]].ToString());
            }
        }

        Text.text = builder.ToString().TrimEnd('\n');
    }
}
