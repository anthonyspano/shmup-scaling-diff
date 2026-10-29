using UnityEngine;

namespace ScalingShmup.Effects
{
    // Shows an engine exhaust sprite only while its ship is moving in the Thrust Direction (the way
    // that engine would push it). Put it on the exhaust object, a child of the ship; the sprite keeps
    // animating while hidden, so it reappears mid-flicker rather than restarting.
    [RequireComponent(typeof(SpriteRenderer))]
    public class DirectionalExhaust : MonoBehaviour
    {
        [Tooltip("The ship whose movement is checked. Defaults to this object's parent.")]
        [SerializeField] private Transform _ship;
        [Tooltip("Ship movement direction that turns this exhaust on, e.g. the ship's forward for a rear engine.")]
        [SerializeField] private Vector2 _thrustDirection = Vector2.up;
        [Tooltip("On: Thrust Direction turns with the ship (local). Off: it is a fixed screen direction.")]
        [SerializeField] private bool _directionIsLocal = true;
        [Tooltip("Speed in that direction (units per second) needed before the exhaust shows.")]
        [SerializeField, Min(0f)] private float _minSpeed = 0.2f;

        // Physics-driven ships only move on fixed steps, so a rendered frame often sees no movement.
        // A frame with no movement keeps the last decision until this long has passed.
        private const float StillHoldTime = 0.1f;

        private SpriteRenderer _spriteRenderer;
        private Vector2 _lastShipPosition;
        private float _lastSampleTime;

        public bool IsShowing => _spriteRenderer != null && _spriteRenderer.enabled;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();

            if (_ship == null)
            {
                _ship = transform.parent;
            }
        }

        private void OnEnable()
        {
            if (_ship != null)
            {
                _lastShipPosition = _ship.position;
            }

            _lastSampleTime = Time.time;
            _spriteRenderer.enabled = false;
        }

        private void LateUpdate()
        {
            float elapsed = Time.time - _lastSampleTime;

            if (_ship == null || elapsed <= 0f)
            {
                return;
            }

            Vector2 position = _ship.position;

            if (position == _lastShipPosition && elapsed < StillHoldTime)
            {
                return;
            }

            Vector2 velocity = (position - _lastShipPosition) / elapsed;
            _lastShipPosition = position;
            _lastSampleTime = Time.time;

            Vector2 thrust = _directionIsLocal ? (Vector2)_ship.TransformDirection(_thrustDirection) : _thrustDirection;
            _spriteRenderer.enabled = Vector2.Dot(velocity, thrust.normalized) > _minSpeed;
        }
    }
}
