using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FantasyTD.V2.Core
{
    /// <summary>Positions are expressed in 1/100 of a map world unit.</summary>
    public struct MapPoint
    {
        public readonly int X;
        public readonly int Y;
        public MapPoint(int x, int y) { X = x; Y = y; }
    }

    public enum Element { Physical, Frost, Nature, Fire, Arcane }

    public sealed class EnemyDefinition
    {
        public readonly string Id;
        public readonly int Health;
        public readonly int SpeedPerTick;
        public readonly int PhysicalResistancePercent;
        public readonly int EssenceReward;
        public readonly int LeakDamage;

        public EnemyDefinition(string id, int health, int speedPerTick,
            int physicalResistancePercent, int essenceReward, int leakDamage)
        {
            if (string.IsNullOrWhiteSpace(id) || health <= 0 || speedPerTick <= 0 ||
                physicalResistancePercent < 0 || physicalResistancePercent > 90 ||
                essenceReward < 0 || leakDamage <= 0)
                throw new ArgumentException("Invalid enemy definition.");
            Id = id;
            Health = health;
            SpeedPerTick = speedPerTick;
            PhysicalResistancePercent = physicalResistancePercent;
            EssenceReward = essenceReward;
            LeakDamage = leakDamage;
        }
    }

    public sealed class TowerDefinition
    {
        public readonly string Id;
        public readonly int Cost;
        public readonly int Damage;
        public readonly Element DamageElement;
        public readonly int RangeCentimeters;
        public readonly int CooldownTicks;
        public readonly int SlowPercent;
        public readonly int SlowTicks;
        public readonly int IncomeAmount;
        public readonly int IncomeEveryTicks;

        public TowerDefinition(string id, int cost, int damage, Element element,
            int rangeCentimeters, int cooldownTicks, int slowPercent = 0,
            int slowTicks = 0, int incomeAmount = 0, int incomeEveryTicks = 0)
        {
            if (string.IsNullOrWhiteSpace(id) || cost < 0 || damage < 0 ||
                rangeCentimeters < 0 || cooldownTicks < 1 ||
                slowPercent < 0 || slowPercent > 90 || slowTicks < 0 ||
                incomeAmount < 0 || incomeEveryTicks < 0)
                throw new ArgumentException("Invalid tower definition.");
            if (slowPercent > 0 && (slowTicks < 1 || damage == 0))
                throw new ArgumentException("Slow must be attached to an attack.");
            if (incomeAmount > 0 && (incomeEveryTicks < 1 || damage > 0))
                throw new ArgumentException("Income tower must have an income interval and no weapon.");
            if (incomeAmount == 0 && damage == 0)
                throw new ArgumentException("Tower must provide income or damage.");
            Id = id; Cost = cost; Damage = damage; DamageElement = element;
            RangeCentimeters = rangeCentimeters; CooldownTicks = cooldownTicks;
            SlowPercent = slowPercent; SlowTicks = slowTicks;
            IncomeAmount = incomeAmount; IncomeEveryTicks = incomeEveryTicks;
        }
    }

    public struct SpawnOrder
    {
        public readonly string EnemyId;
        public readonly int AtTick;
        public SpawnOrder(string enemyId, int atTick)
        {
            if (string.IsNullOrWhiteSpace(enemyId) || atTick < 0)
                throw new ArgumentException("Invalid wave spawn.");
            EnemyId = enemyId;
            AtTick = atTick;
        }
    }

    public sealed class WaveDefinition
    {
        public readonly IReadOnlyList<SpawnOrder> Spawns;
        public WaveDefinition(IList<SpawnOrder> spawns)
        {
            if (spawns == null || spawns.Count == 0)
                throw new ArgumentException("A wave must contain enemies.");
            var copy = new List<SpawnOrder>(spawns);
            // Stable order for simultaneous spawns, independent of dictionary enumeration.
            copy.Sort((a, b) => a.AtTick.CompareTo(b.AtTick));
            Spawns = new ReadOnlyCollection<SpawnOrder>(copy);
        }
    }

    /// <summary>Fully validated immutable simulation input. Views/assets live outside Core.</summary>
    public sealed class LevelDefinition
    {
        public readonly string Id;
        public readonly IReadOnlyList<MapPoint> Path;
        public readonly IReadOnlyDictionary<string, MapPoint> Sites;
        public readonly IReadOnlyDictionary<string, EnemyDefinition> Enemies;
        public readonly IReadOnlyDictionary<string, TowerDefinition> Towers;
        public readonly IReadOnlyList<WaveDefinition> Waves;
        public readonly int StartingEssence;
        public readonly int StartingBaseHp;
        public readonly int PathLength;

        public LevelDefinition(string id, IList<MapPoint> path,
            IDictionary<string, MapPoint> sites,
            IDictionary<string, EnemyDefinition> enemies,
            IDictionary<string, TowerDefinition> towers,
            IList<WaveDefinition> waves, int startingEssence, int startingBaseHp)
        {
            if (string.IsNullOrWhiteSpace(id) || path == null || path.Count < 2 ||
                sites == null || sites.Count == 0 || enemies == null || enemies.Count == 0 ||
                towers == null || towers.Count == 0 || waves == null || waves.Count == 0 ||
                startingEssence < 0 || startingBaseHp < 1)
                throw new ArgumentException("Invalid level content.");
            Id = id;
            Path = new ReadOnlyCollection<MapPoint>(new List<MapPoint>(path));
            Sites = new ReadOnlyDictionary<string, MapPoint>(
                new Dictionary<string, MapPoint>(sites, StringComparer.Ordinal));
            Enemies = new ReadOnlyDictionary<string, EnemyDefinition>(
                new Dictionary<string, EnemyDefinition>(enemies, StringComparer.Ordinal));
            Towers = new ReadOnlyDictionary<string, TowerDefinition>(
                new Dictionary<string, TowerDefinition>(towers, StringComparer.Ordinal));
            Waves = new ReadOnlyCollection<WaveDefinition>(new List<WaveDefinition>(waves));
            StartingEssence = startingEssence;
            StartingBaseHp = startingBaseHp;

            long length = 0;
            for (int i = 1; i < Path.Count; i++)
            {
                long dx = (long)Path[i].X - Path[i - 1].X;
                long dy = (long)Path[i].Y - Path[i - 1].Y;
                long squared = dx * dx + dy * dy;
                if (squared == 0) throw new ArgumentException("Zero-length path segment.");
                length += (long)Math.Round(Math.Sqrt(squared));
            }
            if (length < 1 || length > int.MaxValue)
                throw new ArgumentException("Invalid path length.");
            PathLength = (int)length;

            foreach (var site in Sites)
                if (string.IsNullOrWhiteSpace(site.Key))
                    throw new ArgumentException("Build site id must not be empty.");
            foreach (var entry in Towers)
                if (entry.Value == null || !string.Equals(entry.Key, entry.Value.Id, StringComparison.Ordinal))
                    throw new ArgumentException("Tower id does not match key.");
            foreach (var entry in Enemies)
                if (entry.Value == null || !string.Equals(entry.Key, entry.Value.Id, StringComparison.Ordinal))
                    throw new ArgumentException("Enemy id does not match key.");
            foreach (var wave in Waves)
            {
                if (wave == null) throw new ArgumentException("Null wave.");
                foreach (var spawn in wave.Spawns)
                    if (!Enemies.ContainsKey(spawn.EnemyId))
                        throw new ArgumentException("Wave references unknown enemy: " + spawn.EnemyId);
            }
        }

        public MapPoint AtDistance(int distance)
        {
            if (distance <= 0) return Path[0];
            if (distance >= PathLength) return Path[Path.Count - 1];

            int travelled = 0;
            for (int i = 1; i < Path.Count; i++)
            {
                MapPoint a = Path[i - 1], b = Path[i];
                long dx = (long)b.X - a.X, dy = (long)b.Y - a.Y;
                int segment = (int)Math.Round(Math.Sqrt(dx * dx + dy * dy));
                if (distance <= travelled + segment)
                {
                    int d = distance - travelled;
                    return new MapPoint(a.X + (int)(dx * d / segment),
                                        a.Y + (int)(dy * d / segment));
                }
                travelled += segment;
            }
            return Path[Path.Count - 1];
        }
    }
}
