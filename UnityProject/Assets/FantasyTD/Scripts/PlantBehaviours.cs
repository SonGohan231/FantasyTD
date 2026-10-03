using UnityEngine;

namespace FantasyTD.MapA
{
    public abstract class PlantBehaviour : MonoBehaviour
    {
        protected GameSession Session;
        protected TowerInstance Tower;

        protected int Level => Tower != null ? Tower.Level : 1;

        public void Initialize(GameSession session, TowerInstance tower = null)
        {
            Session = session;
            Tower = tower;
        }

        public virtual void OnTowerLevelChanged(int previousLevel, int newLevel) { }
    }

    public sealed class SeedShooterPlant : PlantBehaviour
    {
        private float _cooldown;

        private void Update()
        {
            if (Session == null || Session.IsFinished) return;
            _cooldown -= Time.deltaTime;
            if (_cooldown > 0f) return;

            float range = TowerUpgradeBalance.SeedRange(Level);
            EnemyUnit target = Targeting.FindFirst(Session, transform.position, range);
            if (target == null) return;

            Shoot(
                target,
                new Color(0.57f,0.86f,0.20f),
                TowerUpgradeBalance.SeedDamage(Level),
                DamageType.Physical,
                0f,
                0f);
            _cooldown = TowerUpgradeBalance.SeedCooldown(Level);
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

            int income = TowerUpgradeBalance.FlowerIncome(Level);
            Session.Economy.Add(income);
            RuntimeVfx.EssencePulse(transform.position);
            Session.Hud.ShowFloatingMessage(
                transform.position,
                "+" + income + " Esencji",
                new Color(1f,0.86f,0.22f));
            _timer += TowerUpgradeBalance.FlowerInterval(Level);
        }

        public override void OnTowerLevelChanged(int previousLevel, int newLevel)
        {
            float oldInterval = TowerUpgradeBalance.FlowerInterval(previousLevel);
            float newInterval = TowerUpgradeBalance.FlowerInterval(newLevel);
            if (oldInterval <= 0f) return;

            float remainingFraction = Mathf.Clamp01(_timer / oldInterval);
            _timer = remainingFraction * newInterval;
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

            EnemyUnit target = Targeting.FindFirst(
                Session,
                transform.position,
                TowerUpgradeBalance.OakRange(Level));
            if (target == null) return;

            target.ApplyRoot(TowerUpgradeBalance.OakRootDuration(Level));
            target.TakeDamage(TowerUpgradeBalance.OakDamage(Level), DamageType.Root);
            RuntimeVfx.RootBurst(target.transform.position);
            Session.Hud.ShowFloatingMessage(
                target.transform.position,
                "KORZENIE!",
                new Color(0.46f,0.84f,0.24f));
            _cooldown = TowerUpgradeBalance.OakCadence(Level);
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

            EnemyUnit target = Targeting.FindFirst(
                Session,
                transform.position,
                TowerUpgradeBalance.FrostRange(Level));
            if (target == null) return;

            Color frost = new Color(0.50f,0.90f,1f);
            GameObject seed = RuntimeArt.SpriteObject(
                "FrostSeed", null, transform.position, new Vector2(0.18f,0.18f), frost, 50);
            RuntimeVfx.AttachProjectile(seed, frost, true);
            SeedProjectile projectile = seed.AddComponent<SeedProjectile>();
            projectile.Initialize(
                target,
                TowerUpgradeBalance.FrostDamage(Level),
                7f,
                DamageType.Frost,
                TowerUpgradeBalance.FrostSlow(Level),
                TowerUpgradeBalance.FrostDuration(Level));
            _cooldown = TowerUpgradeBalance.FrostCooldown(Level);
        }
    }
}
