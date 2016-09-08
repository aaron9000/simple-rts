using UnityEngine;
using System;

public static class M
{

    public static Vector2 NormalizedRadialSpread()
    {
        var angle = UnityEngine.Random.value * Mathf.PI * 2.0f;
        var radius = Mathf.Sqrt(UnityEngine.Random.value);
        var vec2 = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        return vec2 * radius;
    }

    public static float RotateTowardsTarget(float currentAngle, float desiredAngle, float degreesPerSecond, float dt)
    {
        var deltaAngle = Mathf.DeltaAngle(desiredAngle, currentAngle);
        return currentAngle - Mathf.Sign(deltaAngle) * Mathf.Min(Mathf.Abs(deltaAngle), degreesPerSecond * dt);
    }
}