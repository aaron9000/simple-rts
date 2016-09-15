using UnityEngine;
using System.Collections.Generic;

public class BaseState
{
    public Vector2 Position = Vector2.zero;
    public float Angle;
    public string LaneKey = "0";
    public Side Side = Side.Neutral;
    public string Id;

    public BaseState()
    {
    }

    public BaseState(Events.SpawnEvent e)
    {
        Position = e.Position;
        Side = e.Side;
        LaneKey = e.LaneKey;
        Angle = e.Angle;
    }
}

public class BarrierState : BaseState
{
    public BarrierState()
    {
    }

    public BarrierState(Events.SpawnEvent e) : base(e)
    {
    }
}

public class DecalState : BaseState
{
    public DecalType DecalType = DecalType.Blood;

    public DecalState()
    {
    }

    public DecalState(Events.SpawnEvent e, DecalType type) : base(e)
    {
        DecalType = type;
    }
}

public class LightGlowState : BaseState
{
    public LightGlowType LightGlowType;

    public LightGlowState()
    {
    }

    public LightGlowState(Events.SpawnEvent e, LightGlowType type) : base(e)
    {
        LightGlowType = type;
    }
}

public class BaseUnitState : BaseState
{
    public float ShootCooldown = 0;
    public string TargetId = null;
    public float Health;
    public UnitType UnitType;

    public BaseUnitState()
    {
    }

    public BaseUnitState(Events.SpawnEvent e) : base(e)
    {
        Angle = e.Angle;
    }
}

public class ControlPointState : BaseUnitState
{
    public float ProductionCooldown;

    public ControlPointState()
    {
        UnitType = UnitType.ControlPoint;
    }

    public ControlPointState(Events.SpawnEvent e) : base(e)
    {
        UnitType = UnitType.ControlPoint;
    }
}

public class SoldierState : BaseUnitState
{
    public SoldierState()
    {
        Health = BalanceConsts.SoldierHealth;
        UnitType = UnitType.Soldier;
    }

    public SoldierState(Events.SpawnEvent e) : base(e)
    {
        Health = BalanceConsts.SoldierHealth;
        UnitType = UnitType.Soldier;
    }
}

public class EnemyAIState : BaseState
{
    public float SpawnCooldown = 3.0f;

    public EnemyAIState()
    {
    }
}

public class TurretState : BaseUnitState
{
    public float ProductionCooldown;

    public TurretState()
    {
        Health = BalanceConsts.TurretHealth;
        UnitType = UnitType.Turret;
        ProductionCooldown = BalanceConsts.ControlPointProductionCooldown * (0.8f + 0.2f * Random.value);
    }

    public TurretState(Events.SpawnEvent e) : base(e)
    {
        Health = BalanceConsts.TurretHealth;
        UnitType = UnitType.Turret;
        ProductionCooldown = BalanceConsts.ControlPointProductionCooldown * (0.8f + 0.2f * Random.value);
    }
}


public class GameState
{
    public List<SoldierState> Soldiers = new List<SoldierState>();
    public List<ControlPointState> ControlPoints = new List<ControlPointState>();
    public List<TurretState> Turrets = new List<TurretState>();
    public EnemyAIState EnemyAI;
    public Side Winner = Side.Neutral;
    public Difficulty Difficulty = Difficulty.Easy;
    public int PlayerResources = BalanceConsts.StartingResources;
    public int EnemyResources = BalanceConsts.StartingResources;


    public GameState()
    {
    }

    public GameState(Difficulty d)
    {
        Difficulty = d;
    }

    // TODO: wish there was a deep clone
    public GameState Clone()
    {
        return new GameState
        {
            Winner = Winner,
            Difficulty = Difficulty,
            PlayerResources = PlayerResources,
            EnemyResources = EnemyResources,
            EnemyAI = F.ShallowCloneObject(EnemyAI),
            Soldiers = F.DeepCloneObjectCollection<SoldierState, List<SoldierState>>(Soldiers),
            ControlPoints = F.DeepCloneObjectCollection<ControlPointState, List<ControlPointState>>(ControlPoints),
            Turrets = F.DeepCloneObjectCollection<TurretState, List<TurretState>>(Turrets)
        };
    }
}