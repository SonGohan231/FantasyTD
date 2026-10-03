using UnityEngine;

namespace FantasyTD.MapA
{
    [RequireComponent(typeof(Camera))]
    public sealed class MobileCameraFitter : MonoBehaviour
    {
        private Camera _camera;
        private int _lastW;
        private int _lastH;
        private float _width = MapAData.Width;
        private float _height = MapAData.Height;

        public void Configure(float width, float height)
        {
            if (width <= 0f || height <= 0f || float.IsNaN(width) || float.IsNaN(height) ||
                float.IsInfinity(width) || float.IsInfinity(height))
                throw new System.ArgumentOutOfRangeException("Map dimensions must be finite and positive.");
            _width = width;
            _height = height;
            if (_camera == null) _camera = GetComponent<Camera>();
            Fit();
        }

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            Fit();
        }

        private void Update()
        {
            if (Screen.width != _lastW || Screen.height != _lastH)
                Fit();
        }

        private void Fit()
        {
            _lastW = Screen.width;
            _lastH = Screen.height;

            float aspect = Mathf.Max(0.1f, (float)Screen.width / Mathf.Max(1,Screen.height));
            float fitHeight = _height * 0.5f + 0.55f;
            float fitWidth = _width / (2f * aspect) + 0.55f;
            _camera.orthographicSize = Mathf.Max(fitHeight,fitWidth);
            transform.position = new Vector3(_width*0.5f,_height*0.5f,-10f);
        }
    }
}
