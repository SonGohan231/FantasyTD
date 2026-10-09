#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using FantasyTD.MapA;
using FantasyTD.MapA.Editor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace FantasyTD.CI
{
    internal static class MapAIntegrityValidator
    {
        internal sealed class Result
        {
            private readonly List<string> _facts = new List<string>();
            private readonly List<string> _errors = new List<string>();

            internal bool Success => _errors.Count == 0;
            internal IReadOnlyList<string> Errors => _errors;

            internal void Fact(string key, object value)
            {
                _facts.Add(key + "=" + Convert.ToString(value, CultureInfo.InvariantCulture));
            }

            internal void Check(string key, bool passed, string failure)
            {
                Fact(key, passed);
                if (!passed)
                    _errors.Add(failure);
            }

            internal string ToReportText()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("DYRYGENT UNITY EXECUTOR MAP-A INTEGRITY");
                sb.AppendLine("timestampUtc=" + DateTime.UtcNow.ToString("O"));
                sb.AppendLine("unityVersion=" + Application.unityVersion);
                sb.AppendLine("status=" + (Success ? "PASS" : "FAIL"));
                foreach (string fact in _facts)
                    sb.AppendLine(fact);
                sb.AppendLine("errorCount=" + _errors.Count);
                for (int i = 0; i < _errors.Count; i++)
                    sb.AppendLine("error." + (i + 1) + "=" + _errors[i].Replace("\n", " ").Replace("\r", " "));
                return sb.ToString();
            }
        }

        internal static Result Run()
        {
            Result result = new Result();
            List<string> consoleErrors = new List<string>();
            bool editorValidationPassLogFound = false;

            Application.LogCallback handler = delegate(string condition, string stackTrace, LogType type)
            {
                if (condition.StartsWith("MAP-A Android v2 validation PASS.", StringComparison.Ordinal))
                    editorValidationPassLogFound = true;

                if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                    consoleErrors.Add(condition);
            };

            Application.logMessageReceived += handler;
            try
            {
                MapAEditorSetup.OpenMap();
                Scene scene = SceneManager.GetActiveScene();
                result.Check("sceneOpened", scene.IsValid() && scene.isLoaded && scene.path == MapAEditorSetup.ScenePath,
                    "MAP-A scene did not open at the expected path.");
                result.Fact("scenePath", scene.path);

                MapAEditorSetup.Validate();
                result.Check("editorValidationCommandPass", editorValidationPassLogFound,
                    "Tools/Fantasy TD/Validate MAP-A did not emit its PASS result.");

                MapABootstrap bootstrap = UnityEngine.Object.FindFirstObjectByType<MapABootstrap>(FindObjectsInactive.Include);
                result.Check("bootstrapFound", bootstrap != null, "MAP-A bootstrap was not found in the opened scene.");
                if (bootstrap == null)
                    return Finish(result, consoleErrors);

                InvokeBootstrapGeometry(bootstrap);

                ValidateLogicalBounds(result);
                ValidateRoad(result);
                ValidateRouteEnds(result);
                ValidatePlantingSpots(result);
                ValidateGridAbsence(result);
                ValidatePlayableTilemap(result);
                ValidateCameraFraming(result);

                result.Fact("runtimeGeometrySource", "MapABootstrap.BuildCamera+BuildMap+BuildPlantingSpots");
                result.Fact("playModeEntered", false);
                result.Fact("androidBuildRun", false);
            }
            catch (Exception ex)
            {
                Exception root = ex is TargetInvocationException && ex.InnerException != null ? ex.InnerException : ex;
                result.Check("validatorExceptionFree", false, root.GetType().Name + ": " + root.Message);
            }
            finally
            {
                Application.logMessageReceived -= handler;
            }

            return Finish(result, consoleErrors);
        }

        private static Result Finish(Result result, List<string> consoleErrors)
        {
            result.Fact("validationConsoleErrorCount", consoleErrors.Count);
            for (int i = 0; i < consoleErrors.Count; i++)
                result.Fact("consoleError." + (i + 1), consoleErrors[i].Replace("\n", " ").Replace("\r", " "));
            result.Check("noValidationConsoleErrors", consoleErrors.Count == 0,
                "Unity logged one or more errors/exceptions/asserts during MAP-A integrity validation.");
            return result;
        }

        private static void InvokeBootstrapGeometry(MapABootstrap bootstrap)
        {
            Type type = typeof(MapABootstrap);
            Invoke(type.GetMethod("BuildCamera", BindingFlags.NonPublic | BindingFlags.Static), null);
            Invoke(type.GetMethod("BuildMap", BindingFlags.NonPublic | BindingFlags.Static), null);
            Invoke(type.GetMethod("BuildPlantingSpots", BindingFlags.NonPublic | BindingFlags.Instance), bootstrap);

            Camera camera = Camera.main;
            if (camera != null)
            {
                MobileCameraFitter fitter = camera.GetComponent<MobileCameraFitter>();
                if (fitter != null)
                    Invoke(typeof(MobileCameraFitter).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance), fitter);
            }
        }

        private static void Invoke(MethodInfo method, object target)
        {
            if (method == null)
                throw new MissingMethodException("Required MAP-A runtime geometry method was not found.");
            method.Invoke(target, null);
        }

        private static void ValidateLogicalBounds(Result result)
        {
            bool size = Mathf.Approximately(MapAData.Width, 24f) && Mathf.Approximately(MapAData.Height, 14f);
            result.Check("logicalMapSize24x14", size, "MAP-A logical size is not 24x14 RU.");
            result.Fact("mapWidthRU", MapAData.Width);
            result.Fact("mapHeightRU", MapAData.Height);
            result.Fact("pathLengthRU", MapAData.TotalPathLength.ToString("0.000", CultureInfo.InvariantCulture));

            bool waypointsInside = MapAData.Waypoints.All(p => InsideBounds(p));
            bool spotsInside = MapAData.PlantingSpots.All(s => InsideBounds(s.Position));
            result.Check("allWaypointsInsideLogicalBounds", waypointsInside, "One or more route waypoints are outside MAP-A logical bounds.");
            result.Check("allPlantingDefinitionsInsideLogicalBounds", spotsInside, "One or more planting spot definitions are outside MAP-A logical bounds.");

            GameObject grass = GameObject.Find("Grass");
            SpriteRenderer renderer = grass != null ? grass.GetComponent<SpriteRenderer>() : null;
            bool grassBounds = renderer != null &&
                Approximately(renderer.bounds.min.x, 0f, 0.02f) &&
                Approximately(renderer.bounds.max.x, 24f, 0.02f) &&
                Approximately(renderer.bounds.min.y, 0f, 0.02f) &&
                Approximately(renderer.bounds.max.y, 14f, 0.02f);
            result.Check("grassRendererMatchesLogicalBounds", grassBounds, "Grass renderer does not cover the expected 24x14 RU logical map.");
        }

        private static void ValidateRoad(Result result)
        {
            string[] names = { "RoadShadow", "Road", "RoadDust" };
            int validRenderers = 0;
            bool primaryMatches = false;

            foreach (string name in names)
            {
                GameObject go = GameObject.Find(name);
                LineRenderer lr = go != null ? go.GetComponent<LineRenderer>() : null;
                bool visual = lr != null && lr.enabled && lr.sharedMaterial != null && lr.positionCount == MapAData.Waypoints.Length &&
                              lr.startWidth > 0f && lr.endWidth > 0f;
                if (visual) validRenderers++;

                if (name == "Road" && lr != null && lr.positionCount == MapAData.Waypoints.Length)
                {
                    primaryMatches = true;
                    for (int i = 0; i < MapAData.Waypoints.Length; i++)
                    {
                        Vector3 actual = lr.GetPosition(i);
                        Vector2 expected = MapAData.Waypoints[i];
                        if (Vector2.Distance(new Vector2(actual.x, actual.y), expected) > 0.001f)
                        {
                            primaryMatches = false;
                            break;
                        }
                    }
                }
            }

            result.Fact("roadVisualRendererCount", validRenderers);
            result.Check("roadVisualRenderersPresent", validRenderers == 3, "Expected three enabled/material-backed road LineRenderers.");
            result.Check("roadGeometryMatchesWaypoints", primaryMatches, "Primary road LineRenderer does not match MAP-A waypoints.");
        }

        private static void ValidateRouteEnds(Result result)
        {
            Vector2 first = MapAData.Waypoints[0];
            Vector2 last = MapAData.Waypoints[MapAData.Waypoints.Length - 1];

            GameObject portal = GameObject.Find("PortalOuter");
            GameObject home = GameObject.Find("HomeTrunk");
            SpriteRenderer portalRenderer = portal != null ? portal.GetComponent<SpriteRenderer>() : null;
            SpriteRenderer homeRenderer = home != null ? home.GetComponent<SpriteRenderer>() : null;

            bool portalVisual = portalRenderer != null && portalRenderer.enabled && portalRenderer.sprite != null;
            bool homeVisual = homeRenderer != null && homeRenderer.enabled && homeRenderer.sprite != null;
            bool portalAtStart = portal != null && Vector2.Distance(portal.transform.position, first) <= 0.001f;
            Vector2 homeAnchor = home != null ? (Vector2)home.transform.position + new Vector2(0f, 0.18f) : Vector2.positiveInfinity;
            bool homeAtEnd = home != null && Vector2.Distance(homeAnchor, last) <= 0.001f;

            result.Check("spawnPortalVisualPresent", portalVisual, "Spawn portal visual is missing or not renderable.");
            result.Check("spawnAtRouteStart", portalAtStart, "Spawn portal is not anchored at the first route waypoint.");
            result.Check("baseTreeVisualPresent", homeVisual, "Home/base tree visual is missing or not renderable.");
            result.Check("baseAtRouteEnd", homeAtEnd, "Home/base tree is not anchored at the final route waypoint.");
            result.Check("routeEndsAreOpposite", Vector2.Distance(first, last) > MapAData.Width * 0.75f,
                "Route endpoints do not span opposite sides of the logical map.");
            result.Fact("routeStart", Format(first));
            result.Fact("routeEnd", Format(last));
        }

        private static void ValidatePlantingSpots(Result result)
        {
            PlantingSpot[] spots = UnityEngine.Object.FindObjectsByType<PlantingSpot>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            result.Fact("runtimePlantingSpotCount", spots.Length);
            result.Check("exactly12RuntimePlantingSpots", spots.Length == 12, "Runtime MAP-A does not contain exactly 12 PlantingSpot components.");

            Dictionary<string, PlantingSpot> byId = spots.Where(s => !string.IsNullOrEmpty(s.Id))
                .GroupBy(s => s.Id).ToDictionary(g => g.Key, g => g.First());
            bool unique = byId.Count == spots.Length;
            bool allMatch = unique && MapAData.PlantingSpots.All(def =>
            {
                PlantingSpot spot;
                if (!byId.TryGetValue(def.Id, out spot))
                    return false;
                SpriteRenderer sr = spot.GetComponent<SpriteRenderer>();
                Transform ring = spot.transform.Find("Ring");
                SpriteRenderer ringRenderer = ring != null ? ring.GetComponent<SpriteRenderer>() : null;
                return Vector2.Distance(spot.transform.position, def.Position) <= 0.001f &&
                       sr != null && sr.enabled && sr.sprite != null &&
                       ringRenderer != null && ringRenderer.enabled && ringRenderer.sprite != null;
            });

            result.Check("plantingSpotIdsUnique", unique, "Runtime planting spot IDs are not unique.");
            result.Check("plantingSpotsMatchDefinitionsAndAreVisible", allMatch,
                "Runtime planting spots do not exactly match all definitions/positions or lack visible renderers.");
        }

        private static void ValidateGridAbsence(Result result)
        {
            Component[] components = Resources.FindObjectsOfTypeAll<Component>();
            int gridComponents = components.Count(c =>
                c != null && c.gameObject != null && c.gameObject.scene.IsValid() &&
                (c.GetType().Name == "Grid" || c.GetType().Name == "GridLayout"));

            Renderer[] renderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int namedVisibleGridRenderers = renderers.Count(r =>
                r != null && r.enabled && r.gameObject.activeInHierarchy &&
                r.gameObject.name.IndexOf("Grid", StringComparison.OrdinalIgnoreCase) >= 0);

            result.Fact("gridComponentCount", gridComponents);
            result.Fact("namedVisibleGridRendererCount", namedVisibleGridRenderers);
            result.Check("noGameplayGridVisible", gridComponents == 1 && namedVisibleGridRenderers == 0,
                "MAP-A needs one hidden logical tilemap Grid and no visible grid-line renderer.");
        }


        private static void ValidatePlayableTilemap(Result result)
        {
            string[] layerNames =
            {
                "Ground_Tilemap", "PathEdge_Tilemap", "Foliage_Tilemap", "SolidObstacles_Tilemap"
            };
            int valid = 0;
            for (int i = 0; i < layerNames.Length; i++)
            {
                GameObject go = GameObject.Find(layerNames[i]);
                Tilemap map = go != null ? go.GetComponent<Tilemap>() : null;
                if (map != null)
                    valid++;
            }
            result.Fact("tilemapLayerCount", valid);
            result.Check("fourTilemapLayers", valid == layerNames.Length,
                "Expected ground, path edge, foliage and solid obstacle Tilemaps.");

            GameObject groundObject = GameObject.Find("Ground_Tilemap");
            Tilemap ground = groundObject != null ? groundObject.GetComponent<Tilemap>() : null;
            int populated = 0;
            if (ground != null)
                for (int y = 0; y < MapATilemapLayers.HeightCells; y++)
                    for (int x = 0; x < MapATilemapLayers.WidthCells; x++)
                        if (ground.HasTile(new Vector3Int(x,y,0)))
                            populated++;
            result.Fact("groundTileCount", populated);
            result.Check("groundTilesCoverMap", populated == 336,
                "Expected 24x14 populated ground tiles.");

            GameObject solidsObject = GameObject.Find("SolidObstacles_Tilemap");
            TilemapCollider2D collision = solidsObject != null
                ? solidsObject.GetComponent<TilemapCollider2D>() : null;
            result.Check("solidObstacleCollisionPresent", collision != null && collision.enabled,
                "The obstacles require an active TilemapCollider2D.");

            string failure;
            bool navigable = MapATilemapLayers.ValidateNavigation(out failure);
            result.Fact("navigationFailure", failure);
            result.Check("enemyRouteAndBuildPositionsClear", navigable,
                "Route/build-position clearance failed: " + failure);

            GameObject sessionObject = GameObject.Find("GameSession");
            result.Fact("gameSessionExistsOutsidePlayMode", sessionObject != null);
        }

        private static void ValidateCameraFraming(Result result)
        {
            Camera camera = Camera.main;
            MobileCameraFitter fitter = camera != null ? camera.GetComponent<MobileCameraFitter>() : null;
            bool cameraReady = camera != null && camera.orthographic && fitter != null;
            result.Check("mapCameraConfigured", cameraReady, "MAP-A Main Camera/MobileCameraFitter is missing or not orthographic.");
            if (!cameraReady)
                return;

            float aspect = Mathf.Max(0.1f, (float)Screen.width / Mathf.Max(1, Screen.height));
            float halfHeight = camera.orthographicSize;
            float halfWidth = halfHeight * aspect;
            Vector3 center = camera.transform.position;
            float left = center.x - halfWidth;
            float right = center.x + halfWidth;
            float bottom = center.y - halfHeight;
            float top = center.y + halfHeight;

            bool centered = Mathf.Abs(center.x - MapAData.Width * 0.5f) <= 0.001f &&
                            Mathf.Abs(center.y - MapAData.Height * 0.5f) <= 0.001f;
            bool contains = left <= 0.001f && right >= MapAData.Width - 0.001f &&
                            bottom <= 0.001f && top >= MapAData.Height - 0.001f;

            result.Fact("gameViewWidth", Screen.width);
            result.Fact("gameViewHeight", Screen.height);
            result.Fact("gameViewAspectUsed", aspect.ToString("0.###", CultureInfo.InvariantCulture));
            result.Fact("cameraOrthographicSize", halfHeight.ToString("0.###", CultureInfo.InvariantCulture));
            result.Check("cameraCenteredOnLogicalMap", centered, "MAP-A camera is not centered on the 24x14 logical map.");
            result.Check("logicalMapContainedInGameView", contains, "MAP-A logical bounds are not fully contained by the configured Game View camera.");
        }

        private static bool InsideBounds(Vector2 p)
        {
            return p.x >= 0f && p.x <= MapAData.Width && p.y >= 0f && p.y <= MapAData.Height;
        }

        private static bool Approximately(float a, float b, float tolerance)
        {
            return Mathf.Abs(a - b) <= tolerance;
        }

        private static string Format(Vector2 value)
        {
            return value.x.ToString("0.###", CultureInfo.InvariantCulture) + "," +
                   value.y.ToString("0.###", CultureInfo.InvariantCulture);
        }
    }
}
#endif
