using System;
using Unity.Scripting.LifecycleManagement;
using UnityEngine;

namespace ScalingShmup.Audio
{
    // Plays one-shot sound effects through a single shared 2D AudioSource, created on first use and
    // kept across scene loads. PlayOneShot lets any number of sounds overlap without allocating.
    // AutoStaticsCleanup resets it when Play mode starts.
    [AutoStaticsCleanup]
    public static partial class SoundEffects
    {
        private static AudioSource s_source;

        // Raised for every sound played, e.g. for debugging or a sound log.
        public static event Action<AudioClip> Played;

        public static void Play(AudioClip clip, float volume = 1f)
        {
            if (clip == null)
            {
                return;
            }

            if (s_source == null)
            {
                var go = new GameObject("SoundEffects");
                UnityEngine.Object.DontDestroyOnLoad(go);
                s_source = go.AddComponent<AudioSource>();
                s_source.playOnAwake = false;
                s_source.spatialBlend = 0f;
            }

            s_source.PlayOneShot(clip, volume);
            Played?.Invoke(clip);
        }
    }
}
