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
            Graphic[] graphics = Object.FindObjectsByType<Graphic>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);

            for (int i = 0; i < graphics.Length; i++)
            {
                Graphic graphic = graphics[i];
                if (graphic == null || !graphic.isActiveAndEnabled || !graphic.raycastTarget)
                    continue;

                Canvas canvas = graphic.canvas;
                if (canvas == null || !canvas.isActiveAndEnabled)
                    continue;

                Camera eventCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
                    ? null
                    : (canvas.worldCamera != null ? canvas.worldCamera : Camera.main);

                if (!RectTransformUtility.RectangleContainsScreenPoint(
                        graphic.rectTransform,
                        screenPoint,
                        eventCamera))
                    continue;

                if (graphic.Raycast(screenPoint, eventCamera))
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
            float best = 0.82f * 0.82f;

            for (int i = 0; i < _spots.Count; i++)
            {
                PlantingSpot spot = _spots[i];
                if (spot == null) continue;

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

            if (nearest.Occupied && nearest.Instance != null)
                SelectTower(nearest);
            else if (!nearest.Occupied)
                SelectBuildSpot(nearest);
        }

        private void SelectBuildSpot(PlantingSpot spot)
        {
            if (_selected != null) _selected.SetSelected(false);
            _selected = spot;
            _selected.SetSelected(true);
            _session.Hud.CloseTowerPanel();
            _session.Hud.OpenBuildRadial(_selected, BuildSelected, CloseSelection);
        }

        private void SelectTower(PlantingSpot spot)
        {
            if (_selected != null) _selected.SetSelected(false);
            _selected = spot;
            _selected.SetSelected(true);
            _session.Hud.CloseBuildRadial();
            RefreshTowerPanel();
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
            PlantInstance instance = plant.AddComponent<PlantInstance>();
            instance.Initialize(_session, _selected, kind, cost);

            PlantBehaviour behaviour = null;
            switch (kind)
            {
                case PlantKind.SeedShooter: behaviour = plant.AddComponent<SeedShooterPlant>(); break;
                case PlantKind.EssenceFlower: behaviour = plant.AddComponent<EssenceFlowerPlant>(); break;
                case PlantKind.OakGuardian: behaviour = plant.AddComponent<OakGuardianPlant>(); break;
                case PlantKind.FrostBud: behaviour = plant.AddComponent<FrostBudPlant>(); break;
            }

            if (behaviour != null)
                behaviour.Initialize(_session, instance);

            _selected.Occupy(kind, instance);
            CloseSelection();
            _session.Hud.Refresh();
        }

        private void UpgradeSelected()
        {
            if (_selected == null || _selected.Instance == null) return;
            PlantInstance instance = _selected.Instance;
            if (!instance.CanUpgrade)
            {
                RefreshTowerPanel();
                return;
            }

            int cost = instance.UpgradeCost;
            if (!_session.Economy.TrySpend(cost))
            {
                _session.Hud.PulseEssence();
                return;
            }

            instance.ApplyUpgrade(cost);
            _session.Hud.ShowFloatingMessage(
                instance.transform.position,
                "POZIOM " + instance.Level,
                new Color(1f, 0.84f, 0.25f));
            _session.Hud.Refresh();
            RefreshTowerPanel();
        }

        private void SellSelected()
        {
            if (_selected == null || _selected.Instance == null) return;

            PlantInstance instance = _selected.Instance;
            int refund = instance.SellValue;
            GameObject plantObject = instance.gameObject;
            Vector2 pos = plantObject.transform.position;

            _selected.ClearOccupancy();
            _session.Economy.Add(refund);
            Destroy(plantObject);

            _session.Hud.ShowFloatingMessage(
                pos,
                "+" + refund + " Esencji",
                new Color(1f, 0.78f, 0.22f));

            CloseSelection();
            _session.Hud.Refresh();
        }

        private void RefreshTowerPanel()
        {
            if (_selected == null || _selected.Instance == null)
            {
                CloseSelection();
                return;
            }

            _session.Hud.OpenTowerPanel(
                _selected.Instance,
                UpgradeSelected,
                SellSelected,
                CloseSelection);
        }

        private void CloseSelection()
        {
            if (_selected != null) _selected.SetSelected(false);
            _selected = null;

            if (_session != null && _session.Hud != null)
            {
                _session.Hud.CloseBuildRadial();
                _session.Hud.CloseTowerPanel();
            }
        }
    }
}
