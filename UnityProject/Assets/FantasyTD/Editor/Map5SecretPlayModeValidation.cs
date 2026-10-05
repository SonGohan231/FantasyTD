#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FantasyTD.MapA;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FantasyTD.CI
{
    internal static class Map5SecretPlayModeValidation
    {
        // Tests a fresh real scene. No Economy.Add, enemy damage call or artificial reward.
        internal static IEnumerator Run(Action<bool,string> check, Action<string> fact)
        {
            bool hadKey=PlayerPrefs.HasKey(Map5SecretController.MutationUnlockKey);
            int oldValue=PlayerPrefs.GetInt(Map5SecretController.MutationUnlockKey,0);
            float oldTimeScale=Time.timeScale;
            try
            {
                PlayerPrefs.DeleteKey(Map5SecretController.MutationUnlockKey);
                PlayerPrefs.Save();
                Time.timeScale=1f;
                SceneManager.LoadScene(Map5SourceDefinition.Definition.SceneName);
                for (int i=0;i<30;i++) yield return null;
                GameSession session=UnityEngine.Object.FindFirstObjectByType<GameSession>();
                Map5SecretController secret=UnityEngine.Object.FindFirstObjectByType<Map5SecretController>();
                Require(session!=null && secret!=null,check,"MAP5 scene must contain session and secret controller.");
                Require(secret.State==Map5SecretController.SecretState.Hidden,check,"Fresh lab state is hidden.");
                Require(!Map5SecretController.MutationTowerUnlocked,check,"Locked save profile is actually reset.");

                Click("LabSourceButton",check);
                yield return null;
                Require(secret.State==Map5SecretController.SecretState.Discovered,check,"Actual lab button discovers the lab.");
                Require(!Map5SecretController.MutationTowerUnlocked,check,"Discovery alone must not unlock mutation tower.");
                Image lab=GameObject.Find("LabSourceButton").GetComponent<Image>();
                Require(lab.sprite!=null && lab.sprite.name=="Approved_MAP5_LAB_OPEN_FINAL",check,"Lab uses approved open-video final frame.");
                fact("map5Secret.discovery=real-source-button-open-state");

                foreach (string id in new[]{"F1","F2","F3"})
                {
                    IEnumerator build=Build(session,id,PlantKind.EssenceFlower,check);
                    while (build.MoveNext()) yield return null;
                }
                Require(session.Economy.Essence==0,check,"Three real flower purchases spend initial 300 Essence.");
                Time.timeScale=8f;
                fact("map5Secret.simulationSpeed=8x;balanceValuesUnchanged");
                float incomeStarted=Time.time;
                float incomeRealStarted=Time.realtimeSinceStartup;
                int incomeFrameStarted=Time.frameCount;
                int incomeEssenceStarted=session.Economy.Essence;
                fact("map5Secret.incomeBefore="+IncomeState(session));
                float incomeDeadline=incomeStarted+650f;
                float incomeRealDeadline=incomeRealStarted+180f;
                while (session.Economy.Essence<1300 && Time.time<incomeDeadline &&
                    Time.realtimeSinceStartup<incomeRealDeadline && !session.IsFinished) yield return null;
                string incomeResult=IncomeState(session)+
                    ";elapsedGameSeconds="+(Time.time-incomeStarted).ToString("0.###",CultureInfo.InvariantCulture)+
                    ";elapsedRealSeconds="+(Time.realtimeSinceStartup-incomeRealStarted).ToString("0.###",CultureInfo.InvariantCulture)+
                    ";elapsedFrames="+(Time.frameCount-incomeFrameStarted)+
                    ";earnedEssence="+(session.Economy.Essence-incomeEssenceStarted);
                fact("map5Secret.incomeAfter="+incomeResult);
                Require(session.Economy.Essence>=1300,check,
                    "Real flower production must earn 1300 preparation funds within 650 game seconds and 180 real seconds; "+incomeResult);
                fact("map5Secret.earnedEssenceBeforeCombat="+session.Economy.Essence);

                foreach (string id in new[]{"F6","F11","F12"})
                {
                    IEnumerator build=Build(session,id,PlantKind.SeedShooter,check);
                    while (build.MoveNext()) yield return null;
                    IEnumerator upgrades=UpgradeViaUi(session,id,check);
                    while (upgrades.MoveNext()) yield return null;
                }
                IEnumerator frost=Build(session,"F10",PlantKind.FrostBud,check);
                while (frost.MoveNext()) yield return null;
                IEnumerator frostUpgrades=UpgradeViaUi(session,"F10",check);
                while (frostUpgrades.MoveNext()) yield return null;

                Time.timeScale=4f;
                fact("map5Secret.combatSimulationSpeed=4x;balanceValuesUnchanged");
                Click("LabSourceButton",check);
                yield return null;
                EnemyUnit mother=secret.MotherMycelium;
                Require(secret.State==Map5SecretController.SecretState.Fighting && mother!=null,check,"Second lab click spawns real Mother Mycelium.");
                Require(session.Enemies.Contains(mother),check,"Secret miniboss is registered for wave completion.");
                Require(mother.MaxHp==2200f && !Map5SecretController.MutationTowerUnlocked,check,"Real boss profile exists; awakening grants no unlock.");
                bool killed=false;
                mother.Killed+=enemy=>killed=true;
                float initialHp=mother.CurrentHp, deadline=Time.time+125f;
                bool damaged=false;
                while (secret.State==Map5SecretController.SecretState.Fighting && Time.time<deadline && !session.IsFinished)
                {
                    if (mother!=null && mother.CurrentHp<initialHp) damaged=true;
                    yield return null;
                }
                fact("map5Secret.actualProjectileDamage="+damaged);
                fact("map5Secret.result="+secret.State);
                Require(damaged,check,"Production tower projectiles must damage the miniboss.");
                Require(killed && secret.State==Map5SecretController.SecretState.Defeated,check,"Natural tower combat must kill Mother Mycelium, not leak.");
                Require(Map5SecretController.MutationTowerUnlocked,check,"Only the real kill must persist mutation unlock.");
                Require(session.Base.Hp==Balance.BaseHp,check,"Miniboss kill must happen before any base leak.");

                Time.timeScale=1f;
                SceneManager.LoadScene(Map5SourceDefinition.Definition.SceneName);
                for(int i=0;i<30;i++) yield return null;
                session=UnityEngine.Object.FindFirstObjectByType<GameSession>();
                Require(Map5SecretController.MutationTowerUnlocked,check,"Mutation unlock survives actual scene restart.");
                IEnumerator mutation=Build(session,"F4",PlantKind.MutationTower,check);
                while (mutation.MoveNext()) yield return null;
                PlantingSpot spot=FindSpot("F4");
                Require(spot.Tower.Kind==PlantKind.MutationTower &&
                    spot.Tower.GetComponent<SeedShooterPlant>()!=null,check,"Unlocked source plant is a real purchased production tower.");
                Require(session.Economy.Essence==200 && spot.Tower.TotalInvested==100,check,"Mutation purchase uses explicit existing 100 Essence cost.");
                fact("map5Secret.reward=earned-natural-kill-persistent-purchasable-source-tower");
            }
            finally
            {
                Time.timeScale=oldTimeScale;
                if (hadKey) PlayerPrefs.SetInt(Map5SecretController.MutationUnlockKey,oldValue);
                else PlayerPrefs.DeleteKey(Map5SecretController.MutationUnlockKey);
                PlayerPrefs.Save();
            }
        }

        private static string IncomeState(GameSession session) =>
            "gameTime="+Time.time.ToString("0.###",CultureInfo.InvariantCulture)+
            ";realTime="+Time.realtimeSinceStartup.ToString("0.###",CultureInfo.InvariantCulture)+
            ";frame="+Time.frameCount+
            ";essence="+session.Economy.Essence+
            ";timeScale="+Time.timeScale.ToString("0.###",CultureInfo.InvariantCulture)+
            ";finished="+session.IsFinished+
            ";focused="+Application.isFocused+
            ";runInBackground="+Application.runInBackground+
            ";editorPaused="+UnityEditor.EditorApplication.isPaused+
            ";activeFlowers="+UnityEngine.Object.FindObjectsByType<EssenceFlowerPlant>(
                FindObjectsInactive.Exclude,FindObjectsSortMode.None).Count(plant=>plant.isActiveAndEnabled);

        private static IEnumerator Build(GameSession session,string id,PlantKind kind,Action<bool,string> check)
        {
            PlantingSpot spot=FindSpot(id);
            Require(spot!=null && !spot.Occupied,check,"Free authored build spot required: "+id);
            session.Planting.TryHandleScreenPress(Camera.main.WorldToScreenPoint(spot.transform.position));
            yield return null;
            Require(session.Planting.SelectedSpot==spot,check,"Production input must select "+id);
            Button option=FindButton("PlantOption_"+kind);
            Require(option!=null && option.interactable,check,"Actual affordable radial option required: "+kind);
            if (!Map5SecretController.MutationTowerUnlocked)
                Require(FindButton("PlantOption_MutationTower")==null,check,"Locked radial must omit mutation option.");
            Click(option,check);
            yield return null;
            yield return null;
            Require(spot.Occupied && spot.Tower!=null && spot.Tower.Kind==kind,check,"Radial click must create actual tower at "+id);
        }

        private static IEnumerator UpgradeViaUi(GameSession session,string id,Action<bool,string> check)
        {
            PlantingSpot spot=FindSpot(id);
            session.Planting.TryHandleScreenPress(Camera.main.WorldToScreenPoint(spot.transform.position));
            yield return null;
            Require(session.Planting.SelectedTower==spot.Tower,check,"Occupied spot must open production tower panel.");
            for(int i=0;i<2;i++)
            {
                Button upgrade=UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Exclude,FindObjectsSortMode.None)
                    .FirstOrDefault(b=>b.GetComponentInChildren<Text>()!=null &&
                        b.GetComponentInChildren<Text>().text.StartsWith("ULEPSZ",StringComparison.Ordinal));
                Require(upgrade!=null && upgrade.interactable,check,"Affordable actual upgrade button required.");
                Click(upgrade,check);
                yield return null;
            }
            Require(spot.Tower.Level==3,check,"Two real upgrade clicks must create level3 at "+id);
            Click("TowerPanelClose",check);
            yield return null;
            yield return null;
        }

        private static PlantingSpot FindSpot(string id) =>
            UnityEngine.Object.FindObjectsByType<PlantingSpot>(FindObjectsInactive.Exclude,FindObjectsSortMode.None)
                .FirstOrDefault(s=>s.gameObject.name=="PlantingSpot_"+id);
        private static Button FindButton(string name) =>
            UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Exclude,FindObjectsSortMode.None)
                .FirstOrDefault(b=>b.gameObject.name==name);
        private static void Click(string name,Action<bool,string> check) => Click(FindButton(name),check);
        private static void Click(Button button,Action<bool,string> check)
        {
            Require(button!=null && button.interactable && EventSystem.current!=null,check,"Actual enabled UI target required.");
            Canvas canvas=button.GetComponentInParent<Canvas>();
            Camera camera=canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:Camera.main;
            Vector2 screen=RectTransformUtility.WorldToScreenPoint(camera,button.transform.position);
            PointerEventData data=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,position=screen};
            List<RaycastResult> hits=new List<RaycastResult>();
            EventSystem.current.RaycastAll(data,hits);
            Require(hits.Count>0 && ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject)==button.gameObject,
                check,"UI raycast must reach intended production button: "+button.gameObject.name);
            ExecuteEvents.Execute(button.gameObject,data,ExecuteEvents.pointerClickHandler);
        }

        private static void Require(bool result,Action<bool,string> check,string message)
        {
            check(result,message);
            if (!result) throw new InvalidOperationException(message);
        }
    }
}
#endif
