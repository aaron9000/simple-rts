using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class EnemyAI
{

    public static float[] GetDistributionForDifficulty(Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Difficulty.VeryEasy:
                return new float[] {0.4f, 0.3f, 0.3f};
            case Difficulty.Easy:
                return new float[] {0.5f, 0.3f, 0.2f};
            case Difficulty.Medium:
                return new float[] {0.6f, 0.3f, 0.1f};
            default:
                return new float[] {0.7f, 0.2f, 0.1f};
        }
    }

    public static float GetCooldownForDifficulty(Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Difficulty.VeryEasy:
                return 1.0f;
            case Difficulty.Easy:
                return 0.75f;
            case Difficulty.Medium:
                return 0.50f;
            default:
                return 0.3f;
        }
    }

    public static string GetSpawnLaneKey(GameState state, Queries queries)
    {
        var lanes = F.Range(0, BalanceConsts.Lanes);
        var listOfTuples = new List<F.Tuple<string, float>>();
        var distribution = GetDistributionForDifficulty(state.Difficulty);
        var tuples = F.Reduce((accum, i) =>
        {
            var laneKey = i.ToString();
            var metrics = queries.GetLaneMetrics(laneKey);
            if (metrics.EnemyBaseHealthPercentage > 0f)
            {
                accum.Add(new F.Tuple<string, float>(laneKey, metrics.GetTargetValue()));
            }
            return accum;
        }, listOfTuples, lanes);
        var shuffledTuples = F.ShuffleList(tuples.ToList());
        var sorted = shuffledTuples.OrderByDescending(a => a.Second)
            .ToList();
        var index = M.SampleFromProbabilities(distribution);
        return sorted[index].First;
    }
}
