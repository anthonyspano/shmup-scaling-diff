using UnityEngine;

namespace ScalingShmup.Combat
{
    // Damages whatever it touches, e.g. an enemy diving into the player.
    [RequireComponent(typeof(Collider2D))]
    public class ContactDamage : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _damage = 1f;
        [SerializeField] private DamageType _damageType = DamageType.Contact;
        [SerializeField] private LayerMask _targetLayers;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if ((_targetLayers.value & (1 << other.gameObject.layer)) == 0)
            {
                return;
            }

            IDamageable target = other.GetComponentInParent<IDamageable>();
            target?.TakeDamage(new DamageInfo(_damage, _damageType, gameObject));
        }
    }
}
