using System.Collections.Generic;
using UnityEngine;

namespace FantasyTD.MapA
{
    public sealed class GameSession : MonoBehaviour
    {
        private readonly List<EnemyUnit> _enemies = new List<EnemyUnit>();

        public Economy Economy { get; private set; }
        public BaseHealth Base { get; private set; }
        public WaveDirector Waves { get; private set; }
        public MobileHud Hud { get; private set; }
        public TowerPanelHud TowerPanel { get; private set; }
        public PlantingSystem Planting { get; private set; }
        public IReadOnlyList<EnemyUnit> Enemies => _enemies;

        public bool IsFinished { get; private set; }
        public bool IsVictory { get; private set; }

        public void Initialize(List<PlantingSpot> spots)
        {
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

        public EnemyUnit SpawnEnemy(EnemyKind kind)
        {
            GameObject go = FantasyVisualFactory.CreateEnemy(kind);
            EnemyUnit enemy = go.AddComponent<EnemyUnit>();
            enemy.Initialize(this,kind);
            return enemy;
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
