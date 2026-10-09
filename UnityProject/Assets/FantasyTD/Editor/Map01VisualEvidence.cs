#if UNITY_EDITOR
using System;
using System.IO;
using FantasyTD.MapA;
using FantasyTD.MapA.Editor;
using UnityEditor;
using UnityEngine;

namespace FantasyTD.CI
{
    /// <summary>
    /// Evidence captured from the real in-engine Camera during Unity Play Mode,
    /// under Xvfb with graphics enabled. Never produces concept mockups.
    /// </summary>
    [InitializeOnLoad]
    public static class Map01VisualEvidence
    {
        private const string Active = "FantasyTD.Map01.VisualEvidence.Active";
        private const string Stage = "FantasyTD.Map01.VisualEvidence.Stage";
        private const string Output = "Artifacts/UnityExecutor/Map01Visual";
        private const int CaptureWidth = 1280;
        private const int CaptureHeight = 720;
        private static int _frames;
        private static int _count;
        private static bool _started;
        private static bool _failed;

        static Map01VisualEvidence()
        {
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
        }

        public static void Start()
        {
            Directory.CreateDirectory(Output);
            SessionState.SetBool(Active, true);
            SessionState.SetString(Stage, "play");
            _frames = 0;
            _count = 0;
            _started = false;
            _failed = false;
            try
            {
                MapAEditorSetup.OpenMap();
                EditorApplication.EnterPlaymode();
            }
            catch (Exception e)
            {
                Debug.LogError("Map01 capture could not enter Play Mode: " + e);
                Finish(false);
            }
        }

        private static void Tick()
        {
            if (!SessionState.GetBool(Active, false)) return;

            if (SessionState.GetString(Stage, "") == "exit")
            {
                if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    SessionState.SetBool(Active, false);
                    EditorApplication.Exit(_failed ? 1 : 0);
                }
                return;
            }

            if (!EditorApplication.isPlaying) return;
            _frames++;

            try
            {
                GameSession session = UnityEngine.Object.FindFirstObjectByType<GameSession>();
                Camera camera = Camera.main;
                if (session == null || camera == null || session.Waves == null)
                {
                    if (_frames > 240)
                        throw new Exception("No GameSession/Camera/WaveDirector in Play Mode.");
                    return;
                }

                if (!_started && _frames > 20)
                {
                    session.Waves.StartNextWave();
                    _started = true;
                }

                if (_frames == 45 || _frames == 90 || _frames == 135)
                    Capture(camera, "map01-gameplay-" + _frames.ToString("000") + ".png");

                if (_frames >= 148)
                    Finish(_count == 3);
            }
            catch (Exception e)
            {
                Debug.LogError("Map01 in-engine image capture failed: " + e);
                Finish(false);
            }
        }

        private static void Capture(Camera camera, string file)
        {
            RenderTexture oldTarget = camera.targetTexture;
            RenderTexture oldActive = RenderTexture.active;
            float oldSize = camera.orthographicSize;
            float oldAspect = camera.aspect;

            RenderTexture render = new RenderTexture(CaptureWidth, CaptureHeight, 24, RenderTextureFormat.ARGB32);
            Texture2D texture = new Texture2D(CaptureWidth, CaptureHeight, TextureFormat.RGB24, false);
            try
            {
                // Frame the actual MAP-A gameplay world, irrespective of runner GameView size.
                camera.orthographicSize = 7.55f;
                camera.aspect = (float)CaptureWidth / CaptureHeight;
                camera.targetTexture = render;
                RenderTexture.active = render;
                camera.Render();
                texture.ReadPixels(new Rect(0, 0, CaptureWidth, CaptureHeight), 0, 0);
                texture.Apply(false, false);
                byte[] bytes = texture.EncodeToPNG();
                if (bytes == null || bytes.Length < 3000)
                    throw new Exception("GPU capture returned an unexpectedly small PNG.");
                string path = Path.Combine(Output, file);
                File.WriteAllBytes(path, bytes);
                Debug.Log("MAP01_ACTUAL_UNITY_SCREENSHOT=" + path + " bytes=" + bytes.Length);
                _count++;
            }
            finally
            {
                camera.targetTexture = oldTarget;
                RenderTexture.active = oldActive;
                camera.orthographicSize = oldSize;
                camera.aspect = oldAspect;
                UnityEngine.Object.DestroyImmediate(texture);
                render.Release();
                UnityEngine.Object.DestroyImmediate(render);
            }
        }

        private static void Finish(bool successful)
        {
            if (SessionState.GetString(Stage, "") == "exit") return;
            _failed = !successful;
            string report =
                "MAP01 REAL UNITY RENDER EVIDENCE\n" +
                "timestampUtc=" + DateTime.UtcNow.ToString("O") + "\n" +
                "unityVersion=" + Application.unityVersion + "\n" +
                "inPlayMode=" + EditorApplication.isPlaying + "\n" +
                "capturedPngCount=" + _count + "\n" +
                "status=" + (successful ? "PASS" : "FAIL") + "\n";
            File.WriteAllText(Path.Combine(Output, "visual-evidence.txt"), report);
            Debug.Log(report);
            SessionState.SetString(Stage, "exit");
            if (EditorApplication.isPlaying)
                EditorApplication.ExitPlaymode();
            else
            {
                SessionState.SetBool(Active, false);
                EditorApplication.Exit(_failed ? 1 : 0);
            }
        }
    }
}
#endif
