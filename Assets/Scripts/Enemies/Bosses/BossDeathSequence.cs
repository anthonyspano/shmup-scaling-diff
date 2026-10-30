using System;
using System.Collections;
using ScalingShmup.Audio;
using ScalingShmup.Combat;
using ScalingShmup.Effects;
using UnityEngine;

namespace ScalingShmup.Enemies.Bosses
{
    // When the boss's health runs out, switches off every other script and collider on the boss
    // (and its turrets, exhausts and health bar), then plays its explosions one after another at
    // random points within Spawn Radius of the boss's centre. The boss is destroyed the moment the
    // last explosion fires; Finished reports how long that explosion still has to play, so
    // listeners (e.g. BossEncounter) can wait for it to end.
    // Turn off Remove On Death on the boss's Enemy component, so the boss stays for the sequence.
    [RequireComponent(typeof(Health))]
    public class BossDeathSequence : MonoBehaviour
    {
        [Serializable]
        private class Blast
        {
            public AnimationClip Animation;
            [Min(0.01f)] public float Scale = 3f;
            public SoundCue Sound = new SoundCue();
        }

        [SerializeField] private ExplosionEffect _explosionPrefab;
        [Tooltip("Played in order, one after another.")]
        [SerializeField] private Blast[] _explosions = new Blast[5];
        [Tooltip("Seconds between the start of one explosion and the next.")]
        [SerializeField, Min(0f)] private float _timeBetweenExplosions = 0.4f;
        [Tooltip("Explosions appear at random points up to this many units from the boss's centre.")]
        [SerializeField, Min(0f)] private float _spawnRadius = 1.5f;

        private Health _health;

        // Raised as the boss is destroyed, with the seconds left until the last explosion ends.
        public event Action<float> Finished;

        private void Awake()
        {
            _health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            _health.Died += OnDied;
        }

        private void OnDisable()
        {
            _health.Died -= OnDied;
        }

        private void OnDied(Health health)
        {
            _health.Died -= OnDied;
            ShutDownBoss();
            StartCoroutine(Explode());
        }

        // Scripts are switched off and their coroutines stopped (switching a script off alone
        // leaves its coroutines running, e.g. the boss's movement).
        private void ShutDownBoss()
        {
            foreach (Behaviour behaviour in GetComponentsInChildren<Behaviour>(true))
            {
                if (behaviour == this)
                {
                    continue;
                }

                if (behaviour is MonoBehaviour script)
                {
                    script.StopAllCoroutines();
                }

                // The engines are dead, so their flames go out.
                if (behaviour is DirectionalExhaust && behaviour.TryGetComponent(out SpriteRenderer flame))
                {
                    flame.enabled = false;
                }

                behaviour.enabled = false;
            }

            foreach (Collider2D bossCollider in GetComponentsInChildren<Collider2D>(true))
            {
                bossCollider.enabled = false;
            }
        }

        private IEnumerator Explode()
        {
            for (int i = 0; i < _explosions.Length; i++)
            {
                Blast blast = _explosions[i];
                bool isLast = i == _explosions.Length - 1;

                if (blast == null)
                {
                    continue;
                }

                PlayAnimation(blast);
                blast.Sound.Play();

                if (isLast)
                {
                    // The explosion and its sound live on their own objects, so they keep playing.
                    float soundLength = blast.Sound.Clip != null ? blast.Sound.Clip.length : 0f;
                    float animationLength = blast.Animation != null ? blast.Animation.length : 0f;
                    Finish(Mathf.Max(soundLength, animationLength));
                    yield break;
                }

                yield return new WaitForSeconds(_timeBetweenExplosions);
            }

            Finish(0f);
        }

        private void Finish(float secondsUntilLastExplosionEnds)
        {
            Finished?.Invoke(secondsUntilLastExplosionEnds);
            Destroy(gameObject);
        }

        private void PlayAnimation(Blast blast)
        {
            if (_explosionPrefab == null || blast.Animation == null)
            {
                return;
            }

            // A separate instance per blast, so overlapping explosions don't cut each other off.
            Vector2 position = (Vector2)transform.position + UnityEngine.Random.insideUnitCircle * _spawnRadius;
            ExplosionEffect explosion = Instantiate(_explosionPrefab);
            explosion.Play(blast.Animation, position, blast.Scale, finished => Destroy(finished.gameObject));
        }
    }
}
