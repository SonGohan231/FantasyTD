using UnityEngine;

namespace FantasyTD.MapA
{
    public static class Targeting
    {
        public static EnemyUnit FindFirst(GameSession session, Vector2 origin, float range)
        {
            EnemyUnit best = null;
            float bestProgress = float.NegativeInfinity;
            float rangeSq = range * range;

            var enemies = session.Enemies;
            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyUnit candidate = enemies[i];
                if (candidate == null || candidate.IsDead) continue;

                Vector2 d = (Vector2)candidate.transform.position - origin;
                if (d.sqrMagnitude > rangeSq) continue;

                if (candidate.ProgressDistance > bestProgress)
                {
                    best = candidate;
                    bestProgress = candidate.ProgressDistance;
                }
            }

            return best;
        }
    }
}
