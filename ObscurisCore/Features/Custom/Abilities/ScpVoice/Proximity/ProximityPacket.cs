using LabExtended.API.Custom.Voice.Threading;

namespace ObscurisCore.Features.Custom.Abilities.ScpVoice.Proximity;

/// <summary>
/// Represents a packet containing information about the proximity effect.
/// </summary>
public class ProximityPacket : VoiceThreadPacket
{
    /// <summary>
    /// The volume of the proximity effect.
    /// </summary>
    public volatile float Volume = 0f;
}