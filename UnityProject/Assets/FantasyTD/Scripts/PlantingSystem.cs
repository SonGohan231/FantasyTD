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
        private TowerInstance _selectedTower;

        public PlantingSpot SelectedSpot => _selected;
        public TowerInstance SelectedTower => _selectedTower;

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

            Vector3 world3 = cam.ScreenToWorldPoint(
                new Vector3(screenPoint.x, screenPoint.y, -cam.transform.position.z));
            Vector2 world = new Vector2(world3.x, world3.y);

            PlantingSpot nearest = null;
            float best = 0.88f * 0.88f;

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

            if (nearest.Occupied && nearest.Tower != null)
                SelectTower(nearest);
            else
                SelectForBuild(nearest);
        }

        private void SelectForBuild(PlantingSpot spot)
        {
            CloseSelection();

            _selected = spot;
            _selectedTower = null;
            _selected.SetSelected(true);
            _session.Hud.OpenBuildRadial(_selected, BuildSelected, CloseSelection);
        }

        private void SelectTower(PlantingSpot spot)
        {
            CloseSelection();

            _selected = spot;
            _selectedTower = spot.Tower;
            _selected.SetSelected(true);

            _session.TowerPanel.Open(
                _selectedTower,
                UpgradeSelectedTower,
                SellSelectedTower,
                CloseSelection);
        }

        private void BuildSelected(PlantKind kind)
        {
            if (_selected == null || _selected.Occupied) return;
            if (kind == PlantKind.MutationTower && !Map5SecretController.MutationTowerUnlocked) return;

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
                case PlantKind.SeedShooter:
                case PlantKind.MutationTower: behaviour = plant.AddComponent<SeedShooterPlant>(); break;
                case PlantKind.EssenceFlower: behaviour = plant.AddComponent<EssenceFlowerPlant>(); break;
                case PlantKind.OakGuardian: behaviour = plant.AddComponent<OakGuardianPlant>(); break;
                case PlantKind.FrostBud: behaviour = plant.AddComponent<FrostBudPlant>(); break;
            }

            TowerInstance tower = plant.AddComponent<TowerInstance>();
            tower.Initialize(_session, _selected, kind, behaviour, cost);
            if (behaviour != null)
                behaviour.Initialize(_session, tower);

            _selected.Occupy(kind, tower);
            CloseSelection();
            _session.Hud.Refresh();
        }

        private void UpgradeSelectedTower()
        {
            if (_selectedTower == null) return;

            if (!_selectedTower.TryUpgrade())
            {
                if (_selectedTower.CanUpgrade)
                    _session.Hud.PulseEssence();
                return;
            }

            _session.Hud.ShowFloatingMessage(
                _selectedTower.transform.position,
                "POZIOM " + _selectedTower.Level,
                new Color(1f, 0.84f, 0.24f));
            _session.TowerPanel.Refresh();
            _session.Hud.Refresh();
        }

        private void SellSelectedTower()
        {
            if (_selectedTower == null) return;

            TowerInstance tower = _selectedTower;
            PlantingSpot spot = _selected;

            _selectedTower = null;
            _selected = null;

            if (spot != null)
                spot.SetSelected(false);

            int refund = tower.SellRefund;
            tower.Sell();

            _session.TowerPanel.Close();
            _session.Hud.ShowFloatingMessage(
                spot != null ? (Vector2)spot.transform.position : (Vector2)tower.transform.position,
                "+" + refund + " Esencji",
                new Color(1f, 0.72f, 0.20f));
            _session.Hud.Refresh();
        }

        private void CloseSelection()
        {
            if (_selected != null)
                _selected.SetSelected(false);

            _selected = null;
            _selectedTower = null;

            if (_session != null)
            {
                if (_session.Hud != null)
                    _session.Hud.CloseBuildRadial();
                if (_session.TowerPanel != null)
                    _session.TowerPanel.Close();
            }
        }
    }
}
