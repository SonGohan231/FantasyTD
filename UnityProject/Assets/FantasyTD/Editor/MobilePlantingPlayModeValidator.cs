#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using FantasyTD.MapA;
using FantasyTD.MapA.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FantasyTD.CI
{
    [InitializeOnLoad]
    internal static class MobilePlantingPlayModeValidator
    {
        private const string ActiveKey = "FantasyTD.T0003.Active";
        private const string PhaseKey = "FantasyTD.T0003.Phase";
        private const string StartedKey = "FantasyTD.T0003.StartedUtc";
        private const string ExitCodeKey = "FantasyTD.T0003.ExitCode";
        private const string ReportPath = "Artifacts/UnityExecutor/mobile-planting.txt";
        private const float TimeoutSeconds = 300f;

        private static readonly List<string> Errors = new List<string>();
        private static readonly List<string> RuntimeWarnings = new List<string>();
        private static readonly List<SpotRecord> SpotRecords = new List<SpotRecord>();

        private static bool _logSubscribed;
        private static bool _initialized;
        private static int _warmupFrames;
        private static int _spotIndex;
        private static int _stage;
        private static SpotRecord _currentSpotRecord;
        private static GameSession _session;
        private static PlantingSystem _planting;
        private static MobileHud _hud;
        private static List<PlantingSpot> _spots;
        private static GameObject _uiBlocker;
        private static int _uiBlockerClicks;
        private static bool _uiRaycastHit;
        private static bool _uiWorldPressAccepted;
        private static bool _uiWorldSelectionBlocked;
        private static int _occupiedPlantCountBefore;
        private static bool _occupiedBuilt;
        private static bool _occupiedSecondPressBlocked;
        private static bool _occupiedSecondPlantPrevented;
        private static bool _unaffordableSpendDownSucceeded;
        private static bool _unaffordableAllDisabled;
        private static bool _unaffordableClickBlocked;
        private static int _unaffordableEssenceBeforeClick;
        private static int _unaffordableEssenceAfterClick;

        static MobilePlantingPlayModeValidator()
        {
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
            if (SessionState.GetBool(ActiveKey, false))
                SubscribeLog();
        }

        internal static void Start()
        {
            ResetRuntimeState();
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "Artifacts/UnityExecutor");
            SessionState.SetBool(ActiveKey, true);
            SessionState.SetString(PhaseKey, "enter-play");
            SessionState.SetString(StartedKey, DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));
            SessionState.SetInt(ExitCodeKey, 1);
            SubscribeLog();

            try
            {
                MapAEditorSetup.OpenMap();
                EditorApplication.EnterPlaymode();
            }
            catch (Exception ex)
            {
                Errors.Add("startException=" + ex.GetType().Name + ": " + ex.Message);
                WriteReport(false, false);
                SessionState.SetBool(ActiveKey, false);
                UnsubscribeLog();
                EditorApplication.Exit(1);
            }
        }

        private static void Tick()
        {
            if (!SessionState.GetBool(ActiveKey, false))
                return;

            if (TimedOut())
            {
                FailAndLeavePlayMode("Validator timeout exceeded " + TimeoutSeconds.ToString("0", CultureInfo.InvariantCulture) + " seconds.");
                return;
            }

            string phase = SessionState.GetString(PhaseKey, "enter-play");

            if (phase == "exit-editor" && !EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                int exitCode = SessionState.GetInt(ExitCodeKey, 1);
                SessionState.SetBool(ActiveKey, false);
                UnsubscribeLog();
                EditorApplication.Exit(exitCode);
                return;
            }

            if (!EditorApplication.isPlaying)
                return;

            SubscribeLog();

            if (!_initialized)
            {
                _warmupFrames++;
                if (_warmupFrames < 5)
                    return;

                if (!TryInitializeContext())
                    return;

                SessionState.SetString(PhaseKey, "validating");
                _initialized = true;
                _stage = 0;
            }

            try
            {
                RunStage();
            }
            catch (Exception ex)
            {
                FailAndLeavePlayMode("validatorException=" + ex.GetType().Name + ": " + ex.Message);
            }
        }

        private static bool TryInitializeContext()
        {
            _session = UnityEngine.Object.FindFirstObjectByType<GameSession>();
            if (_session == null || _session.Planting == null || _session.Hud == null || Camera.main == null || EventSystem.current == null)
                return false;

            _planting = _session.Planting;
            _hud = _session.Hud;

            PlantingSpot[] found = UnityEngine.Object.FindObjectsByType<PlantingSpot>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);

            _spots = new List<PlantingSpot>();
            foreach (PlantingSpotDefinition definition in MapAData.PlantingSpots)
            {
                PlantingSpot spot = found.FirstOrDefault(s => s.Id == definition.Id);
                if (spot == null)
                {
                    Errors.Add("missingSpot=" + definition.Id);
                    FailAndLeavePlayMode("One or more expected planting spots are missing in Play Mode.");
                    return false;
                }
                _spots.Add(spot);
            }

            if (_spots.Count != 12)
            {
                FailAndLeavePlayMode("Expected exactly 12 planting spots.");
                return false;
            }

            return true;
        }

        private static void RunStage()
        {
            if (_spotIndex < _spots.Count)
            {
                if (_stage == 0)
                {
                    BeginSpotValidation(_spots[_spotIndex]);
                    _stage = 1;
                    return;
                }

                if (_stage == 1)
                {
                    FinishSpotCloseValidation();
                    _spotIndex++;
                    _stage = 0;
                    return;
                }
            }

            switch (_stage)
            {
                case 0:
                    BeginOccupiedValidation();
                    _stage = 2;
                    break;
                case 2:
                    VerifyOccupiedBuildAndAttemptSecondPress();
                    _stage = 3;
                    break;
                case 3:
                    VerifyOccupiedSecondPress();
                    _stage = 4;
                    break;
                case 4:
                    CreateUiBlockerFixture();
                    _stage = 5;
                    break;
                case 5:
                    RunUiWorldBlockingValidation();
                    _stage = 6;
                    break;
                case 6:
                    CleanupUiBlocker();
                    _stage = 7;
                    break;
                case 7:
                    BeginUnaffordableValidation();
                    _stage = 8;
                    break;
                case 8:
                    VerifyUnaffordableValidation();
                    _stage = 9;
                    break;
                case 9:
                    VerifyUnaffordableCloseAndFinish();
                    break;
            }
        }

        private static void BeginSpotValidation(PlantingSpot spot)
        {
            Canvas.ForceUpdateCanvases();

            Vector2 screen = Camera.main.WorldToScreenPoint(spot.transform.position);
            bool accepted = _planting.TryHandleScreenPress(screen);
            Canvas.ForceUpdateCanvases();

            SpotRecord record = new SpotRecord();
            record.Id = spot.Id;
            record.Screen = screen;
            record.WorldPressAccepted = accepted;
            record.Selected = _planting.SelectedSpot == spot;
            record.SelectionStateSelected = IsSpotSelected(spot);
            record.RadialOpen = _hud.RadialRoot != null;

            Button[] optionButtons = GetPlantOptionButtons();
            record.OptionCount = optionButtons.Count(b => b.gameObject.activeInHierarchy);
            record.AllFourOptionsVisible = ExpectedPlantKinds().All(kind =>
            {
                Button button = FindPlantButton(kind);
                return button != null && button.gameObject.activeInHierarchy &&
                       button.GetComponent<Graphic>() != null && button.GetComponent<Graphic>().enabled;
            });

            BoundsPixels bounds = GetRadialButtonScreenBounds();
            record.Bounds = bounds;
            record.InGameView = bounds.Valid &&
                                bounds.MinX >= -0.5f &&
                                bounds.MinY >= -0.5f &&
                                bounds.MaxX <= Screen.width + 0.5f &&
                                bounds.MaxY <= Screen.height + 0.5f;

            Vector2 requestedLocal;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_hud.SafeArea, screen, null, out requestedLocal);
            Vector2 actualLocal = _hud.RadialRoot != null ? _hud.RadialRoot.anchoredPosition : requestedLocal;
            record.ClampShift = actualLocal - requestedLocal;
            record.Clamped = record.ClampShift.sqrMagnitude > 0.25f;
            record.EdgeSpot = screen.x < Screen.width * 0.25f || screen.x > Screen.width * 0.75f ||
                              screen.y < Screen.height * 0.25f || screen.y > Screen.height * 0.75f;

            bool preClose = accepted && record.Selected && record.SelectionStateSelected && record.RadialOpen &&
                            record.OptionCount == 4 && record.AllFourOptionsVisible && record.InGameView;
            if (!preClose)
                Errors.Add("spot." + spot.Id + ".preCloseValidationFailed");

            Button close = FindButton("RadialCloseButton");
            record.CloseClickDispatched = close != null && ExecuteButtonClick(close);
            if (!record.CloseClickDispatched)
                Errors.Add("spot." + spot.Id + ".closeClickDispatchFailed");

            _currentSpotRecord = record;
        }

        private static void FinishSpotCloseValidation()
        {
            Canvas.ForceUpdateCanvases();
            PlantingSpot spot = _spots[_spotIndex];

            _currentSpotRecord.CloseSuccess =
                _hud.RadialRoot == null &&
                _planting.SelectedSpot == null &&
                !IsSpotSelected(spot);

            _currentSpotRecord.Pass =
                _currentSpotRecord.WorldPressAccepted &&
                _currentSpotRecord.Selected &&
                _currentSpotRecord.SelectionStateSelected &&
                _currentSpotRecord.RadialOpen &&
                _currentSpotRecord.OptionCount == 4 &&
                _currentSpotRecord.AllFourOptionsVisible &&
                _currentSpotRecord.InGameView &&
                _currentSpotRecord.CloseClickDispatched &&
                _currentSpotRecord.CloseSuccess;

            if (!_currentSpotRecord.CloseSuccess)
                Errors.Add("spot." + spot.Id + ".closeValidationFailed");

            SpotRecords.Add(_currentSpotRecord);
            _currentSpotRecord = null;
        }

        private static void BeginOccupiedValidation()
        {
            PlantingSpot spot = FindSpot("A1");
            _occupiedPlantCountBefore = CountPlantsAt(spot.transform.position);

            Vector2 screen = Camera.main.WorldToScreenPoint(spot.transform.position);
            bool accepted = _planting.TryHandleScreenPress(screen);
            Canvas.ForceUpdateCanvases();

            Button build = FindPlantButton(PlantKind.SeedShooter);
            bool click = accepted && _planting.SelectedSpot == spot && build != null && ExecuteButtonClick(build);
            if (!click)
                Errors.Add("occupied.initialBuildClickFailed");
        }

        private static void VerifyOccupiedBuildAndAttemptSecondPress()
        {
            PlantingSpot spot = FindSpot("A1");
            int count = CountPlantsAt(spot.transform.position);
            _occupiedBuilt = spot.Occupied && spot.Plant == PlantKind.SeedShooter &&
                             count == _occupiedPlantCountBefore + 1;

            if (!_occupiedBuilt)
                Errors.Add("occupied.initialBuildDidNotOccupyExactlyOnce");

            Vector2 screen = Camera.main.WorldToScreenPoint(spot.transform.position);
            _planting.TryHandleScreenPress(screen);
        }

        private static void VerifyOccupiedSecondPress()
        {
            PlantingSpot spot = FindSpot("A1");
            int count = CountPlantsAt(spot.transform.position);
            _occupiedSecondPressBlocked = _planting.SelectedSpot == null && _hud.RadialRoot == null;
            _occupiedSecondPlantPrevented = count == _occupiedPlantCountBefore + 1;

            if (!_occupiedSecondPressBlocked)
                Errors.Add("occupied.secondPressOpenedSelection");
            if (!_occupiedSecondPlantPrevented)
                Errors.Add("occupied.secondPlantWasCreated");
        }

        private static void CreateUiBlockerFixture()
        {
            PlantingSpot spot = FindSpot("A3");
            Vector2 screen = Camera.main.WorldToScreenPoint(spot.transform.position);

            _uiBlocker = new GameObject(
                "T0003_UiInputBlocker",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button));
            _uiBlocker.transform.SetParent(_hud.SafeArea, false);

            RectTransform rect = _uiBlocker.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f,0.5f);
            rect.anchorMax = new Vector2(0.5f,0.5f);
            rect.pivot = new Vector2(0.5f,0.5f);
            rect.sizeDelta = new Vector2(140f,140f);

            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_hud.SafeArea, screen, null, out local);
            rect.anchoredPosition = local;

            Image image = _uiBlocker.GetComponent<Image>();
            image.color = new Color(1f,1f,1f,0.01f);
            image.raycastTarget = true;

            Button blockerButton = _uiBlocker.GetComponent<Button>();
            blockerButton.onClick.AddListener(delegate { _uiBlockerClicks++; });

            Canvas.ForceUpdateCanvases();
        }

        private static void RunUiWorldBlockingValidation()
        {
            PlantingSpot spot = FindSpot("A3");
            Vector2 screen = Camera.main.WorldToScreenPoint(spot.transform.position);
            Canvas.ForceUpdateCanvases();

            Button blockerButton = _uiBlocker != null ? _uiBlocker.GetComponent<Button>() : null;
            Graphic blockerGraphic = _uiBlocker != null ? _uiBlocker.GetComponent<Graphic>() : null;
            Canvas blockerCanvas = blockerGraphic != null ? blockerGraphic.canvas : null;
            Camera blockerCamera = blockerCanvas != null && blockerCanvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? (blockerCanvas.worldCamera != null ? blockerCanvas.worldCamera : Camera.main)
                : null;
            _uiRaycastHit =
                blockerGraphic != null &&
                blockerGraphic.isActiveAndEnabled &&
                blockerGraphic.raycastTarget &&
                RectTransformUtility.RectangleContainsScreenPoint(
                    blockerGraphic.rectTransform,
                    screen,
                    blockerCamera) &&
                blockerGraphic.Raycast(screen, blockerCamera);

            bool uiClick = ExecuteButtonClick(blockerButton, screen);
            _uiWorldPressAccepted = _planting.TryHandleScreenPress(screen);
            _uiWorldSelectionBlocked =
                uiClick &&
                _uiBlockerClicks == 1 &&
                !_uiWorldPressAccepted &&
                _planting.SelectedSpot == null &&
                _hud.RadialRoot == null;

            if (!_uiRaycastHit)
                Errors.Add("uiBlock.raycasterDidNotHitFixture");
            if (!_uiWorldSelectionBlocked)
                Errors.Add("uiBlock.worldSelectionNotBlocked");
        }

        private static void CleanupUiBlocker()
        {
            if (_uiBlocker != null)
                UnityEngine.Object.Destroy(_uiBlocker);
            _uiBlocker = null;
        }

        private static void BeginUnaffordableValidation()
        {
            _unaffordableSpendDownSucceeded = _session.Economy.Essence == 200 && _session.Economy.TrySpend(150);
            if (!_unaffordableSpendDownSucceeded || _session.Economy.Essence != 50)
            {
                Errors.Add("unaffordable.couldNotReach50EssenceThroughEconomy");
                return;
            }

            PlantingSpot spot = FindSpot("A2");
            Vector2 screen = Camera.main.WorldToScreenPoint(spot.transform.position);
            bool accepted = _planting.TryHandleScreenPress(screen);
            Canvas.ForceUpdateCanvases();

            Button[] buttons = ExpectedPlantKinds().Select(FindPlantButton).Where(b => b != null).ToArray();
            _unaffordableAllDisabled =
                accepted &&
                _planting.SelectedSpot == spot &&
                buttons.Length == 4 &&
                buttons.All(b => !b.interactable && b.gameObject.activeInHierarchy);

            if (!_unaffordableAllDisabled)
                Errors.Add("unaffordable.optionStateIncorrect");

            Button attempt = FindPlantButton(PlantKind.SeedShooter);
            _unaffordableEssenceBeforeClick = _session.Economy.Essence;
            if (attempt != null)
                ExecuteButtonClick(attempt);
        }

        private static void VerifyUnaffordableValidation()
        {
            PlantingSpot spot = FindSpot("A2");
            _unaffordableEssenceAfterClick = _session.Economy.Essence;
            _unaffordableClickBlocked =
                !spot.Occupied &&
                _unaffordableEssenceBeforeClick == 50 &&
                _unaffordableEssenceAfterClick == 50 &&
                CountPlantsAt(spot.transform.position) == 0;

            if (!_unaffordableClickBlocked)
                Errors.Add("unaffordable.disabledButtonAllowedPurchase");

            Button close = FindButton("RadialCloseButton");
            if (close == null || !ExecuteButtonClick(close))
                Errors.Add("unaffordable.closeFailed");
        }

        private static void VerifyUnaffordableCloseAndFinish()
        {
            bool closeSuccess = _hud.RadialRoot == null && _planting.SelectedSpot == null;
            if (!closeSuccess)
                Errors.Add("unaffordable.menuDidNotClose");

            bool allSpotsPassed = SpotRecords.Count == 12 && SpotRecords.All(r => r.Pass);
            bool success =
                allSpotsPassed &&
                _occupiedBuilt &&
                _occupiedSecondPressBlocked &&
                _occupiedSecondPlantPrevented &&
                _uiRaycastHit &&
                _uiWorldSelectionBlocked &&
                _unaffordableSpendDownSucceeded &&
                _unaffordableAllDisabled &&
                _unaffordableClickBlocked &&
                Errors.Count == 0;

            WriteReport(success, true);
            LeavePlayMode(success ? 0 : 1);
        }

        private static void FailAndLeavePlayMode(string message)
        {
            if (!string.IsNullOrEmpty(message))
                Errors.Add(message);

            WriteReport(false, EditorApplication.isPlaying);
            LeavePlayMode(1);
        }

        private static void LeavePlayMode(int exitCode)
        {
            SessionState.SetInt(ExitCodeKey, exitCode);
            SessionState.SetString(PhaseKey, "exit-editor");
            UnsubscribeLog();

            if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
                EditorApplication.ExitPlaymode();
            else
            {
                SessionState.SetBool(ActiveKey, false);
                EditorApplication.Exit(exitCode);
            }
        }

        private static void WriteReport(bool success, bool playModeEntered)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "Artifacts/UnityExecutor");

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("DYRYGENT UNITY EXECUTOR T0003 MOBILE PLANTING");
            sb.AppendLine("timestampUtc=" + DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));
            sb.AppendLine("unityVersion=" + Application.unityVersion);
            sb.AppendLine("status=" + (success ? "PASS" : "FAIL"));
            sb.AppendLine("playModeEntered=" + playModeEntered);
            sb.AppendLine("gameViewWidth=" + Screen.width);
            sb.AppendLine("gameViewHeight=" + Screen.height);
            sb.AppendLine("spotEvidenceCount=" + SpotRecords.Count);

            foreach (SpotRecord record in SpotRecords)
            {
                string prefix = "spot." + record.Id + ".";
                sb.AppendLine(prefix + "screen=" + F(record.Screen.x) + "," + F(record.Screen.y));
                sb.AppendLine(prefix + "worldPressAccepted=" + record.WorldPressAccepted);
                sb.AppendLine(prefix + "selected=" + record.Selected);
                sb.AppendLine(prefix + "selectionStateSelected=" + record.SelectionStateSelected);
                sb.AppendLine(prefix + "radialOpen=" + record.RadialOpen);
                sb.AppendLine(prefix + "optionCount=" + record.OptionCount);
                sb.AppendLine(prefix + "allFourOptionsVisible=" + record.AllFourOptionsVisible);
                sb.AppendLine(prefix + "boundsPx=" + record.Bounds.ToString());
                sb.AppendLine(prefix + "inGameView=" + record.InGameView);
                sb.AppendLine(prefix + "edgeSpot=" + record.EdgeSpot);
                sb.AppendLine(prefix + "clamped=" + record.Clamped);
                sb.AppendLine(prefix + "clampShift=" + F(record.ClampShift.x) + "," + F(record.ClampShift.y));
                sb.AppendLine(prefix + "closeClickDispatched=" + record.CloseClickDispatched);
                sb.AppendLine(prefix + "closeSuccess=" + record.CloseSuccess);
                sb.AppendLine(prefix + "result=" + (record.Pass ? "PASS" : "FAIL"));
            }

            sb.AppendLine("occupied.A1.initialBuild=" + _occupiedBuilt);
            sb.AppendLine("occupied.A1.secondPressBlocked=" + _occupiedSecondPressBlocked);
            sb.AppendLine("occupied.A1.secondPlantPrevented=" + _occupiedSecondPlantPrevented);

            sb.AppendLine("uiBlock.underlyingSpot=A3");
            sb.AppendLine("uiBlock.raycastHit=" + _uiRaycastHit);
            sb.AppendLine("uiBlock.uiClickCount=" + _uiBlockerClicks);
            sb.AppendLine("uiBlock.worldPressAccepted=" + _uiWorldPressAccepted);
            sb.AppendLine("uiBlock.worldSelectionBlocked=" + _uiWorldSelectionBlocked);

            sb.AppendLine("unaffordable.spendDownTo50=" + _unaffordableSpendDownSucceeded);
            sb.AppendLine("unaffordable.allFourButtonsDisabled=" + _unaffordableAllDisabled);
            sb.AppendLine("unaffordable.essenceBeforeClick=" + _unaffordableEssenceBeforeClick);
            sb.AppendLine("unaffordable.essenceAfterClick=" + _unaffordableEssenceAfterClick);
            sb.AppendLine("unaffordable.purchaseBlocked=" + _unaffordableClickBlocked);

            sb.AppendLine("runtimeWarningCount=" + RuntimeWarnings.Count);
            for (int i = 0; i < RuntimeWarnings.Count; i++)
                sb.AppendLine("runtimeWarning." + (i + 1) + "=" + Sanitize(RuntimeWarnings[i]));

            sb.AppendLine("validationErrorCount=" + Errors.Count);
            for (int i = 0; i < Errors.Count; i++)
                sb.AppendLine("validationError." + (i + 1) + "=" + Sanitize(Errors[i]));

            File.WriteAllText(ReportPath, sb.ToString());
            Debug.Log(sb.ToString());
        }

        private static Button[] GetPlantOptionButtons()
        {
            if (_hud.RadialRoot == null) return new Button[0];
            return _hud.RadialRoot.GetComponentsInChildren<Button>(true)
                .Where(b => b.gameObject.name.StartsWith("PlantOption_", StringComparison.Ordinal))
                .ToArray();
        }

        private static IEnumerable<PlantKind> ExpectedPlantKinds()
        {
            yield return PlantKind.SeedShooter;
            yield return PlantKind.EssenceFlower;
            yield return PlantKind.OakGuardian;
            yield return PlantKind.FrostBud;
        }

        private static Button FindPlantButton(PlantKind kind)
        {
            return FindButton("PlantOption_" + kind);
        }

        private static Button FindButton(string name)
        {
            if (_hud == null || _hud.RadialRoot == null) return null;
            return _hud.RadialRoot.GetComponentsInChildren<Button>(true)
                .FirstOrDefault(b => b.gameObject.name == name);
        }

        private static bool ExecuteButtonClick(Button button)
        {
            if (button == null) return false;
            Vector2 position = RectCenterScreen(button.GetComponent<RectTransform>());
            return ExecuteButtonClick(button, position);
        }

        private static bool ExecuteButtonClick(Button button, Vector2 position)
        {
            if (button == null || EventSystem.current == null) return false;
            PointerEventData pointer = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left,
                position = position
            };
            return ExecuteEvents.Execute<IPointerClickHandler>(
                button.gameObject,
                pointer,
                ExecuteEvents.pointerClickHandler);
        }

        private static Vector2 RectCenterScreen(RectTransform rect)
        {
            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            Vector3 center = (corners[0] + corners[2]) * 0.5f;
            return RectTransformUtility.WorldToScreenPoint(null, center);
        }

        private static BoundsPixels GetRadialButtonScreenBounds()
        {
            BoundsPixels result = new BoundsPixels();
            if (_hud.RadialRoot == null) return result;

            Button[] buttons = _hud.RadialRoot.GetComponentsInChildren<Button>(true);
            if (buttons.Length == 0) return result;

            result.MinX = float.PositiveInfinity;
            result.MinY = float.PositiveInfinity;
            result.MaxX = float.NegativeInfinity;
            result.MaxY = float.NegativeInfinity;

            foreach (Button button in buttons)
            {
                RectTransform rect = button.GetComponent<RectTransform>();
                Vector3[] corners = new Vector3[4];
                rect.GetWorldCorners(corners);
                for (int i = 0; i < corners.Length; i++)
                {
                    Vector2 screen = RectTransformUtility.WorldToScreenPoint(null, corners[i]);
                    result.MinX = Mathf.Min(result.MinX, screen.x);
                    result.MinY = Mathf.Min(result.MinY, screen.y);
                    result.MaxX = Mathf.Max(result.MaxX, screen.x);
                    result.MaxY = Mathf.Max(result.MaxY, screen.y);
                }
            }

            result.Valid = true;
            return result;
        }

        private static bool IsSpotSelected(PlantingSpot spot)
        {
            return spot != null && spot.IsSelected;
        }

        private static int CountPlantsAt(Vector2 position)
        {
            PlantBehaviour[] plants = UnityEngine.Object.FindObjectsByType<PlantBehaviour>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            return plants.Count(p => p != null && Vector2.Distance(p.transform.position, position) < 0.01f);
        }

        private static PlantingSpot FindSpot(string id)
        {
            return _spots.First(s => s.Id == id);
        }

        private static void SubscribeLog()
        {
            if (_logSubscribed) return;
            Application.logMessageReceived += OnLog;
            _logSubscribed = true;
        }

        private static void UnsubscribeLog()
        {
            if (!_logSubscribed) return;
            Application.logMessageReceived -= OnLog;
            _logSubscribed = false;
        }

        private static void OnLog(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                Errors.Add("console." + type + "=" + condition);
            else if (type == LogType.Warning)
                RuntimeWarnings.Add(condition);
        }

        private static bool TimedOut()
        {
            string started = SessionState.GetString(StartedKey, string.Empty);
            DateTime parsed;
            if (!DateTime.TryParse(started, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out parsed))
                return false;
            return (DateTime.UtcNow - parsed.ToUniversalTime()).TotalSeconds > TimeoutSeconds;
        }

        private static void ResetRuntimeState()
        {
            Errors.Clear();
            RuntimeWarnings.Clear();
            SpotRecords.Clear();
            _initialized = false;
            _warmupFrames = 0;
            _spotIndex = 0;
            _stage = 0;
            _currentSpotRecord = null;
            _session = null;
            _planting = null;
            _hud = null;
            _spots = null;
            _uiBlocker = null;
            _uiBlockerClicks = 0;
            _uiRaycastHit = false;
            _uiWorldPressAccepted = false;
            _uiWorldSelectionBlocked = false;
            _occupiedPlantCountBefore = 0;
            _occupiedBuilt = false;
            _occupiedSecondPressBlocked = false;
            _occupiedSecondPlantPrevented = false;
            _unaffordableSpendDownSucceeded = false;
            _unaffordableAllDisabled = false;
            _unaffordableClickBlocked = false;
            _unaffordableEssenceBeforeClick = 0;
            _unaffordableEssenceAfterClick = 0;
        }

        private static string F(float value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        private static string Sanitize(string value)
        {
            return (value ?? string.Empty).Replace("\r", " ").Replace("\n", " ");
        }

        private sealed class SpotRecord
        {
            internal string Id;
            internal Vector2 Screen;
            internal bool WorldPressAccepted;
            internal bool Selected;
            internal bool SelectionStateSelected;
            internal bool RadialOpen;
            internal int OptionCount;
            internal bool AllFourOptionsVisible;
            internal BoundsPixels Bounds;
            internal bool InGameView;
            internal bool EdgeSpot;
            internal bool Clamped;
            internal Vector2 ClampShift;
            internal bool CloseClickDispatched;
            internal bool CloseSuccess;
            internal bool Pass;
        }

        private struct BoundsPixels
        {
            internal bool Valid;
            internal float MinX;
            internal float MinY;
            internal float MaxX;
            internal float MaxY;

            public override string ToString()
            {
                if (!Valid) return "invalid";
                return F(MinX) + "," + F(MinY) + "," + F(MaxX) + "," + F(MaxY);
            }
        }
    }
}
#endif
