using System;
using UnityEngine;

namespace ScalingShmup.Audio
{
    // A sound slot shown in the Inspector: the clip to play and how loud. Leave the clip empty for
    // no sound.
    [Serializable]
    public class SoundCue
    {
        [SerializeField] private AudioClip _clip;
        [SerializeField, Range(0f, 1f)] private float _volume = 1f;

        public AudioClip Clip => _clip;
        public float Volume => _volume;

        public void Play()
        {
            SoundEffects.Play(_clip, _volume);
        }
    }
}
