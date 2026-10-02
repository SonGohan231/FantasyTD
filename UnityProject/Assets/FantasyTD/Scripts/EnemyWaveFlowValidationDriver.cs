#if UNITY_EDITOR
using System;
using UnityEngine;

namespace FantasyTD.CI
{
    public sealed class EnemyWaveFlowValidationDriver : MonoBehaviour
    {
        public static Action PlayerFrame;

        private void Update()
        {
            PlayerFrame?.Invoke();
        }

        private void OnDestroy()
        {
            PlayerFrame = null;
        }
    }
}
#endif
