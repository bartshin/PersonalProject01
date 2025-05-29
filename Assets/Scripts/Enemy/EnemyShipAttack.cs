using System;
using System.Collections.Generic;
using UnityEngine;
using Architecture;

public class EnemyShipAttack
{
  public struct Configs
  {
    public float ShootDelay;
    public int ShootDamage;
    public float ShootRange;
    public float ProjectileSpeed;
  }

  public Configs configs;
  public float TargetDistance;
  IDamagable target;
  GameObject ship;
  float remainingDelay;
  MonoBehaviourPool<EnemyProjectile> projectilePool;

  public EnemyShipAttack(GameObject ship, GameObject projectilePrefab, Configs configs)
  {
    this.ship = ship;
    this.configs = configs;
    this.projectilePool = new (
      poolSize: 20,
      maxPoolSize: 80,
      prefab: projectilePrefab
    );
  }

  public void SetTarget(IDamagable target)
  {
    this.target = target;
  }

  public void RemoveTarget()
  {
    this.target = null;
  }

  public void Update(float deltaTime)
  {
    if (this.target != null && this.remainingDelay <= 0 &&
        this.TargetDistance < this.configs.ShootRange) {
      this.Shoot(this.target);
      this.remainingDelay = this.configs.ShootDelay;
    }
    else {
      this.remainingDelay -= deltaTime;
    }
  }

  void Shoot(IDamagable target)
  {
    var projectile = this.projectilePool.Get();
    projectile.transform.position = this.ship.transform.position;
    projectile.Damage = this.configs.ShootDamage;
    projectile.Speed = this.configs.ProjectileSpeed;
    projectile.FiredShip = this.ship;
    projectile.Target = target;
  }
}
