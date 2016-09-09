using UnityEngine;
using System.Collections;

public enum Side
{
    Enemy,
    Player,
    Neutral
}

public enum Difficulty
{
    VeryEasy,
    Easy,
    Medium,
    Hard
}


public enum LightGlowType
{
    Muzzle,
    TurretMuzzle,
    Explosion
}

public enum DecalType
{
    Blood,
    Scorch,
    Bullet
}

public enum UnitType
{
    Soldier,
    ControlPoint,
    Turret
}

public enum ObjectType
{
    MapBarrier,

    UnitSoldier,
    UnitControlPoint,
    UnitTurret,

    DecalBlood,
    DecalBullet,
    DecalExplosion,

    ParticleBlood,
    ParticleExplosionSmoke,
    ParticleExplosionFire,
    ParticleBulletImpact,
    ParticleSpawn,
    ParticleResource,

    LightMuzzle,
    LightTurretMuzzle,
    LightExplosion,

    EnemyAI
}


public enum SoundType
{
    Shoot,
    Splat,
    Victory,
    Defeat,
    Capture,
    LoseCapture,
    GainResource,
    Spawn,
    PurchaseUpgrade,
    TurretShoot
}

public class Scene
{
    public static string Game = "GameScene";
    public static string Menu = "MenuScene";
}

public class LayerKey
{
    public static string BackgroundFar = "BackgroundFar";
    public static string BackgroundNear = "BackgroundNear";
    public static string Decals = "Decals";
    public static string Default = "Default";
    public static string Effects = "Effects";
    public static string Foreground = "Foreground";
    public static string Menu = "Menu";
}

public class ObjectTag
{
    public const string Soldier = "Soldier";
    public const string ControlPoint = "ControlPoint";
    public const string Turret = "Turret";
}

public class ScreenConsts
{
    public const float WidthInPixels = 1080;
    public const float HeightInPixels = 1920;
}

public class PhysicsConsts
{
    public const float SoldierRadius = 32.0f;
    public const float SoldierMoveSpeed = 65.0f;
    public const float SoldierMuzzleLength = 32 * 1.4f;

    public const float BarrierPadding = 30.0f;

    public const float TurretMuzzleLength = 140.0f;
}

public class ParticleConsts
{
    public const float Z = -0.1f;
}

public class BalanceConsts
{
    public const int Lanes = 3;
    public const int StartingResources = 12;

    public const float SoldierDamage = 1;
    public const float SoldierAttackCooldown = 0.4f;
    public const float SoldierAttackDistance = 240.0f;
    public const float SoldierHealth = 10;
    public const float SoldierRotateSpeed = 90f;

    public const float TurretDamage = 5;
    public const float TurretSplashDamage = 5;
    public const float TurretSplashRadius = 180f;
    public const float TurretAttackCooldown = 2.0f;
    public const float TurretAttackDistance = 500.0f;
    public const float TurretHealth = 50;
    public const float TurretRotateSpeed = 40f;

    public const float ControlPointCaptureDistance = 22f;
    public const float ControlPointProductionCooldown = 6.0f;
}