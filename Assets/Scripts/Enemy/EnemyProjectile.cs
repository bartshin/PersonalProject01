using System;
using System.Collections.Generic;
using UnityEngine;
using Architecture;

public class EnemyProjectile : BaseProjectile, IPooedObject, IProjectile
{

  public Action<IPooedObject> OnDisabled 
  { 
    get => this.onDisabled as Action<IPooedObject>; 
    set {
      this.onDisabled = value;
    }
  }
  Action<EnemyProjectile> onDisabled;

  // Start is called before the first frame update
  void Start()
  {
  }

  // Update is called once per frame
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
    if (collider.gameObject == this.Target.gameObject) {
      return (this.Target);
    }
    else {
      return (IDamagable.GetDamagable(collider.gameObject) ??
          IDamagable.FindIDamagableFrom(collider.gameObject));
    }
  }

  protected override void DestroySelf()
  {
    this.gameObject.SetActive(false);
  }
}
