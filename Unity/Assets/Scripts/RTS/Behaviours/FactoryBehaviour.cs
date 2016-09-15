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

    private void _instantiatePrefabWithNoState(Vector3 pos, GameObject prefab)
    {
        var a = Instantiate(prefab);
        var v = Map.ConvertToWorldCoordinates(pos);
        a.transform.position = new Vector3(v.x, v.y, pos.z);
    }

    private void _instantiatePrefabAndAssignState<TBehaviour>(BaseState state, GameObject prefab)
        where TBehaviour : MonoBehaviour
    {
        var a = Instantiate(prefab);
        var b = a.GetComponentInChildren<TBehaviour>();
        a.transform.position = new Vector3(state.Position.x, state.Position.y, 0);
        state.Id = a.gameObject.GetInstanceID().ToString();
        F.SetValue("State", state, b);
    }

    private void _instantiatePrefabWithState(ObjectType objectType, BaseState objectState, GameObject prefab)
    {
        switch (objectType)
        {
            // Units
            case ObjectType.UnitControlPoint:
                _instantiatePrefabAndAssignState<ControlPointBehaviour>(objectState, prefab);
                break;
            case ObjectType.UnitSoldier:
                _instantiatePrefabAndAssignState<SoldierBehaviour>(objectState, prefab);
                break;
            case ObjectType.UnitTurret:
                _instantiatePrefabAndAssignState<TurretBehaviour>(objectState, prefab);
                break;

            // Map + Scenery
            case ObjectType.MapBarrier:
                _instantiatePrefabAndAssignState<BarrierBehaviour>(objectState, prefab);
                break;

            // Misc
            case ObjectType.EnemyAI:
                _instantiatePrefabAndAssignState<EnemyAIBehaviour>(objectState, prefab);
                break;

            // Decal
            case ObjectType.DecalBlood:
                _instantiatePrefabAndAssignState<DecalBehaviour>(objectState, prefab);
                break;
            case ObjectType.DecalExplosion:
                _instantiatePrefabAndAssignState<DecalBehaviour>(objectState, prefab);
                break;
            case ObjectType.DecalBullet:
                _instantiatePrefabAndAssignState<DecalBehaviour>(objectState, prefab);
                break;

            // Lights
            case ObjectType.LightMuzzle:
                _instantiatePrefabAndAssignState<LightGlowBehaviour>(objectState, prefab);
                break;
            case ObjectType.LightExplosion:
                _instantiatePrefabAndAssignState<LightGlowBehaviour>(objectState, prefab);
                break;
            case ObjectType.LightTurretMuzzle:
                _instantiatePrefabAndAssignState<LightGlowBehaviour>(objectState, prefab);
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


    private void _spawnPrefab(Vector3 position, ObjectType objectType, BaseState objectState)
    {
        var prefab = _instance._getPrefabForObjectType(objectType);
        if (prefab == null)
        {
            Debug.LogError("FactoryBehaviour: could not find prefab for ObjectType " + objectType);
            return;
        }

        // Spawn stateful or stateless object
        if (objectState != null)
        {
            _instance._instantiatePrefabWithState(objectType, objectState, prefab);
        }
        else
        {
            _instance._instantiatePrefabWithNoState(position, prefab);
        }
    }

    #endregion


    #region Public Static Methods

    public static void SpawnPrefab(Vector3 position, ObjectType type, BaseState objectState)
    {
        // Instantiate prefabs and add them to the scene (requires instance living in scene)
        if (_instance == null)
        {
            Debug.LogWarning("FactoryBehaviour: SpawnObject: make sure a FactoryBehaviour.cs script lives in the scene");
        }
        else
        {
            _instance._spawnPrefab(position, type, objectState);
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