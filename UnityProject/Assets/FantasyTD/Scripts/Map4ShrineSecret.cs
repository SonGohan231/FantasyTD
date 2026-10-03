using UnityEngine;
using UnityEngine.EventSystems;

namespace FantasyTD.MapA
{
    public sealed class Map4ShrineSecret : MonoBehaviour
    {
        public const string DiscoveryKey = "FantasyTD.MAP4.PearlShrine.Discovered";
        private SpriteRenderer _renderer;
        private Sprite _open;
        private Vector2 _size;
        public bool IsDiscovered { get; private set; }

        public void Initialize(SpriteRenderer renderer, Sprite opened, Vector2 size)
        {
            _renderer = renderer;
            _open = opened;
            _size = size;
            IsDiscovered = PlayerPrefs.GetInt(DiscoveryKey, 0) == 1;
            if (IsDiscovered) ShowOpened();
        }

        private void ShowOpened()
        {
            _renderer.sprite = _open;
            transform.localScale = new Vector3(_size.x / _open.bounds.size.x, _size.y / _open.bounds.size.y, 1f);
        }

        public bool Discover()
        {
            if (IsDiscovered || _renderer == null || _open == null) return false;
            IsDiscovered = true;
            ShowOpened();
            PlayerPrefs.SetInt(DiscoveryKey, 1);
            PlayerPrefs.Save();
            Debug.Log("MAP4 pearl shrine discovered through production interaction.");
            return true;
        }

        private void Update()
        {
            if (IsDiscovered || Camera.main == null) return;
            Vector2 screen;
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase != TouchPhase.Began) return;
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId)) return;
                screen = touch.position;
            }
            else
            {
                if (!Input.GetMouseButtonDown(0)) return;
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
                screen = Input.mousePosition;
            }
            Vector3 world = Camera.main.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -Camera.main.transform.position.z));
            if (Vector2.Distance(world, transform.position) <= 0.95f) Discover();
        }
    }
}
