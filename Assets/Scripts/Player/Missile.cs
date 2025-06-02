using System;
using System.Collections.Generic;
using UnityEngine;
using Architecture;

public class Missile : BaseProjectile, IPooedObject
{
  static readonly System.Random ROTATE_RAND = new ();
  static readonly (int min, int max) ROTATE_RANGE = (70, 90);
  public float Acceleration;
  public Action<IPooedObject> OnDisabled 
  { 
    get => this.onDisabled as Action<IPooedObject>;
    set {
      this.onDisabled = value;
    }
  }
  public override Vector3 Direction 
  { 
    get => base.Direction;
    set {
      base.Direction = value;
      this.transform.forward = value;
    }
  }

  Action<Missile> onDisabled;
  Vector3 velocity;
  Vector3 rotation;

  override protected void OnEnable()
  {
    base.OnEnable();
    this.velocity = this.Direction * this.InitialSpeed;
    var zRotation = Missile.ROTATE_RAND.Next(
      Missile.ROTATE_RANGE.min,
      Missile.ROTATE_RANGE.max
    );
    this.rotation = new Vector3(
      0, 0, (float)zRotation * (zRotation % 2 == 0 ? -1: 1)
    );
  }

  void Update()
  {
    this.remainingLifeTime -= Time.deltaTime;
    this.velocity += this.Direction * this.Acceleration * Time.deltaTime;
    this.transform.position += this.velocity;

    this.transform.Rotate(
      this.rotation * Time.deltaTime
    );
    if (this.remainingLifeTime < 0) {
      this.DestroySelf();
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
