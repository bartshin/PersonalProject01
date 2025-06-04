using System;
using System.Collections.Generic;
using UnityEngine;
using Architecture;

public class MotherShipHealth : ShipHealth
{
  static readonly (int min, int max) SHIELD_HIT_VOLUME = (20, 30);
  static readonly (int min, int max) HIT_VOLUME = (10, 15);
  static readonly (int min, int max) SHIELD_HIT_EFFECT_SCALE = (3, 5);
  static readonly (int min, int max) HIT_EFFECT_SCALE = (10, 20);
  static readonly System.Random SCALE_RAND = new ();
  const float HIT_EFFECT_LIFE_TIME = 1f;

  [Header("References")]
  [SerializeField]
  StatusController status;
  [SerializeField]
  AudioClip hitSound;
  [SerializeField]
  AudioClip shieldHitSound;
  [SerializeField]
  GameObject shieldHitEffect;
  [SerializeField]
  GameObject hitEffect;

  [Header("Configs")]
  [SerializeField]
  int maxBarrier;
  [SerializeField]
  int barrierRestore;
  public ObservableValue<(int current, int max)> Barrier;
  (float current, float max) innerBarrier;
  [SerializeField]
  float barrierEfficiency;

  MonoBehaviourPool<BaseExplosion> shieldHitEffectPool;
  MonoBehaviourPool<BaseExplosion> hitEffectPool;

  override protected void Awake()
  {
    base.Awake();
    if (this.status == null) {
      this.status = this.GetComponent<StatusController>();
    }
    this.Barrier = new ((this.maxBarrier, this.maxBarrier));
    this.innerBarrier = ((float)this.maxBarrier, (float) this.maxBarrier);
    this.OnTakeDamage += this.OnTakeDamageFrom;
    this.shieldHitEffectPool = new (
      poolSize: 10,
      maxPoolSize: 30,
      prefab: this.shieldHitEffect
    );
    this.hitEffectPool = new (
      poolSize: 10,
      maxPoolSize: 30,
      prefab: this.hitEffect
    );
  }

  void Start()
  {
    this.barrierRestore = (int)((float)this.status.Distribution.MotherShipBarrier.Value * this.barrierEfficiency);
    this.status.Distribution.MotherShipBarrier.OnChanged += this.OnPowerChanged;
    GameManager.Shared.OnHealPlayer += this.OnHeal;
  }

  void OnHeal(int amount)
  {
    Debug.Log(amount);
    if (this.Hp.Value.current == this.Hp.Value.max) {
      var (current, max) = this.Barrier.Value;
      this.Barrier.Value = (Math.Min(current + amount * 2, max), max);
    }
    else {
      var (current, max) = this.Hp.Value;
      this.Hp.Value = (Math.Min(current + max, max), max);
    }
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

  void OnTakeDamageFrom(int damage, Transform attacker, Nullable<Vector3> attackedPosition) 
  {
    if (this.Barrier.Value.current > 0) {
      this.OnDamagedToShield(attackedPosition);
    }
    else 
    {
      this.OnDamagedToHp(attackedPosition);
    }
  }

  void OnDamagedToHp(Nullable<Vector3> attackedPosition)
  {
    var pos = attackedPosition ?? this.gameObject.transform.position;
    var sfx = AudioManager.Shared.GetSfxController();
    sfx.PlaySound(
      this.hitSound,
      attackedPosition ?? this.transform.position,
      MotherShipHealth.HIT_VOLUME
    );
    this.SpawnHitEffect(
      this.hitEffectPool, pos, MotherShipHealth.HIT_EFFECT_SCALE);
  }

  void OnDamagedToShield(Nullable<Vector3> attackedPosition)
  {
    var pos = attackedPosition ?? this.gameObject.transform.position;
    var sfx = AudioManager.Shared.GetSfxController();
    sfx.PlaySound(
      this.shieldHitSound,
      attackedPosition ?? this.transform.position,
      MotherShipHealth.SHIELD_HIT_VOLUME
    );
    this.SpawnHitEffect(
      this.shieldHitEffectPool, pos, MotherShipHealth.SHIELD_HIT_EFFECT_SCALE);
  }

  void SpawnHitEffect(MonoBehaviourPool<BaseExplosion> pool, Vector3 position, (int min, int max) scaleRange)
  {
    var effect = pool.Get();
    effect.transform.position = position;
    effect.LifeTime = MotherShipHealth.HIT_EFFECT_LIFE_TIME;
    var scale = MotherShipHealth.SCALE_RAND.Next(
        scaleRange.min, scaleRange.max);
    effect.transform.localScale = new Vector3(
      scale, scale, scale);
  }
}
