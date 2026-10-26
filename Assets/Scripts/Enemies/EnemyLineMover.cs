using System;
using UnityEngine;

namespace ScalingShmup.Enemies
{
    // Flies in a straight line from its spawn point through a target point, and reports when it
    // has left the screen so its spawner can reclaim it.
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyLineMover : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float _speed = 4f;
        [Tooltip("How far outside the screen the enemy must be before it counts as gone.")]
        [SerializeField, Min(0f)] private float _exitMargin = 1f;
        [Tooltip("Defaults to Camera.main.")]
        [SerializeField] private Camera _camera;

        [Header("Facing")]
        [SerializeField] private bool _faceTravelDirection = true;
        [Tooltip("Direction the nose points in the unrotated sprite, in degrees. -90 is down, 90 is up, 180 is left.")]
        [SerializeField] private float _spriteForwardAngle = -90f;

        private Rigidbody2D _rigidbody;
        private Vector2 _direction;
        private bool _hasEnteredScreen;
        private bool _isMoving;

        public event Action<EnemyLineMover> LeftScreen;

        // False once the enemy has crossed the screen and left it, until it is launched again.
        public bool IsMoving => _isMoving;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _rigidbody.bodyType = RigidbodyType2D.Kinematic;

            if (_camera == null)
            {
                _camera = Camera.main;
            }
        }

        public void Launch(Vector2 spawnPosition, Vector2 target)
        {
            // Set both so a reused enemy doesn't start from where it was last released.
            transform.position = spawnPosition;
            _rigidbody.position = spawnPosition;
            _direction = (target - spawnPosition).normalized;
            _hasEnteredScreen = false;
            _isMoving = true;

            // The path is a straight line, so the heading only needs setting once per launch.
            float rotation = _faceTravelDirection
                ? Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg - _spriteForwardAngle
                : 0f;
            transform.rotation = Quaternion.Euler(0f, 0f, rotation);
            _rigidbody.rotation = rotation;
        }

        private void FixedUpdate()
        {
            if (!_isMoving)
            {
                return;
            }

            Vector2 next = _rigidbody.position + _direction * (_speed * Time.fixedDeltaTime);
            _rigidbody.MovePosition(next);

            // Enemies spawn off screen, so only an enemy that has been on screen can leave it.
            bool onScreen = IsInsideScreen(next, 0f);
            _hasEnteredScreen |= onScreen;

            if (_hasEnteredScreen && !IsInsideScreen(next, _exitMargin))
            {
                _isMoving = false;
                LeftScreen?.Invoke(this);
            }
        }

        private bool IsInsideScreen(Vector2 position, float margin)
        {
            if (_camera == null || !_camera.orthographic)
            {
                return true;
            }

            Vector2 centre = _camera.transform.position;
            float halfHeight = _camera.orthographicSize + margin;
            float halfWidth = _camera.orthographicSize * _camera.aspect + margin;
            return Mathf.Abs(position.x - centre.x) <= halfWidth && Mathf.Abs(position.y - centre.y) <= halfHeight;
        }
    }
}
