using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class Queries
{
    private Dictionary<string, BaseUnitState> _unitsById = new Dictionary<string, BaseUnitState>();
    private Dictionary<string, List<BaseUnitState>> _unitsByLane = new Dictionary<string, List<BaseUnitState>>();
    private Dictionary<string, LaneMetrics> _metricsByLane = new Dictionary<string, LaneMetrics>();
    private GameMetrics _gameMetrics = new GameMetrics();

    public Queries()
    {
    }

    public void RebuildLookups(GameState state)
    {
        var a = F.Map(i => i.ToString(), F.Range(0, BalanceConsts.Lanes));
        var b = F.Map(laneKey => new object[] {laneKey, new List<BaseUnitState>()}, a);
        var c = F.FromPairs(b);
        var d = F.CoerceDictionary<List<BaseUnitState>>(c);

        _unitsByLane = d;
        _unitsById.Clear();

        // TODO: this should probably be an F method of some kind
        var units = F.EmptyList<BaseUnitState>()
            .Concat(state.Soldiers.Cast<BaseUnitState>())
            .Concat(state.ControlPoints.Cast<BaseUnitState>())
            .Concat(state.Turrets.Cast<BaseUnitState>())
            .ToArray();
        foreach (var s in units)
        {
            _unitsByLane[s.LaneKey].Add(s);
            _unitsById.Add(s.Id, s);
        }

        // Metrics
        _metricsByLane.Clear();
        foreach (var laneKey in a)
        {
            _metricsByLane.Add(laneKey, CalculateLaneMetrics(laneKey));
        }
        _gameMetrics.Rebuild(_metricsByLane);
    }

    public List<BaseUnitState> GetUnitsByLaneKey(string laneKey, Func<BaseUnitState, bool> filter)
    {
        return _unitsByLane[laneKey]
            .Where(u => filter(u))
            .ToList();
    }

    public BaseUnitState GetUnitById(string id)
    {
        return _unitsById.ContainsKey(id) ? _unitsById[id] : null;
    }

    public List<F.Tuple<float, BaseUnitState>> GetNearestUnitTuples(NearestUnit parameters,
        Func<BaseUnitState, bool> filter)
    {
        // Try preference first
        var pos = parameters.SearchFrom;
        var maxDistance = parameters.MaxDistance;
        var laneKey = parameters.LaneKey;
        var listOfTuples = new List<F.Tuple<float, BaseUnitState>>();

        // Check against other units in lane
        var units = _unitsByLane[laneKey];
        var a = F.Reduce((accum, value) =>
        {
            if (filter(value) == false)
            {
                return accum;
            }
            var dist = Vector2.Distance(pos, value.Position);
            if (dist > maxDistance)
            {
                return accum;
            }
            accum.Add(new F.Tuple<float, BaseUnitState>(dist, value));
            return accum;
        }, listOfTuples, units);
        return a.ToList();
    }

    public BaseUnitState GetNearestUnit(NearestUnit parameters, Func<BaseUnitState, bool> filter)
    {
        var a = GetNearestUnitTuples(parameters, filter);
        var preferredId = parameters.PreferredId;
        if (preferredId != null && a.Any(t => t.Second.Id == preferredId))
        {
            return GetUnitById(preferredId);
        }
        var b = a.OrderBy(x => x.First);
        return a.Count > 0 ? b.First().Second : null;
    }

    public BaseUnitState GetNearestUnit(BaseUnitState fromUnit, float distance)
    {
        var q = new NearestUnit
        {
            LaneKey = fromUnit.LaneKey,
            MaxDistance = distance,
            PreferredId = fromUnit.TargetId,
            SearchFrom = fromUnit.Position
        };
        return GetNearestUnit(q, unit =>
        {
            return unit.Id != fromUnit.Id &&
                   unit.Side != fromUnit.Side &&
                   unit.UnitType != UnitType.ControlPoint;
        });
    }

    private LaneMetrics CalculateLaneMetrics(string laneKey)
    {
        var units = GetUnitsByLaneKey(laneKey, u => true);
        return F.Reduce((accum, value) =>
        {
            switch (value.UnitType)
            {
                case UnitType.ControlPoint:
                    accum.EnemyControlledPoints += value.Side == Side.Enemy ? 1 : 0;
                    accum.PlayerControlledPoints += value.Side == Side.Player ? 1 : 0;
                    accum.UncontrolledPoints += value.Side == Side.Neutral ? 1 : 0;
                    break;
                case UnitType.Soldier:
                    accum.EnemyUnits += value.Side == Side.Enemy ? 1 : 0;
                    accum.PlayerUnits += value.Side == Side.Player ? 1 : 0;
                    break;
                case UnitType.Turret:
                    var v = Mathf.Clamp01(value.Health / BalanceConsts.TurretHealth);
                    if (value.Side == Side.Player)
                    {
                        accum.PlayerBaseHealthPercentage = v;
                    }
                    else
                    {
                        accum.EnemyBaseHealthPercentage = v;
                    }
                    break;
            }
            return accum;
        }, new LaneMetrics(), units);
    }

    public GameMetrics GetGameMetrics()
    {
        return _gameMetrics;
    }

    public class GameMetrics
    {
        public bool PlayerHasBase;
        public bool EnemyHasBase;
        public int PlayerBaseCount;
        public int EnemyBaseCount;
        public int PlayerIncome;
        public int EnemyIncome;

        public GameMetrics()
        {
        }

        public void Rebuild(Dictionary<string, LaneMetrics> laneMetrics)
        {
            var metrics = F.Map(k => laneMetrics[k], laneMetrics.Keys);
            PlayerBaseCount = metrics.Count(m => m.PlayerBaseHealthPercentage > 0f);
            EnemyBaseCount = metrics.Count(m => m.PlayerBaseHealthPercentage > 0f);
            EnemyIncome = metrics.Sum(m => m.EnemyControlledPoints) + EnemyBaseCount;
            PlayerIncome = metrics.Sum(m => m.PlayerControlledPoints) + PlayerBaseCount;
            PlayerHasBase = PlayerBaseCount > 0;
            EnemyHasBase = EnemyBaseCount > 0;
        }
    }


    public LaneMetrics GetLaneMetrics(string laneKey)
    {
        return _metricsByLane[laneKey];
    }

    public class LaneMetrics
    {
        public int UncontrolledPoints;
        public int EnemyControlledPoints;
        public int PlayerControlledPoints;
        public int PlayerUnits;
        public int EnemyUnits;
        public float EnemyBaseHealthPercentage;
        public float PlayerBaseHealthPercentage;

        public LaneMetrics()
        {
        }

        public float GetEconomyValue()
        {
            var totalPoints = EnemyControlledPoints + PlayerControlledPoints + UncontrolledPoints;
            var c = 1f / totalPoints;
            return PlayerControlledPoints * c + UncontrolledPoints * c * 0.5f;
        }

        public float GetMilitaryValue()
        {
            var desiredEnemyUnits = PlayerUnits * 1.1f;
            if (EnemyUnits >= desiredEnemyUnits)
            {
                return EnemyUnits > 0 ? 0.0f : 0.1f;
            }
            return Mathf.Clamp01(1f - EnemyUnits / desiredEnemyUnits);
        }

        public float GetObjectiveValue()
        {
            if (PlayerBaseHealthPercentage >= 1f)
            {
                return 0.5f;
            }
            if (PlayerBaseHealthPercentage > 0)
            {
                return Mathf.Clamp01(0.5f + (1f - PlayerBaseHealthPercentage) * 0.5f);
            }
            return 0;
        }

        public float GetTargetValue()
        {
            return GetEconomyValue() + GetMilitaryValue() + GetObjectiveValue();
        }
    }

    public class NearestUnit
    {
        public Vector2 SearchFrom;
        public float MaxDistance;
        public string LaneKey;
        public string PreferredId;

        public NearestUnit()
        {
        }
    }
}