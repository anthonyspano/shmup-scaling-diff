using System.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace ScalingShmup.Effects
{
    // Loops one sprite animation clip for as long as the object is enabled, e.g. a power-up
    // pickup's idle animation, with an optional pause on the first frame between plays. Like
    // ExplosionEffect, it plays the clip through a PlayableGraph, so no Animator Controller is needed.
    [RequireComponent(typeof(Animator))]
    public class SpriteAnimationLoop : MonoBehaviour
    {
        [Tooltip("Sprite animation to loop.")]
        [SerializeField] private AnimationClip _clip;
        [SerializeField, Min(0f)] private float _speed = 1f;
        [Tooltip("Seconds to hold the first frame between each play of the animation. 0 loops with no pause.")]
        [SerializeField, Min(0f)] private float _pauseBetweenLoops = 0f;

        private PlayableGraph _graph;
        private AnimationClipPlayable _playable;

        private void OnEnable()
        {
            if (_clip == null)
            {
                return;
            }

            _playable = AnimationPlayableUtilities.PlayClip(GetComponent<Animator>(), _clip, out _graph);
            _playable.SetSpeed(_speed);
            StartCoroutine(Loop());
        }

        private void OnDisable()
        {
            // Disabling the component (not the GameObject) keeps coroutines running, so stop the loop
            // before its playable is destroyed.
            StopAllCoroutines();

            if (_graph.IsValid())
            {
                _graph.Destroy();
            }
        }

        private IEnumerator Loop()
        {
            float playLength = _speed > 0f ? _clip.length / _speed : float.PositiveInfinity;

            while (true)
            {
                _playable.SetTime(0);
                _playable.SetSpeed(_speed);
                yield return new WaitForSeconds(playLength);

                if (_pauseBetweenLoops > 0f)
                {
                    // Rest on the first frame until the next play.
                    _playable.SetTime(0);
                    _playable.SetSpeed(0);
                    yield return new WaitForSeconds(_pauseBetweenLoops);
                }
            }
        }
    }
}
