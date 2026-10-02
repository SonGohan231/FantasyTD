using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FantasyTD.MapA
{
    public sealed class PlantingSystem : MonoBehaviour
    {
        private GameSession _session;
        private readonly List<PlantingSpot> _spots = new List<PlantingSpot>();
        private PlantingSpot _selected;

        public PlantingSpot SelectedSpot => _selected;

        public void Initialize(GameSession session, List<PlantingSpot> spots)
        {
            _session = session;
            _spots.AddRange(spots);
        }

        private void Update()
        {
            if (_session == null || _session.IsFinished) return;

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Ended)
                    TryHandleScreenPress(touch.position);
            }
#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetMouseButtonDown(0))
                TryHandleScreenPress(Input.mousePosition);
#endif
        }

        public bool TryHandleScreenPress(Vector2 screenPoint)
        {
            if (_session == null || _session.IsFinished)
                return false;

            if (IsOverUiAt(screenPoint))
                return false;

            HandleScreenTap(screenPoint);
            return true;
        }

        private static bool IsOverUiAt(Vector2 screenPoint)
        {
            if (EventSystem.current == null) return false;

            PointerEventData pointer = new PointerEventData(EventSystem.current)
            {
                position = screenPoint
            };
            List<RaycastResult> results = new List<RaycastResult>();
            GraphicRaycaster[] raycasters = Object.FindObjectsByType<GraphicRaycaster>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);

            for (int i = 0; i < raycasters.Length; i++)
            {
                GraphicRaycaster raycaster = raycasters[i];
                if (raycaster == null || !raycaster.isActiveAndEnabled)
                    continue;

                results.Clear();
                raycaster.Raycast(pointer, results);
                if (results.Count > 0)
                    return true;
            }

            return false;
        }

        private void HandleScreenTap(Vector2 screenPoint)
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            Vector3 world3 = cam.ScreenToWorldPoint(new Vector3(screenPoint.x,screenPoint.y,-cam.transform.position.z));
            Vector2 world = new Vector2(world3.x,world3.y);

            PlantingSpot nearest = null;
            float best = 0.78f * 0.78f;

            for (int i = 0; i < _spots.Count; i++)
            {
                PlantingSpot spot = _spots[i];
                if (spot == null || spot.Occupied) continue;
                float d = ((Vector2)spot.transform.position - world).sqrMagnitude;
                if (d <= best)
                {
                    best = d;
                    nearest = spot;
                }
            }

            if (nearest == null)
            {
                CloseSelection();
                return;
            }

            Select(nearest);
        }

        private void Select(PlantingSpot spot)
        {
            if (_selected != null) _selected.SetSelected(false);
            _selected = spot;
            _selected.SetSelected(true);
            _session.Hud.OpenBuildRadial(_selected, BuildSelected, CloseSelection);
        }

        private void BuildSelected(PlantKind kind)
        {
            if (_selected == null || _selected.Occupied) return;

            int cost = Balance.Cost(kind);
            if (!_session.Economy.TrySpend(cost))
            {
                _session.Hud.PulseEssence();
                return;
            }

            GameObject plant = FantasyVisualFactory.CreatePlant(kind, _selected.transform.position);
            PlantBehaviour behaviour = null;

            switch (kind)
            {
                case PlantKind.SeedShooter: behaviour = plant.AddComponent<SeedShooterPlant>(); break;
                case PlantKind.EssenceFlower: behaviour = plant.AddComponent<EssenceFlowerPlant>(); break;
                case PlantKind.OakGuardian: behaviour = plant.AddComponent<OakGuardianPlant>(); break;
                case PlantKind.FrostBud: behaviour = plant.AddComponent<FrostBudPlant>(); break;
            }

            if (behaviour != null) behaviour.Initialize(_session);
            _selected.Occupy(kind);
            CloseSelection();
            _session.Hud.Refresh();
        }

        private void CloseSelection()
        {
            if (_selected != null) _selected.SetSelected(false);
            _selected = null;
            if (_session != null && _session.Hud != null)
                _session.Hud.CloseBuildRadial();
        }
    }
}
