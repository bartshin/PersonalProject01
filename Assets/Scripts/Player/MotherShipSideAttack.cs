using System;
using System.Collections.Generic;
using UnityEngine;
using Architecture;

public class MotherShipSideAttack 
{
  const float MUZZLE_FLASH_LIFETIME = 0.5f;
  const float MISSILE_EXPLOSION_LIFETIME = 0.5f;
  static readonly Vector3 MISSILE_EXPLOSION_SCALE = new Vector3(5f, 5f, 5f);
  static readonly System.Random VOLUME_RAND;
  static readonly (int min, int max)  MISSILE_LAUNCH_VOLUME = (40, 60);
  static readonly (int min, int max)  MISSILE_HIT_VOLUME = (40, 60);
  static readonly (int min, int max)  BULLET_FIRE_VOLUME = (70, 80);
  static readonly (int min, int max)  BULLET_HIT_VOLUME = (40, 60);
  static readonly Vector3 MUZZLE_FLASH_SCALE = new Vector3(1.5f, 1.5f, 1.5f);
  static readonly Vector3 MUZZLE_OFFSET = new Vector3(0, -0.5f, 0);
  static readonly AudioClip bulletFireSound;
  static readonly AudioClip bulletHitSound;
  static readonly AudioClip missileLaunchSound;
  static readonly AudioClip missileHitSound;
  static MotherShipSideAttack()
  {
    MotherShipSideAttack.VOLUME_RAND = new();
    MotherShipSideAttack.bulletFireSound = Resources.Load<AudioClip>("Audio/soft_laser_blast");
    MotherShipSideAttack.missileLaunchSound = Resources.Load<AudioClip>("Audio/missile_firing");
    MotherShipSideAttack.bulletHitSound = Resources.Load<AudioClip>("Audio/bullet_hit");
    MotherShipSideAttack.missileHitSound = Resources.Load<AudioClip>("Audio/missile_hit");
  }
  public struct Configs
  {
    public float BulletDelay;
    public float BulletSpeed; 
    public int BulletPower; 
    public float BulletLifeTime; 
    public float MissileDelay;
    public float MissileInitialSpeed; 
    public float MissileAcceleration; 
    public int MissilePower; 
    public float MissileLifeTime; 
  }

  public Configs configs;
  public Quaternion AimDirection;
  public bool IsActive
  {
    get => this.isActive;
    set {
      this.isActive = value;
      if (value) {
        CameraManager.Shared.SetSideViewDir(this.AimDirection);
      }
    }
  }
  public Direction AttackDirection;
  bool isActive;
  GameObject ship;
  (float bullet, float missile) remainingDelay;
  (float xAngle, float yDot) maxRotation = (30f, 0.7f);
  float rotationClampAngle = 45f;
  MonoBehaviourPool<PlayerBullet> bulletPool;
  MonoBehaviourPool<Missile> missilePool;
  MonoBehaviourPool<SimplePooledObject> missileExplosionPool;
  MonoBehaviourPool<SimplePooledObject> muzzleFlashPool;

  float fireDist = 5f;

  public MotherShipSideAttack(
      GameObject ship,
      GameObject bulletPrefab,
      GameObject muzzleFlashPrefab,
      GameObject missilePrefab,
      GameObject missileExplosion,
      Configs configs
      )
  { 
    this.ship = ship;
    this.configs = configs;
    this.bulletPool = new (
      poolSize: 30,
      maxPoolSize: 100,
      prefab: bulletPrefab
    );
    this.missilePool = new (
      poolSize: 20,
      maxPoolSize: 50,
      prefab: missilePrefab
    );
    this.missileExplosionPool = new (
      poolSize: 20,
      maxPoolSize: 50,
      prefab: missileExplosion
    );

    this.muzzleFlashPool = new (
      poolSize: 10,
      maxPoolSize: 30,
      prefab: muzzleFlashPrefab
    );
  }

  // Update is called once per frame
  public void Update(float deltaTime)
  {
    this.UpdateDelay(deltaTime);
    if (this.IsActive) {
      this.ClampAim(deltaTime);
      this.RotateAim(deltaTime);
      if (UserInputManager.Shared.MainOperation.HasRegistered &&
          this.remainingDelay.bullet <= 0) {
        UserInputManager.Shared.MainOperation.HasTriggered = true;
        this.FireBullet(); 
        this.remainingDelay = (this.configs.BulletDelay, this.remainingDelay.missile);
      }
      if (UserInputManager.Shared.SubOperation.HasRegistered &&
          this.remainingDelay.missile <= 0) {
        UserInputManager.Shared.SubOperation.HasRegistered = true;
        this.FireMissile();
        this.remainingDelay = (this.remainingDelay.bullet, this.configs.MissileDelay);
      }
    }
  }

  void UpdateDelay(float deltaTime)
  {
    var (bullet, missile) = this.remainingDelay;
    this.remainingDelay = (bullet - deltaTime, missile - deltaTime);
  }

  void FireMissile()
  {
    var dir = this.AimDirection * Vector3.forward;
    var projectile = this.missilePool.Get();
    projectile.transform.position = this.ship.transform.position + this.fireDist * dir;
    projectile.InitialSpeed = this.configs.MissileInitialSpeed;
    projectile.Damage = this.configs.BulletPower;
    projectile.Direction = dir;
    projectile.LifeTime = this.configs.MissileLifeTime;
    projectile.Acceleration = this.configs.MissileAcceleration;
    projectile.FiredShip = this.ship;
    projectile.OnHit = this.OnMissileHit;
    var volume = MotherShipSideAttack.VOLUME_RAND.Next(
      MotherShipSideAttack.MISSILE_LAUNCH_VOLUME.min,
      MotherShipSideAttack.MISSILE_LAUNCH_VOLUME.max
    );
    var sfx = AudioManager.Shared.GetSfxController();
    sfx.PlaySound(
      MotherShipSideAttack.missileLaunchSound, 
      this.ship.transform.position,
      MotherShipSideAttack.MISSILE_LAUNCH_VOLUME);
  }

  void FireBullet()
  {
    var dir = this.AimDirection * Vector3.forward;
    var projectile = this.bulletPool.Get();
    projectile.transform.SetPositionAndRotation(
     this.ship.transform.position + this.fireDist * dir,
     this.AimDirection);
    projectile.InitialSpeed = this.configs.BulletSpeed;
    projectile.Damage = this.configs.BulletPower;
    projectile.Direction = this.AimDirection * Vector3.forward;
    projectile.LifeTime = this.configs.BulletLifeTime;
    projectile.FiredShip = this.ship;
    projectile.OnHit = this.OnBulletHit;
    var muzzleFlash = this.muzzleFlashPool.Get();
    muzzleFlash.transform.position = this.ship.transform.position + MotherShipSideAttack.MUZZLE_OFFSET + (this.fireDist + 2f) * dir;
    muzzleFlash.transform.forward = Vector3.Lerp(
          this.ship.transform.up,
          dir,
          0.65f);
    muzzleFlash.transform.localScale = MotherShipSideAttack.MUZZLE_FLASH_SCALE;
    muzzleFlash.LifeTime = MotherShipSideAttack.MUZZLE_FLASH_LIFETIME;
    var volume = MotherShipSideAttack.VOLUME_RAND.Next(
      MotherShipSideAttack.BULLET_FIRE_VOLUME.min,
      MotherShipSideAttack.BULLET_FIRE_VOLUME.max
    );
    var sfx = AudioManager.Shared.GetSfxController();
    sfx.PlaySound(
      MotherShipSideAttack.bulletFireSound,
      this.ship.transform.position,
      MotherShipSideAttack.BULLET_FIRE_VOLUME);
  }

  void OnMissileHit(BaseProjectile projectile, Collider target)
  {
    var explosion = this.missileExplosionPool.Get();
    explosion.transform.position = projectile.transform.position;
    explosion.LifeTime = MotherShipSideAttack.MISSILE_EXPLOSION_LIFETIME;
    explosion.transform.localScale = MotherShipSideAttack.MISSILE_EXPLOSION_SCALE;
    var sfx = AudioManager.Shared.GetSfxController();
    sfx.PlaySound(
      MotherShipSideAttack.missileHitSound,
      projectile.transform.position,
      MotherShipSideAttack.MISSILE_HIT_VOLUME
    );
  }

  void OnBulletHit(BaseProjectile projectile, Collider target)
  {
    var sfx = AudioManager.Shared.GetSfxController();
    sfx.PlaySound(
      MotherShipSideAttack.bulletHitSound,
      target.transform.position,
      MotherShipSideAttack.BULLET_HIT_VOLUME
    );
  }

  void ClampAim(float deltaTime)
  {
    var centerDir = this.ship.transform.right * (this.AttackDirection == Direction.Left ? -1f : 1f);
    var currentDir = this.AimDirection * Vector3.forward;
    var shipDir = this.ship.transform.forward;
    if (Vector3.Dot(
          new Vector3(currentDir.x, shipDir.y, currentDir.z),
        centerDir) <
        this.maxRotation.yDot) {
      var currentAngle = this.AimDirection.eulerAngles.y;
      var shipAngle = Quaternion.LookRotation(
        this.ship.transform.right
      ).eulerAngles.y;
      var offset = currentAngle < shipAngle ? 
        -this.rotationClampAngle: this.rotationClampAngle;
      var clampedDir = this.AimDirection * Quaternion.Euler(
          Vector3.up * (shipAngle - currentAngle + offset)
      );
      this.AimDirection = Quaternion.Lerp(
        this.AimDirection,
        clampedDir,
        20f * deltaTime
      );
    }
  }

  void RotateAim(float deltaTime)
  {
    var shipDir = this.ship.transform.forward;
    var delta = UserInputManager.Shared.PointerDelta * InputSettings.SideAttackCameraSensitivity * deltaTime;
    var centerDir = this.ship.transform.right * (this.AttackDirection == Direction.Left ? -1f : 1f);
    var yRotated = this.AimDirection * Quaternion.Euler(
      Vector3.up * delta.x
    );
    var yRotatedDir = yRotated * Vector3.forward;
    if (Vector3.Dot(
          new Vector3(yRotatedDir.x, shipDir.y, yRotatedDir.z),
          centerDir) > this.maxRotation.yDot) {
      this.AimDirection = yRotated;
    }
    var xAngle = this.AimDirection.eulerAngles.x - delta.y;
    if (xAngle > 180f) {
      xAngle -= 360f;
    }
    if (Math.Abs(xAngle) < this.maxRotation.xAngle) {
      this.AimDirection *= Quaternion.Euler(
        Vector3.right * -delta.y
      );
    }
    CameraManager.Shared.SetSideViewDir(this.AimDirection);
  }
}
