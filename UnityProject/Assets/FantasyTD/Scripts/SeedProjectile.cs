using UnityEngine;

namespace FantasyTD.MapA
{
    public sealed class SeedProjectile : MonoBehaviour
    {
        private EnemyUnit _target;
        private float _damage;
        private float _speed;
        private DamageType _type;
        private float _slow;
        private float _slowDuration;

        public void Initialize(EnemyUnit target, float damage, float speed, DamageType type, float slow = 0f, float slowDuration = 0f)
        {
            _target = target;
            _damage = damage;
            _speed = speed;
            _type = type;
            _slow = slow;
            _slowDuration = slowDuration;
        }

        private void Update()
        {
            if (_target == null || _target.IsDead)
            {
                Destroy(gameObject);
                return;
            }
            Vector2 current = transform.position;
            Vector2 target = _target.transform.position;
            Vector2 next = Vector2.MoveTowards(current, target, _speed * Time.deltaTime);
            // Segment collision runs before impact damage, so a fast projectile
            // cannot tunnel through an active authored MAP4 bubble.
            if (Map4BubbleHazard.TryAbsorbSegment(current, next))
            {
                Destroy(gameObject);
                return;
            }
            transform.position = new Vector3(next.x, next.y, -0.35f);
            Vector2 direction = target - current;
            if (direction.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f);
            if (Vector2.Distance(next, target) <= 0.10f)
            {
                RuntimeVfx.Impact(next, _type);
                _target.TakeDamage(_damage, _type);
                if (_slow > 0f) _target.ApplySlow(_slow, _slowDuration);
                Destroy(gameObject);
            }
        }
    }
}
