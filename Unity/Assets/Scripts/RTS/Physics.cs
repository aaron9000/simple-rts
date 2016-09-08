using UnityEngine;
using System.Collections;

public static class Physics  {

    public static void Update(Queries queries)
    {

        var barrier = PhysicsConsts.BarrierPadding;
        var radius = PhysicsConsts.SoldierRadius;
        var min = radius * 2;
        foreach (int laneIndex in F.Range(0, BalanceConsts.Lanes))
        {
            var laneKey = laneIndex.ToString();
            var minX = Map.GetLeftEdgeOfLane(laneIndex) + (radius + barrier);
            var maxX = Map.GetRightEdgeOfLane(laneIndex) - (radius + barrier);
            var units = queries.GetUnitsByLaneKey(laneKey, u => u.UnitType == UnitType.Soldier);
            var len = units.Count;

            // Collide with other units
            for (var i = 0; i < len; i++)
            {
                var a = units[i];
                for (var j = i + 1; j < len; j++)
                {
                    var b = units[j];
                    if (a.Position == b.Position)
                    {
                        a.Position += new Vector2(0, 0.001f);
                    }
                    var delta = a.Position - b.Position;
                    var intersect = min - delta.magnitude;
                    if (intersect > 0)
                    {
                        var travel = intersect * -0.5f;
                        a.Position = Vector2.MoveTowards(a.Position, b.Position, travel);
                        b.Position = Vector2.MoveTowards(b.Position, a.Position, travel);
                    }
                }

                // Stay inside of lane
                a.Position.x = Mathf.Clamp(a.Position.x, minX, maxX);
            }
        }
    }
}
