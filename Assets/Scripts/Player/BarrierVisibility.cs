using ScalingShmup.Combat;
using UnityEngine;

namespace ScalingShmup.Player
{
    // Shows the barrier only while the player has overshield left. Lives on the Player rather
    // than the barrier, because a disabled barrier could not switch itself back on.
    [RequireComponent(typeof(Health))]
    public class BarrierVisibility : MonoBehaviour
    {
        [SerializeField] private GameObject _barrier;

        private Health _health;

        private void Awake()
        {
            _health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            _health.OvershieldChanged += OnOvershieldChanged;
        }

        private void OnDisable()
        {
            _health.OvershieldChanged -= OnOvershieldChanged;
        }

        // Health raises no event for its starting value, so apply it once everything has woken up.
        private void Start()
        {
            OnOvershieldChanged(_health.CurrentOvershield, _health.MaxOvershield);
        }

        private void OnOvershieldChanged(float current, float max)
        {
            if (_barrier != null)
            {
                _barrier.SetActive(current > 0f);
            }
        }
    }
}
