using UnityEngine;
using System.Collections;

public static class Factory {

    private static BaseState _getStateForSpawnEvent(Events.SpawnEvent e)
    {
        switch (e.ObjectType)
        {
            // Units
            case ObjectType.UnitControlPoint:
                return new ControlPointState(e);
            case ObjectType.UnitSoldier:
                return new SoldierState(e);
            case ObjectType.UnitTurret:
                return new TurretState(e);

            // Map + Scenery
            case ObjectType.MapBarrier:
                return new BarrierState(e);

            // Misc
            case ObjectType.EnemyAI:
                return new EnemyAIState();

            // Decal
            case ObjectType.DecalBlood:
                return new DecalState(e, DecalType.Blood);
            case ObjectType.DecalExplosion:
                return new DecalState(e, DecalType.Scorch);
            case ObjectType.DecalBullet:
                return new DecalState(e, DecalType.Bullet);

            // Lights
            case ObjectType.LightMuzzle:
                return new LightGlowState(e, LightGlowType.Muzzle);
            case ObjectType.LightExplosion:
                return new LightGlowState(e, LightGlowType.Explosion);
            case ObjectType.LightTurretMuzzle:
                return new LightGlowState(e, LightGlowType.TurretMuzzle);

            // Particles
            default:
                return null;
        }
    }

    private static void _updateInternalState(ObjectType objectType, BaseState objectState, GameState privateState)
    {
        if (objectState == null)
            return;

        // Assign a temporary Id (so unit tests play nice)
        objectState.Id = Random.Range(1, 1000000).ToString();

        switch (objectType)
        {
            case ObjectType.UnitControlPoint:
                privateState.ControlPoints.Add((ControlPointState) objectState);
                break;
            case ObjectType.UnitTurret:
                privateState.Turrets.Add((TurretState) objectState);
                break;
            case ObjectType.UnitSoldier:
                privateState.Soldiers.Add((SoldierState) objectState);
                break;
            case ObjectType.EnemyAI:
                privateState.EnemyAI = (EnemyAIState) objectState;
                break;
        }
    }

    public static void Spawn(Events.SpawnEvent e, GameState privateState)
    {
        var objectState = _getStateForSpawnEvent(e);
        var position = M.SetZ(e.Position, ParticleConsts.Z);
        _updateInternalState(e.ObjectType, objectState, privateState);
        FactoryBehaviour.SpawnPrefab(position, e.ObjectType, objectState);
    }
}
