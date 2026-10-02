#if UNITY_EDITOR
using System;
using UnityEngine;

namespace FantasyTD.CI
{
    public sealed class BossEndStatesValidationDriver : MonoBehaviour
    {
        public static Action PlayerFrame;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

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
