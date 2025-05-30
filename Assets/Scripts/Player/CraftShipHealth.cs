using System;
using System.Collections.Generic;
using UnityEngine;
using Architecture;

public class CraftShipHealth : ShipHealth
{
  [SerializeField]
  public int MaxBarrier; 

  public ObservableValue<(int current, int max)> Barrier { get; private set; }
  public Action<CraftShipHealth> OnPowerDown;

  public int RestoreBarrier(int amount)
  {
    var (current, max) = this.Barrier.Value;
    var restoredAmouont = Math.Min(amount, max - current);
    this.Barrier.Value = (current + restoredAmouont, max);
    return (restoredAmouont);
  }

  void Update()
  {
    //FIXME: Remove Test ****************
    if (Input.GetKeyDown(KeyCode.Alpha6)) {
      Debug.Log($"{this.gameObject.name} barrier: {this.Barrier.Value.current}/{this.Barrier.Value.max} hp: {this.Hp.Value.current}/{this.Hp.Value.max}");
    }
    //***********************************
  }

  protected override void Awake()
  {
    base.Awake();
    this.Barrier = new ((this.MaxBarrier, this.MaxBarrier));
  }

  protected override int GetDamaged(int attackDamage)
  {
    attackDamage -= this.defense;
    if (attackDamage > 0) {
      var barrierDamage = this.TakeDamageTo(attackDamage, this.Barrier);
      var remainDamage = attackDamage - barrierDamage;
      if (remainDamage > 0) {
        this.TakeDamageTo(remainDamage, this.Hp);
      }
    }
    return (Math.Max(attackDamage, 0));
  }

  protected override void OnRunoutHp()
  {
      Debug.Log($"{this.gameObject.name} Destroyed");
    if (this.OnPowerDown != null) {
      this.OnPowerDown.Invoke(this);
    }
    else {
    }
  }

  int TakeDamageTo(int damage, ObservableValue<(int, int)> target) 
  {
    var (current, max) = target.Value;
    var damageTaken = Math.Min(damage, current);
    target.Value = (current - damageTaken, max);
    return (damageTaken);
  }
}
