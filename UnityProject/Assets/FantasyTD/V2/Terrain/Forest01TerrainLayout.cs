using System;
using System.Collections.Generic;
using UnityEngine;

namespace FantasyTD.V2.Terrain
{
    /// <summary>Pure, deterministic authored plan for the isolated isometric terrain test.</summary>
    public static class Forest01TerrainLayout
    {
        public const int Width = 28;
        public const int Height = 20;

        public readonly struct Cell : IEquatable<Cell>
        {
            public readonly int X;
            public readonly int Y;
            public Cell(int x, int y) { X = x; Y = y; }
            public Vector3Int Vector => new Vector3Int(X, Y, 0);
            public bool Equals(Cell other) => X == other.X && Y == other.Y;
            public override bool Equals(object obj) => obj is Cell other && Equals(other);
            public override int GetHashCode() => (X * 397) ^ Y;
            public override string ToString() => "(" + X + "," + Y + ")";
        }

        public static readonly Cell[] RoadCheckpoints =
        {
            new Cell(0, 13), new Cell(4, 13), new Cell(4, 11),
            new Cell(9, 11), new Cell(9, 13), new Cell(13, 13),
            new Cell(13, 9), new Cell(18, 9), new Cell(18, 7),
            new Cell(22, 7), new Cell(22, 9), new Cell(27, 9)
        };

        public static readonly Cell[] Road = ExpandOrthogonal(RoadCheckpoints);

        public static readonly Cell[] Sites =
        {
            new Cell(2, 16), new Cell(5, 15), new Cell(7, 9),
            new Cell(10, 16), new Cell(11, 10), new Cell(15, 15),
            new Cell(15, 7), new Cell(20, 11), new Cell(23, 5),
            new Cell(25, 12)
        };

        public static readonly Cell[] Water =
        {
            new Cell(16, 2), new Cell(17, 2), new Cell(18, 2), new Cell(19, 2),
            new Cell(15, 3), new Cell(16, 3), new Cell(17, 3),
            new Cell(18, 3), new Cell(19, 3), new Cell(20, 3),
            new Cell(16, 4), new Cell(17, 4), new Cell(18, 4),
            new Cell(19, 4), new Cell(20, 4),
            new Cell(17, 5), new Cell(18, 5), new Cell(19, 5)
        };

        public static readonly Cell Bridge = new Cell(18, 4);

        public static readonly Cell[] Trees =
        {
            new Cell(1, 18), new Cell(4, 18), new Cell(6, 17),
            new Cell(11, 18), new Cell(14, 17), new Cell(20, 16),
            new Cell(26, 17), new Cell(0, 4), new Cell(4, 3),
            new Cell(9, 4), new Cell(25, 3), new Cell(27, 1)
        };

        public static readonly Cell[] Rocks =
        {
            new Cell(2, 2), new Cell(7, 5), new Cell(12, 3),
            new Cell(14, 16), new Cell(23, 16), new Cell(26, 7)
        };

        public static bool InBounds(Cell cell) =>
            cell.X >= 0 && cell.Y >= 0 && cell.X < Width && cell.Y < Height;

        public static int Manhattan(Cell a, Cell b) =>
            Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);

        private static Cell[] ExpandOrthogonal(Cell[] points)
        {
            var cells = new List<Cell>();
            if (points == null || points.Length < 2)
                throw new InvalidOperationException("Road requires endpoints");
            cells.Add(points[0]);
            for (int i = 1; i < points.Length; i++)
            {
                Cell prev = points[i - 1], next = points[i];
                int dx = Math.Sign(next.X - prev.X);
                int dy = Math.Sign(next.Y - prev.Y);
                if ((dx == 0) == (dy == 0))
                    throw new InvalidOperationException("Road steps must be nonzero and orthogonal");
                Cell current = prev;
                while (!current.Equals(next))
                {
                    current = new Cell(current.X + dx, current.Y + dy);
                    cells.Add(current);
                }
            }
            return cells.ToArray();
        }

        public static void Validate()
        {
            if (Width != 28 || Height != 20) throw new InvalidOperationException("Grid contract changed");
            if (Road.Length != 40 || Sites.Length != 10)
                throw new InvalidOperationException("Road or site count changed");
            var roadCells = new HashSet<Cell>();
            for (int i = 0; i < Road.Length; i++)
            {
                Cell c = Road[i];
                if (!InBounds(c) || !roadCells.Add(c))
                    throw new InvalidOperationException("Road cell outside grid or repeated: " + c);
                if (i > 0 && Manhattan(Road[i - 1], c) != 1)
                    throw new InvalidOperationException("Road gap at " + c);
            }
            var waterCells = new HashSet<Cell>();
            foreach (Cell c in Water)
                if (!InBounds(c) || !waterCells.Add(c) || roadCells.Contains(c))
                    throw new InvalidOperationException("Water conflicts with road or bounds: " + c);
            if (!waterCells.Contains(Bridge))
                throw new InvalidOperationException("Bridge must be over the pond");
            var siteCells = new HashSet<Cell>();
            foreach (Cell c in Sites)
            {
                if (!InBounds(c) || !siteCells.Add(c) ||
                    waterCells.Contains(c) || roadCells.Contains(c))
                    throw new InvalidOperationException("Build site conflicts: " + c);
                foreach (Cell path in Road)
                    if (Manhattan(c, path) < 2)
                        throw new InvalidOperationException("Build site too close to path: " + c);
            }
            var decorations = new HashSet<Cell>();
            foreach (Cell c in Trees)
                if (!InBounds(c) || !decorations.Add(c) ||
                    siteCells.Contains(c) || roadCells.Contains(c) || waterCells.Contains(c))
                    throw new InvalidOperationException("Tree placement conflicts: " + c);
            foreach (Cell c in Rocks)
                if (!InBounds(c) || !decorations.Add(c) ||
                    siteCells.Contains(c) || roadCells.Contains(c) || waterCells.Contains(c))
                    throw new InvalidOperationException("Rock placement conflicts: " + c);
        }
    }
}
