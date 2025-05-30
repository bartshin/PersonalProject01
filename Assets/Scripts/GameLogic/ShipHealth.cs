using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architecture;

public class ShipHealth : MonoBehaviour, IDamagable
{
  [Header("Configs")]
  [SerializeField]
  protected int maxHp;
  [SerializeField]
  protected int defense;
  public Action<ShipHealth> OnDestroyed;
  public Action<int, Transform> OnTakeDamage;
  public float WaitToDestroy = 4f;
  Action<IDamagable> IDamagable.OnDestroyed 
  { 
    get => this.OnDestroyed as Action<IDamagable>; 
    set {
      this.OnDestroyed = value;
    }
  }

  public Action<IDamagable> OnDisabled { get; set; }

  public ObservableValue<(int current, int max)> Hp;


  protected virtual void Awake()
  {
    IDamagable.Register(this.gameObject, this);
    this.Hp = new((this.maxHp, this.maxHp));
  }

  protected virtual void OnDisable()
  {
    if (this.OnDisabled != null) {
      this.OnDisabled.Invoke(this);
    }
  }

  public virtual int TakeDamage(int attackDamage)
  {
    var damage = (this.GetDamaged(attackDamage));
    if (this.OnTakeDamage != null) {
      this.OnTakeDamage.Invoke(damage, null);
    }
    if (damage > 0 && this.Hp.Value.current <= 0) {
      this.OnRunoutHp();
    }
    return (damage);
  }

  public virtual int TakeDamage(int attackDamage, Transform attacker)
  {
    var damage = this.GetDamaged(attackDamage);
    if (this.OnTakeDamage != null) {
      this.OnTakeDamage.Invoke(damage, attacker);
    }
    if (damage > 0 && this.Hp.Value.current <= 0) {
      this.OnRunoutHp();
    }
    return (damage);
  }

  protected virtual int GetDamaged(int attackDamage)
  {
    var damage = Math.Max(attackDamage - this.defense, 0);
    var damageTaken = Math.Min(
      damage,
      this.Hp.Value.current
    );
    var (current, max) = this.Hp.Value;
    this.Hp.Value = (current - damageTaken, max);
    return (damageTaken);
  }

  protected virtual void OnRunoutHp()
  {
    if (this.OnDestroyed != null) {
      this.OnDestroyed(this);
    }
    this.StartCoroutine(this.DestorySelf());
  }

  protected IEnumerator DestorySelf()
  {
    this.gameObject.SetActive(false);
    yield return (new WaitForSeconds(this.WaitToDestroy));
    Destroy (this.gameObject);
  }
}
