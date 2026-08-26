using System.Collections.Concurrent;

using LabExtended.API.Custom.Voice.Threading;
using LabExtended.Core;

using UnityEngine;

namespace ObscurisCore.Features.Abilities.ScpVoice.Proximity;

/// <summary>
/// Processes the proximity effect.
/// </summary>
public class ProximityProcessor : IVoiceThreadAction
{
    private static volatile ConcurrentQueue<float[]> pcmPool = new();

    /// <summary>
    /// The instance of the ProximityProcessor.
    /// </summary>
    public static volatile ProximityProcessor Instance = new();

    /// <summary>
    /// Modifies the provided voice thread packet to apply proximity effects, including
    /// decoding, adjusting volume, and re-encoding the audio data.
    /// </summary>
    /// <param name="packet">
    /// The voice thread packet to be modified. Must be an instance of <c>ProximityPacket</c>.
    /// </param>
    public void Modify(ref VoiceThreadPacket packet)
    {
        if (packet is not ProximityPacket proximityPacket)
        {
            ApiLog.Warn($"Received packet of type {packet.GetType()} instead of ProximityPacket.");
            return;
        }

        if (!pcmPool.TryDequeue(out var pcm))
            pcm = new float[48000];
        
        var length = proximityPacket.Decoder.Decode(proximityPacket.Data, proximityPacket.Length, pcm);

        if (length > 0)
        {
            for (var x = 0; x < pcm.Length; x++)
                pcm[x] = Mathf.Clamp(pcm[x] * proximityPacket.Volume, -1f, 1f);

            packet.Length = packet.Encoder.Encode(pcm, packet.Data, 480);

            Array.Clear(pcm, 0, length);
        }
        else
        {
            ApiLog.Warn("Decoded empty PCM audio!");
        }

        pcmPool.Enqueue(pcm);
    }
}