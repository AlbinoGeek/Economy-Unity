// <copyright file="ActivityLog.cs" company="Mewzor Holdings Inc.">
//     Copyright (c) Mewzor Holdings Inc. All rights reserved.
// </copyright>
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 
/// </summary>
[DisallowMultipleComponent]
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed.  We want to have public methods.")]
public class ActivityLog : MonoBehaviour
{
    public GUIStyle Style;

    private List<LogEntry> messages;
    
    private System.Text.StringBuilder builder;
    
    public void Append(LogEntry entry)
    {
        messages.Add(entry);
    }

    public void Append(string message)
    {
        Append(new LogEntry(message));
        RegenerateLog();
    }

    #region Unity
    /// <summary>
    /// gets references to map
    /// </summary>
    private void Awake()
    {
        builder = new System.Text.StringBuilder();
        messages = new List<LogEntry>();
    }

    /// <summary>
    /// show the activity log across the bottom of the screen
    /// </summary>
    private void OnGUI()
    {
        GUI.Box(new Rect(0, Screen.height - 200, 600, 200), builder.ToString(), Style);
    }
    #endregion

    private void RegenerateLog()
    {
        builder = new System.Text.StringBuilder();

        // Fetch at most, the last ten messages.
        int start = messages.Count - 8;
        if (start < 0)
        {
            start = 0;
        }

        // Put them into the string Builder
        for (int i = start; i < messages.Count; i++)
        {
            builder.AppendLine(messages[i].ToString());
        }
    }
}
