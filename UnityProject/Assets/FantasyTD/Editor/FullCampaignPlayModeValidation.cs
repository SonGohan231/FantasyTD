#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using FantasyTD.MapA;
using FantasyTD.MapA.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FantasyTD.CI
{
    [InitializeOnLoad]
    internal static class FullCampaignPlayModeValidation
    {
        private const string ActiveKey = "FantasyTD.FullCampaign.Active";
        private const string PhaseKey = "FantasyTD.FullCampaign.Phase";
        private const string StartedKey = "FantasyTD.FullCampaign.StartedUtc";
        private const string ExitCodeKey = "FantasyTD.FullCampaign.ExitCode";
        private const string PlayModeEnteredKey = "FantasyTD.FullCampaign.PlayModeEntered";
        private const string OriginalBackgroundKey = "FantasyTD.FullCampaign.OriginalRunInBackground";
        private const string OriginalTimeScaleKey = "FantasyTD.FullCampaign.OriginalTimeScale";
        private const string ReportPath = "Artifacts/UnityExecutor/full-campaign.txt";
        private const string ScreenshotPath = "Artifacts/UnityExecutor/full-campaign-960x540.png";
        private const float TimeoutSeconds = 600f;

        private static readonly List<string> Facts = new List<string>();
        private static readonly List<string> Errors = new List<string>();
        private static readonly List<string> RuntimeWarnings = new List<string>();

        private static bool _subscribed;
        private static int _warmupFrames;
        private static int _lastPlayerFrame = -1;
        private static GameSession _session;
        private static IEnumerator _flow;

        static FullCampaignPlayModeValidation()
        {
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
            if (SessionState.GetBool(ActiveKey, false))
                SubscribeLog();
        }

        internal static void Start()
        {
            Facts.Clear();
            Errors.Clear();
            RuntimeWarnings.Clear();
            _warmupFrames = 0;
            _lastPlayerFrame = -1;
            _session = null;
            _flow = null;
            SessionState.SetBool(OriginalBackgroundKey, Application.runInBackground);
            SessionState.SetFloat(OriginalTimeScaleKey, Time.timeScale);
            Application.runInBackground = true;

            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "Artifacts/UnityExecutor");
            SessionState.SetBool(ActiveKey, true);
            SessionState.SetBool(PlayModeEnteredKey, false);
            SessionState.SetString(PhaseKey, "enter-play");
            SessionState.SetString(StartedKey, DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));
            SessionState.SetInt(ExitCodeKey, 1);
            SubscribeLog();

            try
            {
                FantasyTD.Product.Editor.ProductFlowEditorSetup.EnsureProductScenes();
                MapAEditorSetup.OpenMap();
                EditorApplication.EnterPlaymode();
            }
            catch (Exception ex)
            {
                Errors.Add("startException=" + ex.GetType().Name + ": " + ex.Message);
                FinishImmediately(false);
            }
        }

        private static void Tick()
        {
            if (!SessionState.GetBool(ActiveKey, false))
                return;

            string phase = SessionState.GetString(PhaseKey, "enter-play");
            if (phase == "exit-editor")
            {
                if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    int exitCode = SessionState.GetInt(ExitCodeKey, 1);
                    SessionState.SetBool(ActiveKey, false);
                    UnsubscribeLog();
                    EditorApplication.Exit(exitCode);
                }
                return;
            }
            if (TimedOut())
            {
                Errors.Add("timeout=" + TimeoutSeconds.ToString("0", CultureInfo.InvariantCulture));
                Complete(false);
                return;
            }

            if (!EditorApplication.isPlaying)
                return;

            EditorApplication.QueuePlayerLoopUpdate();
            if (_lastPlayerFrame == Time.frameCount)
                return;
            _lastPlayerFrame = Time.frameCount;

            SessionState.SetBool(PlayModeEnteredKey, true);
            SubscribeLog();

            if (_session == null && _flow == null)
            {
                _warmupFrames++;
                if (_warmupFrames < 6)
                    return;

                _session = UnityEngine.Object.FindFirstObjectByType<GameSession>();
                if (_session == null ||
                    _session.Economy == null ||
                    _session.Planting == null ||
                    _session.Hud == null ||
                    _session.TowerPanel == null ||
                    Camera.main == null ||
                    EventSystem.current == null)
                    return;

                _flow = RunFlow();
                SessionState.SetString(PhaseKey, "validating");
            }

            try
            {
                if (_flow != null && !_flow.MoveNext())
                    Complete(Errors.Count == 0);
            }
            catch (Exception ex)
            {
                Errors.Add("validatorException=" + ex.ToString());
                Complete(false);
            }
        }


        private sealed class BattleStats
        {
            internal readonly HashSet<int> SpawnIds = new HashSet<int>();
            internal readonly HashSet<int> KillIds = new HashSet<int>();
            internal int Kills, Leaks, UnexpectedDespawns;
            internal bool BossSeen, BossKilled, BossLeaked;
        }
        private static BattleStats _battle;
        private static float _idleSeconds;
        private static float _preparationRealDeadline;
        private static int _incomeWaitOrdinal;

        private static IEnumerator RunFlow()
        {
            AddFact("validationResolution","headless functional gameplay simulation; rendering/UI pixels are gated separately by visual acceptance");
            AddFact("strategy","Headless functional simulation with real uGUI events, actual flower income, natural waves and combat; no damage/cash/HP/victory injections");
            Time.timeScale=4f;
            AddFact("timeScale",Time.timeScale);
            try
            {
                IEnumerator map1=RunNaturalMap("MAP1",FantasyTD.Product.ProductFlowScenes.MapASceneName,
                    new[]{"A2","A3","A4","A6","A7","A8","A10"},new[]{"A1","A5","A9","A11","A12"});
                while(map1.MoveNext()) yield return null;
                IEnumerator map2=RunNaturalMap("MAP2",FantasyTD.Product.ProductFlowScenes.Map2SceneName,
                    new[]{"B1","B2","B3","B4","B10","B12"},new[]{"B5","B6","B7","B8","B9","B11"});
                while(map2.MoveNext()) yield return null;
            }
            finally {Time.timeScale=SessionState.GetFloat(OriginalTimeScaleKey,1f);}
        }

        private static IEnumerator RunNaturalMap(string mapId,string sceneName,string[] flowers,string[] defenders)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            for(int frame=0;frame<120;frame++)
            {
                yield return null;
                _session=UnityEngine.Object.FindFirstObjectByType<GameSession>();
                if(_session!=null && _session.MapDefinition!=null && _session.MapDefinition.Id==mapId && _session.Hud!=null) break;
            }
            Check(_session!=null && _session.MapDefinition!=null && _session.MapDefinition.Id==mapId,mapId+" fresh session missing.");
            if(_session==null || _session.MapDefinition==null || _session.MapDefinition.Id!=mapId) yield break;
            Check(_session.Economy.Essence==Balance.StartEssence && _session.Base.Hp==Balance.BaseHp && _session.Waves.CurrentWave==0,
                mapId+" did not start from production initial state.");
            _battle=new BattleStats();
            _idleSeconds=0f;
            float prepStarted=Time.time;
            float prepRealStarted=Time.realtimeSinceStartup;
            int prepFrameStarted=Time.frameCount;
            float prepDeadline=prepStarted+600f;
            _preparationRealDeadline=prepRealStarted+180f;
            _incomeWaitOrdinal=0;
            AddFact(mapId+".preparationBefore",IncomeState());
            int errorsAtStart=Errors.Count;
            bool progressBefore=CampaignProgress.HasVictory(mapId);
            foreach(string id in flowers)
            {
                IEnumerator build=BuildWithIncome(mapId,id,PlantKind.EssenceFlower,prepDeadline);
                while(build.MoveNext()) yield return null;
                if(Errors.Count>errorsAtStart) yield break;
            }
            foreach(string id in flowers)
            {
                IEnumerator upgrade=UpgradeWithIncome(mapId,id,prepDeadline);
                while(upgrade.MoveNext()) yield return null;
                if(Errors.Count>errorsAtStart) yield break;
            }
            foreach(string id in defenders)
            {
                IEnumerator build=BuildWithIncome(mapId,id,PlantKind.FrostBud,prepDeadline);
                while(build.MoveNext()) yield return null;
                IEnumerator upgrade=UpgradeWithIncome(mapId,id,prepDeadline);
                while(upgrade.MoveNext()) yield return null;
                if(Errors.Count>errorsAtStart) yield break;
            }
            CloseTowerPanel();
            yield return null;
            AddFact(mapId+".preparationGameSeconds",F(Time.time-prepStarted));
            AddFact(mapId+".preparationRealSeconds",F(Time.realtimeSinceStartup-prepRealStarted));
            AddFact(mapId+".preparationFrames",Time.frameCount-prepFrameStarted);
            AddFact(mapId+".preparationAfter",IncomeState());
            AddFact(mapId+".passiveIncomeWaitGameSeconds",F(_idleSeconds));
            AddFact(mapId+".essenceBeforeWave1",_session.Economy.Essence);
            AddFact(mapId+".flowersL3",flowers.Length);
            AddFact(mapId+".frostDefendersL3",defenders.Length);
            Check(Time.time-prepStarted<=600f,mapId+" preparation exceeded 600 game seconds.");

            int[] expectedSpawns={6,8,9,13,15,12};
            for(int wave=1;wave<=6 && !_session.IsFinished;wave++)
            {
                int spawnsBefore=_battle.SpawnIds.Count,killsBefore=_battle.Kills,leaksBefore=_battle.Leaks;
                int baseBefore=_session.Base.Hp,essenceBefore=_session.Economy.Essence;
                Check(_session.Waves.ReadyForNextWave,mapId+" wave "+wave+" was not ready.");
                // Exercise the exact production wave API headlessly. The rendered START button
                // is exercised separately by ProductVisualAcceptanceValidator under Xvfb.
                _session.Waves.StartNextWave();
                AddFact(mapId+".wave"+wave+".startPath","production WaveDirector.StartNextWave");
                yield return null;
                Check(_session.Waves.CurrentWave==wave,mapId+" unexpected current wave.");
                float waveStarted=Time.time,deadline=Time.time+320f;
                while(_session.Waves.Running && !_session.IsFinished && Time.time<deadline)
                {
                    ObserveSpawnedEnemies();
                    yield return null;
                }
                ObserveSpawnedEnemies();
                for(int frame=0;frame<8;frame++) yield return null;
                int spawns=_battle.SpawnIds.Count-spawnsBefore;
                int killsThisWave=_battle.Kills-killsBefore;
                int leaksThisWave=Mathf.Max(0,spawns-killsThisWave);
                _battle.Leaks+=leaksThisWave;
                if(killsThisWave>spawns) _battle.UnexpectedDespawns+=killsThisWave-spawns;
                AddFact(mapId+".wave"+wave+".gameSeconds",F(Time.time-waveStarted));
                AddFact(mapId+".wave"+wave+".spawns",spawns);
                AddFact(mapId+".wave"+wave+".productionKills",killsThisWave);
                AddFact(mapId+".wave"+wave+".naturalLeaks",leaksThisWave);
                AddFact(mapId+".wave"+wave+".baseHpBefore",baseBefore);
                AddFact(mapId+".wave"+wave+".baseHpAfter",_session.Base.Hp);
                AddFact(mapId+".wave"+wave+".essenceBefore",essenceBefore);
                AddFact(mapId+".wave"+wave+".essenceAfter",_session.Economy.Essence);
                Check(Time.time<deadline,mapId+" wave "+wave+" exceeded natural timeout.");
                Check(spawns==expectedSpawns[wave-1],mapId+" wave "+wave+" spawn count mismatch.");
                Check(_battle.Kills-killsBefore+_battle.Leaks-leaksBefore==spawns,mapId+" wave "+wave+" unresolved enemies.");
                if(wave<6 && !_session.IsFinished) Check(_session.Waves.ReadyForNextWave,mapId+" next wave unavailable.");
                for(int frame=0;frame<8;frame++) yield return null;
            }
            AddFact(mapId+".totalSpawns",_battle.SpawnIds.Count);
            AddFact(mapId+".totalProductionKills",_battle.Kills);
            AddFact(mapId+".totalNaturalLeaks",_battle.Leaks);
            AddFact(mapId+".naturalVictory",_session.IsFinished && _session.IsVictory);
            if(_battle.BossSeen && !_battle.BossKilled) _battle.BossLeaked=true;
            AddFact(mapId+".bossSeen",_battle.BossSeen);
            AddFact(mapId+".bossKilledByProduction",_battle.BossKilled);
            AddFact(mapId+".bossLeaked",_battle.BossLeaked);
            AddFact(mapId+".baseHpFinal",_session.Base.Hp);
            AddFact(mapId+".progressBefore",progressBefore);
            AddFact(mapId+".progressAfter",CampaignProgress.HasVictory(mapId));
            AddFact(mapId+".progressEnabled",_session.MapDefinition.IsValidatedPlayable);
            Check(_session.Waves.CurrentWave==6 && _battle.SpawnIds.Count==63,mapId+" did not complete six natural waves.");
            Check(_session.IsFinished && _session.IsVictory && _session.Base.Hp>0,mapId+" did not achieve natural victory.");
            Check(_battle.BossSeen && _battle.BossKilled && !_battle.BossLeaked,mapId+" boss not defeated by combat; leaked boss is not full acceptance.");
            Check(_battle.UnexpectedDespawns==0,mapId+" unexpected despawn.");
            if(_session.MapDefinition.IsValidatedPlayable && _session.IsVictory) Check(CampaignProgress.HasVictory(mapId),mapId+" genuine victory not persisted.");
            else Check(CampaignProgress.HasVictory(mapId)==progressBefore,mapId+" unvalidated/failed map wrote progression.");
        }

        private static IEnumerator WaitForIncome(string mapId,int cost,float deadline)
        {
            float started=Time.time;
            float realStarted=Time.realtimeSinceStartup;
            int frameStarted=Time.frameCount;
            string key=mapId+".incomeWait"+(++_incomeWaitOrdinal);
            AddFact(key+".before","requiredEssence="+cost+";"+IncomeState());
            while(_session.Economy.Essence<cost && !_session.IsFinished && Time.time<deadline &&
                Time.realtimeSinceStartup<_preparationRealDeadline) yield return null;
            _idleSeconds+=Time.time-started;
            string result=IncomeState()+";elapsedGameSeconds="+F(Time.time-started)+
                ";elapsedRealSeconds="+F(Time.realtimeSinceStartup-realStarted)+
                ";elapsedFrames="+(Time.frameCount-frameStarted);
            AddFact(key+".after",result);
            Check(_session.Economy.Essence>=cost,mapId+" flower income cannot afford "+cost+
                " within 600 game seconds / 180 real seconds of preparation; "+result);
        }

        private static string IncomeState() =>
            "gameTime="+F(Time.time)+";realTime="+F(Time.realtimeSinceStartup)+
            ";frame="+Time.frameCount+";essence="+_session.Economy.Essence+
            ";timeScale="+F(Time.timeScale)+";finished="+_session.IsFinished+
            ";focused="+Application.isFocused+";runInBackground="+Application.runInBackground+
            ";editorPaused="+EditorApplication.isPaused+
            ";activeFlowers="+UnityEngine.Object.FindObjectsByType<EssenceFlowerPlant>(
                FindObjectsInactive.Exclude,FindObjectsSortMode.None).Count(plant=>plant.isActiveAndEnabled);

        private static IEnumerator BuildWithIncome(string mapId,string id,PlantKind kind,float deadline)
        {
            int cost=Balance.Cost(kind);
            IEnumerator wait=WaitForIncome(mapId,cost,deadline);
            while(wait.MoveNext()) yield return null;
            if(_session.Economy.Essence<cost) yield break;
            CloseTowerPanel();
            yield return null;
            PlantingSpot spot=FindSpot(id);
            Check(spot!=null,mapId+" planned spot missing: "+id);
            if(spot==null) yield break;
            int before=_session.Economy.Essence;
            Check(_session.Planting.TryHandleScreenPress(Camera.main.WorldToScreenPoint(spot.transform.position)),mapId+" build tap blocked: "+id);
            yield return null;
            Check(DispatchClick(FindButton("PlantOption_"+kind)),mapId+" build button missing: "+id);
            yield return null;
            yield return null;
            Check(spot.Occupied && spot.Tower!=null && spot.Tower.Kind==kind,mapId+" production build failed: "+id);
            Check(spot.Tower!=null && spot.Tower.TotalInvested==cost,mapId+" build investment mismatch: "+id);
            AddFact(mapId+".build."+id,kind+";cost="+cost+";essenceBefore="+before+";essenceAfter="+_session.Economy.Essence);
        }

        private static IEnumerator UpgradeWithIncome(string mapId,string id,float deadline)
        {
            PlantingSpot spot=FindSpot(id);
            Check(spot!=null && spot.Tower!=null,mapId+" missing upgrade tower: "+id);
            if(spot==null || spot.Tower==null) yield break;
            TowerInstance tower=spot.Tower;
            while(tower.Level<3)
            {
                int cost=tower.NextUpgradeCost;
                IEnumerator wait=WaitForIncome(mapId,cost,deadline);
                while(wait.MoveNext()) yield return null;
                if(_session.Economy.Essence<cost) yield break;
                CloseTowerPanel();
                yield return null;
                Check(_session.Planting.TryHandleScreenPress(Camera.main.WorldToScreenPoint(spot.transform.position)),mapId+" upgrade tap blocked: "+id);
                yield return null;
                int levelBefore=tower.Level,investedBefore=tower.TotalInvested;
                Check(DispatchClick(FindButton("Button_ULEPSZ")),mapId+" upgrade button missing: "+id);
                yield return null;
                Check(tower.Level==levelBefore+1 && tower.TotalInvested==investedBefore+cost,mapId+" upgrade/cost failed: "+id);
                if(tower.Level!=levelBefore+1) yield break;
            }
            CloseTowerPanel();
            yield return null;
        }

        private static void CloseTowerPanel()
        {
            Button close=FindButton("TowerPanelClose");
            if(close!=null) DispatchClick(close);
        }

        private static void ObserveSpawnedEnemies()
        {
            foreach(EnemyUnit enemy in _session.Enemies)
            {
                if(enemy==null) continue;
                int id=enemy.GetInstanceID();
                if(!_battle.SpawnIds.Add(id)) continue;
                if(enemy.IsBoss) _battle.BossSeen=true;
                enemy.Killed+=OnEnemyKilled;
            }
        }

        private static void OnEnemyKilled(EnemyUnit enemy)
        {
            if(enemy==null || _battle==null) return;
            int id=enemy.GetInstanceID();
            if(!_battle.KillIds.Add(id)) return;
            _battle.Kills++;
            if(enemy.IsBoss) _battle.BossKilled=true;
        }

        private static PlantingSpot FindSpot(string id)
        {
            return UnityEngine.Object.FindObjectsByType<PlantingSpot>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .FirstOrDefault(s => s != null && s.Id == id);
        }

        private static Button FindButton(string name)
        {
            return UnityEngine.Object.FindObjectsByType<Button>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .FirstOrDefault(b => b != null &&
                                     b.gameObject.name == name &&
                                     b.gameObject.activeInHierarchy);
        }

        private static bool DispatchClick(Button button)
        {
            if (button == null || !button.gameObject.activeInHierarchy || !button.interactable)
                return false;

            if (EventSystem.current == null)
                return false;

            PointerEventData data = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute(button.gameObject, data, ExecuteEvents.pointerClickHandler);
            return true;
        }

        private static void Check(bool condition, string message)
        {
            if (!condition)
                Errors.Add(message);
        }

        private static void CheckApprox(float actual, float expected, float tolerance, string label)
        {
            if (Mathf.Abs(actual - expected) > tolerance)
                Errors.Add(label + " expected=" + F(expected) + " actual=" + F(actual));
        }

        private static void AddFact(string key, object value)
        {
            Facts.Add(key + "=" + (value == null ? "null" : value.ToString()));
        }

        private static string F(float value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        private static void Complete(bool success)
        {
            (_flow as IDisposable)?.Dispose();
            _flow = null;
            Time.timeScale = SessionState.GetFloat(OriginalTimeScaleKey,1f);
            Application.runInBackground = SessionState.GetBool(OriginalBackgroundKey,false);
            AddFact("playModeEntered", SessionState.GetBool(PlayModeEnteredKey, false));
            WriteReport(success && Errors.Count == 0);

            SessionState.SetInt(ExitCodeKey, success && Errors.Count == 0 ? 0 : 1);
            SessionState.SetString(PhaseKey, "exit-editor");

            if (EditorApplication.isPlaying)
                EditorApplication.ExitPlaymode();
        }

        private static void FinishImmediately(bool success)
        {
            Time.timeScale = SessionState.GetFloat(OriginalTimeScaleKey,1f);
            Application.runInBackground = SessionState.GetBool(OriginalBackgroundKey,false);
            WriteReport(success);
            SessionState.SetInt(ExitCodeKey, success ? 0 : 1);
            SessionState.SetString(PhaseKey, "exit-editor");

            if (EditorApplication.isPlaying)
                EditorApplication.ExitPlaymode();
            else
            {
                SessionState.SetBool(ActiveKey, false);
                UnsubscribeLog();
                EditorApplication.Exit(success ? 0 : 1);
            }
        }

        private static void WriteReport(bool success)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "Artifacts/UnityExecutor");

            List<string> lines = new List<string>();
            lines.Add("DYRYGENT UNITY EXECUTOR FULL NATURAL CAMPAIGN");
            lines.Add("status=" + (success ? "PASS" : "FAIL"));
            lines.Add("timestampUtc=" + DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));
            lines.Add("unityVersion=" + Application.unityVersion);
            lines.AddRange(Facts);
            lines.Add("runtimeWarningCount=" + RuntimeWarnings.Count);
            for (int i = 0; i < RuntimeWarnings.Count; i++)
                lines.Add("runtimeWarning." + (i + 1) + "=" + OneLine(RuntimeWarnings[i]));
            lines.Add("validationErrorCount=" + Errors.Count);
            for (int i = 0; i < Errors.Count; i++)
                lines.Add("validationError." + (i + 1) + "=" + OneLine(Errors[i]));

            File.WriteAllLines(ReportPath, lines.ToArray());
            Debug.Log(string.Join("\n", lines.ToArray()));
        }

        private static bool TimedOut()
        {
            DateTime started;
            if (!DateTime.TryParse(
                    SessionState.GetString(StartedKey, string.Empty),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out started))
                return false;

            return (DateTime.UtcNow - started).TotalSeconds > TimeoutSeconds;
        }

        private static string OneLine(string value)
        {
            return (value ?? string.Empty).Replace("\r", " ").Replace("\n", " ");
        }

        private static void SubscribeLog()
        {
            if (_subscribed) return;
            Application.logMessageReceived += OnLog;
            _subscribed = true;
        }

        private static void UnsubscribeLog()
        {
            if (!_subscribed) return;
            Application.logMessageReceived -= OnLog;
            _subscribed = false;
        }

        private static void OnLog(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Warning)
                RuntimeWarnings.Add(condition);
            else if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                Errors.Add("console=" + condition);
        }
    }
}
#endif
