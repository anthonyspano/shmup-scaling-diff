using UnityEngine;
using UnityEngine.InputSystem;

namespace ScalingShmup.Player
{
    // Tank-style controls on the XY plane: left and right turn the ship, and up moves it forward
    // along the way its nose points (its local up axis). There is no reverse. The ship is kept on screen.
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private InputActionReference _moveAction;
        [Tooltip("Units per second when moving forward or backward.")]
        [SerializeField, Min(0f)] private float _speed = 8f;
        [Tooltip("Degrees per second when turning.")]
        [SerializeField, Min(0f)] private float _turnSpeed = 180f;
        [Tooltip("Defaults to Camera.main.")]
        [SerializeField] private Camera _camera;
        [Tooltip("Distance kept between the ship's centre and the screen edge.")]
        [SerializeField] private Vector2 _screenPadding = new Vector2(0.5f, 0.5f);

        private Rigidbody2D _rigidbody;
        private Vector2 _moveInput;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _rigidbody.bodyType = RigidbodyType2D.Kinematic;

            if (_camera == null)
            {
                _camera = Camera.main;
            }
        }

        private void OnEnable()
        {
            _moveAction.action.Enable();
        }

        private void OnDisable()
        {
            _moveInput = Vector2.zero;
        }

        private void Update()
        {
            // Axes are used separately (turn and thrust), so diagonals must not shrink either one.
            // Thrust is clamped at 0 so the ship can only move forward, never reverse.
            Vector2 input = _moveAction.action.ReadValue<Vector2>();
            _moveInput = new Vector2(Mathf.Clamp(input.x, -1f, 1f), Mathf.Clamp(input.y, 0f, 1f));
        }

        private void FixedUpdate()
        {
            // Left input is negative x, and positive rotation turns counter-clockwise (to the left).
            float rotation = _rigidbody.rotation - _moveInput.x * _turnSpeed * Time.fixedDeltaTime;
            _rigidbody.MoveRotation(rotation);

            Vector2 forward = Quaternion.Euler(0f, 0f, rotation) * Vector2.up;
            Vector2 target = _rigidbody.position + forward * (_moveInput.y * _speed * Time.fixedDeltaTime);
            _rigidbody.MovePosition(ClampToScreen(target));
        }

        // Recomputed every step so the bounds follow resolution and aspect-ratio changes.
        private Vector2 ClampToScreen(Vector2 position)
        {
            if (_camera == null || !_camera.orthographic)
            {
                return position;
            }

            Vector2 centre = _camera.transform.position;
            float halfHeight = _camera.orthographicSize;
            Vector2 halfExtents = new Vector2(halfHeight * _camera.aspect, halfHeight) - _screenPadding;

            return new Vector2(
                Mathf.Clamp(position.x, centre.x - halfExtents.x, centre.x + halfExtents.x),
                Mathf.Clamp(position.y, centre.y - halfExtents.y, centre.y + halfExtents.y));
        }
    }
}
