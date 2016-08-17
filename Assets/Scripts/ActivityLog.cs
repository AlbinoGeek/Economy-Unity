// <copyright file="ActivityLog.cs" company="Mewzor Holdings Inc.">
//     Copyright (c) Mewzor Holdings Inc. All rights reserved.
// </copyright>
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// 
/// </summary>
[DisallowMultipleComponent]
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed.  We want to have public methods.")]
public class ActivityLog : MonoBehaviour
{
    public GUIStyle Style;

    private MapController map;
    private List<string> messages;

    public void Append(string message)
    {
        messages.Add(message);
    }

    #region Unity
    /// <summary>
    /// gets references to map
    /// </summary>
    private void Awake()
    {
        map = GameObject.Find("Map").GetComponent<MapController>();
        messages = new List<string>();
    }

    /// <summary>
    /// show the activity log across the bottom of the screen
    /// </summary>
    private void OnGUI()
    {
        StringBuilder sb = new StringBuilder();
        int count = 0;

        // TODO(Albino) The building should not be done inside OnGUI!
        for (int i = messages.Count - 1; i >= 0; i--)
        {
            sb.AppendLine(messages[i]);
            count++;

            if (count > 6)
            {
                break;
            }
        }

        GUI.Box(new Rect(0, Screen.height - 200, 600, 200), sb.ToString(), Style);
    }
    #endregion
}
