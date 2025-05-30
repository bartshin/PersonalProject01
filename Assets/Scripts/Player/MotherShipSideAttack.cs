using System;
using System.Collections.Generic;
using UnityEngine;
using Architecture;

public class MotherShipSideAttack 
{
  public struct Configs
  {
    public float LaserDelay;
    public float LaserSpeed; 
    public int LaserPower; 
    public float LaserLifeTime; 
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
  (float laser, float missile) remainingDelay;
  (float xAngle, float yDot) maxRotation = (30f, 0.7f);
  float rotationClampAngle = 45f;
  AudioClip laserSound;
  AudioClip missileSound;
  MonoBehaviourPool<BorneCraftProjectile> laserPool;
  MonoBehaviourPool<Missile> missilePool;

  public MotherShipSideAttack(
      GameObject ship,
      GameObject laserPrefab,
      GameObject missilePrefab,
      Configs configs
      )
  { 
    this.ship = ship;
    this.configs = configs;
    this.laserSound = Resources.Load<AudioClip>("Audio/soft_laser_blast");
    this.missileSound = Resources.Load<AudioClip>("Audio/missile_firing");
    this.laserPool = new (
      poolSize: 30,
      maxPoolSize: 100,
      prefab: laserPrefab
    );
    this.missilePool = new (
      poolSize: 20,
      maxPoolSize: 50,
      prefab: missilePrefab
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
          this.remainingDelay.laser <= 0) {
        UserInputManager.Shared.MainOperation.HasTriggered = true;
        this.FireLaser(); 
        this.remainingDelay = (this.configs.LaserDelay, this.remainingDelay.missile);
      }
      if (UserInputManager.Shared.SubOperation.HasRegistered &&
          this.remainingDelay.missile <= 0) {
        UserInputManager.Shared.SubOperation.HasRegistered = true;
        this.FireMissile();
        this.remainingDelay = (this.remainingDelay.laser, this.configs.MissileDelay);
      }
    }
  }

  void UpdateDelay(float deltaTime)
  {
    var (laser, missile) = this.remainingDelay;
    this.remainingDelay = (laser - deltaTime, missile - deltaTime);
  }

  void FireMissile()
  {
    var projectile = this.missilePool.Get();
    projectile.transform.position = this.ship.transform.position;
    projectile.InitialSpeed = this.configs.MissileInitialSpeed;
    projectile.Damage = this.configs.LaserPower;
    projectile.Direction = this.AimDirection * Vector3.forward;
    projectile.LifeTime = this.configs.MissileLifeTime;
    projectile.Acceleration = this.configs.MissileAcceleration;
    projectile.FiredShip = this.ship;
    this.PlayLaserSound(this.missileSound);
  }

  void FireLaser()
  {
    var projectile = this.laserPool.Get();
    projectile.transform.position = this.ship.transform.position;
    projectile.InitialSpeed = this.configs.LaserSpeed;
    projectile.Damage = this.configs.LaserPower;
    projectile.Direction = this.AimDirection * Vector3.forward;
    projectile.LifeTime = this.configs.LaserLifeTime;
    projectile.FiredShip = this.ship;
    this.PlayLaserSound(this.laserSound);
  }

  void PlayLaserSound(AudioClip clip)
  {
    var sfx = AudioManager.Shared.GetSfxController();
    sfx.transform.position = this.ship.transform.position;
    sfx.PlaySound(clip);
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
