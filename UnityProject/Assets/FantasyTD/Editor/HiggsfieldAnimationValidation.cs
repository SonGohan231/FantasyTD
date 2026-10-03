#if UNITY_EDITOR
using System;
using System.Collections;
using FantasyTD.MapA;
using UnityEngine;

namespace FantasyTD.CI
{
    /// <summary>Called inside the existing Play Mode validator; requires an unfinished session.</summary>
    internal static class HiggsfieldAnimationValidation
    {
        internal static IEnumerator Run(GameSession session, Action<bool, string> check, Action<string> fact)
        {
            GameObject actor = FantasyVisualFactory.CreateEnemy(EnemyKind.Wanderer);
            EnemyUnit enemy = actor.AddComponent<EnemyUnit>();
            enemy.Initialize(session, EnemyKind.Wanderer);
            HiggsfieldUnitAnimation animation = actor.GetComponent<HiggsfieldUnitAnimation>();
            check(animation != null && animation.HasAuthoredWalk, "Chochlik requires source-authored walk poses.");
            yield return null;
            int first = animation.WalkFrameIndex;
            float until = Time.time + 0.25f;
            while (Time.time < until) yield return null;
            check(animation.WalkFrameIndex != first, "Moving Chochlik must advance its source pose.");
            fact("animation.walkSprite=" + animation.CurrentSprite.name);

            enemy.ApplyRoot(1f);
            // Allow EnemyUnit.Update to report its zero movement speed before observing pause.
            yield return null;
            yield return null;
            int rootedFrame = animation.WalkFrameIndex;
            until = Time.time + 0.3f;
            while (Time.time < until) yield return null;
            check(animation.WalkFrameIndex == rootedFrame, "Rooted Chochlik must pause its source pose.");
            fact("animation.rootPausedFrame=" + rootedFrame);

            GameObject projectile = RuntimeVfx.CreateProjectile("AnimationValidationSeed", actor.transform.position, false);
            SpriteRenderer renderer = projectile.GetComponent<SpriteRenderer>();
            check(renderer != null && renderer.sprite != null &&
                renderer.sprite.name == "Pedostrzal_AuthoredSeedShot",
                "Projectile must render the authored flying seed crop.");
            check(projectile.GetComponentsInChildren<SpriteRenderer>().Length == 1,
                "Projectile must not contain procedural halos or tails.");
            fact("animation.projectileSprite=" + renderer.sprite.name);
            UnityEngine.Object.Destroy(projectile);
            UnityEngine.Object.Destroy(actor);
            yield return null;
        }
    }
}
#endif
