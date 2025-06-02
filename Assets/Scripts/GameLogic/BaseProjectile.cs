using System;
using System.Collections.Generic;
using UnityEngine;
using Architecture;

public abstract class BaseProjectile : MonoBehaviour, IProjectile
{

  abstract protected IDamagable GetTargetFrom(Collider collider);
  public Action<BaseProjectile, Collider> OnHit;

  virtual public Vector3 TargetPosition
  {
    get => this.targetPosition;
    set {
      this.targetPosition = value;
      this.Direction = (this.targetPosition - this.transform.position).normalized;
      this.transform.LookAt(this.targetPosition);
    }
  }

  virtual public Vector3 Direction { get; set; }
  protected Vector3 direction;
  virtual public int Damage { get; set; }
  virtual public float InitialSpeed { get; set; }
  virtual public float LifeTime { get; set; } = 5f;
  virtual public GameObject FiredShip { get; set; }
  virtual public IDamagable Target  {
    get => this.target;
    set {
      this.target = value;
      this.TargetPosition = value.gameObject.transform.position;
    }
  }
  protected IDamagable target;
  protected Vector3 targetPosition;
  protected float remainingLifeTime; 

  protected virtual void OnEnable()
  {
    this.remainingLifeTime = this.LifeTime;
  }

  protected virtual void OnTriggerEnter(Collider collider)
  {
    if (this.OnHit != null) {
      this.OnHit.Invoke(this, collider);
    }
    var damagable = this.GetTargetFrom(collider);
    if (damagable != null) {
      damagable.TakeDamage(this.Damage, this.FiredShip.transform);
    }
    this.DestroySelf();
  }

  protected abstract void DestroySelf();
}
