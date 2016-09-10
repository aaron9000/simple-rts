using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class BaseEvent
{
    public string SourceId;
    public float EventFireDelay;

    public BaseEvent()
    {
    }

    public virtual void Fire(GameState state, Queries queries)
    {
        Debug.LogError("event not handled: " + GetType().Name);
    }
}

public class Events
{
    public class PlaySoundEvent : BaseEvent
    {
        public SoundType SoundType;

        public PlaySoundEvent()
        {
        }

        public PlaySoundEvent(SoundType soundType)
        {
            SoundType = soundType;
        }

        public PlaySoundEvent(SoundType soundType, float fireDelay)
        {
            SoundType = soundType;
            EventFireDelay = fireDelay;
        }

        public override void Fire(GameState state, Queries queries)
        {
            AudioPlayerBehaviour.PlaySound(SoundType);
        }
    }

    public class GameStartEvent : BaseEvent
    {
        public GameStartEvent()
        {
        }


        public override void Fire(GameState state, Queries queries)
        {
            HUDBehaviour.ShowMessage("Hold the points!");
        }
    }

    public class CheckGameEndEvent : BaseEvent
    {
        public CheckGameEndEvent()
        {
            EventFireDelay = 6.0f;
        }


        public override void Fire(GameState state, Queries queries)
        {
            if (state.Winner != Side.Neutral)
            {
                Application.LoadLevel(Scene.Menu);
            }
        }
    }

    public class SoldierAttackEvent : BaseEvent
    {
        public float Damage;
        public string TargetId;

        public SoldierAttackEvent(string sourceId, string targetId, float damage)
        {
            SourceId = sourceId;
            Damage = damage;
            TargetId = targetId;
        }

        public override void Fire(GameState state, Queries queries)
        {
            var source = queries.GetUnitById(SourceId);
            var target = queries.GetUnitById(TargetId);
            if (source != null && target != null)
            {
                // Muzzle
                var pos = Vector2.MoveTowards(source.Position, target.Position, PhysicsConsts.SoldierMuzzleLength);
                var m = new SpawnEvent(pos, ObjectType.LightMuzzle);
                FactoryBehaviour.SpawnObject(m, state);

                // Hurt target
                var impactPos = target.Position + M.NormalizedRadialSpread() * PhysicsConsts.SoldierMuzzleLength;
                var e = new SpawnEvent(impactPos, ObjectType.ParticleBulletImpact);
                FactoryBehaviour.SpawnObject(e, state);
                target.Health -= Damage;

                AudioPlayerBehaviour.PlaySound(SoundType.Shoot);
            }
        }
    }

    public class ExplosionEvent : BaseEvent
    {
        public float Damage;
        public string LaneKey;
        public Vector2 Position;
        public float SplashRadius;
        public float Scale;
        public Side Side;


        public ExplosionEvent(Vector2 pos, float damage, float radius, float scale, string laneKey, Side side)
        {
            Position = pos;
            Damage = damage;
            LaneKey = laneKey;
            SplashRadius = radius;
            Scale = scale;
            Side = side;
        }

        public override void Fire(GameState state, Queries queries)
        {
            // Splash damage
            var q = new Queries.NearestUnit
            {
                SearchFrom = Position,
                LaneKey = LaneKey,
                MaxDistance = SplashRadius
            };
            var a = queries.GetNearestUnitTuples(q, s => s.Side != Side);
            var spawns = new List<SpawnEvent>();
            F.Reduce((accum, value) =>
            {
                var dist = value.First;
                var unit = value.Second;
                var ratio = 1.0f - dist / SplashRadius;
                unit.Health -= ratio * Damage;
                return accum;
            }, spawns, a);


            // Explosion effect
            spawns.Add(new SpawnEvent(Position, ObjectType.DecalExplosion));
            spawns.Add(new SpawnEvent(Position, ObjectType.LightExplosion));
            spawns.Add(new SpawnEvent(Position, ObjectType.ParticleExplosionSmoke));
            spawns.Add(new SpawnEvent(Position, ObjectType.ParticleExplosionFire));

            // Spawn all the things
            foreach (var spawn in spawns)
            {
                FactoryBehaviour.SpawnObject(spawn, state);
            }
        }
    }

    public class TurretAttackEvent : BaseEvent
    {
        public float Damage;
        public string TargetId;

        public TurretAttackEvent(string sourceId, string targetId, float damage)
        {
            SourceId = sourceId;
            Damage = damage;
            TargetId = targetId;
        }

        public override void Fire(GameState state, Queries queries)
        {
            var source = queries.GetUnitById(SourceId);
            var target = queries.GetUnitById(TargetId);
            if (source != null && target != null)
            {
                // Muzzle
                var muzzlePos = Vector2.MoveTowards(source.Position, target.Position, PhysicsConsts.TurretMuzzleLength);
                var m = new SpawnEvent(muzzlePos, ObjectType.LightTurretMuzzle);
                FactoryBehaviour.SpawnObject(m, state);
                AudioPlayerBehaviour.PlaySound(SoundType.TurretShoot);

                // Hurt target
                target.Health -= Damage;
            }
        }
    }

    public class SoldierDieEvent : BaseEvent
    {
        public Vector2 Position;

        public SoldierDieEvent(string sourceId, Vector2 pos)
        {
            SourceId = sourceId;
            Position = pos;
        }

        public override void Fire(GameState state, Queries queries)
        {
            var decal = new SpawnEvent(Position, ObjectType.DecalBlood);
            var blood = new SpawnEvent(Position, ObjectType.ParticleBlood);

            FactoryBehaviour.SpawnObject(decal, state);
            FactoryBehaviour.SpawnObject(blood, state);

            AudioPlayerBehaviour.PlaySound(SoundType.Splat);
        }
    }

    public class TurretDieEvent : BaseEvent
    {
        public TurretDieEvent(string sourceId)
        {
            SourceId = sourceId;
        }

        public override void Fire(GameState state, Queries queries)
        {
            var turret = queries.GetUnitById(SourceId);
            var pos = turret.Position;
            var decal = new SpawnEvent(pos, ObjectType.DecalExplosion);
            var blood = new SpawnEvent(pos, ObjectType.ParticleBlood);

            FactoryBehaviour.SpawnObject(decal, state);
            FactoryBehaviour.SpawnObject(blood, state);

            var message = String.Format("{0} base destroyed!", turret.Side == Side.Player ? "Player" : "Enemy");
            HUDBehaviour.ShowMessage(message);

            var lanes = F.Range(0, BalanceConsts.Lanes);
            var metrics = F.Map(i => queries.GetLaneMetrics(i.ToString()), lanes);
            var playerHasBase = metrics.Any(m => m.PlayerBaseHealthPercentage > 0f);
            var enemyHasBase = metrics.Any(m => m.EnemyBaseHealthPercentage > 0f);
            if (!playerHasBase)
            {
                state.Winner = Side.Enemy;
                HUDBehaviour.ShowLoseMessage("The enemy has won!");
                AudioPlayerBehaviour.PlaySound(SoundType.Defeat);
            }
            else if (!enemyHasBase)
            {
                state.Winner = Side.Player;
                HUDBehaviour.ShowWinMessage("You have won!");
                AudioPlayerBehaviour.PlaySound(SoundType.Victory);
            }
        }
    }

    public class ProduceResourceEvent : BaseEvent
    {
        public ProduceResourceEvent(string sourceId)
        {
            SourceId = sourceId;
        }

        public override void Fire(GameState state, Queries queries)
        {
            var point = queries.GetUnitById(SourceId);
            if (point == null)
            {
                Debug.Log("cant find unit: " + SourceId);
                return;
            }
            if (point.Side == Side.Player)
            {
                state.PlayerResources++;
                HUDBehaviour.AnimateMoneyCounter();
            }
            else if (point.Side == Side.Enemy)
            {
                state.EnemyResources++;
            }
        }
    }

    public class PurchaseEvent : BaseEvent
    {
        public Side Side;
        public int LaneIndex;

        public PurchaseEvent(Side side, int laneIndex)
        {
            Side = side;
            LaneIndex = laneIndex;
        }

        public override void Fire(GameState state, Queries queries)
        {
            // Decrement resources
            if (Side == Side.Player)
            {
                state.PlayerResources--;
            }
            else if (Side == Side.Enemy)
            {
                state.EnemyResources--;
            }

            // Spawn effect
            var p = Map.GetSoldierSpawnPosition(LaneIndex, Side);
            var e = new SpawnEvent(p, ObjectType.ParticleSpawn);
            FactoryBehaviour.SpawnObject(e, state);
            AudioPlayerBehaviour.PlaySound(SoundType.Spawn);

            // Spawn a unit
            var u = new SpawnEvent(p, Random.value * 360f, Side, LaneIndex.ToString(), ObjectType.UnitSoldier);
            FactoryBehaviour.SpawnObject(u, state);
        }
    }


    public class SpawnEvent : BaseEvent
    {
        public ObjectType ObjectType;
        public string LaneKey;
        public Side Side;
        public Vector2 Position;
        public float Angle;

        public SpawnEvent(Vector2 pos, ObjectType objectType)
        {
            ObjectType = objectType;
            Position = pos;
            Angle = Random.value * 360f;
        }

        public SpawnEvent(Vector2 pos, float angle, Side side, string laneKey, ObjectType objectType)
        {
            ObjectType = objectType;
            Side = side;
            LaneKey = laneKey;
            Position = pos;
            Angle = angle;
        }

        public override void Fire(GameState state, Queries queries)
        {
            FactoryBehaviour.SpawnObject(this, state);
        }
    }
}