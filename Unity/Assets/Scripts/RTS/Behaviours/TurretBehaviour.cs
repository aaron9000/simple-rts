using System;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class TurretBehaviour : MonoBehaviour
{
    public TurretState State;
    public GameObject Turret;
    public TurretGlowBehaviour TurretGlow;

    private float _restAngle;

    // Use this for initialization
    void Start()
    {
        TurretGlow.ChangeSide(State.Side);
    }

    private F.Tuple<BaseUnitState, float> _getTargetingData()
    {
        var target = Game.Queries.GetNearestUnit(State, BalanceConsts.TurretAttackDistance);
        if (target == null)
        {
            return null;
        }
        var vectorToTarget = target.Position - State.Position;
        var angleToTarget = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
        return new F.Tuple<BaseUnitState, float>(target, angleToTarget);
    }

    private float _getRestAngle()
    {
        var startAngle = Map.GetFaceAngle(State.Side);
        if (Random.value < 0.1 * Time.deltaTime)
        {
            _restAngle = Random.value * 60f - 30f;
        }
        return startAngle + _restAngle;
    }

    private void _shootTarget(BaseUnitState target)
    {
        var shootEvent = new Events.TurretAttackEvent(State.Id, target.Id, BalanceConsts.TurretDamage);
        Game.PushEvent(shootEvent);

        var pos = target.Position + M.NormalizedRadialSpread() * PhysicsConsts.SoldierRadius * 1.5f;
        var explosion = new Events.ExplosionEvent(pos, BalanceConsts.TurretSplashDamage,
            BalanceConsts.TurretSplashRadius, 1.0f, State.LaneKey, State.Side);
        Game.PushEvent(explosion);
        State.ShootCooldown = BalanceConsts.TurretAttackCooldown;
        State.TargetId = target.Id;
    }

    private void _die()
    {
        Game.PushEvent(new Events.TurretDieEvent(State.Id));
        Destroy(gameObject);

        var offset = State.Side == Side.Player ? 80.0f : -80f;
        F.Map(v =>
        {
            var pos = State.Position + M.NormalizedRadialSpread() * 60.0f + Vector2.up * v * offset;
            var explosion = new Events.ExplosionEvent(pos, BalanceConsts.TurretSplashDamage,
                BalanceConsts.TurretSplashRadius, 1.0f, State.LaneKey, State.Side);
            var fireDelay = v * 0.3f;
            explosion.EventFireDelay = fireDelay;
            Game.PushEvent(explosion);
            var sound = new Events.PlaySoundEvent(SoundType.TurretShoot, fireDelay);
            Game.PushEvent(sound);
            Game.PushEvent(new Events.CheckGameEndEvent());
            return 0;
        }, F.Range(0, 3));
    }

    private void _produceResources()
    {
        State.ProductionCooldown -= Time.deltaTime;
        if (State.ProductionCooldown <= 0f)
        {
            State.ProductionCooldown = BalanceConsts.ControlPointProductionCooldown;
            Game.PushEvent(new Events.ProduceResourceEvent(State.Id));

            if (State.Side == Side.Player)
            {
                var laneIndex = Int32.Parse(State.LaneKey);
                var e = new Events.SpawnEvent(Map.GetSoldierSpawnPosition(laneIndex, State.Side),
                    ObjectType.ParticleResource);
                Game.PushEvent(e);
            }
        }
    }

    void Update()
    {
        // Cooldowns
        State.ShootCooldown -= Time.deltaTime;

        // Produce money
        _produceResources();

        // Update rotation and position
        var rotateSpeed = BalanceConsts.TurretRotateSpeed;
        var targetingData = _getTargetingData();
        var desiredAngle = targetingData == null ? _getRestAngle() : targetingData.Second;
        State.Angle = M.RotateTowardsTarget(State.Angle, desiredAngle, rotateSpeed, Time.deltaTime);
        Turret.transform.rotation = Quaternion.Euler(0, 0, State.Angle);
        transform.transform.rotation = Quaternion.Euler(0, 0, Map.GetFaceAngle(State.Side));
        transform.position = Map.ConvertToWorldCoordinates(State.Position);

        // Shoot
        if (targetingData != null)
        {
            var deltaAngle = Mathf.DeltaAngle(desiredAngle, State.Angle);
            var isAimed = Mathf.Abs(deltaAngle) < 5.0f;
            if (State.ShootCooldown <= 0 && isAimed)
            {
                _shootTarget(targetingData.First);
            }
        }

        // Death
        if (State.Health < 0)
        {
            _die();
        }
    }
}