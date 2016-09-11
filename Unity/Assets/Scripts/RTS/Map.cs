using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Map
{
    public static List<BaseEvent> GetMapSpawnEvents()
    {
        var spawns = new List<BaseEvent>();
        var playerAngle = GetFaceAngle(Side.Player);
        var enemyAngle = GetFaceAngle(Side.Enemy);

        // Add enemy player AI
        spawns.Add(new Events.SpawnEvent(Vector2.zero, ObjectType.EnemyAI));

        // Add turrets, control points, and barriers
        foreach (var laneIndex in F.Range(0, BalanceConsts.Lanes))
        {
            var laneKey = laneIndex.ToString();
            var rightEdge = GetRightEdgeOfLane(laneIndex);

            // Add barrier
            if (laneIndex < BalanceConsts.Lanes - 1)
            {
                var barrierPos = new Vector2(rightEdge, ScreenConsts.HeightInPixels * 0.5f);
                spawns.Add(new Events.SpawnEvent(barrierPos, playerAngle, Side.Neutral, laneKey, ObjectType.MapBarrier));
            }

            // Turrets
            var playerTurretPos = GetTurretSpawnPosition(laneIndex, Side.Player);
            spawns.Add(new Events.SpawnEvent(playerTurretPos, playerAngle, Side.Player, laneKey, ObjectType.UnitTurret));
            var enemyTurretPos = GetTurretSpawnPosition(laneIndex, Side.Enemy);
            spawns.Add(new Events.SpawnEvent(enemyTurretPos, enemyAngle, Side.Enemy, laneKey, ObjectType.UnitTurret));

            // Add Control points
            var center = GetLaneCenterX(laneIndex);
            var controlPoints = F.Map(
                y =>
                {
                    var p = new Vector2(center, y);
                    return new Events.SpawnEvent(p, 0, Side.Neutral, laneKey, ObjectType.UnitControlPoint);
                },
                GetControlPointYPositions());
            spawns.AddRange(controlPoints);
        }
        return spawns;
    }

    public static float GetFaceAngle(Side side)
    {
        return side == Side.Enemy ? 270f : 90f;
    }

    public static Vector2 ConvertToWorldCoordinates(Vector2 gameCoord)
    {
        var screenInPixels = new Vector2(ScreenConsts.WidthInPixels, ScreenConsts.HeightInPixels);
        return (gameCoord - screenInPixels * 0.5f) * Screen.GetPixelToWorldRatio();
    }


    public static float GetLaneCenterX(int laneIndex)
    {
        return (GetLeftEdgeOfLane(laneIndex) + GetRightEdgeOfLane(laneIndex)) * 0.5f;
    }

    public static Vector2 GetTurretSpawnPosition(int laneIndex, Side side)
    {
        var x = GetLaneCenterX(laneIndex);
        var w = 130.0f;
        var y = side == Side.Player ? w : ScreenConsts.HeightInPixels - w;
        return new Vector2(x, y);
    }

    public static Vector2 GetSoldierSpawnPosition(int laneIndex, Side side)
    {
        var x = GetLaneCenterX(laneIndex);
        var w = 340f;
        var y = side == Side.Player ? w : ScreenConsts.HeightInPixels - w;
        return new Vector2(x, y);
    }

    public static float GetLaneWidth()
    {
        return ScreenConsts.WidthInPixels / BalanceConsts.Lanes;
    }

    public static float GetLeftEdgeOfLane(int laneIndex)
    {
        return laneIndex * GetLaneWidth();
    }

    public static float GetRightEdgeOfLane(int laneIndex)
    {
        return (laneIndex + 1) * GetLaneWidth();
    }

    public static List<float> GetControlPointYPositions()
    {
        var h = ScreenConsts.HeightInPixels * 0.5f;
        return new List<float>(new[]
        {
            h + 275f,
            h,
            h - 275f
        });
    }
}