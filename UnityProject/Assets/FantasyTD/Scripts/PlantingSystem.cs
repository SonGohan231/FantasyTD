using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FantasyTD.MapA
{
    public sealed class PlantingSystem : MonoBehaviour
    {
        private GameSession _session;
        private readonly List<PlantingSpot> _spots = new List<PlantingSpot>();
        private PlantingSpot _selected;

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
                if (touch.phase == TouchPhase.Ended && !IsOverUi(touch.fingerId))
                    HandleScreenTap(touch.position);
            }
#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetMouseButtonDown(0) && !IsOverUi())
                HandleScreenTap(Input.mousePosition);
#endif
        }

        private bool IsOverUi(int fingerId = -1)
        {
            if (EventSystem.current == null) return false;
            return fingerId >= 0
                ? EventSystem.current.IsPointerOverGameObject(fingerId)
                : EventSystem.current.IsPointerOverGameObject();
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
