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
    [Range(3, 30)]
    public int MessagesToShow;

    private List<LogEntry> entries;
    
    private System.Text.StringBuilder builder;
    
    /// <summary>
    /// adds a new message to \ref entries
    /// </summary>
    /// <param name="entry">log to add</param>
    public void Append(LogEntry entry)
    {
        entries.Add(entry);
    }

    /// <summary>
    /// helper method creates a LogEntry \see Append
    /// </summary>
    /// <param name="message">description log to create</param>
    public void Append(string message)
    {
        Append(new LogEntry(message));
        UpdateLog();
    }

    #region Unity
    /// <summary>
    /// gets references to map
    /// </summary>
    private void Awake()
    {
        entries = new List<LogEntry>();
    }
    #endregion

    private void UpdateLog()
    {
        builder = new System.Text.StringBuilder();

        // Fetch at most, the last few messages.
        int start = entries.Count - MessagesToShow;
        if (start < 0)
        {
            start = 0;
        }

        // Put them into the string Builder
        for (int i = start; i < entries.Count; i++)
        {
            builder.AppendLine(entries[i].ToString());
        }

        Text.text = builder.ToString();
    }
}
