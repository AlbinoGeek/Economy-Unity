// <copyright file="LogEntry.cs" company="Mewzor Holdings Inc.">
//     Copyright (c) Mewzor Holdings Inc. All rights reserved.
// </copyright>

/// <summary>
/// represents an entry in the \ref ActivityLog
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed.  We want to have public methods.")]
public class LogEntry
{
    public LogEntry(string message)
    {
        Time = UnityEngine.Time.timeSinceLevelLoad;
        Message = message;
    }

    public LogEntry(string message, UnityEngine.Color color) : this(message)
    {
        Color = color;
    }

    public UnityEngine.Color Color { get; private set; }

    public string Message { get; private set; }

    public float Time { get; private set; }

    public string TimeStamp
    {
        get
        {
            return string.Format("[{0:0}]", Time);
        }
    }

    public override string ToString()
    {
        return string.Format("{0}: {1}", TimeStamp, Message);
    }
}
