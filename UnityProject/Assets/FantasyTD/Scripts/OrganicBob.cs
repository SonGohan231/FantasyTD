using UnityEngine;

namespace FantasyTD.MapA
{
    public sealed class OrganicBob : MonoBehaviour
    {
        private Vector3 _baseScale;
        private float _phase;

        private void Awake()
        {
            _baseScale = transform.localScale;
            _phase = Random.Range(0f, Mathf.PI * 2f);
        }

        private void Update()
        {
            float pulse = 1f + Mathf.Sin(Time.time * 2.1f + _phase) * 0.025f;
            transform.localScale = _baseScale * pulse;
        }
    }
}
