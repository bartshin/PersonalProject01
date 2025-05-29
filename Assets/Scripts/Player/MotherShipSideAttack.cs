using System;
using System.Collections.Generic;
using UnityEngine;

public class MotherShipSideAttack 
{
  public struct Configs
  {
    public float Delay;
    public float ProjectileSpeed; 
    public float Power; 
    public float LifeTime; 
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
  Transform ship;
  GameObject projectilePrefab;
  float remainingDelay;
  (float xAngle, float yDot) maxRotation = (30f, 0.7f);

  public MotherShipSideAttack(
      Transform ship,
      GameObject projectilePrefab,
      Configs configs
      )
  { 
    this.ship = ship;
    this.projectilePrefab = projectilePrefab;
    this.configs = configs;
  }

  // Update is called once per frame
  public void Update(float deltaTime)
  {
    if (this.IsActive) {
      this.RotateAim(deltaTime);
    }
    this.remainingDelay -= deltaTime;
  }

  void RotateAim(float deltaTime)
  {
    var shipDir = this.ship.forward;
    var delta = UserInputManager.Shared.MouseDelta * InputSettings.SideAttackCameraSensitivity * deltaTime;
    var centerDir = this.ship.right * (this.AttackDirection == Direction.Left ? -1f : 1f);
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
