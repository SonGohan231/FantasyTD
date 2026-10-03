using System;
using System.Collections.Generic;
using UnityEngine;

namespace FantasyTD.MapA
{
    public sealed class EnemyUnit : MonoBehaviour
    {
        private GameSession _session;
        private Vector2[] _route;
        private EnemyKind _kind;
        private int _segmentIndex;
        private float _hp;
        private float _maxHp;
        private float _speed;
        private int _reward;
        private int _baseDamage;
        private float _physicalResistance;
        private float _slowStrength;
        private float _slowUntil;
        private float _rootUntil;
        private bool _dead;
        private float _progressDistance;
        private EnemyHealthBar _healthBar;
        private HiggsfieldUnitAnimation _animation;

        public bool IsDead => _dead;
        public float ProgressDistance => _progressDistance;
        public EnemyKind Kind => _kind;
        public float CurrentHp => Mathf.Max(0f, _hp);
        public float MaxHp => _maxHp;
        public bool IsBoss => _kind == EnemyKind.Boss;
        public bool IsRooted => !_dead && Time.time < _rootUntil;
        public bool IsSlowed => !_dead && !IsRooted && Time.time < _slowUntil;

        public void Initialize(GameSession session, EnemyKind kind, IReadOnlyList<Vector2> route = null)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));
            // An enemy owns its complete immutable lane; later graph choices cannot
            // move an already spawned enemy to another branch.
            _route = GameSession.SnapshotRoute(route ?? session.Waypoints ?? MapAData.Waypoints);
            _session = session;
            _kind = kind;
            _animation = GetComponent<HiggsfieldUnitAnimation>();
            Configure(kind);
            _maxHp = _hp;
            _segmentIndex = 0;
            _dead = false;
            _progressDistance = 0f;

            Vector2 start = _route[0];
            transform.position = new Vector3(start.x,start.y,-0.20f);

            _healthBar = gameObject.AddComponent<EnemyHealthBar>();
            _healthBar.Initialize(this, kind == EnemyKind.Boss);

            _session.RegisterEnemy(this);
        }

        private void Configure(EnemyKind kind)
        {
            switch (kind)
            {
                case EnemyKind.Wanderer:
                    _hp=170f; _speed=0.80f; _reward=12; _baseDamage=1; _physicalResistance=0f; break;
                case EnemyKind.Runner:
                    _hp=110f; _speed=1.15f; _reward=12; _baseDamage=1; _physicalResistance=0f; break;
                case EnemyKind.Brute:
                    _hp=550f; _speed=0.45f; _reward=26; _baseDamage=2; _physicalResistance=0f; break;
                case EnemyKind.Armored:
                    _hp=260f; _speed=0.55f; _reward=22; _baseDamage=1; _physicalResistance=0.15f; break;
                case EnemyKind.Boss:
                    _hp=2200f; _speed=0.30f; _reward=150; _baseDamage=5; _physicalResistance=0.20f; break;
            }
        }

        private void Update()
        {
            if (_dead || _session == null) return;
            if (_session.IsFinished)
            {
                _animation?.SetMovement(Vector2.zero, 0f);
                return;
            }

            float speedMultiplier = 1f;
            if (Time.time < _rootUntil)
                speedMultiplier = 0f;
            else if (Time.time < _slowUntil)
                speedMultiplier = 1f - _slowStrength;

            Vector2 direction = _segmentIndex < _route.Length - 1
                ? _route[_segmentIndex + 1] - (Vector2)transform.position : Vector2.zero;
            _animation?.SetMovement(direction, speedMultiplier);
            Move(_speed * speedMultiplier * Time.deltaTime);
        }

        public void ApplySlow(float strength, float duration)
        {
            if (_kind == EnemyKind.Boss)
                strength = Mathf.Min(strength, 0.20f);

            if (strength > _slowStrength || Time.time >= _slowUntil)
                _slowStrength = Mathf.Clamp01(strength);

            _slowUntil = Mathf.Max(_slowUntil, Time.time + duration);
        }

        public void ApplyRoot(float duration)
        {
            if (_kind == EnemyKind.Boss)
                duration *= 0.35f;
            _rootUntil = Mathf.Max(_rootUntil, Time.time + duration);
        }

        public void TakeDamage(float amount, DamageType damageType)
        {
            if (_dead || amount <= 0f) return;

            if (damageType == DamageType.Physical)
                amount *= 1f - _physicalResistance;

            _animation?.PlayHit();
            _hp -= amount;
            if (_healthBar != null)
                _healthBar.Refresh();

            if (_hp <= 0f)
                Die();
        }

        private void Move(float distance)
        {
            while (distance > 0f && _segmentIndex < _route.Length - 1)
            {
                Vector2 current = transform.position;
                Vector2 target = _route[_segmentIndex + 1];
                float remaining = Vector2.Distance(current, target);

                if (remaining < 0.001f)
                {
                    _segmentIndex++;
                    continue;
                }

                if (distance < remaining)
                {
                    Vector2 next = Vector2.MoveTowards(current, target, distance);
                    transform.position = new Vector3(next.x,next.y,-0.20f);
                    _progressDistance += distance;
                    return;
                }

                transform.position = new Vector3(target.x,target.y,-0.20f);
                _progressDistance += remaining;
                distance -= remaining;
                _segmentIndex++;
            }

            if (_segmentIndex >= _route.Length - 1)
                ReachBase();
        }

        private void Die()
        {
            if (_dead) return;
            _dead = true;
            _session.Economy.Add(_reward);
            _session.UnregisterEnemy(this);
            if (_healthBar != null)
                foreach (Canvas canvas in GetComponentsInChildren<Canvas>()) canvas.gameObject.SetActive(false);
            _animation?.PlayDeath();
            // Gameplay removal/reward is immediate; only imported visual art fades out.
            Destroy(gameObject, _animation != null ? 0.24f : 0f);
        }

        private void ReachBase()
        {
            if (_dead) return;
            _dead = true;
            _session.Base.Damage(_baseDamage);
            _session.UnregisterEnemy(this);
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (_session != null)
                _session.UnregisterEnemy(this);
        }
    }
}
