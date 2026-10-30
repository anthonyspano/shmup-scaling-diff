using System.Collections.Generic;
using UnityEngine;

namespace ScalingShmup.Enemies.Bosses
{
    // Every frame, points the boss's turrets that are closest to the player straight at the
    // player. The rest keep sweeping. Shot-down turrets are skipped, so the next closest take over.
    // Put it on the boss; the aimed fire rate is set on each BossTurret.
    public class BossTurretTargeting : MonoBehaviour
    {
        [Tooltip("How many of the closest turrets aim at the player at once.")]
        [SerializeField, Min(0)] private int _aimingTurretCount = 2;
        [Tooltip("Defaults to the object tagged Player.")]
        [SerializeField] private Transform _player;

        private BossTurret[] _turrets;
        private readonly List<BossTurret> _candidates = new List<BossTurret>();

        private void Awake()
        {
            // Include inactive turrets: downed turrets are switched off and can revive.
            _turrets = GetComponentsInChildren<BossTurret>(true);

            if (_player == null)
            {
                GameObject player = GameObject.FindWithTag("Player");

                if (player != null)
                {
                    _player = player.transform;
                }
            }
        }

        private void OnDisable()
        {
            foreach (BossTurret turret in _turrets)
            {
                turret.Target = null;
            }
        }

        private void Update()
        {
            _candidates.Clear();

            foreach (BossTurret turret in _turrets)
            {
                turret.Target = null;

                if (turret.gameObject.activeInHierarchy)
                {
                    _candidates.Add(turret);
                }
            }

            // No one to aim at while the player is respawning or gone.
            if (_player == null || !_player.gameObject.activeInHierarchy)
            {
                return;
            }

            Vector2 playerPosition = _player.position;
            _candidates.Sort((a, b) =>
                ((Vector2)a.transform.position - playerPosition).sqrMagnitude
                    .CompareTo(((Vector2)b.transform.position - playerPosition).sqrMagnitude));

            for (int i = 0; i < _candidates.Count && i < _aimingTurretCount; i++)
            {
                _candidates[i].Target = _player;
            }
        }
    }
}
