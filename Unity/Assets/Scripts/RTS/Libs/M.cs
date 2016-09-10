using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public static class M
{

    public static Vector2 NormalizedRadialSpread()
    {
        var angle = Random.value * Mathf.PI * 2.0f;
        var radius = Mathf.Sqrt(Random.value);
        var vec2 = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        return vec2 * radius;
    }

    public static float RotateTowardsTarget(float currentAngle, float desiredAngle, float degreesPerSecond, float dt)
    {
        var deltaAngle = Mathf.DeltaAngle(desiredAngle, currentAngle);
        return currentAngle - Mathf.Sign(deltaAngle) * Mathf.Min(Mathf.Abs(deltaAngle), degreesPerSecond * dt);
    }

    public static int SampleFromProbabilities(float[] probabilities)
    {
        if (probabilities.Any(v => v < 0))
        {
            throw new ArgumentException("M: SampleFromProbabilities: probabilities must be >= 0");
        }
        var sum = probabilities.Sum();
        if (sum <= 0f)
        {
            throw new ArgumentException("M: SampleFromProbabilities: sum of probabilities must be >= 0");
        }
        var normalized = F.Map(v => v / sum, probabilities);
        var rand = Random.value;
        var s = 0f;
        for (var i = 0; i < probabilities.Length; i++)
        {
            s += normalized[i];
            if (rand <= s)
                return i;
        }
        return 0;
    }
}