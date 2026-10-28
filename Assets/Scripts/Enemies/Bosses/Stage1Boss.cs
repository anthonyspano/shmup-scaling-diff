using System.Collections;
using ScalingShmup.Combat;
using ScalingShmup.Projectiles;
using UnityEngine;

namespace ScalingShmup.Enemies.Bosses
{
    // Stage 1 boss. Turrets spray for the whole fight; turrets above the boss's centre start their
    // sweep at the top-left corner and turrets below start at the bottom-left, so they mirror.
    // Set 1: rise from the middle to the top of the screen, then sweep top to bottom and back.
    // Set 2 (health below the threshold): any shot-down turrets revive at full health, then the boss
    // traces a figure 8, pauses, traces it in reverse, pauses, and repeats.
    [RequireComponent(typeof(Health), typeof(Rigidbody2D))]
    public class Stage1Boss : MonoBehaviour
    {
        [Tooltip("Defaults to Camera.main. Movement limits follow its view.")]
        [SerializeField] private Camera _camera;

        [Header("Set 1: vertical sweeps")]
        [SerializeField, Min(0.1f)] private float _verticalSpeed = 2.5f;
        [Tooltip("Distance kept between the boss's centre and the top and bottom of the screen.")]
        [SerializeField, Min(0f)] private float _screenMargin = 1.5f;

        [Header("Set 2: figure 8")]
        [Tooltip("Set 2 starts when health falls below this fraction of max health.")]
        [SerializeField, Range(0f, 1f)] private float _setTwoHealthFraction = 0.5f;
        [SerializeField, Min(0.1f)] private float _figureEightWidth = 3f;
        [Tooltip("Height of the figure 8. It is capped to fit inside the screen margins.")]
        [SerializeField, Min(0.1f)] private float _figureEightHeight = 7f;
        [Tooltip("Seconds to trace the figure 8 once.")]
        [SerializeField, Min(0.1f)] private float _figureEightDuration = 6f;
        [Tooltip("Seconds the boss waits after finishing each figure 8 before tracing the next.")]
        [SerializeField, Min(0f)] private float _pauseBetweenPaths = 1f;

        private Health _health;
        private Rigidbody2D _rigidbody;
        private BossTurret[] _turrets;
        private Coroutine _movement;
        private bool _inSetTwo;

        public bool InSetTwo => _inSetTwo;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _rigidbody.bodyType = RigidbodyType2D.Kinematic;

            if (_camera == null)
            {
                _camera = Camera.main;
            }

            ConfigureTurrets();
        }

        private void OnEnable()
        {
            _health.HealthChanged += OnHealthChanged;
        }

        private void OnDisable()
        {
            _health.HealthChanged -= OnHealthChanged;
        }

        private void Start()
        {
            _movement = StartCoroutine(SetOneVerticalSweeps());
        }

        // Above or below is judged on screen, so it holds however the boss object is rotated.
        private void ConfigureTurrets()
        {
            var owner = GetComponent<Enemy>();

            // Include inactive turrets: downed turrets are switched off, not destroyed, so they can revive.
            _turrets = GetComponentsInChildren<BossTurret>(true);

            foreach (BossTurret turret in _turrets)
            {
                bool aboveCentre = turret.transform.position.y > transform.position.y;
                turret.Configure(owner, _camera, aboveCentre);
            }
        }

        private void OnHealthChanged(float current, float max)
        {
            if (!_inSetTwo && current > 0f && current < max * _setTwoHealthFraction)
            {
                _inSetTwo = true;
                ReviveTurrets();
                StopCoroutine(_movement);
                _movement = StartCoroutine(SetTwoFigureEights());
            }
        }

        // Turrets that were shot down come back at full health when set 2 starts.
        private void ReviveTurrets()
        {
            foreach (BossTurret turret in _turrets)
            {
                if (turret == null || turret.gameObject.activeSelf)
                {
                    continue;
                }

                turret.gameObject.SetActive(true);

                if (turret.TryGetComponent(out Health turretHealth))
                {
                    turretHealth.RestoreToFull();
                }
            }
        }

        private IEnumerator SetOneVerticalSweeps()
        {
            float top = ScreenCentre().y + MaxOffsetY();
            float bottom = ScreenCentre().y - MaxOffsetY();

            yield return MoveTo(new Vector2(_rigidbody.position.x, top), _verticalSpeed);

            while (true)
            {
                yield return MoveTo(new Vector2(_rigidbody.position.x, bottom), _verticalSpeed);
                yield return MoveTo(new Vector2(_rigidbody.position.x, top), _verticalSpeed);
            }
        }

        private IEnumerator SetTwoFigureEights()
        {
            // The figure 8 is centred on the boss's lane, at the screen's vertical middle.
            var centre = new Vector2(_rigidbody.position.x, ScreenCentre().y);
            yield return MoveTo(centre, _verticalSpeed);

            bool forward = true;

            while (true)
            {
                yield return TraceFigureEight(centre, forward);
                forward = !forward;
                yield return new WaitForSeconds(_pauseBetweenPaths);
            }
        }

        // Vertical figure 8: x = w/2 sin(2t), y = h/2 sin(t). It starts and ends at the centre,
        // so running t backwards retraces the same path in reverse.
        private IEnumerator TraceFigureEight(Vector2 centre, bool forward)
        {
            float halfWidth = _figureEightWidth * 0.5f;
            float halfHeight = Mathf.Min(_figureEightHeight * 0.5f, MaxOffsetY());
            float elapsed = 0f;

            while (elapsed < _figureEightDuration)
            {
                yield return new WaitForFixedUpdate();
                elapsed = Mathf.Min(elapsed + Time.fixedDeltaTime, _figureEightDuration);

                float progress = elapsed / _figureEightDuration;
                float t = 2f * Mathf.PI * (forward ? progress : 1f - progress);
                _rigidbody.MovePosition(centre + new Vector2(halfWidth * Mathf.Sin(2f * t), halfHeight * Mathf.Sin(t)));
            }
        }

        private IEnumerator MoveTo(Vector2 target, float speed)
        {
            while ((_rigidbody.position - target).sqrMagnitude > 0.0001f)
            {
                yield return new WaitForFixedUpdate();
                _rigidbody.MovePosition(Vector2.MoveTowards(_rigidbody.position, target, speed * Time.fixedDeltaTime));
            }
        }

        private Vector2 ScreenCentre()
        {
            return _camera != null ? (Vector2)_camera.transform.position : Vector2.zero;
        }

        private float MaxOffsetY()
        {
            float halfHeight = _camera != null && _camera.orthographic ? _camera.orthographicSize : 5f;
            return Mathf.Max(0f, halfHeight - _screenMargin);
        }
    }
}
