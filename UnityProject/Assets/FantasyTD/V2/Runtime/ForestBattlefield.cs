using System;
using System.Collections.Generic;
using FantasyTD.MapA;
using FantasyTD.V2.Content;
using FantasyTD.V2.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FantasyTD.V2.Runtime
{
    /// <summary>
    /// Playable first V2 battlefield. Scene contains only this component:
    /// all encounter logic is delegated to BattleSimulator, with no duplicate combat rules.
    /// Temporary procedural sprites are intentionally not production artwork.
    /// </summary>
    public sealed class ForestBattlefield : MonoBehaviour
    {
        private const float SecondsPerTick = 1f / BattleSimulator.TicksPerSecond;
        private const string CampaignSaveKey = "FantasyTD.V2.UnlockedStage.v1";
        private static readonly string[] TowerIds = { "thorn", "frost", "bloom" };
        private static Sprite _forestBackdrop;
        private static readonly Color Grass = new Color(.11f, .25f, .17f);
        private static readonly Color PathOuter = new Color(.26f, .18f, .13f);
        private static readonly Color PathInner = new Color(.57f, .42f, .28f);
        private readonly Dictionary<string, SpriteRenderer> _siteMarkers =
            new Dictionary<string, SpriteRenderer>();
        private readonly Dictionary<int, EnemyView> _enemyViews =
            new Dictionary<int, EnemyView>();

        private sealed class EnemyView
        {
            public GameObject Root;
            public Transform HealthFill;
        }

        private LevelDefinition _level;
        private BattleSimulator _battle;
        private GameObject _world;
        private Canvas _hud;
        private Text _stats;
        private Text _prompt;
        private Button _nextWave;
        private Button _pauseButton;
        private readonly BattlePauseGate _pauseGate = new BattlePauseGate();
        private Button _previousStageButton;
        private Button _nextStageButton;
        private Button[] _towerButtons;
        private BattleFeedbackPresenter _feedback;
        private int _currentStage;
        private bool _victoryRecorded;
        private string _selectedSite;
        private float _tickAccumulator;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            _currentStage = Mathf.Clamp(PlayerPrefs.GetInt(CampaignSaveKey, 1),
                1, CampaignCatalog.LevelCount);
            SetupCamera();
            SetupCanvas();
            RestartBattle();
        }

        private void SetupCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                var cameraObject = new GameObject("V2 Camera");
                cameraObject.tag = "MainCamera";
                cam = cameraObject.AddComponent<Camera>();
            }
            cam.orthographic = true;
            cam.orthographicSize = 8.35f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.backgroundColor = Grass;
            cam.clearFlags = CameraClearFlags.SolidColor;
        }

        private void SetupCanvas()
        {
            if (EventSystem.current == null)
            {
                var eventSystem = new GameObject("V2 EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
            }
            var canvasObject = new GameObject("V2 Battle HUD",
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            _hud = canvasObject.GetComponent<Canvas>();
            _hud.renderMode = RenderMode.ScreenSpaceOverlay;
            _hud.sortingOrder = 50;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = .5f;
            CreateHudPanel("Top status panel", new Vector2(.5f, 1f),
                new Vector2(0f, -73f), new Vector2(1800f, 135f),
                new Color(.08f, .16f, .12f, .86f));
            CreateHudPanel("Bottom controls panel", new Vector2(.5f, 0f),
                new Vector2(0f, 94f), new Vector2(1860f, 164f),
                new Color(.09f, .17f, .12f, .84f));
            _feedback = canvasObject.AddComponent<BattleFeedbackPresenter>();
            _feedback.Initialize(_hud, Camera.main);

            _stats = RuntimeArt.CreateText(_hud.transform, "Battle stats", 37,
                TextAnchor.MiddleCenter, Color.white);
            Position(_stats.rectTransform, new Vector2(.5f, 1f),
                new Vector2(0f, -73f), new Vector2(1700f, 120f));

            _prompt = RuntimeArt.CreateText(_hud.transform, "Battle guidance", 37,
                TextAnchor.MiddleCenter, new Color(1f, .91f, .6f));
            Position(_prompt.rectTransform, new Vector2(.5f, 0f),
                new Vector2(0f, 220f), new Vector2(1600f, 70f));

            string[] captions = {
                "Pędostrzał · 55", "Mroźny Pąk · 75", "Kwiat Esencji · 45"
            };
            Color[] colors = {
                new Color(.22f, .47f, .21f),
                new Color(.24f, .48f, .64f),
                new Color(.67f, .48f, .19f)
            };
            _towerButtons = new Button[TowerIds.Length];
            for (int i = 0; i < TowerIds.Length; i++)
            {
                string towerId = TowerIds[i];
                _towerButtons[i] = RuntimeArt.CreateButton(_hud.transform,
                    captions[i], colors[i], 32);
                Position((RectTransform)_towerButtons[i].transform,
                    new Vector2(.5f, 0f),
                    new Vector2(-570f + i * 360f, 90f),
                    new Vector2(333f, 110f));
                _towerButtons[i].onClick.AddListener(() => Build(towerId));
            }

            _nextWave = RuntimeArt.CreateButton(_hud.transform,
                "Następna fala", new Color(.53f, .24f, .21f), 32);
            Position((RectTransform)_nextWave.transform, new Vector2(.5f, 0f),
                new Vector2(590f, 90f), new Vector2(330f, 110f));
            _nextWave.onClick.AddListener(AdvanceOrRestart);

            _previousStageButton = RuntimeArt.CreateButton(_hud.transform,
                "< Poprzedni", new Color(.21f, .30f, .38f), 27);
            Position((RectTransform)_previousStageButton.transform,
                new Vector2(0f, 1f), new Vector2(175f, -155f),
                new Vector2(300f, 75f));
            _previousStageButton.onClick.AddListener(() => ChangeStage(-1));

            _nextStageButton = RuntimeArt.CreateButton(_hud.transform,
                "Następny >", new Color(.21f, .30f, .38f), 27);
            Position((RectTransform)_nextStageButton.transform,
                new Vector2(1f, 1f), new Vector2(-175f, -155f),
                new Vector2(300f, 75f));
            _nextStageButton.onClick.AddListener(() => ChangeStage(1));

            _pauseButton = RuntimeArt.CreateButton(_hud.transform,
                "Pauza", new Color(.26f, .32f, .23f), 27);
            Position((RectTransform)_pauseButton.transform,
                new Vector2(1f, 1f), new Vector2(-170f, -255f),
                new Vector2(270f, 76f));
            _pauseButton.onClick.AddListener(TogglePause);
        }

        private void CreateHudPanel(string name, Vector2 anchor,
            Vector2 position, Vector2 size, Color tint)
        {
            GameObject panel = new GameObject(name,
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(_hud.transform, false);
            panel.transform.SetAsFirstSibling();
            Image image = panel.GetComponent<Image>();
            image.sprite = RuntimeArt.White;
            image.color = tint;
            image.raycastTarget = false;
            Position((RectTransform)panel.transform, anchor, position, size);
        }

        private static void Position(RectTransform rect, Vector2 anchor,
            Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(.5f, .5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        public void RestartBattle()
        {
            if (_world != null) Destroy(_world);
            _enemyViews.Clear();
            _siteMarkers.Clear();
            _selectedSite = null;
            _tickAccumulator = 0f;
            _pauseGate.Reset();
            _victoryRecorded = false;
            _level = CampaignCatalog.CreateLevel(_currentStage);
            _battle = new BattleSimulator(_level);
            _battle.EventRaised += OnBattleEvent;
            _world = new GameObject("V2 World");
            if (_feedback != null) _feedback.BindWorld(_world.transform);
            DrawTerrainAndPath();
            DrawBuildSites();
            _prompt.text = "Wybierz świecące miejsce, posadź obrońcę i uruchom falę.";
            RefreshHud();
        }

        private void DrawTerrainAndPath()
        {
            Sprite("Moss meadow", new Vector2(0f, 0f),
                new Vector2(29f, 16f), Grass, -30, false);
            // Existing MAP1 approved artwork, recovered by SHA from the archived
            // visual branch. Route and build sites remain driven by gameplay data.
            // Unity imports the authored forest PNG as a Sprite. Loading it as a
            // Texture2D alone may return null even though the PNG exists.
            Sprite artSprite = Resources.Load<Sprite>("FT2/Forest01Art");
            if (artSprite == null)
            {
                Texture2D fallbackTexture = Resources.Load<Texture2D>("FT2/Forest01Art");
                if (fallbackTexture != null && _forestBackdrop == null)
                    _forestBackdrop = UnityEngine.Sprite.Create(fallbackTexture,
                        new Rect(0, 0, fallbackTexture.width, fallbackTexture.height),
                        new Vector2(.5f, .5f), 100f);
                artSprite = _forestBackdrop;
            }
            else _forestBackdrop = artSprite;
            if (artSprite != null)
            {
                SpriteRenderer background = Sprite("Recovered forest art",
                    Vector2.zero, Vector2.one, Color.white, -29, false);
                background.sprite = artSprite;
                background.color = Color.white;
                Vector2 bounds = artSprite.bounds.size;
                background.transform.localScale = new Vector3(
                    29f / Mathf.Max(.01f, bounds.x),
                    16f / Mathf.Max(.01f, bounds.y), 1f);
            }
            else Debug.LogError("FT2: Forest01Art missing from Resources/FT2.");
            // Flat procedural placeholders maintain legible background/route layering.
            for (int i = 0; i < (artSprite == null ? 48 : 0); i++)
            {
                float x = Mathf.Sin(i * 17.17f) * 12.8f;
                float y = Mathf.Cos(i * 9.3f) * 6.2f;
                Sprite("Moss tuft", new Vector2(x, y),
                    new Vector2(.30f + (i % 3) * .13f, .21f),
                    new Color(.17f, .33f + (i % 4) * .025f, .20f), -25, true);
            }
            for (int i = 1; i < _level.Path.Count; i++)
            {
                Vector2 a = Point(_level.Path[i - 1]);
                Vector2 b = Point(_level.Path[i]);
                Vector2 offset = b - a;
                Vector2 center = (a + b) * .5f;
                float angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
                SpriteRenderer rim = Sprite("Path border " + i, center,
                    new Vector2(offset.magnitude + .72f, 1.36f),
                    artSprite != null ? new Color(.20f, .17f, .10f, .26f) : PathOuter,
                    -21, false);
                SpriteRenderer surface = Sprite("Walkable path " + i, center,
                    new Vector2(offset.magnitude + .44f, 1.07f),
                    artSprite != null ? new Color(.71f, .55f, .34f, .21f) : PathInner,
                    -20, false);
                rim.transform.rotation = Quaternion.Euler(0f, 0f, angle);
                surface.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
            Sprite("Forest gate", Point(_level.Path[0]),
                new Vector2(1.1f, 1.1f), new Color(.47f, .75f, .34f), -9, true);
            Sprite("Forest sanctuary", Point(_level.Path[_level.Path.Count - 1]),
                new Vector2(1.55f, 1.55f), new Color(.95f, .70f, .20f), -9, true);
        }

        private void DrawBuildSites()
        {
            foreach (var kv in _level.Sites)
            {
                Vector2 pos = Point(kv.Value);
                Sprite("Site shadow " + kv.Key, pos, new Vector2(1.2f, 1.2f),
                    new Color(.04f, .11f, .09f), -7, true);
                SpriteRenderer marker = Sprite("Build site " + kv.Key, pos,
                    new Vector2(.91f, .91f), new Color(.5f, .80f, .49f), -6, true);
                var trigger = marker.gameObject.AddComponent<CircleCollider2D>();
                trigger.isTrigger = true;
                trigger.radius = .48f;
                _siteMarkers.Add(kv.Key, marker);
                Sprite("Site seed " + kv.Key, pos, new Vector2(.20f, .20f),
                    new Color(.92f, .81f, .38f), -5, true);
            }
        }

        private SpriteRenderer Sprite(string name, Vector2 pos, Vector2 size,
            Color tint, int sorting, bool circle)
        {
            return RuntimeArt.SpriteObject(name, _world.transform,
                pos, size, tint, sorting, circle).GetComponent<SpriteRenderer>();
        }

        private static Vector2 Point(MapPoint point)
        {
            return new Vector2(point.X / 100f, point.Y / 100f);
        }

        private void OnBattleEvent(BattleEvent evt)
        {
            if (_feedback == null) return;
            Vector3 location = Vector3.zero;
            if (evt.EntityId > 0)
            {
                EnemyView view;
                if (_enemyViews.TryGetValue(evt.EntityId, out view) && view.Root != null)
                    location = view.Root.transform.position;
                else
                {
                    foreach (EnemyState enemy in _battle.Enemies)
                        if (enemy.InstanceId == evt.EntityId)
                        {
                            Vector2 p = Point(_level.AtDistance(enemy.Distance));
                            location = new Vector3(p.x, p.y, 0f);
                            break;
                        }
                }
            }
            else if (evt.Kind == BattleEventKind.TowerPlaced &&
                _selectedSite != null && _level.Sites.ContainsKey(_selectedSite))
            {
                Vector2 p = Point(_level.Sites[_selectedSite]);
                location = new Vector3(p.x, p.y, 0f);
            }
            else if (evt.Kind == BattleEventKind.Income)
            {
                foreach (TowerState tower in _battle.PlacedTowers.Values)
                {
                    if (tower.Definition.Id != evt.ContentId) continue;
                    Vector2 p = Point(tower.Position);
                    location = new Vector3(p.x, p.y, 0f);
                    break;
                }
            }
            _feedback.Show(evt, location);
        }

        private void TogglePause()
        {
            if (_battle == null || _battle.Result != BattleResult.InProgress) return;
            _pauseGate.Toggle();
            _tickAccumulator = 0f;
            _prompt.text = _pauseGate.IsPaused
                ? "PAUZA — dotknij Wznów, aby kontynuować."
                : "Wznowiono obronę.";
            RefreshHud();
        }

        private void Update()
        {
            if (_battle == null) return;
            if (_pauseGate.IsPaused)
            {
                _tickAccumulator = 0f;
                RefreshHud();
                return;
            }
            HandleBattlefieldTouch();
            _tickAccumulator += Mathf.Min(Time.deltaTime, .25f);
            int maxSteps = 5;
            while (_tickAccumulator >= SecondsPerTick && maxSteps-- > 0)
            {
                _tickAccumulator -= SecondsPerTick;
                _battle.Step();
            }
            // Prevent a long background pause from fast-forwarding combat.
            if (_tickAccumulator >= SecondsPerTick)
                _tickAccumulator = 0f;
            SyncEnemyViews();
            RecordVictoryIfNeeded();
            RefreshHud();
        }

        private void HandleBattlefieldTouch()
        {
            if (_battle.Result != BattleResult.InProgress) return;

            Vector3 screen;
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase != TouchPhase.Began) return;
                if (EventSystem.current != null &&
                    EventSystem.current.IsPointerOverGameObject(touch.fingerId)) return;
                screen = touch.position;
            }
            else
            {
                if (!Input.GetMouseButtonDown(0)) return;
                if (EventSystem.current != null &&
                    EventSystem.current.IsPointerOverGameObject()) return;
                screen = Input.mousePosition;
            }

            Camera camera = Camera.main;
            if (camera == null) return;
            Vector3 world = camera.ScreenToWorldPoint(screen);
            string closest = null;
            float distance = .68f;
            foreach (var site in _level.Sites)
            {
                if (_battle.PlacedTowers.ContainsKey(site.Key)) continue;
                float d = Vector2.Distance(new Vector2(world.x, world.y), Point(site.Value));
                if (d >= distance) continue;
                closest = site.Key;
                distance = d;
            }
            _selectedSite = closest;
            foreach (var marker in _siteMarkers)
                marker.Value.color = marker.Key == _selectedSite
                    ? new Color(1f, .93f, .51f) : new Color(.50f, .80f, .49f);
            _prompt.text = closest == null
                ? "Dotknij wolnego miejsca budowy."
                : "Wybrano " + closest + " — wybierz obrońcę poniżej.";
        }

        private void Build(string towerId)
        {
            if (string.IsNullOrEmpty(_selectedSite))
            {
                _prompt.text = "Najpierw wskaż wolne miejsce na mapie.";
                return;
            }
            if (!_battle.TryPlaceTower(_selectedSite, towerId))
            {
                _prompt.text = "Nie można posadzić: za mało Esencji lub miejsce zajęte.";
                return;
            }
            Vector2 pos = Point(_level.Sites[_selectedSite]);
            DrawDefender(_selectedSite, towerId, pos);
            _siteMarkers[_selectedSite].gameObject.SetActive(false);
            _selectedSite = null;
            _prompt.text = "Obrońca posadzony. Przygotuj kolejną jednostkę lub falę.";
        }

        private void DrawDefender(string site, string type, Vector2 pos)
        {
            Color tint = type == "frost" ? new Color(.56f, .83f, 1f)
                : type == "bloom" ? new Color(1f, .81f, .32f)
                : new Color(.38f, .71f, .30f);
            Sprite("Stem " + site, pos + new Vector2(0f, -.2f),
                new Vector2(.24f, .74f), new Color(.18f, .41f, .16f), 1, false);
            for (int i = 0; i < 5; i++)
            {
                float radians = i * 2f * Mathf.PI / 5f;
                Vector2 petal = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * .29f;
                Sprite("Petal " + site + "-" + i, pos + petal,
                    new Vector2(.48f, .48f), tint, 2, true);
            }
            Sprite("Core " + site, pos, new Vector2(.48f, .48f),
                new Color(.93f, .96f, .67f), 3, true);
        }

        private void SyncEnemyViews()
        {
            var seen = new HashSet<int>();
            foreach (EnemyState enemy in _battle.Enemies)
            {
                seen.Add(enemy.InstanceId);
                EnemyView view;
                if (!_enemyViews.TryGetValue(enemy.InstanceId, out view))
                {
                    view = CreateEnemyView(enemy);
                    _enemyViews.Add(enemy.InstanceId, view);
                }
                Vector2 point = Point(_level.AtDistance(enemy.Distance));
                view.Root.transform.localPosition = new Vector3(point.x, point.y, 0f);
                if (view.HealthFill != null)
                {
                    var scale = view.HealthFill.localScale;
                    scale.x = Mathf.Clamp01((float)enemy.Health / enemy.Definition.Health);
                    view.HealthFill.localScale = scale;
                }
            }
            var retired = new List<int>();
            foreach (var kv in _enemyViews)
            {
                if (seen.Contains(kv.Key)) continue;
                Destroy(kv.Value.Root);
                retired.Add(kv.Key);
            }
            foreach (int id in retired) _enemyViews.Remove(id);
        }

        private EnemyView CreateEnemyView(EnemyState enemy)
        {
            var root = new GameObject("Enemy " + enemy.InstanceId + " " + enemy.Definition.Id);
            root.transform.SetParent(_world.transform, false);
            Color tint = enemy.Definition.Id == "runner" ? new Color(.91f, .49f, .25f)
                : enemy.Definition.Id == "armored" ? new Color(.46f, .46f, .62f)
                : new Color(.44f, .74f, .32f);
            RuntimeArt.SpriteObject("Body", root.transform, Vector2.zero,
                new Vector2(.85f, .85f), tint, 15, true);
            RuntimeArt.SpriteObject("Eyes", root.transform, new Vector2(.14f, .16f),
                new Vector2(.25f, .12f), Color.white, 16, false);
            RuntimeArt.SpriteObject("HP background", root.transform, new Vector2(0f, .64f),
                new Vector2(.90f, .13f), new Color(.15f, .14f, .15f), 17, false);
            GameObject hp = RuntimeArt.SpriteObject("HP fill", root.transform,
                new Vector2(0f, .64f), new Vector2(.78f, .08f),
                new Color(.58f, .95f, .40f), 18, false);
            return new EnemyView { Root = root, HealthFill = hp.transform };
        }

        private void RecordVictoryIfNeeded()
        {
            if (_victoryRecorded || _battle.Result != BattleResult.Victory)
                return;
            _victoryRecorded = true;
            int highest = Mathf.Clamp(PlayerPrefs.GetInt(CampaignSaveKey, 1),
                1, CampaignCatalog.LevelCount);
            int next = Mathf.Min(CampaignCatalog.LevelCount, _currentStage + 1);
            if (next > highest)
            {
                PlayerPrefs.SetInt(CampaignSaveKey, next);
                PlayerPrefs.Save();
            }
        }

        private void ChangeStage(int offset)
        {
            if (_battle == null || _battle.WaveActive) return;
            int unlocked = Mathf.Clamp(PlayerPrefs.GetInt(CampaignSaveKey, 1),
                1, CampaignCatalog.LevelCount);
            int target = _currentStage + offset;
            if (target < 1 || target > unlocked) return;
            _currentStage = target;
            RestartBattle();
        }

        private void AdvanceOrRestart()
        {
            if (_battle.Result != BattleResult.InProgress)
            {
                if (_battle.Result == BattleResult.Victory &&
                    _currentStage < CampaignCatalog.LevelCount)
                {
                    RecordVictoryIfNeeded();
                    _currentStage++;
                }
                RestartBattle();
                return;
            }
            if (_battle.StartNextWave())
                _prompt.text = "Fala " + _battle.WaveNumber + " rozpoczęta!";
            else
                _prompt.text = "Najpierw zakończ obecną falę.";
        }

        private void RefreshHud()
        {
            if (_stats == null || _battle == null) return;
            _stats.text = CampaignCatalog.WorldName(_currentStage) + "  " +
                _currentStage + "/" + CampaignCatalog.LevelCount +
                "  |  Esencja " + _battle.Essence +
                "  |  Życie " + _battle.BaseHp +
                "  |  Fala " + _battle.WaveNumber + "/" + _battle.TotalWaves +
                "  |  Pokonani " + _battle.Kills;

            bool gameOver = _battle.Result != BattleResult.InProgress;
            for (int i = 0; i < _towerButtons.Length; i++)
                _towerButtons[i].interactable = !gameOver &&
                    _selectedSite != null && !_pauseGate.IsPaused &&
                    _level.Towers.ContainsKey(TowerIds[i]) &&
                    _battle.Essence >= _level.Towers[TowerIds[i]].Cost;
            _nextWave.interactable = !_pauseGate.IsPaused &&
                (gameOver || _battle.CanStartWave);
            int unlocked = Mathf.Clamp(PlayerPrefs.GetInt(CampaignSaveKey, 1),
                1, CampaignCatalog.LevelCount);
            _previousStageButton.interactable = !_pauseGate.IsPaused &&
                !_battle.WaveActive && _currentStage > 1;
            _nextStageButton.interactable = !_pauseGate.IsPaused &&
                !_battle.WaveActive && _currentStage < unlocked;
            _pauseButton.interactable = !gameOver;
            Text pauseLabel = _pauseButton.GetComponentInChildren<Text>();
            if (pauseLabel != null)
                pauseLabel.text = _pauseGate.IsPaused ? "Wznów" : "Pauza";
            Text label = _nextWave.GetComponentInChildren<Text>();
            if (label != null) label.text = _battle.Result == BattleResult.Victory &&
                _currentStage < CampaignCatalog.LevelCount ? "Następny poziom"
                : gameOver ? "Zagraj ponownie" : "Następna fala";
            if (_battle.Result == BattleResult.Victory)
                _prompt.text = "ZWYCIĘSTWO! Ukończony etap " + _currentStage + ".";
            else if (_battle.Result == BattleResult.Defeat)
                _prompt.text = "PORAŻKA. Spróbuj ponownie lub wybierz etap.";
        }
    }
}
