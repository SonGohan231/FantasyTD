#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using FantasyTD.MapA;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FantasyTD.CI
{
    internal static class SourceCampaignPlayModeValidation
    {
        private static readonly FieldInfo EnemyRoute=typeof(EnemyUnit).GetField("_route",BindingFlags.Instance|BindingFlags.NonPublic);

        internal static IEnumerator Run(Action<bool,string> check,Action<string> fact)
        {
            float originalScale=Time.timeScale;
            try
            {
                Time.timeScale=4f;
                foreach(string id in new[]{"MAP3","MAP4","MAP5","MAP6","MAP7","MAP8"})
                {
                    IEnumerator scenario=RunMap(id,check,fact);
                    while(scenario.MoveNext()) yield return null;
                }
            }
            finally {Time.timeScale=originalScale;}
        }

        private static IEnumerator RunMap(string id,Action<bool,string> check,Action<string> fact)
        {
            CampaignMapDefinition definition=CampaignCatalog.Get(id);
            check(definition!=null,id+" campaign definition missing.");
            if(definition==null) yield break;
            SceneManager.LoadScene(definition.SceneName);
            GameSession session=null;
            for(int frame=0;frame<120;frame++)
            {
                yield return null;
                session=UnityEngine.Object.FindFirstObjectByType<GameSession>();
                if(session!=null && session.MapDefinition!=null && session.MapDefinition.Id==id && session.Hud!=null && Camera.main!=null) break;
            }
            check(session!=null && session.MapDefinition!=null && session.MapDefinition.Id==id,id+" fresh session missing.");
            if(session==null || session.MapDefinition==null || session.MapDefinition.Id!=id) yield break;
            check(session.Economy.Essence==Balance.StartEssence && session.Base.Hp==Balance.BaseHp && session.Waves.CurrentWave==0 && !session.IsFinished,
                id+" scene retained previous game state.");
            Camera camera=Camera.main;
            float depth=-camera.transform.position.z;
            Vector3 cameraMin=camera.ViewportToWorldPoint(new Vector3(0f,0f,depth));
            Vector3 cameraMax=camera.ViewportToWorldPoint(new Vector3(1f,1f,depth));
            check(camera.orthographic && cameraMin.x<=0f && cameraMin.y<=0f && cameraMax.x>=definition.Width && cameraMax.y>=definition.Height,
                id+" camera does not display complete logical "+definition.Width+"x"+definition.Height+" map.");
            fact(id+".cameraBounds="+cameraMin.x+","+cameraMin.y+","+cameraMax.x+","+cameraMax.y);
            ValidateAuthoredRenderers(id,definition,check,fact);
            PlantingSpot[] spots=UnityEngine.Object.FindObjectsByType<PlantingSpot>(FindObjectsSortMode.None);
            check(spots.Length==definition.PlantingSpots.Length,id+" runtime planting count differs from source definition.");
            List<PlantingSpot> reachable=new List<PlantingSpot>();
            foreach(PlantingSpot spot in spots)
            {
                bool accepted=session.Planting.TryHandleScreenPress(camera.WorldToScreenPoint(spot.transform.position));
                yield return null;
                bool selected=accepted && session.Planting.SelectedSpot==spot && session.Hud.RadialRoot!=null;
                check(selected,id+" authored planting pad blocked/unselectable: "+spot.Id);
                if(selected) reachable.Add(spot);
                Button close=FindButton("RadialCloseButton");
                if(selected) check(Click(close),id+" radial close click failed: "+spot.Id);
                else if(close!=null) Click(close);
                yield return null;
                yield return null;
            }
            check(reachable.Count>0,id+" no reachable production build pads.");
            if(reachable.Count==0) yield break;
            PlantingSpot target=reachable.OrderBy(p=>EntryCoverageScore(definition,p.transform.position)).First();
            check(EntryCoverageScore(definition,target.transform.position)<3.2f,id+" no reachable pad is in starter-lane SeedShooter range.");
            int essenceBefore=session.Economy.Essence;
            check(session.Planting.TryHandleScreenPress(camera.WorldToScreenPoint(target.transform.position)),id+" strategic seed pad input failed.");
            yield return null;
            check(Click(FindButton("PlantOption_SeedShooter")),id+" production SeedShooter button failed.");
            yield return null;
            yield return null;
            check(target.Occupied && target.Tower!=null && target.Tower.Kind==PlantKind.SeedShooter,id+" production tower missing after build.");
            check(session.Economy.Essence==essenceBefore-Balance.Cost(PlantKind.SeedShooter),id+" build did not spend actual starter cost.");
            fact(id+".seedPad="+target.Id);
            Directory.CreateDirectory("Artifacts/UnityExecutor");
            ScreenCapture.CaptureScreenshot("Artifacts/UnityExecutor/source-campaign-"+id+"-prepared.png");
            for(int frame=0;frame<8;frame++) yield return null;
            check(Click(FindButton("Button_START")),id+" normal first-wave start button failed.");
            yield return null;
            check(session.Waves.CurrentWave==1 && session.Waves.Running,id+" normal first wave did not start.");
            HashSet<int> spawnIds=new HashSet<int>();
            HashSet<string> laneSignatures=new HashSet<string>();
            Dictionary<EnemyUnit,Vector2[]> routeReferences=new Dictionary<EnemyUnit,Vector2[]>();
            Dictionary<EnemyUnit,Vector2[]> expectedSnapshots=new Dictionary<EnemyUnit,Vector2[]>();
            bool productionDamage=false,observedMovement=false;
            float deadline=Time.realtimeSinceStartup+35f;
            while(session.Waves.Running && !session.IsFinished && Time.realtimeSinceStartup<deadline)
            {
                foreach(EnemyUnit enemy in session.Enemies)
                {
                    if(enemy==null) continue;
                    if(spawnIds.Add(enemy.GetInstanceID()))
                    {
                        int ordinal=spawnIds.Count-1;
                        Vector2[] expected=definition.GetSpawnRoute(ordinal);
                        Vector2[] actual=EnemyRoute!=null ? EnemyRoute.GetValue(enemy) as Vector2[] : null;
                        check(actual!=null,id+" read-only route inspection failed.");
                        if(actual!=null)
                        {
                            check(RoutesMatch(actual,expected),id+" production spawn "+ordinal+" chose wrong immutable branch.");
                            routeReferences[enemy]=actual;
                            // Independent values detect in-place changes without modifying the enemy.
                            expectedSnapshots[enemy]=(Vector2[])expected.Clone();
                            string signature=RouteSignature(actual);
                            laneSignatures.Add(signature);
                            fact(id+".spawn"+ordinal+".route="+signature);
                            check(Vector2.Distance(actual[actual.Length-1],definition.GetSpawnRoute(0).Last())<0.01f,
                                id+" branch does not reach shared base.");
                        }
                    }
                    if(routeReferences.TryGetValue(enemy,out Vector2[] initialReference))
                    {
                        Vector2[] current=EnemyRoute.GetValue(enemy) as Vector2[];
                        Vector2[] expected=expectedSnapshots[enemy];
                        check(ReferenceEquals(current,initialReference) && RoutesMatch(current,expected),id+" existing enemy switched/mutated branch.");
                        check(DistanceToRoute(enemy.transform.position,expected)<0.05f,id+" enemy left source-authoritative road centerline.");
                    }
                    productionDamage|=enemy.CurrentHp<enemy.MaxHp;
                    observedMovement|=enemy.ProgressDistance>0.5f;
                }
                yield return null;
            }
            for(int frame=0;frame<8;frame++) yield return null;
            fact(id+".naturalFirstWaveSpawns="+spawnIds.Count);
            fact(id+".distinctSpawnRoutes="+laneSignatures.Count);
            fact(id+".productionSeedDamage="+productionDamage);
            fact(id+".naturalMovement="+observedMovement);
            fact(id+".baseHpAfterNaturalWave="+session.Base.Hp);
            fact(id+".nextWaveReady="+session.Waves.ReadyForNextWave);
            fact(id+".campaignPlayableFlag="+definition.IsValidatedPlayable);
            int expectedLanes=id=="MAP7" ? 5 : id=="MAP3" ? 2 : (id=="MAP4" || id=="MAP8" ? 4 : 3);
            check(spawnIds.Count==6,id+" natural first wave did not observe exactly six production enemies.");
            check(laneSignatures.Count==expectedLanes,id+" first-wave spawns did not exercise every authored alternate route.");
            check(productionDamage,id+" natural enemy received no production SeedShooter damage.");
            check(observedMovement,id+" natural enemy movement not observed.");
            check(session.Waves.CurrentWave==1 && session.Waves.ReadyForNextWave && session.Enemies.Count==0 && !session.IsFinished,
                id+" natural first wave did not settle into next-wave-ready state.");
            ScreenCapture.CaptureScreenshot("Artifacts/UnityExecutor/source-campaign-"+id+"-after-first-wave.png");
            for(int frame=0;frame<8;frame++) yield return null;
        }

        private static void ValidateAuthoredRenderers(string id,CampaignMapDefinition definition,Action<bool,string> check,Action<string> fact)
        {
            Texture2D terrain=Resources.Load<Texture2D>(definition.BackgroundResource);
            check(terrain!=null,id+" required approved terrain not loadable: "+definition.BackgroundResource);
            SpriteRenderer[] ground=UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None)
                .Where(r=>r.name.StartsWith(id,StringComparison.Ordinal) && r.sortingOrder<=-99).ToArray();
            check(ground.Length>0,id+" authored terrain has no visible sprite renderers.");
            if(ground.Length>0)
            {
                Bounds coverage=ground[0].bounds;
                foreach(SpriteRenderer renderer in ground)
                {
                    check(renderer.enabled && renderer.sprite!=null && renderer.sprite.texture==terrain,id+" field did not preserve approved source texture.");
                    coverage.Encapsulate(renderer.bounds);
                }
                check(coverage.min.x<=0.01f && coverage.min.y<=0.01f && coverage.max.x>=definition.Width-0.01f && coverage.max.y>=definition.Height-0.01f,
                    id+" authored field does not cover logical bounds.");
            }
            string source=id=="MAP4" ? "Higgsfield/Maps/MAP4_TERRAIN"
                : id=="MAP5" ? "Higgsfield/Maps/MAP2_ROAD_KIT"
                : id=="MAP7" ? Map7SourceDefinition.Resource
                : id=="MAP8" ? Map8SourceDefinition.Resource : "Higgsfield/Maps/MAP1_ROAD_SOURCE";
            Texture2D road=Resources.Load<Texture2D>(source);
            MeshRenderer[] paths=UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None)
                .Where(r=>r.name.StartsWith(id,StringComparison.Ordinal) && r.name.Contains("_SourceRoad_")).ToArray();
            check(paths.Length>0,id+" authored road has no textured meshes.");
            foreach(MeshRenderer renderer in paths)
            {
                Mesh mesh=renderer.GetComponent<MeshFilter>()?.sharedMesh;
                check(renderer.enabled && renderer.sharedMaterial!=null && renderer.sharedMaterial.mainTexture==road && road!=null,
                    id+" road does not render approved source texture.");
                check(mesh!=null && mesh.vertexCount>=4 && mesh.triangles.Length>=6 && mesh.uv.Length==mesh.vertexCount,id+" road mesh/UV incomplete.");
                if(mesh==null) continue;
                Rect crop=id=="MAP4" ? new Rect(600f/1344f,(768f-379f)/768f,110f/1344f,42f/768f)
                    : id=="MAP5" ? new Rect(70f/1344f,(768f-127f)/768f,400f/1344f,61f/768f)
                    : id=="MAP7" ? new Rect(250f/2048f,(1152f-1040f)/1152f,70f/2048f,40f/1152f)
                    : id=="MAP8" ? new Rect(720f/2048f,(1152f-503f)/1152f,50f/2048f,22f/1152f)
                    : new Rect(990f/2048f,(1152f-740f)/1152f,160f/2048f,50f/1152f);
                foreach(Vector2 uv in mesh.uv)
                    check(uv.x>=crop.xMin-0.0001f && uv.x<=crop.xMax+0.0001f && uv.y>=crop.yMin-0.0001f && uv.y<=crop.yMax+0.0001f,
                        id+" road UV uses pixels outside approved authored crop.");
            }
            fact(id+".sourceRoadMeshCount="+paths.Length);
            fact(id+".sourceGroundRendererCount="+ground.Length);
        }

        private static float EntryCoverageScore(CampaignMapDefinition definition,Vector2 point)
        {
            float best=float.PositiveInfinity;
            for(int ordinal=0;ordinal<4;ordinal++)
            {
                Vector2[] route=definition.GetSpawnRoute(ordinal);
                float walked=0f;
                for(int i=0;i<route.Length-1 && walked<6f;i++)
                {
                    Vector2 a=route[i],b=route[i+1];
                    float length=Vector2.Distance(a,b);
                    Vector2 end=Vector2.Lerp(a,b,Mathf.Min(1f,(6f-walked)/Mathf.Max(0.001f,length)));
                    best=Mathf.Min(best,DistanceToSegment(point,a,end)+walked*0.02f);
                    walked+=length;
                }
            }
            return best;
        }
        private static float DistanceToRoute(Vector2 point,Vector2[] route)
        {
            float result=float.PositiveInfinity;
            for(int i=0;i<route.Length-1;i++) result=Mathf.Min(result,DistanceToSegment(point,route[i],route[i+1]));
            return result;
        }
        private static float DistanceToSegment(Vector2 point,Vector2 a,Vector2 b)
        {
            Vector2 delta=b-a;
            float t=delta.sqrMagnitude<=0.00001f ? 0f : Mathf.Clamp01(Vector2.Dot(point-a,delta)/delta.sqrMagnitude);
            return Vector2.Distance(point,a+delta*t);
        }
        private static bool RoutesMatch(Vector2[] actual,Vector2[] expected)
        {
            if(actual==null || expected==null || actual.Length!=expected.Length) return false;
            for(int i=0;i<actual.Length;i++) if(Vector2.Distance(actual[i],expected[i])>0.001f) return false;
            return true;
        }
        private static string RouteSignature(Vector2[] route) => string.Join(";",route.Select(p=>p.x.ToString("0.###",CultureInfo.InvariantCulture)+","+p.y.ToString("0.###",CultureInfo.InvariantCulture)));
        private static Button FindButton(string name) => UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).FirstOrDefault(b=>b.name==name && b.gameObject.activeInHierarchy);
        private static bool Click(Button button)
        {
            if(button==null || !button.interactable || EventSystem.current==null) return false;
            Canvas canvas=button.GetComponentInParent<Canvas>();
            if(canvas==null) return false;
            Camera camera=canvas.renderMode==RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            Vector2 screen=RectTransformUtility.WorldToScreenPoint(camera,button.transform.position);
            PointerEventData pointer=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,position=screen};
            List<RaycastResult> hits=new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer,hits);
            if(hits.Count==0 || ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject)!=button.gameObject) return false;
            return ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerClickHandler);
        }
    }
}
#endif

