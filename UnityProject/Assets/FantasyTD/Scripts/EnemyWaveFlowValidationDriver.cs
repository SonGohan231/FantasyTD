#if UNITY_EDITOR
using System;
using UnityEngine;

namespace FantasyTD.CI
{
    // Sample after normal gameplay Update methods so progress and Time.deltaTime
    // describe the same completed frame even under accelerated validation.
    [DefaultExecutionOrder(1000)]
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
