using System;
using System.Collections.Generic;
using UnityEngine;

namespace FantasyTD.MapA
{
    // Geometry only: every visible pixel comes from the supplied approved crop.
    public static class AuthoredPathRenderer
    {
        public static void Create(string namePrefix, Vector2[][] routes, string resourcePath, Rect cropPixelsTopLeft,
            Vector2 sourceSize, float roadWidth = 1.05f, float tileLength = 1.5f)
        {
            if (routes == null || routes.Length == 0)
                throw new ArgumentException("At least one complete authored route is required.", nameof(routes));
            foreach (Vector2[] route in routes) GameSession.SnapshotRoute(route);
            Texture2D source = Resources.Load<Texture2D>(resourcePath);
            if (source == null) throw new InvalidOperationException("Required authored road texture missing: " + resourcePath);
            if (source.width != sourceSize.x || source.height != sourceSize.y)
                throw new InvalidOperationException("Authored road crop source dimensions changed: " + resourcePath);
            if (cropPixelsTopLeft.xMin < 0f || cropPixelsTopLeft.yMin < 0f ||
                cropPixelsTopLeft.xMax > source.width || cropPixelsTopLeft.yMax > source.height ||
                cropPixelsTopLeft.width <= 0f || cropPixelsTopLeft.height <= 0f || roadWidth <= 0f || tileLength <= 0f)
                throw new ArgumentException("Authored road dimensions must be positive and inside the source.");
            Rect uvCrop = new Rect(cropPixelsTopLeft.x / source.width,
                (source.height - cropPixelsTopLeft.yMax) / source.height,
                cropPixelsTopLeft.width / source.width, cropPixelsTopLeft.height / source.height);
            Material material = RuntimeArt.SpriteMaterial(Color.white);
            material.mainTexture = source;
            HashSet<string> drawnEdges = new HashSet<string>();
            foreach (Vector2[] route in routes)
            for (int i = 0; i < route.Length - 1; i++)
            {
                string key = route[i].ToString("R") + "/" + route[i + 1].ToString("R");
                if (!drawnEdges.Add(key)) continue;
                DrawTexturedEdge(route, i, material, namePrefix, uvCrop, roadWidth, tileLength);
            }
        }

        private static Vector2 EdgeOffset(Vector2[] route, int index, float roadWidth)
        {
            Vector2 before = index > 0 ? (route[index] - route[index - 1]).normalized
                : (route[1] - route[0]).normalized;
            Vector2 after = index < route.Length - 1 ? (route[index + 1] - route[index]).normalized : before;
            Vector2 normalBefore = new Vector2(-before.y, before.x);
            Vector2 normalAfter = new Vector2(-after.y, after.x);
            Vector2 average = (normalBefore + normalAfter).normalized;
            return average * (roadWidth * 0.5f / Mathf.Max(0.5f, Vector2.Dot(average, normalAfter)));
        }

        private static void DrawTexturedEdge(Vector2[] route, int index, Material material,
            string namePrefix, Rect uvCrop, float roadWidth, float tileLength)
        {
            Vector2 start = route[index], end = route[index + 1];
            Vector2 startOffset = EdgeOffset(route, index, roadWidth);
            Vector2 endOffset = EdgeOffset(route, index + 1, roadWidth);
            int tiles = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(start, end) / tileLength));
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            List<int> triangles = new List<int>();
            List<Color> colours = new List<Color>();
            for (int tile = 0; tile < tiles; tile++)
            {
                float a = (float)tile / tiles, b = (float)(tile + 1) / tiles;
                int first = vertices.Count;
                vertices.Add(Vector2.Lerp(start - startOffset, end - endOffset, a));
                vertices.Add(Vector2.Lerp(start + startOffset, end + endOffset, a));
                vertices.Add(Vector2.Lerp(start - startOffset, end - endOffset, b));
                vertices.Add(Vector2.Lerp(start + startOffset, end + endOffset, b));
                uv.Add(new Vector2(uvCrop.xMin, uvCrop.yMin)); uv.Add(new Vector2(uvCrop.xMin, uvCrop.yMax));
                uv.Add(new Vector2(uvCrop.xMax, uvCrop.yMin)); uv.Add(new Vector2(uvCrop.xMax, uvCrop.yMax));
                for (int vertex = 0; vertex < 4; vertex++) colours.Add(Color.white);
                triangles.AddRange(new[] { first, first + 2, first + 1, first + 1, first + 2, first + 3 });
            }
            Mesh mesh = new Mesh { name = namePrefix + "_AuthoredRoadMesh" };
            mesh.SetVertices(vertices); mesh.SetUVs(0, uv); mesh.SetColors(colours);
            mesh.SetTriangles(triangles, 0); mesh.RecalculateBounds();
            GameObject edge = new GameObject(namePrefix + "_SourceRoad_" + index);
            edge.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = edge.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.sortingOrder = -90;
        }
    }
}
