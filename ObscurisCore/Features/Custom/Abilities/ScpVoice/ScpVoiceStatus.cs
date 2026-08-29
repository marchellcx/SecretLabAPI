namespace ObscurisCore.Features.Custom.Abilities.ScpVoice;

/// <summary>
/// Represents the status modes for SCP voice communication.
/// </summary>
public enum ScpVoiceStatus : byte
{
    /// <summary>
    /// Only SCP-chat.
    /// </summary>
    Scp = 0,
    
    /// <summary>
    /// Only proximity chat.
    /// </summary>
    Proximity = 1,
    
    /// <summary>
    /// Both SCP-chat and proximity chat.
    /// </summary>
    Mixed = 2,
}