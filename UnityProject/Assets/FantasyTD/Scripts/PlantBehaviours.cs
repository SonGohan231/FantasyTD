using UnityEngine;

namespace FantasyTD.MapA
{
    public abstract class PlantBehaviour : MonoBehaviour
    {
        protected GameSession Session;
        protected PlantInstance Instance;

        public void Initialize(GameSession session)
        {
            Session = session;
        }

        public void Initialize(GameSession session, PlantInstance instance)
        {
            Session = session;
            Instance = instance;
        }

        protected float DamageMultiplier => Instance != null ? Instance.DamageMultiplier : 1f;
        protected float AttackSpeedMultiplier => Instance != null ? Instance.AttackSpeedMultiplier : 1f;
        protected float RangeBonus => Instance != null ? Instance.RangeBonus : 0f;
        protected float ControlMultiplier => Instance != null ? Instance.ControlMultiplier : 1f;
        protected float EconomyMultiplier => Instance != null ? Instance.EconomyMultiplier : 1f;
        protected int Level => Instance != null ? Instance.Level : 1;
    }

    public sealed class SeedShooterPlant : PlantBehaviour
    {
        private float _cooldown;

        private void Update()
        {
            if (Session == null || Session.IsFinished) return;
            _cooldown -= Time.deltaTime;
            if (_cooldown > 0f) return;

            float range = 3.2f + RangeBonus;
            EnemyUnit target = Targeting.FindFirst(Session, transform.position, range);
            if (target == null) return;

            Shoot(target, new Color(0.57f,0.86f,0.20f), 22f * DamageMultiplier, DamageType.Physical, 0f, 0f);
            _cooldown = 1f / AttackSpeedMultiplier;
        }

        private void Shoot(EnemyUnit target, Color color, float damage, DamageType type, float slow, float slowDuration)
        {
            GameObject seed = RuntimeArt.SpriteObject(
                "MagicSeed", null, transform.position, new Vector2(0.18f,0.18f), color, 50);
            RuntimeVfx.AttachProjectile(seed, color, false);
            SeedProjectile projectile = seed.AddComponent<SeedProjectile>();
            projectile.Initialize(target,damage,7f,type,slow,slowDuration);
        }
    }

    public sealed class EssenceFlowerPlant : PlantBehaviour
    {
        private float _timer = 20f;

        private void Update()
        {
            if (Session == null || Session.IsFinished) return;
            _timer -= Time.deltaTime;
            if (_timer > 0f) return;

            int income = Mathf.RoundToInt(20f * EconomyMultiplier);
            Session.Economy.Add(income);
            RuntimeVfx.EssencePulse(transform.position);
            Session.Hud.ShowFloatingMessage(transform.position, "+" + income + " Esencji", new Color(1f,0.86f,0.22f));
            _timer += 20f / (1f + 0.08f * (Level - 1));
        }
    }

    public sealed class OakGuardianPlant : PlantBehaviour
    {
        private float _cooldown = 1f;

        private void Update()
        {
            if (Session == null || Session.IsFinished) return;
            _cooldown -= Time.deltaTime;
            if (_cooldown > 0f) return;

            EnemyUnit target = Targeting.FindFirst(Session, transform.position, 2.4f + RangeBonus);
            if (target == null) return;

            target.ApplyRoot(1.75f * ControlMultiplier);
            target.TakeDamage(8f * DamageMultiplier, DamageType.Root);
            RuntimeVfx.RootBurst(target.transform.position);
            Session.Hud.ShowFloatingMessage(target.transform.position, "KORZENIE!", new Color(0.46f,0.84f,0.24f));
            _cooldown = 5f / AttackSpeedMultiplier;
        }
    }

    public sealed class FrostBudPlant : PlantBehaviour
    {
        private float _cooldown;

        private void Update()
        {
            if (Session == null || Session.IsFinished) return;
            _cooldown -= Time.deltaTime;
            if (_cooldown > 0f) return;

            EnemyUnit target = Targeting.FindFirst(Session, transform.position, 3.2f + RangeBonus);
            if (target == null) return;

            Color frost = new Color(0.50f,0.90f,1f);
            GameObject seed = RuntimeArt.SpriteObject(
                "FrostSeed", null, transform.position, new Vector2(0.18f,0.18f), frost, 50);
            RuntimeVfx.AttachProjectile(seed, frost, true);
            SeedProjectile projectile = seed.AddComponent<SeedProjectile>();
            projectile.Initialize(target,18f * DamageMultiplier,7f,DamageType.Frost,0.35f,2.5f * ControlMultiplier);
            _cooldown = 1f / AttackSpeedMultiplier;
        }
    }
}
