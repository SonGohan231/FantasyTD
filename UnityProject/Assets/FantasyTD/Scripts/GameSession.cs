using System;
using System.Collections.Generic;
using UnityEngine;

namespace FantasyTD.MapA
{
    public sealed class GameSession : MonoBehaviour
    {
        private readonly List<EnemyUnit> _enemies = new List<EnemyUnit>();
        private int _spawnOrdinal;

        public Economy Economy { get; private set; }
        public BaseHealth Base { get; private set; }
        public WaveDirector Waves { get; private set; }
        public MobileHud Hud { get; private set; }
        public TowerPanelHud TowerPanel { get; private set; }
        public PlantingSystem Planting { get; private set; }
        public IReadOnlyList<EnemyUnit> Enemies => _enemies;
        public CampaignMapDefinition MapDefinition { get; private set; }
        public IReadOnlyList<Vector2> Waypoints { get; private set; }

        public bool IsFinished { get; private set; }
        public bool IsVictory { get; private set; }

        public void Initialize(List<PlantingSpot> spots, CampaignMapDefinition definition = null)
        {
            MapDefinition = definition ?? CampaignMapDefinition.Map1;
            _spawnOrdinal = 0;
            Waypoints = Array.AsReadOnly(SnapshotRoute(
                MapDefinition.Waypoints));
            Economy = gameObject.AddComponent<Economy>();
            Base = gameObject.AddComponent<BaseHealth>();
            Waves = gameObject.AddComponent<WaveDirector>();
            Planting = gameObject.AddComponent<PlantingSystem>();
            Hud = gameObject.AddComponent<MobileHud>();
            TowerPanel = gameObject.AddComponent<TowerPanelHud>();

            Hud.Initialize(this);
            TowerPanel.Initialize(this, Hud.SafeArea);
            FantasyTD.Product.GameplayNavigationHud navigation =
                gameObject.AddComponent<FantasyTD.Product.GameplayNavigationHud>();
            navigation.Initialize(Hud.SafeArea);
            Economy.Initialize(Balance.StartEssence);
            Economy.Changed += delegate
            {
                Hud.Refresh();
                TowerPanel.Refresh();
            };
            Base.Initialize(this,Balance.BaseHp);
            Planting.Initialize(this,spots);
            Waves.Initialize(this);
            Hud.Refresh();
        }

        public EnemyUnit SpawnEnemy(EnemyKind kind, Vector2[] route = null)
        {
            // Validate and copy an explicit graph lane before creating scene objects.
            // Existing waves inherit the session route without changing spawn order.
            Vector2[] routeSnapshot = SnapshotRoute(route ?? MapDefinition.GetSpawnRoute(_spawnOrdinal++));
            GameObject go = FantasyVisualFactory.CreateEnemy(kind);
            EnemyUnit enemy = go.AddComponent<EnemyUnit>();
            enemy.Initialize(this, kind, routeSnapshot);
            return enemy;
        }

        internal static Vector2[] SnapshotRoute(IReadOnlyList<Vector2> route)
        {
            if (route == null || route.Count < 2)
                throw new ArgumentException("An enemy route requires at least two waypoints.", nameof(route));

            Vector2[] snapshot = new Vector2[route.Count];
            bool hasLength = false;
            for (int i = 0; i < route.Count; i++)
            {
                Vector2 point = route[i];
                if (float.IsNaN(point.x) || float.IsInfinity(point.x) ||
                    float.IsNaN(point.y) || float.IsInfinity(point.y))
                    throw new ArgumentException("Enemy route waypoints must be finite.", nameof(route));
                snapshot[i] = point;
                if (i > 0 && Vector2.Distance(snapshot[i - 1], point) > 0.001f)
                    hasLength = true;
            }
            if (!hasLength)
                throw new ArgumentException("An enemy route must have a nonzero length.", nameof(route));
            return snapshot;
        }

        public void RegisterEnemy(EnemyUnit enemy)
        {
            if (enemy != null && !_enemies.Contains(enemy))
                _enemies.Add(enemy);
            if (Hud != null) Hud.Refresh();
        }

        public void UnregisterEnemy(EnemyUnit enemy)
        {
            if (enemy != null)
                _enemies.Remove(enemy);
            if (Hud != null) Hud.Refresh();
        }

        public void Victory()
        {
            if (IsFinished) return;
            IsFinished = true;
            IsVictory = true;
            CampaignProgress.RecordVictory(MapDefinition != null ? MapDefinition.Id : "MAP1");
            if (TowerPanel != null) TowerPanel.Close();
            Hud.ShowEndPanel(true);
        }

        public void Defeat()
        {
            if (IsFinished) return;
            IsFinished = true;
            IsVictory = false;
            if (TowerPanel != null) TowerPanel.Close();
            Hud.ShowEndPanel(false);
        }
    }
}
