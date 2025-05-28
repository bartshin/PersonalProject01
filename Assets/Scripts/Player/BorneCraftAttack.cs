using System;
using System.Collections.Generic;
using UnityEngine;
using Architecture;

public class BorneCraftAttack 
{
  public struct Configs
  {
    public float ShootDelay;
    public int Damage;
  }

  public Action OnShoot;
  public Configs configs;
  float remainDelay;
  Transform body;
  Transform targetTransform;
  MonoBehaviourPool<BorneCraftProjectile> projectilePool;

  public BorneCraftAttack(Transform body, GameObject projectile, Configs configs)
  {
    this.body = body;
    this.configs = configs;
    this.projectilePool = new (
      poolSize: 20,
      maxPoolSize: 100,
      prefab: projectile 
    );
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
    var projectile = this.projectilePool.Get();
    projectile.transform.position = this.body.position;
    projectile.FiredShip = this.body.gameObject;
    projectile.Speed = 20f;
    projectile.Damage = this.configs.Damage;
    projectile.TargetPosition = this.targetTransform.position;
    if (this.OnShoot != null) {
      this.OnShoot.Invoke();
    }
  }

  void WaitToShoot(float deltaTime)
  {
    this.remainDelay -= deltaTime;
  }
}
