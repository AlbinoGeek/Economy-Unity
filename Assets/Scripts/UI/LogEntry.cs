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
        Debug,
        Info,
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

    /// <summary>
    /// Gets color to draw in
    /// </summary>
    public string Color { get; private set; } = "white";

    /// <summary>
    /// Gets textual description
    /// </summary>
    public string Message { get; private set; } = string.Empty;

    public MessageType Type { get; private set; } = MessageType.Info;

    /// <summary>
    /// Gets time entry was created
    /// </summary>
    public float Time { get; private set; }

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

    public override string ToString()
    {
        return $"{TimeStamp}: {Message}";
    }
}
