using System;
using System.Collections.Generic;
using UnityEngine;

namespace Heroes3QuestPrototype
{
    /// <summary>Axial hex coordinate helpers. Independent of XR and renderer.</summary>
    public static class HexNavigation
    {
        public static readonly Vector2Int[] Directions = {
            new Vector2Int(1,0),new Vector2Int(1,-1),new Vector2Int(0,-1),
            new Vector2Int(-1,0),new Vector2Int(-1,1),new Vector2Int(0,1)
        };

        public static int Distance(Vector2Int a, Vector2Int b)
        {
            int dq = a.x - b.x;
            int dr = a.y - b.y;
            return (Math.Abs(dq) + Math.Abs(dr) + Math.Abs(dq + dr)) / 2;
        }

        /// <summary>Returns shortest path length, or -1 if blocked. Predicate must be deterministic.</summary>
        public static int ShortestPathLength(
            Vector2Int origin, Vector2Int target,
            Func<Vector2Int,bool> canEnter, int maxExpansions = 5000)
        {
            if (origin == target) return 0;
            if (canEnter == null) throw new ArgumentNullException(nameof(canEnter));
            var frontier = new Queue<Vector2Int>();
            var distances = new Dictionary<Vector2Int,int> { {origin,0} };
            frontier.Enqueue(origin);
            int expanded = 0;
            while(frontier.Count > 0 && expanded++ < maxExpansions)
            {
                var coord = frontier.Dequeue();
                foreach(var d in Directions)
                {
                    var n = coord + d;
                    if(distances.ContainsKey(n) || !canEnter(n)) continue;
                    if(n == target) return distances[coord] + 1;
                    distances[n] = distances[coord]+1;
                    frontier.Enqueue(n);
                }
            }
            return -1;
        }
    }
}