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
  float remainingDelay;
  (float xAngle, float yDot) maxRotation = (30f, 0.7f);
  AudioClip laserSound;
  MonoBehaviourPool<BorneCraftProjectile> laserPool;

  public MotherShipSideAttack(
      GameObject ship,
      GameObject laserPrefab,
      Configs configs
      )
  { 
    this.ship = ship;
    this.configs = configs;
    this.laserSound = Resources.Load<AudioClip>("Audio/soft_laser_blast");
    this.laserPool = new (
      poolSize: 30,
      maxPoolSize: 100,
      prefab: laserPrefab
    );
  }

  // Update is called once per frame
  public void Update(float deltaTime)
  {
    this.remainingDelay -= deltaTime;
    if (this.IsActive) {
      this.RotateAim(deltaTime);
      if (UserInputManager.Shared.HasMainActionPressed && this.remainingDelay <= 0) {
        UserInputManager.Shared.HasMainActionTrigged = true;
        this.FireLaser(); 
        this.remainingDelay = this.configs.LaserDelay;
      }
    }
  }

  void FireLaser()
  {
    var projectile = this.laserPool.Get();
    projectile.transform.position = this.ship.transform.position;
    projectile.Speed = this.configs.LaserSpeed;
    projectile.Damage = this.configs.LaserPower;
    projectile.Direction = this.AimDirection * Vector3.forward;
    projectile.LifeTime = this.configs.LaserLifeTime;
    projectile.FiredShip = this.ship;
    this.PlaySound();
  }

  void PlaySound()
  {
    var sfx = AudioManager.Shared.GetSfxController();
    sfx.transform.position = this.ship.transform.position;
    sfx.PlaySound(this.laserSound);
  }

  void RotateAim(float deltaTime)
  {
    var shipDir = this.ship.transform.forward;
    var delta = UserInputManager.Shared.MouseDelta * InputSettings.SideAttackCameraSensitivity * deltaTime;
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
