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
    public float ProjectileLifeTime;
  }

  static readonly System.Random VOLUME_RAND = new ();
  static readonly (int min, int max) SHOT_VOLUME = (40, 70);
  static readonly (int min, int max) EXPLOSION_VOLUME = (20, 40);
  static readonly Vector3 EXPLOSION_SCALE = new Vector3(2f, 2f, 2f);
  const float EXPLOSION_LIFE_TIME = 0.5f;

  public Action OnShoot;
  public Configs configs;
  float remainDelay;
  GameObject ship;
  Transform targetTransform;
  MonoBehaviourPool<PlayerLaser> projectilePool;
  MonoBehaviourPool<SimplePooledObject> explosionPool;
  static readonly AudioClip fireSound;
  static readonly AudioClip explosionSound;

  static BorneCraftAttack()
  {
    BorneCraftAttack.fireSound = Resources.Load<AudioClip>("Audio/short_laser");
    BorneCraftAttack.explosionSound = Resources.Load<AudioClip>("Audio/soft_laser_hit");
  }

  public BorneCraftAttack(GameObject ship, GameObject projectile, GameObject explosion, Configs configs)
  {
    this.ship = ship;
    this.configs = configs;
    this.projectilePool = new (
      poolSize: 20,
      maxPoolSize: 100,
      prefab: projectile 
    );
    this.explosionPool = new (
      poolSize: 20,
      maxPoolSize: 100,
      prefab: explosion
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
    projectile.LifeTime = this.configs.ProjectileLifeTime;
    projectile.TargetPosition = this.targetTransform.position;
    if (this.OnShoot != null) {
      this.OnShoot.Invoke();
    }
    projectile.OnHit = this.SpawnExplosion;
    var sfx = AudioManager.Shared.GetSfxController();
    sfx.transform.position = this.ship.transform.position;
    var volume = BorneCraftAttack.VOLUME_RAND.Next(
      BorneCraftAttack.SHOT_VOLUME.min,
      BorneCraftAttack.SHOT_VOLUME.max
    );
    sfx.SetVolume((float)volume * 0.01f);
    sfx.PlaySound(BorneCraftAttack.fireSound);
  }

  void WaitToShoot(float deltaTime)
  {
    this.remainDelay -= deltaTime;
  }

  void SpawnExplosion(BaseProjectile projectile, Collider target)
  {
    var explosion = this.explosionPool.Get();
    explosion.transform.localScale = BorneCraftAttack.EXPLOSION_SCALE;
    explosion.transform.position = projectile.transform.position;
    explosion.LifeTime = BorneCraftAttack.EXPLOSION_LIFE_TIME;
    var sfx = AudioManager.Shared.GetSfxController();
    sfx.transform.position = projectile.transform.position;
    var volume = BorneCraftAttack.VOLUME_RAND.Next(
      BorneCraftAttack.EXPLOSION_VOLUME.min, BorneCraftAttack.EXPLOSION_VOLUME.max);
    sfx.SetVolume((float)volume * 0.01f);
    sfx.PlaySound(BorneCraftAttack.explosionSound);
  }
}
