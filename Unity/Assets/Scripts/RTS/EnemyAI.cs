using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class EnemyAI
{

    public static int GetChoicesForDifficulty(Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Difficulty.VeryEasy:
            case Difficulty.Easy:
                return 3;
            case Difficulty.Medium:
                return 2;
            default:
                return 1;
        }
    }

    public static float GetCooldownForDifficulty(Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Difficulty.VeryEasy:
                return 1.25f;
            case Difficulty.Easy:
                return 0.75f;
            case Difficulty.Medium:
                return 0.55f;
            default:
                return 0.35f;
        }
    }

    public static string GetSpawnLaneKey(GameState state, Queries queries)
    {
        var lanes = F.Range(0, BalanceConsts.Lanes);
        var listOfTuples = new List<F.Tuple<string, float>>();
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
        var choices = sorted.Take(GetChoicesForDifficulty(state.Difficulty))
            .ToList();
        return F.ShuffleList(choices)
            .First()
            .First;
    }
}
