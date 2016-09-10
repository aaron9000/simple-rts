using UnityEngine;
using System.Collections;

public class SoldierBehaviour : MonoBehaviour
{
    // State
    public SoldierState State;

    // Editor connections
    public SpriteRenderer SpriteRenderer;
    public Sprite RedSprite;
    public Sprite GreenSprite;

    // Transient state
    private float _effectiveness;
    private float _animAngle;

    void Start()
    {
        _effectiveness = 0.80f + Random.value * 0.2f;
        _animAngle = Random.value * Mathf.PI;
        SpriteRenderer.sprite = State.Side == Side.Player ? GreenSprite : RedSprite;
    }

    private F.Tuple<BaseUnitState, float> _getTargetingData()
    {
        var target = Game.Queries.GetNearestUnit(State, BalanceConsts.SoldierAttackDistance);
        if (target == null)
        {
            return null;
        }
        var vectorToTarget = target.Position - State.Position;
        var angleToTarget = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
        return new F.Tuple<BaseUnitState, float>(target, angleToTarget);
    }

    private void _die()
    {
        Game.PushEvent(new Events.SoldierDieEvent(State.Id, State.Position));
        Destroy(gameObject);
    }

    private void _shootTarget(BaseUnitState target)
    {
        var shootEvent = new Events.SoldierAttackEvent(State.Id, target.Id, BalanceConsts.SoldierDamage);
        Game.PushEvent(shootEvent);
        State.ShootCooldown = BalanceConsts.SoldierAttackCooldown * _effectiveness;
        State.TargetId = target.Id;
    }

    private void _move()
    {
        var moveSpeed = PhysicsConsts.SoldierMoveSpeed * _effectiveness;
        var a = State.Angle * Mathf.PI / 180f;
        State.Position += new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * moveSpeed * Time.deltaTime;
        _animAngle += Time.deltaTime * 6.5f * _effectiveness;

        if (State.Position.y < -100.0f || State.Position.y > ScreenConsts.HeightInPixels + 100.0f)
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // Cooldowns
        State.ShootCooldown -= Time.deltaTime;

        // Rotate
        var rotateSpeed = BalanceConsts.SoldierRotateSpeed * _effectiveness;
        var targetingData = _getTargetingData();
        var moveAngle = State.Side == Side.Player ? 90 : 270;
        var desiredAngle = targetingData == null ? moveAngle : targetingData.Second;
        State.Angle = M.RotateTowardsTarget(State.Angle, desiredAngle, rotateSpeed, Time.deltaTime);
        transform.rotation = Quaternion.Euler(0, 0, State.Angle + Mathf.Sin(_animAngle) * 3.6f);
        transform.position = Map.ConvertToWorldCoordinates(State.Position);

        // Shoot or move
        if (targetingData != null)
        {
            var deltaAngle = Mathf.DeltaAngle(desiredAngle, State.Angle);
            var isAimed = Mathf.Abs(deltaAngle) < 5.0f;
            if (State.ShootCooldown <= 0 && isAimed)
            {
                _shootTarget(targetingData.First);
            }
        }
        else
        {
            _move();
        }

        // Death
        if (State.Health < 0)
        {
            _die();
        }
    }
}