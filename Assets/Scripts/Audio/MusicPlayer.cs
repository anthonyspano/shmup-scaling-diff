using UnityEngine;

namespace ScalingShmup.Audio
{
    // Plays the background music through this object's AudioSource. The track the AudioSource
    // starts with (e.g. Play On Awake) is the stage music; ChangeTrack swaps it for another.
    [RequireComponent(typeof(AudioSource))]
    public class MusicPlayer : MonoBehaviour
    {
        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        public void ChangeTrack(AudioClip track, bool loop)
        {
            if (track == null)
            {
                return;
            }

            _audioSource.Stop();
            _audioSource.resource = track;
            _audioSource.loop = loop;
            _audioSource.Play();
        }

        public void Stop()
        {
            _audioSource.Stop();
        }
    }
}
