using System;
using System.Collections.Generic;
using UnityEngine;

public class BorneCraftAttack 
{
  public struct Configs
  {
    public float ShootDelay;
  }

  public Action OnShoot;
  public Configs configs;
  float remainDelay;
  Transform targetTransform;

  public BorneCraftAttack(Configs configs)
  {
    this.configs = configs;
  }

  public void SetTarget(Transform transform)
  {
    this.targetTransform = transform;
  }

  public void Reset()
  {
  }

  public void Update(float deltaTime)
  {
    if (this.remainDelay <= 0 && this.targetTransform != null) {
      this.Shoot();
      this.remainDelay = this.configs.ShootDelay;
    }
    else {
      this.WaitToShoot(deltaTime);
    }
  }

  void Shoot() 
  {
    if (this.OnShoot != null) {
      this.OnShoot.Invoke();
    }
  }

  void WaitToShoot(float deltaTime)
  {
    this.remainDelay -= deltaTime;
  }
}
