using NUnit.Framework;
using UnityEngine;

namespace Heroes3QuestPrototype.Tests
{
    public class HexNavigationTests
    {
        [Test] public void Distance_SameCell_IsZero()
        {
            Assert.AreEqual(0, HexNavigation.Distance(Vector2Int.zero, Vector2Int.zero));
        }
        [Test] public void Distance_IsSymmetric()
        {
            var a = new Vector2Int(2, -5);
            var b = new Vector2Int(-3, 4);
            Assert.AreEqual(HexNavigation.Distance(a,b), HexNavigation.Distance(b,a));
        }
        [Test] public void Adjacent_HasUnitDistance()
        {
            foreach (var d in HexNavigation.Directions)
                Assert.AreEqual(1, HexNavigation.Distance(Vector2Int.zero, d));
        }
        [Test] public void ShortestPath_TraversesAdjacentTiles()
        {
            var from = Vector2Int.zero;
            var to = new Vector2Int(2, 0);
            Assert.AreEqual(2, HexNavigation.ShortestPathLength(from,to,c => c.x >= 0 && c.x <= 2 && c.y == 0));
        }
        [Test] public void ShortestPath_RejectsBlockedTile()
        {
            var from=Vector2Int.zero;
            var to=new Vector2Int(2,0);
            Assert.AreEqual(-1, HexNavigation.ShortestPathLength(from,to,c => c==new Vector2Int(2,0)));
        }
    }
}