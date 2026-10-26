using System;
using System.Collections.Generic;
using ScalingShmup.Combat;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ScalingShmup.Player
{
    // Close-range strike. Its damage type is the only one that can hurt enemy health.
    public class MeleeAttack : MonoBehaviour
    {
        [SerializeField] private InputActionReference _attackAction;
        [SerializeField, Min(0f)] private float _damage = 1f;
        [SerializeField, Min(0.05f)] private float _radius = 1f;
        [Tooltip("Centre of the strike relative to the ship, in local space.")]
        [SerializeField] private Vector2 _offset = new Vector2(0f, 1f);
        [SerializeField, Min(0f)] private float _cooldown = 0.4f;
        [SerializeField] private LayerMask _targetLayers;

        private readonly List<Collider2D> _hits = new List<Collider2D>();
        private readonly HashSet<IDamageable> _damagedThisSwing = new HashSet<IDamageable>();
        private float _nextAttackTime;

        public event Action Attacked;

        private void OnEnable()
        {
            _attackAction.action.Enable();
            _attackAction.action.performed += OnAttackPerformed;
        }

        private void OnDisable()
        {
            _attackAction.action.performed -= OnAttackPerformed;
        }

        private void OnAttackPerformed(InputAction.CallbackContext context)
        {
            if (Time.time < _nextAttackTime)
            {
                return;
            }

            _nextAttackTime = Time.time + _cooldown;
            Strike();
        }

        private void Strike()
        {
            var filter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = _targetLayers,
                useTriggers = true,
            };

            Physics2D.OverlapCircle(StrikeCentre(), _radius, filter, _hits);

            // A target with several colliders must only be hit once per swing.
            _damagedThisSwing.Clear();
            var damage = new DamageInfo(_damage, DamageType.Melee, gameObject);

            foreach (Collider2D hit in _hits)
            {
                IDamageable target = hit.GetComponentInParent<IDamageable>();

                if (target != null && _damagedThisSwing.Add(target))
                {
                    target.TakeDamage(damage);
                }
            }

            Attacked?.Invoke();
        }

        private Vector2 StrikeCentre()
        {
            return transform.TransformPoint(_offset);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(StrikeCentre(), _radius);
        }
    }
}
