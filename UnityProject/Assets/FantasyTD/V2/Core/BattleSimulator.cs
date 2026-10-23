using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FantasyTD.V2.Core
{
    public enum BattleResult { InProgress, Victory, Defeat }
    public enum BattleEventKind
    {
        WaveStarted, WaveCleared, EnemySpawned, EnemyDamaged,
        EnemyKilled, EnemyLeaked, TowerPlaced, TowerUpgraded, Income, Victory, Defeat
    }

    public struct BattleEvent
    {
        public readonly BattleEventKind Kind;
        public readonly int EntityId;
        public readonly string ContentId;
        public readonly int Value;
        public readonly int WaveNumber;

        public BattleEvent(BattleEventKind kind, int entityId, string contentId,
            int value, int waveNumber)
        {
            Kind = kind; EntityId = entityId; ContentId = contentId;
            Value = value; WaveNumber = waveNumber;
        }
    }

    public sealed class EnemyState
    {
        public int InstanceId { get; private set; }
        public EnemyDefinition Definition { get; private set; }
        public int Health { get; internal set; }
        public int Distance { get; internal set; }
        public int SlowPercent { get; internal set; }
        public int SlowRemainingTicks { get; internal set; }
        internal bool Leaked;

        internal EnemyState(int id, EnemyDefinition definition)
        {
            InstanceId = id; Definition = definition;
            Health = definition.Health;
        }
    }

    public sealed class TowerState
    {
        public const int MaximumLevel = 3;
        public string SiteId { get; private set; }
        public TowerDefinition Definition { get; private set; }
        public MapPoint Position { get; private set; }
        public int Level { get; internal set; }
        public int UpgradeCost
        {
            get { return Level >= MaximumLevel ? 0 : Math.Max(20, Definition.Cost * Level / 2); }
        }
        public int Damage
        {
            get { return Definition.Damage * (100 + (Level - 1) * 45) / 100; }
        }
        public int RangeCentimeters
        {
            get { return Definition.RangeCentimeters + (Level - 1) * 35; }
        }
        public int CooldownTicks
        {
            get { return Math.Max(1, Definition.CooldownTicks - (Level - 1)); }
        }
        public int IncomeAmount
        {
            get { return Definition.IncomeAmount * (100 + (Level - 1) * 50) / 100; }
        }
        internal int NextAttackTick;
        internal int NextIncomeTick;

        internal TowerState(string site, TowerDefinition def, MapPoint pos, int currentTick)
        {
            SiteId = site; Definition = def; Position = pos;
            Level = 1;
            NextAttackTick = currentTick;
            NextIncomeTick = currentTick + def.IncomeEveryTicks;
        }
    }

    /// <summary>
    /// Explicit 20-Hz fixed-step combat model. No MonoBehaviours, asset code,
    /// wall-clock time, Unity random state or per-frame input dependencies.
    /// Values and targeting are integer-based and deterministic for identical commands.
    /// </summary>
    public sealed class BattleSimulator
    {
        public const int TicksPerSecond = 20;
        private readonly LevelDefinition _level;
        private readonly List<EnemyState> _enemies = new List<EnemyState>();
        private readonly ReadOnlyCollection<EnemyState> _enemyViews;
        private readonly Dictionary<string, TowerState> _occupied =
            new Dictionary<string, TowerState>(StringComparer.Ordinal);
        private readonly List<TowerState> _towerOrder = new List<TowerState>();

        private int _waveIndex = -1;
        private int _waveTick;
        private int _spawnCursor;
        private int _nextEnemyId = 1;
        private bool _waveActive;

        public event Action<BattleEvent> EventRaised;
        public LevelDefinition Level { get { return _level; } }
        public IReadOnlyList<EnemyState> Enemies { get { return _enemyViews; } }
        public IReadOnlyDictionary<string, TowerState> PlacedTowers
        {
            get { return new ReadOnlyDictionary<string, TowerState>(_occupied); }
        }
        public int Essence { get; private set; }
        public int BaseHp { get; private set; }
        public int TotalTicks { get; private set; }
        public int WaveNumber { get { return _waveIndex + 1; } }
        public int TotalWaves { get { return _level.Waves.Count; } }
        public int Kills { get; private set; }
        public int Leaks { get; private set; }
        public BattleResult Result { get; private set; }
        public bool WaveActive { get { return _waveActive; } }
        public bool CanStartWave
        {
            get { return Result == BattleResult.InProgress &&
                         !_waveActive && _waveIndex + 1 < _level.Waves.Count; }
        }

        public BattleSimulator(LevelDefinition level)
        {
            if (level == null) throw new ArgumentNullException("level");
            _level = level;
            _enemyViews = _enemies.AsReadOnly();
            Essence = level.StartingEssence;
            BaseHp = level.StartingBaseHp;
            Result = BattleResult.InProgress;
        }

        public bool TryPlaceTower(string siteId, string towerId)
        {
            if (Result != BattleResult.InProgress ||
                string.IsNullOrEmpty(siteId) || string.IsNullOrEmpty(towerId))
                return false;
            MapPoint pos;
            TowerDefinition def;
            if (!_level.Sites.TryGetValue(siteId, out pos) ||
                !_level.Towers.TryGetValue(towerId, out def) ||
                _occupied.ContainsKey(siteId) || Essence < def.Cost)
                return false;
            Essence -= def.Cost;
            var tower = new TowerState(siteId, def, pos, TotalTicks);
            _occupied.Add(siteId, tower);
            _towerOrder.Add(tower);
            Emit(BattleEventKind.TowerPlaced, 0, towerId, -def.Cost);
            return true;
        }

        public bool TryUpgradeTower(string siteId)
        {
            TowerState tower;
            if (Result != BattleResult.InProgress || string.IsNullOrEmpty(siteId) ||
                !_occupied.TryGetValue(siteId, out tower) ||
                tower.Level >= TowerState.MaximumLevel || Essence < tower.UpgradeCost)
                return false;
            int cost = tower.UpgradeCost;
            Essence -= cost;
            tower.Level++;
            if (tower.Definition.Damage > 0)
                tower.NextAttackTick = Math.Min(tower.NextAttackTick, TotalTicks);
            Emit(BattleEventKind.TowerUpgraded, 0, tower.Definition.Id, -cost);
            return true;
        }

        public bool StartNextWave()
        {
            if (!CanStartWave) return false;
            _waveIndex++;
            _spawnCursor = 0;
            _waveTick = 0;
            _waveActive = true;
            Emit(BattleEventKind.WaveStarted, 0, _level.Id, WaveNumber);
            return true;
        }

        public void Step()
        {
            if (!_waveActive || Result != BattleResult.InProgress)
                return;

            // Stable spawn order even if several enemies share the same schedule tick.
            var orders = _level.Waves[_waveIndex].Spawns;
            while (_spawnCursor < orders.Count && orders[_spawnCursor].AtTick <= _waveTick)
            {
                EnemyDefinition def = _level.Enemies[orders[_spawnCursor].EnemyId];
                var enemy = new EnemyState(_nextEnemyId++, def);
                _enemies.Add(enemy);
                Emit(BattleEventKind.EnemySpawned, enemy.InstanceId, def.Id, def.Health);
                _spawnCursor++;
            }

            // Generate passive income on fixed global game ticks.
            for (int i = 0; i < _towerOrder.Count; i++)
            {
                var tower = _towerOrder[i];
                if (tower.IncomeAmount == 0) continue;
                while (TotalTicks >= tower.NextIncomeTick)
                {
                    Essence += tower.IncomeAmount;
                    tower.NextIncomeTick += tower.Definition.IncomeEveryTicks;
                    Emit(BattleEventKind.Income, 0, tower.Definition.Id,
                        tower.IncomeAmount);
                }
            }

            for (int i = 0; i < _enemies.Count; i++)
            {
                EnemyState enemy = _enemies[i];
                if (enemy.Health <= 0) continue;
                int modifier = 100 - enemy.SlowPercent;
                int displacement = Math.Max(1, enemy.Definition.SpeedPerTick * modifier / 100);
                enemy.Distance += displacement;
                if (enemy.SlowRemainingTicks > 0)
                {
                    enemy.SlowRemainingTicks--;
                    if (enemy.SlowRemainingTicks == 0) enemy.SlowPercent = 0;
                }

                if (enemy.Distance >= _level.PathLength)
                {
                    enemy.Leaked = true;
                    enemy.Health = 0;
                    Leaks++;
                    BaseHp = Math.Max(0, BaseHp - enemy.Definition.LeakDamage);
                    Emit(BattleEventKind.EnemyLeaked, enemy.InstanceId,
                        enemy.Definition.Id, enemy.Definition.LeakDamage);
                }
            }

            if (BaseHp == 0)
            {
                _waveActive = false;
                Result = BattleResult.Defeat;
                Emit(BattleEventKind.Defeat, 0, _level.Id, 0);
                return;
            }

            // Each defender chooses the furthest progressing enemy in range;
            // equal-distance ties are broken by deterministic spawn ID.
            for (int i = 0; i < _towerOrder.Count; i++)
            {
                var tower = _towerOrder[i];
                if (tower.Damage <= 0 || TotalTicks < tower.NextAttackTick)
                    continue;
                EnemyState target = null;
                long radiusSq = (long)tower.RangeCentimeters * tower.RangeCentimeters;
                for (int j = 0; j < _enemies.Count; j++)
                {
                    var e = _enemies[j];
                    if (e.Health <= 0) continue;
                    MapPoint enemyPoint = _level.AtDistance(e.Distance);
                    long dx = (long)enemyPoint.X - tower.Position.X;
                    long dy = (long)enemyPoint.Y - tower.Position.Y;
                    if (dx * dx + dy * dy > radiusSq) continue;
                    if (target == null || e.Distance > target.Distance ||
                        (e.Distance == target.Distance && e.InstanceId < target.InstanceId))
                        target = e;
                }
                if (target == null) continue;

                int damage = tower.Damage;
                if (tower.Definition.DamageElement == Element.Physical)
                    damage = Math.Max(1, damage *
                        (100 - target.Definition.PhysicalResistancePercent) / 100);
                target.Health = Math.Max(0, target.Health - damage);
                tower.NextAttackTick = TotalTicks + tower.CooldownTicks;
                Emit(BattleEventKind.EnemyDamaged, target.InstanceId,
                    tower.Definition.Id, damage);

                if (target.Health > 0 && tower.Definition.SlowPercent > 0 &&
                    (tower.Definition.SlowPercent >= target.SlowPercent ||
                     target.SlowRemainingTicks <= 0))
                {
                    target.SlowPercent = tower.Definition.SlowPercent;
                    target.SlowRemainingTicks = tower.Definition.SlowTicks;
                }
            }

            // All reward payouts happen once, in fixed enemy insertion order.
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                EnemyState enemy = _enemies[i];
                if (enemy.Health > 0) continue;
                if (!enemy.Leaked)
                {
                    Essence += enemy.Definition.EssenceReward;
                    Kills++;
                    Emit(BattleEventKind.EnemyKilled, enemy.InstanceId,
                        enemy.Definition.Id, enemy.Definition.EssenceReward);
                }
                _enemies.RemoveAt(i);
            }

            _waveTick++;
            TotalTicks++;
            if (_spawnCursor == orders.Count && _enemies.Count == 0)
            {
                _waveActive = false;
                Emit(BattleEventKind.WaveCleared, 0, _level.Id, WaveNumber);
                if (_waveIndex == _level.Waves.Count - 1)
                {
                    Result = BattleResult.Victory;
                    Emit(BattleEventKind.Victory, 0, _level.Id, BaseHp);
                }
            }
        }

        private void Emit(BattleEventKind kind, int instanceId, string contentId, int value)
        {
            var listener = EventRaised;
            if (listener != null)
                listener(new BattleEvent(kind, instanceId, contentId, value, WaveNumber));
        }
    }
}
