using UnityEngine;

namespace FantasyTD.MapA
{
    [RequireComponent(typeof(Camera))]
    public sealed class MobileCameraFitter : MonoBehaviour
    {
        private Camera _camera;
        private int _lastW;
        private int _lastH;

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
            float fitHeight = MapAData.Height * 0.5f + 0.55f;
            float fitWidth = MapAData.Width / (2f * aspect) + 0.55f;
            _camera.orthographicSize = Mathf.Max(fitHeight,fitWidth);
            transform.position = new Vector3(MapAData.Width*0.5f,MapAData.Height*0.5f,-10f);
        }
    }
}
