using UnityEngine;

namespace FantasyTD.MapA
{
    public sealed class BaseHealth : MonoBehaviour
    {
        private GameSession _session;
        public int Hp { get; private set; }
        public int MaxHp { get; private set; }

        public void Initialize(GameSession session, int hp)
        {
            _session = session;
            MaxHp = hp;
            Hp = hp;
        }

        public void Damage(int amount)
        {
            if (_session == null || _session.IsFinished) return;
            Hp = Mathf.Max(0, Hp - Mathf.Max(0, amount));
            _session.Hud.Refresh();
            if (Hp <= 0) _session.Defeat();
        }
    }
}
