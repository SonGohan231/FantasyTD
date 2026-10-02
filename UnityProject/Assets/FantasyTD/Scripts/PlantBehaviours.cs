using UnityEngine;

namespace FantasyTD.MapA
{
    public abstract class PlantBehaviour : MonoBehaviour
    {
        protected GameSession Session;
        public void Initialize(GameSession session) { Session = session; }
    }

    public sealed class SeedShooterPlant : PlantBehaviour
    {
        private float _cooldown;

        private void Update()
        {
            if (Session == null || Session.IsFinished) return;
            _cooldown -= Time.deltaTime;
            if (_cooldown > 0f) return;

            EnemyUnit target = Targeting.FindFirst(Session, transform.position, 3.2f);
            if (target == null) return;

            Shoot(target, new Color(0.57f,0.86f,0.20f), 22f, DamageType.Physical, 0f, 0f);
            _cooldown = 1f;
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

            Session.Economy.Add(20);
            RuntimeVfx.EssencePulse(transform.position);
            Session.Hud.ShowFloatingMessage(transform.position, "+20 Esencji", new Color(1f,0.86f,0.22f));
            _timer += 20f;
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

            EnemyUnit target = Targeting.FindFirst(Session, transform.position, 2.4f);
            if (target == null) return;

            target.ApplyRoot(1.75f);
            target.TakeDamage(8f, DamageType.Root);
            RuntimeVfx.RootBurst(target.transform.position);
            Session.Hud.ShowFloatingMessage(target.transform.position, "KORZENIE!", new Color(0.46f,0.84f,0.24f));
            _cooldown = 5f;
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

            EnemyUnit target = Targeting.FindFirst(Session, transform.position, 3.2f);
            if (target == null) return;

            Color frost = new Color(0.50f,0.90f,1f);
            GameObject seed = RuntimeArt.SpriteObject(
                "FrostSeed", null, transform.position, new Vector2(0.18f,0.18f), frost, 50);
            RuntimeVfx.AttachProjectile(seed, frost, true);
            SeedProjectile projectile = seed.AddComponent<SeedProjectile>();
            projectile.Initialize(target,18f,7f,DamageType.Frost,0.35f,2.5f);
            _cooldown = 1f;
        }
    }
}
