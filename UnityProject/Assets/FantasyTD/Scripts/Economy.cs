using System;
using UnityEngine;

namespace FantasyTD.MapA
{
    public sealed class Economy : MonoBehaviour
    {
        public int Essence { get; private set; }
        public event Action<int> Changed;

        public void Initialize(int amount)
        {
            Essence = Mathf.Max(0, amount);
            Changed?.Invoke(Essence);
        }

        public bool TrySpend(int amount)
        {
            if (amount <= 0 || Essence < amount) return false;
            Essence -= amount;
            Changed?.Invoke(Essence);
            return true;
        }

        public void Add(int amount)
        {
            if (amount <= 0) return;
            Essence += amount;
            Changed?.Invoke(Essence);
        }
    }
}
