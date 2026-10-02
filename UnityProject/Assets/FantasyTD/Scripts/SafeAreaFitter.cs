using UnityEngine;

namespace FantasyTD.MapA
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform _rect;
        private Rect _last;

#if UNITY_EDITOR
        private static Rect? _editorOverrideSafeArea;

        public static Rect? EditorOverrideSafeArea => _editorOverrideSafeArea;

        public static void SetEditorOverrideSafeArea(Rect safeArea)
        {
            _editorOverrideSafeArea = safeArea;
        }

        public static void ClearEditorOverrideSafeArea()
        {
            _editorOverrideSafeArea = null;
        }
#endif

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            Apply();
        }

        private void Update()
        {
            Rect effective = EffectiveSafeArea();
            if (_last != effective) Apply();
        }

        private static Rect EffectiveSafeArea()
        {
#if UNITY_EDITOR
            if (_editorOverrideSafeArea.HasValue)
                return _editorOverrideSafeArea.Value;
#endif
            return Screen.safeArea;
        }

        private void Apply()
        {
            _last = EffectiveSafeArea();
            Vector2 min = _last.position;
            Vector2 max = _last.position + _last.size;
            min.x /= Screen.width;
            min.y /= Screen.height;
            max.x /= Screen.width;
            max.y /= Screen.height;
            _rect.anchorMin = min;
            _rect.anchorMax = max;
            _rect.offsetMin = Vector2.zero;
            _rect.offsetMax = Vector2.zero;
        }
    }
}
