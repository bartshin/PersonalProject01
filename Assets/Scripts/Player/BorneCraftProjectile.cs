using System;
using System.Collections.Generic;
using UnityEngine;
using Architecture;

public class BorneCraftProjectile : MonoBehaviour, IPooedObject
{
  public Vector3 TargetPosition
  {
    get => this.targetPosition;
    set {
      this.targetPosition = value;
      this.dir = (this.targetPosition - this.transform.position).normalized;
      this.transform.LookAt(this.targetPosition);
    }
  }

  public Action<IPooedObject> OnDisabled 
  { 
    get => this.onDisabled as Action<IPooedObject>; 
    set {
      this.onDisabled = value;
    }
  }
  Action<BorneCraftProjectile> onDisabled;
  public int Damage;
  public float Speed;
  public float LifeTime = 5f;
  public GameObject FiredShip;
  Vector3 targetPosition;
  Vector3 dir;
  float remainingLifeTime; 

  // Start is called before the first frame update
  void Start()
  {
  }

  // Update is called once per frame
  void Update()
  {
    this.remainingLifeTime -= Time.deltaTime;
    this.transform.position += this.dir * this.Speed * Time.deltaTime;
    if (this.remainingLifeTime < 0) {
      this.gameObject.SetActive(false);
    }
  }

  void OnEnable()
  {
    this.remainingLifeTime = this.LifeTime;
  }

  void OnDisable()
  {
    if (this.OnDisabled != null) {
      this.OnDisabled.Invoke(this);
    }
  }

  void OnTriggerEnter(Collider collider)
  {
    IDamagable damagable = null;
    if (collider.gameObject == CombatManager.Shared.LastHitEnemy.gameObject) {
      damagable = CombatManager.Shared.LastHitEnemy.damagable;
    }
    else {
      damagable = IDamagable.GetDamagable(collider.gameObject) ??
        IDamagable.FindIDamagableFrom(collider.gameObject);
      if (damagable != null) {
        CombatManager.Shared.LastHitEnemy = (collider.gameObject, damagable);
      }
    }
    if (damagable != null) {
      damagable.TakeDamage(this.Damage, this.FiredShip.transform);
    }
    this.gameObject.SetActive(false);
  }
}
