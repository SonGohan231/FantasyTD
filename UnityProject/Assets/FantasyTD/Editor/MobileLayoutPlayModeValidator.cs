#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using FantasyTD.MapA;
using FantasyTD.MapA.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FantasyTD.CI
{
    [InitializeOnLoad]
    internal static class MobileLayoutPlayModeValidator
    {
        private const string ActiveKey = "FantasyTD.T0007.Active";
        private const string PhaseKey = "FantasyTD.T0007.Phase";
        private const string StartedKey = "FantasyTD.T0007.StartedUtc";
        private const string ExitCodeKey = "FantasyTD.T0007.ExitCode";
        private const string PlayModeEnteredKey = "FantasyTD.T0007.PlayModeEntered";
        private const string ReportPath = "Artifacts/UnityExecutor/mobile-layout.txt";
        private const float TimeoutSeconds = 420f;
        private const float PixelEpsilon = 1.5f;

        private static readonly BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly List<string> Errors = new List<string>();
        private static readonly List<string> RuntimeWarnings = new List<string>();
        private static readonly List<LayoutResult> Results = new List<LayoutResult>();
        private static readonly string[] EdgeSpotIds = { "A1", "A8", "A9", "A11", "A12" };

        private static readonly LayoutCase[] Cases =
        {
            new LayoutCase("16x9-native", 1920, 1080, false, new Rect()),
            new LayoutCase("20x9-native", 2400, 1080, false, new Rect()),
            new LayoutCase("20x9-synthetic-safe-area", 2400, 1080, true, new Rect(120f, 0f, 2160f, 1080f))
        };

        private enum Stage
        {
            RequestCase,
            WaitResolution,
            WaitSafeArea,
            ValidateStatic,
            OpenSpot,
            CloseSpot,
            FinishCase,
            FinishAll,
            Done,
            Failed
        }

        private static bool _logSubscribed;
        private static bool _initialized;
        private static int _warmupFrames;
        private static Stage _stage;
        private static int _caseIndex;
        private static int _spotIndex;
        private static int _settleFrames;
        private static int _resolutionWaitFrames;

        private static GameSession _session;
        private static PlantingSystem _planting;
        private static MobileHud _hud;
        private static Camera _camera;
        private static List<PlantingSpot> _spots;
        private static Button _waveButton;
        private static Text _waveButtonText;

        private static LayoutResult _current;
        private static int _caseErrorStart;
        private static int _caseWarningStart;
        private static bool _currentSpotCloseClicked;

        static MobileLayoutPlayModeValidator()
        {
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
            if (SessionState.GetBool(ActiveKey, false))
                SubscribeLog();
        }

        internal static void Start()
        {
            ResetState();
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "Artifacts/UnityExecutor");
            SessionState.SetBool(ActiveKey, true);
            SessionState.SetBool(PlayModeEnteredKey, false);
            SessionState.SetString(PhaseKey, "enter-play");
            SessionState.SetString(StartedKey, DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));
            SessionState.SetInt(ExitCodeKey, 1);
            SubscribeLog();

            try
            {
                string resolutionDetails;
                if (!GameViewResolutionHelper.SetFixedResolution(1920, 1080, "T0007 1920x1080", out resolutionDetails))
                    throw new InvalidOperationException("Cannot configure initial Game View: " + resolutionDetails);

                SafeAreaFitter.ClearEditorOverrideSafeArea();
                MapAEditorSetup.OpenMap();
                EditorApplication.EnterPlaymode();
            }
            catch (Exception ex)
            {
                Errors.Add("startException=" + ex.GetType().Name + ": " + ex.Message);
                WriteReport(false);
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
                FailAndLeavePlayMode("Validator timeout exceeded " + F(TimeoutSeconds) + " seconds.");
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

            SessionState.SetBool(PlayModeEnteredKey, true);
            SubscribeLog();

            if (!_initialized)
            {
                _warmupFrames++;
                if (_warmupFrames < 8)
                    return;
                if (!TryInitializeContext())
                    return;

                _initialized = true;
                _stage = Stage.RequestCase;
                SessionState.SetString(PhaseKey, "validating");
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
            if (_session == null || _session.Hud == null || _session.Planting == null ||
                Camera.main == null || EventSystem.current == null)
                return false;

            _hud = _session.Hud;
            _planting = _session.Planting;
            _camera = Camera.main;

            _waveButton = (Button)RequireField(typeof(MobileHud), "_waveButton").GetValue(_hud);
            _waveButtonText = (Text)RequireField(typeof(MobileHud), "_waveButtonText").GetValue(_hud);

            PlantingSpot[] found = UnityEngine.Object.FindObjectsByType<PlantingSpot>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            _spots = new List<PlantingSpot>();
            foreach (PlantingSpotDefinition definition in MapAData.PlantingSpots)
            {
                PlantingSpot spot = found.FirstOrDefault(s => s.Id == definition.Id);
                if (spot == null)
                {
                    Errors.Add("missingSpot=" + definition.Id);
                    return false;
                }
                _spots.Add(spot);
            }

            if (_spots.Count != 12)
            {
                Errors.Add("plantingSpotCount=" + _spots.Count);
                return false;
            }

            return true;
        }

        private static void RunStage()
        {
            switch (_stage)
            {
                case Stage.RequestCase:
                    RequestCurrentCase();
                    break;
                case Stage.WaitResolution:
                    WaitForResolution();
                    break;
                case Stage.WaitSafeArea:
                    WaitForSafeArea();
                    break;
                case Stage.ValidateStatic:
                    ValidateStaticLayout();
                    break;
                case Stage.OpenSpot:
                    OpenCurrentSpot();
                    break;
                case Stage.CloseSpot:
                    CloseCurrentSpot();
                    break;
                case Stage.FinishCase:
                    FinishCurrentCase();
                    break;
                case Stage.FinishAll:
                    FinishAll();
                    break;
            }
        }

        private static void RequestCurrentCase()
        {
            if (_caseIndex >= Cases.Length)
            {
                _stage = Stage.FinishAll;
                return;
            }

            LayoutCase testCase = Cases[_caseIndex];
            SafeAreaFitter.ClearEditorOverrideSafeArea();

            string details;
            bool configured = GameViewResolutionHelper.SetFixedResolution(
                testCase.Width, testCase.Height,
                "T0007 " + testCase.Width + "x" + testCase.Height,
                out details);

            _current = new LayoutResult();
            _current.Name = testCase.Name;
            _current.RequestedWidth = testCase.Width;
            _current.RequestedHeight = testCase.Height;
            _current.GameViewConfiguration = details;
            _current.SyntheticSafeArea = testCase.UseSyntheticSafeArea;
            _current.SimulationMethod = testCase.UseSyntheticSafeArea
                ? "EditorOnlySyntheticSafeAreaOverride"
                : "NativeEditorScreen.safeArea";
            _current.DeviceSimulatorPackagePresent = DeviceSimulatorPackagePresent();
            _caseErrorStart = Errors.Count;
            _caseWarningStart = RuntimeWarnings.Count;

            if (!configured)
            {
                _current.ResolutionConfigured = false;
                Errors.Add("case." + testCase.Name + ".gameViewConfiguration=" + details);
                _stage = Stage.FinishCase;
                return;
            }

            Screen.SetResolution(testCase.Width, testCase.Height, FullScreenMode.Windowed);
            _resolutionWaitFrames = 0;
            _stage = Stage.WaitResolution;
        }

        private static void WaitForResolution()
        {
            LayoutCase testCase = Cases[_caseIndex];
            _resolutionWaitFrames++;

            if (Screen.width == testCase.Width && Screen.height == testCase.Height)
            {
                _current.ResolutionConfigured = true;
                _current.ObservedWidth = Screen.width;
                _current.ObservedHeight = Screen.height;
                _current.Aspect = (float)Screen.width / Mathf.Max(1, Screen.height);

                if (testCase.UseSyntheticSafeArea)
                    SafeAreaFitter.SetEditorOverrideSafeArea(testCase.SafeArea);
                else
                    SafeAreaFitter.ClearEditorOverrideSafeArea();

                _settleFrames = 0;
                _stage = Stage.WaitSafeArea;
                return;
            }

            if (_resolutionWaitFrames > 120)
            {
                _current.ResolutionConfigured = false;
                _current.ObservedWidth = Screen.width;
                _current.ObservedHeight = Screen.height;
                Errors.Add("case." + testCase.Name + ".resolutionMismatch requested=" +
                           testCase.Width + "x" + testCase.Height + " observed=" +
                           Screen.width + "x" + Screen.height);
                _stage = Stage.FinishCase;
            }
        }

        private static void WaitForSafeArea()
        {
            _settleFrames++;
            Canvas.ForceUpdateCanvases();

            if (_settleFrames < 8)
                return;

            Rect effective = Cases[_caseIndex].UseSyntheticSafeArea
                ? Cases[_caseIndex].SafeArea
                : Screen.safeArea;

            Rect actualSafe = GetScreenRect(_hud.SafeArea);
            if (!RectApproximately(actualSafe, effective, 2.5f))
            {
                if (_settleFrames < 40)
                    return;

                Errors.Add("case." + Cases[_caseIndex].Name +
                           ".safeAreaNotApplied expected=" + RectText(effective) +
                           " actual=" + RectText(actualSafe));
            }

            _current.NativeScreenSafeArea = Screen.safeArea;
            _current.EffectiveSafeArea = effective;
            _current.ActualSafeAreaRect = actualSafe;
            _current.SafeAreaApplied = RectApproximately(actualSafe, effective, 2.5f);
            _spotIndex = 0;
            _currentSpotCloseClicked = false;
            _stage = Stage.ValidateStatic;
        }

        private static void ValidateStaticLayout()
        {
            Canvas.ForceUpdateCanvases();

            Rect safe = _current.ActualSafeAreaRect;
            _current.MapFullyVisible = ValidateMapFraming(out _current.CameraWorldBounds);
            _current.CameraCentered =
                Mathf.Abs(_camera.transform.position.x - MapAData.Width * 0.5f) <= 0.25f &&
                Mathf.Abs(_camera.transform.position.y - MapAData.Height * 0.5f) <= 0.25f;
            _current.CameraPanningRequired = !_current.MapFullyVisible;

            RectTransform essence = FindRect("EssencePanel");
            RectTransform wave = FindRect("WavePanel");
            RectTransform hp = FindRect("HPPanel");
            RectTransform enemy = FindRect("EnemyCount");
            RectTransform hint = FindRect("Hint");

            Rect[] hudRects =
            {
                GetScreenRect(essence),
                GetScreenRect(wave),
                GetScreenRect(hp),
                GetScreenRect(enemy),
                GetScreenRect(hint)
            };

            _current.HudInsideSafeArea = hudRects.All(r => ContainsRect(safe, r));
            _current.HudRects = string.Join("|", hudRects.Select(RectText).ToArray());

            Rect waveButtonRect = GetScreenRect(_waveButton.GetComponent<RectTransform>());
            _current.WaveButtonRect = waveButtonRect;
            _current.WaveButtonVisible = _waveButton.gameObject.activeInHierarchy;
            _current.WaveButtonInsideSafeArea = ContainsRect(safe, waveButtonRect);
            _current.WaveButtonReachable = IsUiControlReachable(_waveButton, waveButtonRect.center);
            _current.WaveButtonPass = _current.WaveButtonVisible &&
                                      _current.WaveButtonInsideSafeArea &&
                                      _current.WaveButtonReachable;

            RecordTouchTarget("WaveButton", waveButtonRect);

            Text[] texts =
            {
                FindText("EssenceText"),
                FindText("WaveText"),
                FindText("HPText"),
                FindText("EnemyCount"),
                _waveButtonText,
                FindText("Hint")
            };

            _current.TextContentFits = texts.All(TextContentFitsAllocatedRect);
            _current.TextOverlapPass = !AnyIndependentTextRectOverlap(texts, out _current.TextOverlapDetails);

            _current.HudPass = _current.HudInsideSafeArea && _current.TextContentFits;
            _stage = Stage.OpenSpot;
        }

        private static void OpenCurrentSpot()
        {
            if (_spotIndex >= _spots.Count)
            {
                _stage = Stage.FinishCase;
                return;
            }

            PlantingSpot spot = _spots[_spotIndex];
            Canvas.ForceUpdateCanvases();
            Vector2 screen = _camera.WorldToScreenPoint(spot.transform.position);

            bool pointInsideScreen =
                screen.x >= -PixelEpsilon && screen.x <= Screen.width + PixelEpsilon &&
                screen.y >= -PixelEpsilon && screen.y <= Screen.height + PixelEpsilon;

            bool accepted = pointInsideScreen && _planting.TryHandleScreenPress(screen);
            Canvas.ForceUpdateCanvases();

            bool selected = accepted && _planting.SelectedSpot == spot && _hud.RadialRoot != null;
            if (selected)
                _current.AccessibleSpotCount++;

            if (!pointInsideScreen)
                _current.InaccessibleSpots.Add(spot.Id + ":offscreen");
            else if (!accepted)
                _current.InaccessibleSpots.Add(spot.Id + ":blocked");
            else if (!selected)
                _current.InaccessibleSpots.Add(spot.Id + ":not-selected");

            bool edge = EdgeSpotIds.Contains(spot.Id);
            if (edge)
            {
                bool edgePass = selected && ValidateEdgeRadial(spot, _current.ActualSafeAreaRect);
                _current.EdgeSpotPass[spot.Id] = edgePass;
                if (!edgePass)
                    _current.RadialEdgePass = false;
            }

            if (!selected)
            {
                _spotIndex++;
                _stage = Stage.OpenSpot;
                return;
            }

            Button close = FindRadialCloseButton();
            _currentSpotCloseClicked = close != null && DispatchPointerClick(close);
            _settleFrames = 0;
            _stage = Stage.CloseSpot;
        }

        private static void CloseCurrentSpot()
        {
            _settleFrames++;
            if (_settleFrames < 2)
                return;

            bool closed = _hud.RadialRoot == null && _planting.SelectedSpot == null;
            if (!closed)
            {
                if (_settleFrames < 12)
                    return;

                Errors.Add("case." + _current.Name + ".spot." + _spots[_spotIndex].Id +
                           ".radialDidNotClose");
            }

            if (!_currentSpotCloseClicked)
                Errors.Add("case." + _current.Name + ".spot." + _spots[_spotIndex].Id +
                           ".closeClickNotDispatched");

            _spotIndex++;
            _currentSpotCloseClicked = false;
            _stage = Stage.OpenSpot;
        }

        private static bool ValidateEdgeRadial(PlantingSpot spot, Rect safe)
        {
            Canvas.ForceUpdateCanvases();
            if (_hud.RadialRoot == null)
                return false;

            Button[] buttons = _hud.RadialRoot.GetComponentsInChildren<Button>(true);
            Button[] active = buttons.Where(b => b.gameObject.activeInHierarchy).ToArray();
            bool hasFourOptions = active.Count(b => b.gameObject.name.StartsWith("PlantOption_", StringComparison.Ordinal)) == 4;
            Button close = active.FirstOrDefault(b => b.gameObject.name == "RadialCloseButton");
            bool allInside = active.Length == 5 && active.All(button =>
            {
                Rect rect = GetScreenRect(button.GetComponent<RectTransform>());
                RecordTouchTarget(spot.Id + "." + button.gameObject.name, rect);
                return ContainsRect(safe, rect);
            });

            Rect rootBounds = GetRelativeChildrenScreenBounds(_hud.RadialRoot);
            _current.EdgeMenuBounds[spot.Id] = RectText(rootBounds);
            return hasFourOptions && close != null && allInside;
        }

        private static void FinishCurrentCase()
        {
            _current.ConsoleErrors = Errors.Count - _caseErrorStart;
            _current.ConsoleWarnings = RuntimeWarnings.Count - _caseWarningStart;
            _current.PlantingSpotsAccessible = _current.AccessibleSpotCount == 12;
            _current.RadialEdgePass = _current.RadialEdgePass &&
                                      EdgeSpotIds.All(id => _current.EdgeSpotPass.ContainsKey(id) &&
                                                            _current.EdgeSpotPass[id]);
            _current.SmallestTouchTargetText =
                _current.SmallestTouchTargetName + ":" +
                F(_current.SmallestTouchWidth) + "x" + F(_current.SmallestTouchHeight) + "px";

            _current.Pass =
                _current.ResolutionConfigured &&
                _current.SafeAreaApplied &&
                _current.MapFullyVisible &&
                _current.CameraCentered &&
                !_current.CameraPanningRequired &&
                _current.HudPass &&
                _current.WaveButtonPass &&
                _current.TextOverlapPass &&
                _current.PlantingSpotsAccessible &&
                _current.RadialEdgePass &&
                _current.SmallestTouchWidth > 0f &&
                _current.SmallestTouchHeight > 0f &&
                _current.ConsoleErrors == 0;

            Results.Add(_current);

            SafeAreaFitter.ClearEditorOverrideSafeArea();
            _caseIndex++;
            _current = null;
            _stage = Stage.RequestCase;
        }

        private static void FinishAll()
        {
            SafeAreaFitter.ClearEditorOverrideSafeArea();
            bool success = Results.Count == Cases.Length &&
                           Results.All(r => r.Pass) &&
                           Errors.Count == 0;

            WriteReport(success);
            SessionState.SetInt(ExitCodeKey, success ? 0 : 1);
            SessionState.SetString(PhaseKey, "exit-editor");
            _stage = success ? Stage.Done : Stage.Failed;
            UnsubscribeLog();
            EditorApplication.ExitPlaymode();
        }

        private static bool ValidateMapFraming(out Rect bounds)
        {
            float halfHeight = _camera.orthographicSize;
            float halfWidth = halfHeight * _camera.aspect;
            float cx = _camera.transform.position.x;
            float cy = _camera.transform.position.y;
            bounds = Rect.MinMaxRect(cx - halfWidth, cy - halfHeight, cx + halfWidth, cy + halfHeight);

            return bounds.xMin <= 0f + 0.01f &&
                   bounds.xMax >= MapAData.Width - 0.01f &&
                   bounds.yMin <= 0f + 0.01f &&
                   bounds.yMax >= MapAData.Height - 0.01f;
        }

        private static bool IsUiControlReachable(Button button, Vector2 screenPoint)
        {
            if (button == null || !button.gameObject.activeInHierarchy || !button.interactable)
                return false;

            Image image = button.GetComponent<Image>();
            if (image == null || !image.raycastTarget)
                return false;

            return RectTransformUtility.RectangleContainsScreenPoint(
                button.GetComponent<RectTransform>(), screenPoint, null) &&
                image.Raycast(screenPoint, null);
        }

        private static bool TextContentFitsAllocatedRect(Text text)
        {
            if (text == null || !text.gameObject.activeInHierarchy)
                return false;

            Rect rect = text.rectTransform.rect;
            float preferredWidth = text.preferredWidth;
            float preferredHeight = text.preferredHeight;

            if (text.resizeTextForBestFit)
                return rect.width > 0f && rect.height > 0f;

            return preferredWidth <= rect.width + 1f &&
                   preferredHeight <= rect.height + 1f;
        }

        private static bool AnyIndependentTextRectOverlap(Text[] texts, out string details)
        {
            List<string> overlaps = new List<string>();
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] == null || !texts[i].gameObject.activeInHierarchy)
                    continue;

                Rect a = GetScreenRect(texts[i].rectTransform);
                for (int j = i + 1; j < texts.Length; j++)
                {
                    if (texts[j] == null || !texts[j].gameObject.activeInHierarchy)
                        continue;

                    Rect b = GetScreenRect(texts[j].rectTransform);
                    Rect intersection = Intersect(a, b);
                    if (intersection.width > PixelEpsilon && intersection.height > PixelEpsilon)
                    {
                        overlaps.Add(texts[i].gameObject.name + "<->" + texts[j].gameObject.name +
                                     ":" + RectText(intersection));
                    }
                }
            }

            details = overlaps.Count == 0 ? "none" : string.Join("|", overlaps.ToArray());
            return overlaps.Count > 0;
        }

        private static void RecordTouchTarget(string name, Rect rect)
        {
            float width = Mathf.Abs(rect.width);
            float height = Mathf.Abs(rect.height);
            float area = width * height;

            if (area <= 0f)
                return;

            if (_current.SmallestTouchArea <= 0f || area < _current.SmallestTouchArea)
            {
                _current.SmallestTouchArea = area;
                _current.SmallestTouchTargetName = name;
                _current.SmallestTouchWidth = width;
                _current.SmallestTouchHeight = height;
            }
        }

        private static RectTransform FindRect(string name)
        {
            GameObject go = GameObject.Find(name);
            if (go == null)
                throw new InvalidOperationException("Missing UI object " + name);
            RectTransform rect = go.GetComponent<RectTransform>();
            if (rect == null)
                throw new InvalidOperationException("Missing RectTransform on " + name);
            return rect;
        }

        private static Text FindText(string name)
        {
            GameObject go = GameObject.Find(name);
            if (go == null)
                throw new InvalidOperationException("Missing Text object " + name);
            Text text = go.GetComponent<Text>();
            if (text == null)
                throw new InvalidOperationException("Missing Text component on " + name);
            return text;
        }

        private static Button FindRadialCloseButton()
        {
            if (_hud.RadialRoot == null)
                return null;
            return _hud.RadialRoot.GetComponentsInChildren<Button>(true)
                .FirstOrDefault(b => b.gameObject.name == "RadialCloseButton");
        }

        private static bool DispatchPointerClick(Button button)
        {
            if (button == null || EventSystem.current == null)
                return false;

            Rect rect = GetScreenRect(button.GetComponent<RectTransform>());
            PointerEventData pointer = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left,
                position = rect.center
            };
            return ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        }

        private static Rect GetRelativeChildrenScreenBounds(RectTransform root)
        {
            RectTransform[] rects = root.GetComponentsInChildren<RectTransform>(true);
            bool initialized = false;
            float minX = 0f, minY = 0f, maxX = 0f, maxY = 0f;

            foreach (RectTransform rect in rects)
            {
                if (rect == root || !rect.gameObject.activeInHierarchy)
                    continue;

                Rect screen = GetScreenRect(rect);
                if (!initialized)
                {
                    minX = screen.xMin;
                    minY = screen.yMin;
                    maxX = screen.xMax;
                    maxY = screen.yMax;
                    initialized = true;
                }
                else
                {
                    minX = Mathf.Min(minX, screen.xMin);
                    minY = Mathf.Min(minY, screen.yMin);
                    maxX = Mathf.Max(maxX, screen.xMax);
                    maxY = Mathf.Max(maxY, screen.yMax);
                }
            }

            return initialized ? Rect.MinMaxRect(minX, minY, maxX, maxY) : new Rect();
        }

        private static Rect GetScreenRect(RectTransform rect)
        {
            if (rect == null)
                return new Rect();

            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            Vector2 p0 = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
            float minX = p0.x;
            float minY = p0.y;
            float maxX = p0.x;
            float maxY = p0.y;

            for (int i = 1; i < 4; i++)
            {
                Vector2 p = RectTransformUtility.WorldToScreenPoint(null, corners[i]);
                minX = Mathf.Min(minX, p.x);
                minY = Mathf.Min(minY, p.y);
                maxX = Mathf.Max(maxX, p.x);
                maxY = Mathf.Max(maxY, p.y);
            }

            return Rect.MinMaxRect(minX, minY, maxX, maxY);
        }

        private static bool ContainsRect(Rect outer, Rect inner)
        {
            return inner.xMin >= outer.xMin - PixelEpsilon &&
                   inner.yMin >= outer.yMin - PixelEpsilon &&
                   inner.xMax <= outer.xMax + PixelEpsilon &&
                   inner.yMax <= outer.yMax + PixelEpsilon;
        }

        private static bool RectApproximately(Rect a, Rect b, float tolerance)
        {
            return Mathf.Abs(a.x - b.x) <= tolerance &&
                   Mathf.Abs(a.y - b.y) <= tolerance &&
                   Mathf.Abs(a.width - b.width) <= tolerance &&
                   Mathf.Abs(a.height - b.height) <= tolerance;
        }

        private static Rect Intersect(Rect a, Rect b)
        {
            float minX = Mathf.Max(a.xMin, b.xMin);
            float minY = Mathf.Max(a.yMin, b.yMin);
            float maxX = Mathf.Min(a.xMax, b.xMax);
            float maxY = Mathf.Min(a.yMax, b.yMax);
            if (maxX <= minX || maxY <= minY)
                return new Rect();
            return Rect.MinMaxRect(minX, minY, maxX, maxY);
        }

        private static bool DeviceSimulatorPackagePresent()
        {
            try
            {
                string manifest = File.ReadAllText("Packages/manifest.json");
                return manifest.IndexOf("com.unity.device-simulator", StringComparison.OrdinalIgnoreCase) >= 0;
            }
            catch
            {
                return false;
            }
        }

        private static FieldInfo RequireField(Type type, string name)
        {
            FieldInfo field = type.GetField(name, PrivateInstance);
            if (field == null)
                throw new MissingFieldException(type.FullName, name);
            return field;
        }

        private static void WriteReport(bool success)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "Artifacts/UnityExecutor");
            List<string> lines = new List<string>
            {
                "DYRYGENT UNITY EXECUTOR T0007 MOBILE LAYOUT",
                "timestampUtc=" + DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                "unityVersion=" + Application.unityVersion,
                "status=" + (success ? "PASS" : "FAIL"),
                "playModeEntered=" + SessionState.GetBool(PlayModeEnteredKey, false),
                "androidBuildRun=False",
                "t0008Started=False",
                "deviceSimulatorPackagePresent=" + DeviceSimulatorPackagePresent(),
                "safeAreaSimulationLimitation=Device Simulator package is absent; synthetic case uses an Editor-only override passed through production SafeAreaFitter."
            };

            foreach (LayoutResult result in Results)
            {
                string prefix = "layout." + result.Name + ".";
                lines.Add(prefix + "requestedResolution=" + result.RequestedWidth + "x" + result.RequestedHeight);
                lines.Add(prefix + "observedResolution=" + result.ObservedWidth + "x" + result.ObservedHeight);
                lines.Add(prefix + "aspectRatio=" + F(result.Aspect));
                lines.Add(prefix + "resolutionConfigured=" + result.ResolutionConfigured);
                lines.Add(prefix + "gameViewConfiguration=" + Sanitize(result.GameViewConfiguration));
                lines.Add(prefix + "simulationMethod=" + result.SimulationMethod);
                lines.Add(prefix + "nativeScreenSafeArea=" + RectText(result.NativeScreenSafeArea));
                lines.Add(prefix + "effectiveSafeArea=" + RectText(result.EffectiveSafeArea));
                lines.Add(prefix + "actualSafeAreaRect=" + RectText(result.ActualSafeAreaRect));
                lines.Add(prefix + "safeAreaApplied=" + result.SafeAreaApplied);
                lines.Add(prefix + "mapFullyVisible=" + result.MapFullyVisible);
                lines.Add(prefix + "cameraWorldBounds=" + RectText(result.CameraWorldBounds));
                lines.Add(prefix + "cameraCentered=" + result.CameraCentered);
                lines.Add(prefix + "cameraPanningRequired=" + result.CameraPanningRequired);
                lines.Add(prefix + "hudPass=" + result.HudPass);
                lines.Add(prefix + "hudInsideSafeArea=" + result.HudInsideSafeArea);
                lines.Add(prefix + "hudRects=" + result.HudRects);
                lines.Add(prefix + "waveButtonPass=" + result.WaveButtonPass);
                lines.Add(prefix + "waveButtonVisible=" + result.WaveButtonVisible);
                lines.Add(prefix + "waveButtonReachable=" + result.WaveButtonReachable);
                lines.Add(prefix + "waveButtonRect=" + RectText(result.WaveButtonRect));
                lines.Add(prefix + "radialEdgePass=" + result.RadialEdgePass);
                lines.Add(prefix + "edgeSpotPasses=" + string.Join(",",
                    EdgeSpotIds.Select(id => id + ":" +
                        (result.EdgeSpotPass.ContainsKey(id) && result.EdgeSpotPass[id])).ToArray()));
                lines.Add(prefix + "edgeMenuBounds=" + string.Join(",",
                    EdgeSpotIds.Select(id => id + ":" +
                        (result.EdgeMenuBounds.ContainsKey(id) ? result.EdgeMenuBounds[id] : "missing")).ToArray()));
                lines.Add(prefix + "textOverlapPass=" + result.TextOverlapPass);
                lines.Add(prefix + "textOverlapDetails=" + result.TextOverlapDetails);
                lines.Add(prefix + "textContentFits=" + result.TextContentFits);
                lines.Add(prefix + "plantingSpotsAccessible=" + result.AccessibleSpotCount + "/12");
                lines.Add(prefix + "inaccessibleSpots=" +
                    (result.InaccessibleSpots.Count == 0 ? "none" : string.Join(",", result.InaccessibleSpots.ToArray())));
                lines.Add(prefix + "smallestTouchTarget=" + result.SmallestTouchTargetText);
                lines.Add(prefix + "consoleErrors=" + result.ConsoleErrors);
                lines.Add(prefix + "consoleWarnings=" + result.ConsoleWarnings);
                lines.Add(prefix + "pass=" + result.Pass);
            }

            lines.Add("runtimeWarningCount=" + RuntimeWarnings.Count);
            for (int i = 0; i < RuntimeWarnings.Count; i++)
                lines.Add("runtimeWarning." + (i + 1) + "=" + Sanitize(RuntimeWarnings[i]));

            lines.Add("validationErrorCount=" + Errors.Count);
            for (int i = 0; i < Errors.Count; i++)
                lines.Add("validationError." + (i + 1) + "=" + Sanitize(Errors[i]));

            string report = string.Join("\n", lines) + "\n";
            File.WriteAllText(ReportPath, report);
            Debug.Log(report);
        }

        private static void FailAndLeavePlayMode(string message)
        {
            if (!string.IsNullOrEmpty(message))
                Errors.Add(message);

            SafeAreaFitter.ClearEditorOverrideSafeArea();
            WriteReport(false);
            SessionState.SetInt(ExitCodeKey, 1);
            SessionState.SetString(PhaseKey, "exit-editor");
            _stage = Stage.Failed;
            UnsubscribeLog();

            if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
                EditorApplication.ExitPlaymode();
            else
            {
                SessionState.SetBool(ActiveKey, false);
                EditorApplication.Exit(1);
            }
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

        private static void ResetState()
        {
            Errors.Clear();
            RuntimeWarnings.Clear();
            Results.Clear();
            _initialized = false;
            _warmupFrames = 0;
            _stage = Stage.RequestCase;
            _caseIndex = 0;
            _spotIndex = 0;
            _settleFrames = 0;
            _resolutionWaitFrames = 0;
            _session = null;
            _planting = null;
            _hud = null;
            _camera = null;
            _spots = null;
            _waveButton = null;
            _waveButtonText = null;
            _current = null;
            _currentSpotCloseClicked = false;
        }

        private static string RectText(Rect rect)
        {
            return F(rect.x) + "," + F(rect.y) + "," + F(rect.width) + "," + F(rect.height);
        }

        private static string F(float value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        private static string Sanitize(string value)
        {
            return (value ?? string.Empty).Replace("\r", " ").Replace("\n", " ");
        }

        private sealed class LayoutCase
        {
            internal readonly string Name;
            internal readonly int Width;
            internal readonly int Height;
            internal readonly bool UseSyntheticSafeArea;
            internal readonly Rect SafeArea;

            internal LayoutCase(string name, int width, int height, bool synthetic, Rect safeArea)
            {
                Name = name;
                Width = width;
                Height = height;
                UseSyntheticSafeArea = synthetic;
                SafeArea = safeArea;
            }
        }

        private sealed class LayoutResult
        {
            internal string Name;
            internal int RequestedWidth;
            internal int RequestedHeight;
            internal int ObservedWidth;
            internal int ObservedHeight;
            internal float Aspect;
            internal bool ResolutionConfigured;
            internal string GameViewConfiguration = string.Empty;
            internal bool SyntheticSafeArea;
            internal string SimulationMethod = string.Empty;
            internal bool DeviceSimulatorPackagePresent;
            internal Rect NativeScreenSafeArea;
            internal Rect EffectiveSafeArea;
            internal Rect ActualSafeAreaRect;
            internal bool SafeAreaApplied;
            internal bool MapFullyVisible;
            internal Rect CameraWorldBounds;
            internal bool CameraCentered;
            internal bool CameraPanningRequired;
            internal bool HudPass;
            internal bool HudInsideSafeArea;
            internal string HudRects = string.Empty;
            internal bool WaveButtonPass;
            internal bool WaveButtonVisible;
            internal bool WaveButtonInsideSafeArea;
            internal bool WaveButtonReachable;
            internal Rect WaveButtonRect;
            internal bool RadialEdgePass = true;
            internal readonly Dictionary<string, bool> EdgeSpotPass = new Dictionary<string, bool>();
            internal readonly Dictionary<string, string> EdgeMenuBounds = new Dictionary<string, string>();
            internal bool TextOverlapPass;
            internal string TextOverlapDetails = string.Empty;
            internal bool TextContentFits;
            internal int AccessibleSpotCount;
            internal readonly List<string> InaccessibleSpots = new List<string>();
            internal float SmallestTouchArea;
            internal string SmallestTouchTargetName = "none";
            internal float SmallestTouchWidth;
            internal float SmallestTouchHeight;
            internal string SmallestTouchTargetText = "none";
            internal int ConsoleErrors;
            internal int ConsoleWarnings;
            internal bool PlantingSpotsAccessible;
            internal bool Pass;
        }
    }
}
#endif
