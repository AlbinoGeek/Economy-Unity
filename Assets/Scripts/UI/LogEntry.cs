// <copyright file="LogEntry.cs" company="Mewzor Holdings Inc.">
//     Copyright (c) Mewzor Holdings Inc. All rights reserved.
// </copyright>

/// <summary>
/// represents an entry in the \ref ActivityLog
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed.  We want to have public methods.")]
public class LogEntry
{
    public enum MessageType
    {
        System,
        World,
        Trade,
        Gather,
        Combat,
        Construction,
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogEntry" /> class given a message 
    /// </summary>
    /// <param name="message">description of entry</param>
    public LogEntry(string message)
    {
        Time = UnityEngine.Time.timeSinceLevelLoad;
        Message = message;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogEntry" /> class given a message and color
    /// </summary>
    /// <param name="message">description of entry</param>
    /// <param name="color">color to write entry in</param>
    public LogEntry(string message, string color) : this(message)
    {
        Color = color;
    }

    public LogEntry(string message, string color, MessageType type) : this(message, color)
    {
        Type = type;
    }

    /// <summary>
    /// Gets color to draw in
    /// </summary>
    public string Color { get; private set; } = "white";

    public int Count { get; set; } = 1;

    /// <summary>
    /// Gets textual description
    /// </summary>
    public string Message { get; private set; } = string.Empty;

    public MessageType Type { get; set; } = MessageType.System;

    /// <summary>
    /// Gets time entry was created
    /// </summary>
    public float Time { get; private set; }

    public float FinalTime { get; set; }

    /// <summary>
    /// Gets textual representation of \ref Time
    /// </summary>
    public string TimeStamp
    {
        get
        {
            return string.Format("[{0:0}]", Time);
        }
    }

    public string RangeStamp
    {
        get
        {
            return string.Format("[{0}-{1}]",
                    string.Format("{0:0}", Time),
                    string.Format("{0:0}", FinalTime));
        }
    }

    public override string ToString()
    {
        if (Count > 1)
        {
            return $"{RangeStamp} {Type}: {Message} (x{Count} times)";
        }

        return $"{TimeStamp} {Type}: {Message}";
    }
}
