using System;
using System.Collections.Generic;
using UnityEngine;

namespace Heroes3QuestPrototype
{
    // Standalone XR tabletop demonstrator. All gameplay below is original placeholder logic.
    // No VCMI data or original Heroes III assets are embedded or simulated as integrated.
    public sealed class QuestHeroesPrototype : MonoBehaviour
    {
        private const float HexRadius = 0.125f;
        private static readonly Vector2Int[] AxialNeighbors = {
            new Vector2Int(1,0),new Vector2Int(1,-1),new Vector2Int(0,-1),
            new Vector2Int(-1,0),new Vector2Int(-1,1),new Vector2Int(0,1)
        };
        private readonly Dictionary<Vector2Int, Tile> tiles = new Dictionary<Vector2Int, Tile>();
        private readonly List<GameObject> pieces = new List<GameObject>();
        private Transform boardRoot;
        private Transform hero;
        private Transform enemy;
        private TextMesh statusText;
        private Camera head;
        private bool battle;
        private bool lastTrigger;
        private bool lastMouse;
        private Vector2Int heroCoord = new Vector2Int(1, 2);
        private Vector2Int enemyCoord = new Vector2Int(6, 3);
        private int movement = 8;
        private int wins;
        private float enemyCooldown;

        private sealed class Tile
        {
            public Vector2Int Coord;
            public GameObject Object;
            public Renderer Renderer;
            public bool Blocked;
        }

        private void Start()
        {
            var rig = new GameObject("Quest XR tracking origin").transform;
            rig.position = new Vector3(0f, 0f, -1.15f);
            var camGo = new GameObject("Quest HMD Camera");
            camGo.tag = "MainCamera";
            camGo.transform.SetParent(rig, false);
            head = camGo.AddComponent<Camera>();
            head.nearClipPlane = 0.035f;
            head.farClipPlane = 100f;
            camGo.AddComponent<AudioListener>();
            camGo.AddComponent<QuestXRInput>();
            var lighting = new GameObject("Sun").AddComponent<Light>();
            lighting.type = LightType.Directional;
            lighting.intensity = 1.25f;
            lighting.transform.rotation = Quaternion.Euler(50f, -35f, 0f);
            RenderSettings.ambientLight = new Color(.52f, .6f, .67f);
            var root = new GameObject("Tabletop board");
            boardRoot = root.transform;
            boardRoot.position = new Vector3(-.63f, .82f, -.08f);
            CreateTitle();
            BuildWorld();
        }

        private void CreateTitle()
        {
            var label = new GameObject("Status panel");
            label.transform.position = new Vector3(0, 1.05f, 0.2f);
            statusText = label.AddComponent<TextMesh>();
            statusText.anchor = TextAnchor.MiddleCenter;
            statusText.alignment = TextAlignment.Center;
            statusText.characterSize = .038f;
            statusText.fontSize = 44;
            statusText.color = Color.white;
            label.transform.localScale = Vector3.one * .8f;
            statusText.text = "Heroes III Quest - XR prototype";
        }

        private void ClearBoard()
        {
            foreach (var item in pieces) if(item) Destroy(item);
            pieces.Clear();
            foreach (var tile in tiles.Values) if(tile.Object) Destroy(tile.Object);
            tiles.Clear();
        }

        private void BuildWorld()
        {
            battle = false;
            ClearBoard();
            for (int q = 0; q < 9; q++)
            for (int r = 0; r < 7; r++)
            {
                var coord = new Vector2Int(q, r);
                bool mountain = ((q == 4 && r < 3) || (q == 7 && r == 5));
                var tint = mountain ? new Color(.44f,.47f,.51f) :
                    ((q + r) % 5 == 0 ? new Color(.30f,.48f,.27f) : new Color(.43f,.64f,.34f));
                AddTile(coord, mountain ? .14f : .065f, tint, mountain);
            }
            hero = SpawnPiece("Your hero", heroCoord, new Color(.2f,.5f,1f));
            enemy = SpawnPiece("Enemy patrol", enemyCoord, new Color(.86f,.27f,.24f));
            UpdateStatus("Explore: point at a reachable green hex and pull trigger.");
        }

        private void BuildBattle()
        {
            battle = true;
            ClearBoard();
            heroCoord = new Vector2Int(1, 3);
            enemyCoord = new Vector2Int(5, 3);
            for (int q = 0; q < 7; q++)
            for (int r = 0; r < 7; r++)
                AddTile(new Vector2Int(q,r), .07f, ((q + r) % 2 == 0)
                    ? new Color(.54f,.42f,.31f) : new Color(.61f,.50f,.37f), false);
            hero = SpawnPiece("Blue unit", heroCoord, new Color(.2f,.5f,1f));
            enemy = SpawnPiece("Red unit", enemyCoord, new Color(.9f,.23f,.19f));
            UpdateStatus("Battle: reach the red unit; select adjacent red hex to attack.");
        }

        private void AddTile(Vector2Int coord, float height, Color tint, bool blocked)
        {
            var go = new GameObject("Hex " + coord.x + "," + coord.y);
            go.transform.SetParent(boardRoot, false);
            var p = HexCenter(coord);
            go.transform.localPosition = p;
            var mesh = MakeHexPrism(HexRadius * .98f, height);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var material = new Material(Shader.Find("Standard"));
            material.color = tint;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.material = material;
            var collider = go.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;
            var hit = go.AddComponent<HexTarget>();
            hit.Coord = coord;
            tiles[coord] = new Tile { Coord=coord, Object=go, Renderer=renderer, Blocked=blocked };
        }

        private Transform SpawnPiece(string name, Vector2Int coord, Color color)
        {
            var pawn = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            pawn.name = name;
            pawn.transform.SetParent(boardRoot, false);
            pawn.transform.localScale = new Vector3(.09f, .12f, .09f);
            pawn.transform.localPosition = HexCenter(coord) + Vector3.up * .20f;
            pawn.GetComponent<Renderer>().material.color = color;
            pieces.Add(pawn);
            return pawn.transform;
        }

        private static Vector3 HexCenter(Vector2Int p) =>
            new Vector3(HexRadius * Mathf.Sqrt(3f) * (p.x + p.y / 2f), 0f, HexRadius * 1.5f * p.y);

        private static Mesh MakeHexPrism(float radius, float height)
        {
            var vertices = new List<Vector3> {new Vector3(0,height,0)};
            for (int i=0;i<6;i++)
            {
                float angle = Mathf.Deg2Rad * (60f * i + 30f);
                vertices.Add(new Vector3(Mathf.Cos(angle)*radius, height, Mathf.Sin(angle)*radius));
            }
            for (int i=0;i<6;i++)
            {
                var v=vertices[i+1]; vertices.Add(new Vector3(v.x,0f,v.z));
            }
            var indices = new List<int>();
            for(int i=0;i<6;i++) {indices.Add(0);indices.Add(1+(i+1)%6);indices.Add(1+i);}
            for(int i=0;i<6;i++)
            {
                int top=1+i,next=1+(i+1)%6,bottom=7+i,bottomNext=7+(i+1)%6;
                indices.Add(top);indices.Add(bottom);indices.Add(next);
                indices.Add(next);indices.Add(bottom);indices.Add(bottomNext);
            }
            var mesh=new Mesh{ name="Procedural Hex"};
            mesh.SetVertices(vertices); mesh.SetTriangles(indices,0);
            mesh.RecalculateNormals();mesh.RecalculateBounds();
            return mesh;
        }

        private void Update()
        {
            var tracking = head.GetComponent<QuestXRInput>();
            tracking.GetPointingRay(out Ray pointer, out bool trigger);
            bool mouseClick=Input.GetMouseButton(0);
            if ((trigger && !lastTrigger) || (mouseClick && !lastMouse))
            {
                Ray targetRay = mouseClick ? head.ScreenPointToRay(Input.mousePosition) : pointer;
                if (Physics.Raycast(targetRay, out var hit, 20f))
                {
                    var tile=hit.collider.GetComponent<HexTarget>();
                    if(tile != null) SelectHex(tile.Coord);
                }
            }
            lastTrigger=trigger;lastMouse=mouseClick;
            if(enemyCooldown > 0f) enemyCooldown -= Time.deltaTime;
            if (head != null && statusText != null)
            {
                statusText.transform.LookAt(head.transform);
                statusText.transform.Rotate(0,180,0);
            }
        }

        private void SelectHex(Vector2Int destination)
        {
            if (!tiles.TryGetValue(destination, out var target) || target.Blocked) return;
            if (destination == enemyCoord)
            {
                if (battle)
                {
                    if(HexDistance(heroCoord, enemyCoord) <= 1)
                    {
                        wins++;
                        UpdateStatus("Victory! Returning to adventure map.");
                        heroCoord = new Vector2Int(1,2);
                        enemyCoord = new Vector2Int(6,3);
                        BuildWorld();
                    }
                    else UpdateStatus("Red unit is out of melee range.");
                }
                else BuildBattle();
                return;
            }
            int distance = GetTraversableDistance(heroCoord, destination);
            if(distance < 0 || distance > (battle ? 3 : movement))
            {
                UpdateStatus("Destination blocked or outside current movement range.");
                return;
            }
            heroCoord=destination;
            hero.localPosition=HexCenter(destination)+Vector3.up*.2f;
            if(battle) UpdateStatus("Blue unit moved. Select red enemy when adjacent.");
            else UpdateStatus("Exploring. Reach the red patrol for a sample tactical battle.");
        }

        private int GetTraversableDistance(Vector2Int from,Vector2Int to)
        {
            var seen = new HashSet<Vector2Int>{from};
            var queue = new Queue<Vector2Int>();
            var depth = new Dictionary<Vector2Int,int>{{from,0}};
            queue.Enqueue(from);
            while(queue.Count>0)
            {
                var current=queue.Dequeue();
                if(current==to) return depth[current];
                foreach(var delta in AxialNeighbors)
                {
                    var next=current+delta;
                    if(seen.Contains(next) || !tiles.TryGetValue(next,out var tile) || tile.Blocked || next==enemyCoord)
                        continue;
                    seen.Add(next);depth[next]=depth[current]+1;queue.Enqueue(next);
                }
            }
            return -1;
        }

        private static int HexDistance(Vector2Int a, Vector2Int b)
        {
            int dq=a.x-b.x, dr=a.y-b.y;
            return HexNavigation.Distance(a,b);
        }

        private void UpdateStatus(string msg)
        {
            statusText.text="Heroes III Quest — 2.5D tech demo\n"+msg+"\nWins: "+wins+
                " | Gameplay is placeholder, NOT connected to VCMI.";
        }
    }

    public sealed class HexTarget:MonoBehaviour
    {
        public Vector2Int Coord;
    }
}