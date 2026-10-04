using UnityEngine;

namespace FantasyTD.MapA
{
    [RequireComponent(typeof(Camera))]
    public sealed class MobileCameraFitter : MonoBehaviour
    {
        private Camera _camera;
        private int _lastW;
        private int _lastH;
        private Rect _lastSafeArea;
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
            if (Screen.width != _lastW || Screen.height != _lastH || SafeArea() != _lastSafeArea)
                Fit();
        }

        private static Rect SafeArea()
        {
#if UNITY_EDITOR
            if (SafeAreaFitter.EditorOverrideSafeArea.HasValue)
                return SafeAreaFitter.EditorOverrideSafeArea.Value;
#endif
            return Screen.safeArea;
        }

        private void Fit()
        {
            _lastW = Screen.width;
            _lastH = Screen.height;

            float screenWidth = Mathf.Max(1, Screen.width);
            float screenHeight = Mathf.Max(1, Screen.height);
            _lastSafeArea = SafeArea();
            Rect safe = _lastSafeArea;
            if (safe.width <= 0f || safe.height <= 0f)
                safe = new Rect(0f, 0f, screenWidth, screenHeight);

            // Match MobileHud's 1920x1080 CanvasScaler with a 0.5 width/height match.
            // Keep the complete field below EnemyCount (156 units from the top)
            // and above START (160 units from the bottom), including a touch gap.
            float hudScale = Mathf.Sqrt((screenWidth / 1920f) * (screenHeight / 1080f));
            float sideInset = 18f * hudScale;
            float bottomInset = 170f * hudScale;
            float topInset = 166f * hudScale;
            float usableWidth = Mathf.Max(1f, safe.width - 2f * sideInset);
            float usableHeight = Mathf.Max(1f, safe.height - bottomInset - topInset);
            Vector2 fieldCenter = new Vector2(safe.center.x,
                safe.yMin + bottomInset + usableHeight * 0.5f);

            float fitHeight = (_height + 1.1f) * screenHeight / (2f * usableHeight);
            float fitWidth = (_width + 1.1f) * screenHeight / (2f * usableWidth);
            _camera.orthographicSize = Mathf.Max(fitHeight,fitWidth);
            float unitsPerPixel = 2f * _camera.orthographicSize / screenHeight;
            transform.position = new Vector3(
                _width * 0.5f - (fieldCenter.x - screenWidth * 0.5f) * unitsPerPixel,
                _height * 0.5f - (fieldCenter.y - screenHeight * 0.5f) * unitsPerPixel,
                -10f);
        }
    }
}
