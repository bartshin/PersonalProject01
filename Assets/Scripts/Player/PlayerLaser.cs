using System;
using System.Collections.Generic;
using UnityEngine;
using Architecture;
using UnityEngine.VFX;

public class PlayerLaser : BaseProjectile, IPooedObject
{
  [SerializeField]
  VisualEffect laserEffect;
  public Action<IPooedObject> OnDisabled { get; set; }

  public override float LifeTime 
  { 
    get => this.lifeTime; 
    set {
      this.lifeTime = value;
      this.laserEffect.SetFloat("duration", value);
    }
  }

  float lifeTime;
  
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
    this.gameObject.SetActive(false);
  }
}
