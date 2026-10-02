using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FantasyTD.MapA
{
    public sealed class WaveDirector : MonoBehaviour
    {
        private GameSession _session;
        private bool _running;
        private bool _ready;
        private int _waveIndex = -1;

        public int CurrentWave => Mathf.Max(0,_waveIndex + 1);
        public int TotalWaves => 6;
        public bool ReadyForNextWave => _ready && !_running && !_session.IsFinished;
        public bool Running => _running;

        public void Initialize(GameSession session)
        {
            _session = session;
            _ready = true;
            _session.Hud.Refresh();
        }

        public void StartNextWave()
        {
            if (!ReadyForNextWave) return;
            _waveIndex++;
            if (_waveIndex >= TotalWaves) return;
            StartCoroutine(RunWave(_waveIndex));
        }

        private IEnumerator RunWave(int index)
        {
            _running = true;
            _ready = false;
            _session.Hud.Refresh();

            List<SpawnOrder> orders = GetWave(index);
            for (int i = 0; i < orders.Count; i++)
            {
                SpawnOrder order = orders[i];
                for (int n = 0; n < order.Count; n++)
                {
                    if (_session.IsFinished) yield break;
                    _session.SpawnEnemy(order.Kind);
                    yield return new WaitForSeconds(order.Interval);
                }
                if (order.GapAfter > 0f)
                    yield return new WaitForSeconds(order.GapAfter);
            }

            while (_session.Enemies.Count > 0 && !_session.IsFinished)
                yield return null;

            _running = false;

            if (_session.IsFinished) yield break;

            if (_waveIndex >= TotalWaves - 1)
                _session.Victory();
            else
            {
                _ready = true;
                _session.Hud.Refresh();
            }
        }

        private List<SpawnOrder> GetWave(int index)
        {
            switch (index)
            {
                case 0:
                    return new List<SpawnOrder> {
                        new SpawnOrder(EnemyKind.Wanderer,6,1.15f,0f)
                    };
                case 1:
                    return new List<SpawnOrder> {
                        new SpawnOrder(EnemyKind.Wanderer,4,1.0f,1f),
                        new SpawnOrder(EnemyKind.Runner,4,0.85f,0f)
                    };
                case 2:
                    return new List<SpawnOrder> {
                        new SpawnOrder(EnemyKind.Wanderer,5,0.95f,1f),
                        new SpawnOrder(EnemyKind.Armored,4,1.20f,0f)
                    };
                case 3:
                    return new List<SpawnOrder> {
                        new SpawnOrder(EnemyKind.Runner,6,0.70f,1f),
                        new SpawnOrder(EnemyKind.Brute,3,1.65f,1f),
                        new SpawnOrder(EnemyKind.Wanderer,4,0.90f,0f)
                    };
                case 4:
                    return new List<SpawnOrder> {
                        new SpawnOrder(EnemyKind.Armored,4,1.0f,0.8f),
                        new SpawnOrder(EnemyKind.Brute,3,1.5f,0.8f),
                        new SpawnOrder(EnemyKind.Runner,8,0.65f,0f)
                    };
                default:
                    return new List<SpawnOrder> {
                        new SpawnOrder(EnemyKind.Armored,4,0.95f,0.6f),
                        new SpawnOrder(EnemyKind.Runner,5,0.65f,0.8f),
                        new SpawnOrder(EnemyKind.Brute,2,1.3f,1.2f),
                        new SpawnOrder(EnemyKind.Boss,1,0.1f,0f)
                    };
            }
        }

        private struct SpawnOrder
        {
            public EnemyKind Kind;
            public int Count;
            public float Interval;
            public float GapAfter;

            public SpawnOrder(EnemyKind kind, int count, float interval, float gapAfter)
            {
                Kind = kind;
                Count = count;
                Interval = interval;
                GapAfter = gapAfter;
            }
        }
    }
}
