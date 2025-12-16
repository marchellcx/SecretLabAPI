using LabExtended.API.Toys;

using MEC;

using NAudio.Wave;

using SecretLabNAudio.Core;
using SecretLabNAudio.Core.Pools;
using SecretLabNAudio.Core.Extensions;

using UnityEngine;

using Utils.NonAllocLINQ;

using SecretLabAPI.Audio.Playback;

namespace SecretLabAPI.Textures.API
{
    /// <summary>
    /// Represents a spawned texture.
    /// </summary>
    public class TextureInstance
    {
        private WaveStream stream;
        private IWaveProvider provider;

        /// <summary>
        /// Gets the assigned ID.
        /// </summary>
        public int Id;

        /// <summary>
        /// Gets the index of the current frame.
        /// </summary>
        public int Index = 0;

        /// <summary>
        /// The original texture.
        /// </summary>
        public TextureInfo Texture;

        /// <summary>
        /// The parent object.
        /// </summary>
        public PrimitiveToy Parent;

        /// <summary>
        /// The list of spawned text toys.
        /// </summary>
        public Dictionary<int, TextToy> Toys = new();

        /// <summary>
        /// Gets the pooled audio.
        /// </summary>
        public AudioPlayer? Audio;

        /// <summary>
        /// Gets the animation coroutine.
        /// </summary>
        public IEnumerator<float> Animator;

        /// <summary>
        /// Gets the handle of the animation coroutine.
        /// </summary>
        public CoroutineHandle AnimatorHandle;

        /// <summary>
        /// Gets or sets the position of the parent object.
        /// </summary>
        public Vector3 Position
        {
            get => Parent.Position;
            set => Parent.Position = value;
        }

        /// <summary>
        /// Gets or sets the rotation of the parent object.
        /// </summary>
        public Quaternion Rotation
        {
            get => Parent.Rotation;
            set => Parent.Rotation = value;
        }

        /// <summary>
        /// Gets or sets the amount of delay between animation frames.
        /// </summary>
        public float AnimationDelay { get; set; }

        /// <summary>
        /// Whether or not this instance is still valid.
        /// </summary>
        public bool IsValid => Parent != null;

        /// <summary>
        /// Whether or not the texture is being animated.
        /// </summary>
        public bool IsAnimating => AnimatorHandle.IsRunning;

        /// <summary>
        /// Whether or not the animation should be looped.
        /// </summary>
        public bool IsAnimationLooped { get; set; }

        /// <summary>
        /// Pauses the animation.
        /// </summary>
        public void PauseAnimation()
        { 
            Timing.PauseCoroutines(AnimatorHandle);

            if (Audio != null)
                Audio.IsPaused = true;
        }

        /// <summary>
        /// Resumes the animation.
        /// </summary>
        public void ResumeAnimation()
        { 
            Timing.ResumeCoroutines(AnimatorHandle);

            if (Audio != null)
                Audio.IsPaused = false;
        }

        /// <summary>
        /// Stops the animation.
        /// </summary>
        public void StopAnimation()
        { 
            Timing.KillCoroutines(AnimatorHandle);

            stream?.Dispose();

            stream = null;
            provider = null;

            if (Audio != null)
                AudioPlayerPool.Return(Audio);

            Audio = null;
        }

        /// <summary>
        /// Restarts animation.
        /// </summary>
        public void StartAnimation()
        {
            StopAnimation();

            Index = 0;

            AnimatorHandle = Timing.RunCoroutine(Animator ??= _Animator(), Segment.RealtimeUpdate);
        }

        /// <summary>
        /// Destroys this instance.
        /// </summary>
        public void Destroy()
        {
            if (Id >= 0)
                TextureManager.SpawnedTextures.Remove(Id);

            if (Parent != null)
                Parent.Delete();

            if (Audio != null)
                AudioPlayerPool.Return(Audio);

            stream?.Dispose();

            stream = null;
            provider = null;

            if (Toys != null)
            {
                Toys.ForEachValue(toy => toy.Delete());
                Toys.Clear();
            }

            Id = -1;

            Audio = null;
            Parent = null!;
        }

        private void OnAudioEnded()
        {
            if (!IsValid)
            {
                stream?.Dispose();

                stream = null;
                provider = null;

                if (Audio != null)
                    AudioPlayerPool.Return(Audio);

                Audio = null;
                return;
            }

            if (Audio != null && IsAnimating 
                && PlaybackUtils.TryLoadClip(Texture.AnimatedSettings.AudioClip, IsAnimationLooped, out stream, out provider)
                && stream != null && provider != null)
            {
                Audio.WithProvider(provider);
            }
        }

        private IEnumerator<float> _Animator()
        {
            TextureUtils.AnimateTexture(Texture, this, Texture.Texture);

            if (!string.IsNullOrWhiteSpace(Texture.AnimatedSettings.AudioClip)
                && PlaybackUtils.TryLoadClip(Texture.AnimatedSettings.AudioClip, IsAnimationLooped, out stream, out provider)
                && stream != null && provider != null)
            {
                Audio = AudioPlayerPool.Rent(SpeakerSettings.Default, Parent.Transform).WithProvider(provider);
                Audio.NoSamplesRead += OnAudioEnded;
            }

            while (true)
            {
                yield return Timing.WaitForOneFrame;

                if (!IsValid)
                {
                    stream?.Dispose();

                    stream = null;
                    provider = null;

                    if (Audio != null)
                        AudioPlayerPool.Return(Audio);

                    Audio = null;
                    yield break;
                }

                if (AnimationDelay > 0f)
                    yield return Timing.WaitForSeconds(AnimationDelay);
                else
                    yield return Timing.WaitForOneFrame;

                Index++;

                if (Index >= Texture.Frames.Count)
                {
                    if (!IsAnimationLooped)
                    {
                        stream?.Dispose();

                        stream = null;
                        provider = null;

                        if (Audio != null)
                            AudioPlayerPool.Return(Audio);

                        Audio = null;
                        yield break;
                    }

                    Index = 0;
                }

                TextureUtils.AnimateTexture(Texture, this, Texture.Frames[Index]);
            }
        }
    }
}