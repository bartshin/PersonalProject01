
using System;
using System.Collections.Generic;
using UnityEngine;
using Architecture;

public class Laser: BaseProjectile, IPooedObject, IProjectile
{
  public Action<IPooedObject> OnDisabled 
  { 
    get => this.onDisabled as Action<IPooedObject>; 
    set {
      this.onDisabled = value;
    }
  }
  Action<BorneCraftProjectile> onDisabled;

  void Start()
  {
  }

  void Update()
  {
    this.remainingLifeTime -= Time.deltaTime;
    this.transform.position += this.Direction * this.InitialSpeed * Time.deltaTime;
    if (this.remainingLifeTime < 0) {
      this.DestroySelf();
    }
  }

  void OnDisable()
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
