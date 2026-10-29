using ScalingShmup.Effects;
using UnityEngine;

namespace ScalingShmup.Audio
{
    // Loops a thruster sound while any of this ship's exhaust flames (child DirectionalExhausts) is
    // showing. One sound per ship, so the player's two engines and the boss's four ports don't stack.
    // SoundEffects only plays one-shots, so this keeps its own looping AudioSource.
    // Runs after DirectionalExhaust so it reacts to this frame's flames, not last frame's.
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(AudioSource))]
    public class ThrusterSound : MonoBehaviour
    {
        [Tooltip("Looped while the ship's exhaust is showing. Leave empty for no sound.")]
        [SerializeField] private SoundCue _thrusterSound = new SoundCue();

        private AudioSource _audioSource;
        private DirectionalExhaust[] _exhausts;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.loop = true;
            _audioSource.spatialBlend = 0f;
            _exhausts = GetComponentsInChildren<DirectionalExhaust>(true);
        }

        private void OnDisable()
        {
            _audioSource.Stop();
        }

        private void LateUpdate()
        {
            bool thrusting = _thrusterSound.Clip != null && AnyExhaustShowing();

            if (thrusting && !_audioSource.isPlaying)
            {
                _audioSource.clip = _thrusterSound.Clip;
                _audioSource.volume = _thrusterSound.Volume;
                _audioSource.Play();
            }
            else if (!thrusting && _audioSource.isPlaying)
            {
                _audioSource.Stop();
            }
        }

        private bool AnyExhaustShowing()
        {
            foreach (DirectionalExhaust exhaust in _exhausts)
            {
                if (exhaust.isActiveAndEnabled && exhaust.IsShowing)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
