using System;
using System.Collections.Generic;
using UnityEngine;
using Architecture;

public class EnemyShipAttack
{

  static readonly AudioClip projectileFireSound;
  static readonly AudioClip projectileHitSound;
  static readonly (int min, int max) FIRE_VOLUME = (30, 50);
  static readonly (int min, int max) HIT_VOLUME = (15, 30);

  static EnemyShipAttack()
  {
    EnemyShipAttack.projectileFireSound = Resources.Load<AudioClip>("Audio/laser_wave");
    EnemyShipAttack.projectileHitSound = Resources.Load<AudioClip>("Audio/laser_explosion");
  }

  public struct Configs
  {
    public float ShootDelay;
    public int ShootDamage;
    public float ShootRange;
    public float ProjectileSpeed;
    public float ProjectileLifetTime;
  }

  public Configs configs;
  public float TargetDistance;
  IDamagable target;
  GameObject ship;
  float remainingDelay;
  MonoBehaviourPool<EnemyLaser> projectilePool;

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
    projectile.LifeTime = this.configs.ProjectileLifetTime;
    projectile.InitialSpeed = this.configs.ProjectileSpeed;
    projectile.FiredShip = this.ship;
    projectile.Target = target;
    projectile.OnHit = this.OnProjectileHit;
    var sfx = AudioManager.Shared.GetSfxController(); 
    sfx.PlaySound(
      EnemyShipAttack.projectileFireSound,
      this.ship.transform.position,
      EnemyShipAttack.FIRE_VOLUME
    );
  }

  void OnProjectileHit(BaseProjectile projectile, Collider target)
  {
    var sfx = AudioManager.Shared.GetSfxController(); 
    sfx.PlaySound(
      EnemyShipAttack.projectileHitSound,
      projectile.transform.position,
      EnemyShipAttack.HIT_VOLUME
    );
  }
}
