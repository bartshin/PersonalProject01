using System;
using System.Collections.Generic;
using UnityEngine;
using Architecture;

public class MotherShipHealth : ShipHealth
{
  [Header("References")]
  [SerializeField]
  StatusController status;

  [Header("Configs")]
  [SerializeField]
  int maxBarrier;
  [SerializeField]
  int barrierRestore;
  ObservableValue<(int current, int max)> Barrier;
  (float current, float max) innerBarrier;
  [SerializeField]
  float barrierEfficiency;

  override protected void Awake()
  {
    base.Awake();
    if (this.status == null) {
      this.status = this.GetComponent<StatusController>();
    }
    this.Barrier = new ((this.maxBarrier, this.maxBarrier));
    this.innerBarrier = ((float)this.maxBarrier, (float) this.maxBarrier);
  }

  void Start()
  {
    this.barrierRestore = (int)((float)this.status.Distribution.MotherShipBarrier.Value * this.barrierEfficiency);
    this.status.Distribution.MotherShipBarrier.OnChanged += this.OnPowerChanged;
  }

  void Update()
  {
    if (this.Barrier.Value.current < this.maxBarrier) {
      this.RestoreBarrier(Time.deltaTime);
    }
    //FIXME: Remove Test ***************************
    if (Input.GetKeyDown(KeyCode.Alpha5)) {
      Debug.Log($"restore: {this.barrierRestore} current baerrier: {this.Barrier.Value.current} / {this.Barrier.Value.max}, hp: {this.Hp.Value.current} / {this.Hp.Value.max}");
    }
    //**********************************************
  }

  void OnPowerChanged(int power)
  {
    this.barrierRestore = (int)((float)power * this.barrierEfficiency);
  }

  void RestoreBarrier(float deltaTime)
  {
    float restore = (float)this.barrierRestore * deltaTime;
    this.innerBarrier.current = Math.Min(
        this.innerBarrier.current + restore, 
        this.innerBarrier.max);
    var (current, max) = this.Barrier.Value;
    this.Barrier.Value = ((int)this.innerBarrier.current, max);
  }

  protected override int GetDamaged(int attackDamage)
  {
    attackDamage -= this.defense;
    if (attackDamage > 0) {
      var barrierDamage = this.TakeDamageTo(attackDamage, this.Barrier);
      this.innerBarrier.current = (float)this.Barrier.Value.current;
      var remainDamage = attackDamage - barrierDamage;
      if (remainDamage > 0) {
        this.TakeDamageTo(remainDamage, this.Hp);
      }
    }
    return (Math.Max(attackDamage, 0));
  }

  int TakeDamageTo(int damage, ObservableValue<(int, int)> target) 
  {
    var (current, max) = target.Value;
    var damageTaken = Math.Min(damage, current);
    target.Value = (current - damageTaken, max);
    return (damageTaken);
  }

  protected override void OnRunoutHp()
  {
    Debug.Log("player died");
  }
}
