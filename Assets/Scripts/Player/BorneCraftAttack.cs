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
    public float ProjectileSpeed;
  }

  public Action OnShoot;
  public Configs configs;
  float remainDelay;
  GameObject ship;
  Transform targetTransform;
  MonoBehaviourPool<BorneCraftLaser> projectilePool;

  public BorneCraftAttack(GameObject ship, GameObject projectile, Configs configs)
  {
    this.ship = ship;
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

  public void RemoveTarget()
  {
    this.targetTransform = null;
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
    projectile.transform.position = this.ship.transform.position;
    projectile.FiredShip = this.ship;
    projectile.InitialSpeed = this.configs.ProjectileSpeed;
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
