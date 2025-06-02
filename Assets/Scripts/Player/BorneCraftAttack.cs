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

  static readonly System.Random ShotVolumeRand = new ();

  public Action OnShoot;
  public Configs configs;
  float remainDelay;
  GameObject ship;
  Transform targetTransform;
  MonoBehaviourPool<Laser> projectilePool;
  AudioClip fireSound;

  public BorneCraftAttack(GameObject ship, GameObject projectile, Configs configs)
  {
    this.ship = ship;
    this.configs = configs;
    this.projectilePool = new (
      poolSize: 20,
      maxPoolSize: 100,
      prefab: projectile 
    );
    this.fireSound = Resources.Load<AudioClip>("Audio/short_laser");
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
    var sfx = AudioManager.Shared.GetSfxController();
    sfx.transform.position = this.ship.transform.position;
    var volume = BorneCraftAttack.ShotVolumeRand.Next(40, 60);
    sfx.SetVolume((float)volume * 0.01f);
    sfx.PlaySound(this.fireSound);
  }

  void WaitToShoot(float deltaTime)
  {
    this.remainDelay -= deltaTime;
  }
}
