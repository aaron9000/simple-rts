using UnityEngine;
using System.Collections.Generic;

public class FactoryBehaviour : MonoBehaviour
{
    public GameObject SoldierPrefab;
    public GameObject ControlPointPrefab;
    public GameObject TurretPrefab;
    public GameObject BarrierPrefab;
    public GameObject DecalPrefab;
    public GameObject BloodParticlePrefab;
    public GameObject BulletImpactParticlePrefab;
    public GameObject ExplosionSmokeParticlePrefab;
    public GameObject ExplosionFireParticlePrefab;
    public GameObject SpawnParticlePrefab;
    public GameObject ResourceParticlePrefab;
    public GameObject LightGlowPrefab;
    public GameObject EnemyAIPrefab;

    private static FactoryBehaviour _instance;

    #region Private Instance Methods

    private void _instantiatePrefabWithNoState(GameObject prefab, Vector2 pos)
    {
        var a = Instantiate(prefab);
        var v = Map.ConvertToWorldCoordinates(pos);
        a.transform.position = new Vector3(v.x, v.y, ParticleConsts.Z);
    }

    private void _instantiatePrefabAndAssignState<TBehaviour>(GameObject prefab, BaseState state)
        where TBehaviour : MonoBehaviour
    {
        var a = Instantiate(prefab);
        var b = a.GetComponentInChildren<TBehaviour>();
        a.transform.position = new Vector3(state.Position.x, state.Position.y, 0);
        state.Id = a.gameObject.GetInstanceID().ToString();
        F.SetValue("State", state, b);
    }

    private void _spawnObjectWithState(ObjectType objectType, BaseState objectState, GameObject prefab)
    {
        switch (objectType)
        {
            // Units
            case ObjectType.UnitControlPoint:
                _instantiatePrefabAndAssignState<ControlPointBehaviour>(prefab, objectState);
                break;
            case ObjectType.UnitSoldier:
                _instantiatePrefabAndAssignState<SoldierBehaviour>(prefab, objectState);
                break;
            case ObjectType.UnitTurret:
                _instantiatePrefabAndAssignState<TurretBehaviour>(prefab, objectState);
                break;

            // Map + Scenery
            case ObjectType.MapBarrier:
                _instantiatePrefabAndAssignState<BarrierBehaviour>(prefab, objectState);
                break;

            // Misc
            case ObjectType.EnemyAI:
                _instantiatePrefabAndAssignState<EnemyAIBehaviour>(prefab, objectState);
                break;

            // Decal
            case ObjectType.DecalBlood:
                _instantiatePrefabAndAssignState<DecalBehaviour>(prefab, objectState);
                break;
            case ObjectType.DecalExplosion:
                _instantiatePrefabAndAssignState<DecalBehaviour>(prefab, objectState);
                break;
            case ObjectType.DecalBullet:
                _instantiatePrefabAndAssignState<DecalBehaviour>(prefab, objectState);
                break;

            // Lights
            case ObjectType.LightMuzzle:
                _instantiatePrefabAndAssignState<LightGlowBehaviour>(prefab, objectState);
                break;
            case ObjectType.LightExplosion:
                _instantiatePrefabAndAssignState<LightGlowBehaviour>(prefab, objectState);
                break;
            case ObjectType.LightTurretMuzzle:
                _instantiatePrefabAndAssignState<LightGlowBehaviour>(prefab, objectState);
                break;
        }
    }

    private GameObject _getPrefabForObjectType(ObjectType objectType)
    {
        switch (objectType)
        {
            // Units
            case ObjectType.UnitControlPoint:
                return ControlPointPrefab;
            case ObjectType.UnitSoldier:
                return SoldierPrefab;
            case ObjectType.UnitTurret:
                return TurretPrefab;

            // Map + Scenery
            case ObjectType.MapBarrier:
                return BarrierPrefab;

            // Misc
            case ObjectType.EnemyAI:
                return EnemyAIPrefab;

            // Decal
            case ObjectType.DecalBlood:
            case ObjectType.DecalExplosion:
            case ObjectType.DecalBullet:
                return DecalPrefab;

            // Lights
            case ObjectType.LightMuzzle:
            case ObjectType.LightExplosion:
            case ObjectType.LightTurretMuzzle:
                return LightGlowPrefab;

            // Particles
            case ObjectType.ParticleBlood:
                return BloodParticlePrefab;
            case ObjectType.ParticleExplosionSmoke:
                return ExplosionSmokeParticlePrefab;
            case ObjectType.ParticleExplosionFire:
                return ExplosionFireParticlePrefab;
            case ObjectType.ParticleBulletImpact:
                return BulletImpactParticlePrefab;
            case ObjectType.ParticleSpawn:
                return SpawnParticlePrefab;
            case ObjectType.ParticleResource:
                return ResourceParticlePrefab;

            default:
                return null;
        }
    }


    private void _spawnObject(Events.SpawnEvent e, BaseState objectState)
    {
        var objectType = e.ObjectType;
        var prefab = _instance._getPrefabForObjectType(objectType);
        if (prefab == null)
        {
            Debug.LogError("FactoryBehaviour: could not find prefab for ObjectType " + objectType);
            return;
        }

        // Spawn stateful or stateless object
        if (objectState != null)
        {
            _instance._spawnObjectWithState(e.ObjectType, objectState, prefab);
        }
        else
        {
            _instance._instantiatePrefabWithNoState(prefab, e.Position);
        }
    }

    #endregion

    #region Private Static Methods

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

    private static void _updateInternalState(Events.SpawnEvent e, BaseState objectState, GameState privateState)
    {
        // Assign a temporary Id (so unit tests play nice)
        objectState.Id = Random.Range(1, 1000000).ToString();

        switch (e.ObjectType)
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

    #endregion

    #region Public Static Methods

    public static void SpawnObject(Events.SpawnEvent e, GameState privateState)
    {
        // Add new object to internal state (if it has any)
        var objectState = _getStateForSpawnEvent(e);
        if (objectState != null)
        {
            _updateInternalState(e, objectState, privateState);
        }

        // Instantiate prefabs and add them to the scene (requires instance living in scene)
        if (_instance == null)
        {
            Debug.LogWarning("FactoryBehaviour: SpawnObject: make sure a FactoryBehaviour.cs script lives in the scene");
        }
        else
        {
            _instance._spawnObject(e, objectState);
        }
    }

    #endregion

    #region Unity Lifecycle

    void Awake()
    {
        _instance = this;
    }

    #endregion
}