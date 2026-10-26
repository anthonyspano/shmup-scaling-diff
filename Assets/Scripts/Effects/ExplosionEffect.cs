using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace ScalingShmup.Effects
{
    // Plays one sprite animation clip and switches itself off when it ends. The clip is played
    // through a PlayableGraph, so no Animator Controller asset is needed per explosion.
    [RequireComponent(typeof(SpriteRenderer), typeof(Animator))]
    public class ExplosionEffect : MonoBehaviour
    {
        private Animator _animator;
        private SpriteRenderer _spriteRenderer;
        private PlayableGraph _graph;
        private Coroutine _routine;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnDisable()
        {
            DestroyGraph();
            _routine = null;
        }

        public void Play(AnimationClip clip, Vector3 position, float scale, Action<ExplosionEffect> onFinished)
        {
            transform.SetPositionAndRotation(position, Quaternion.identity);
            transform.localScale = Vector3.one * scale;
            gameObject.SetActive(true);

            if (_routine != null)
            {
                StopCoroutine(_routine);
            }

            _routine = StartCoroutine(PlayRoutine(clip, onFinished));
        }

        private IEnumerator PlayRoutine(AnimationClip clip, Action<ExplosionEffect> onFinished)
        {
            DestroyGraph();
            _spriteRenderer.enabled = true;
            AnimationPlayableUtilities.PlayClip(_animator, clip, out _graph);

            yield return new WaitForSeconds(clip.length);

            DestroyGraph();
            _routine = null;
            gameObject.SetActive(false);
            onFinished?.Invoke(this);
        }

        private void DestroyGraph()
        {
            if (_graph.IsValid())
            {
                _graph.Destroy();
            }
        }
    }
}
