using System.Collections;
using UnityEngine;

namespace ScalingShmup.Audio
{
    // Plays the background music through this object's AudioSource. The track the AudioSource
    // starts with (e.g. Play On Awake) is the stage music; ChangeTrack swaps it for another.
    [RequireComponent(typeof(AudioSource))]
    public class MusicPlayer : MonoBehaviour
    {
        private AudioSource _audioSource;
        private float _volume;
        private Coroutine _fade;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _volume = _audioSource.volume;
        }

        public void ChangeTrack(AudioClip track, bool loop)
        {
            if (track == null)
            {
                return;
            }

            CancelFade();
            _audioSource.Stop();
            _audioSource.clip = track;
            _audioSource.resource = track;
            _audioSource.loop = loop;
            _audioSource.Play();
        }

        public void FadeOut(float duration)
        {
            CancelFade();
            _fade = StartCoroutine(FadeOutRoutine(duration));
        }

        public void Stop()
        {
            CancelFade();
            _audioSource.Stop();
        }

        private IEnumerator FadeOutRoutine(float duration)
        {
            float startVolume = _audioSource.volume;

            for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
            {
                _audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }

            _fade = null;
            Stop();
        }

        // Also puts the volume back, so the next track starts at full volume.
        private void CancelFade()
        {
            if (_fade != null)
            {
                StopCoroutine(_fade);
                _fade = null;
            }

            _audioSource.volume = _volume;
        }
    }
}
