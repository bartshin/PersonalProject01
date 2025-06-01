using System;
using System.Collections.Generic;
using UnityEngine;
using Architecture;
using UnityEngine.VFX;

public class PlayerBullet : BaseProjectile, IPooedObject, IProjectile
{
  public Action<IPooedObject> OnDisabled 
  { 
    get => this.onDisabled as Action<IPooedObject>; 
    set {
      this.onDisabled = value;
    }
  }
  Action<PlayerBullet> onDisabled;

  [SerializeField]
  TrailRenderer trail;

  public void EnableTrail()
  {
    if (this.trail != null) {
      this.trail.Clear();
      this.trail.time = this.LifeTime;
    }
  }

  protected void Update()
  {
    this.remainingLifeTime -= Time.deltaTime;
    this.transform.position += this.Direction * this.InitialSpeed * Time.deltaTime;
    if (this.remainingLifeTime < 0) {
      this.DestroySelf();
    }
  }

  protected void OnDisable()
  {
    if (this.OnDisabled != null) {
      this.OnDisabled.Invoke(this);
    }
  }

  protected override IDamagable GetTargetFrom(Collider collider)
  {
    if (collider.gameObject == CombatManager.Shared.LastHitEnemy.gameObject) {
      return (CombatManager.Shared.LastHitEnemy.damagable);
    }
    else {
      var damagable = IDamagable.GetDamagable(collider.gameObject) ??
        IDamagable.FindIDamagableFrom(collider.gameObject);
      if (damagable != null) {
        CombatManager.Shared.LastHitEnemy = (collider.gameObject, damagable);
      }
      return (damagable);
    }
  }

  protected override void DestroySelf()
  {
    if (this.trail != null) {
      this.trail.time = -1f;
    }
    this.gameObject.SetActive(false);
  }
}
