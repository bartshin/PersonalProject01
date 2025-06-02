using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyShip : MonoBehaviour
{
  const float DESTORYED_EXPLOSION_LIFETIME = 1.5f;
  const float DESTORYED_SOUND_VOLUME = 0.8f;
  static readonly Vector3 DESTORY_EXPLOSION_SCALE = new Vector3(10f, 10f, 10f);

  [Header("References")]
  [SerializeField]
  Rigidbody rb;
  [SerializeField]
  Transform body;
  [SerializeField]
  GameObject target;
  [SerializeField]
  ShipHealth health;
  [SerializeField]
  SphereCollider detectTrigger;
  [SerializeField]
  GameObject projectile;
  [SerializeField]
  GameObject destoryedEffect;
  [SerializeField]
  GauageImageUI HpBar;
  [SerializeField]
  PointerHandler HpBarPointerHandler;

  [Header("Movement Configs")]
  [SerializeField]
  EnemyShipMovement.State initialState;
  [SerializeField]
  float distToStopChase;
  [SerializeField]
  float distToStartChase;
  [SerializeField]
  float detectRange;
  [SerializeField]
  float acceleration;
  [SerializeField]
  float maxSpeed;
  [SerializeField]
  float chasingTimeWhenAttacked;

  [Header("Attack Configs")]
  [SerializeField]
  float shootRange;
  [SerializeField]
  float shootDelay;
  [SerializeField]
  int shootDamage;
  [SerializeField]
  float projectileSpeed;
  [SerializeField]
  float projectileLifeTime;

  EnemyShipAttack attack;
  EnemyShipMovement movement;
  float targetDistance;
  int playerMotherShipLayer;
  int playerBorneCraftLayer;

  void Awake()
  {
    if (this.rb == null) {
      this.rb = this.GetComponent<Rigidbody>();
    }
    if (this.body == null) {
      this.body = this.transform;
    }
    if (this.health == null) {
      this.health = this.GetComponent<ShipHealth>();
    }
    if (this.detectTrigger != null) {
      this.detectTrigger.radius = this.detectRange;
    }
    this.attack = this.InitAttack();
    this.movement = this.InitMovement();
    this.playerMotherShipLayer = LayerMask.NameToLayer("Player");
    this.playerBorneCraftLayer = LayerMask.NameToLayer("PlayerBorneCraft");
  }

  EnemyShipAttack InitAttack()
  {
    var attack = new EnemyShipAttack( 
      ship: this.gameObject,
      projectilePrefab: this.projectile,
      configs: this.CreateAttackConfigs()
    );
    if (this.target != null) {
      attack.SetTarget(this.target.GetComponent<IDamagable>());
    }
    return (attack);
  }

  EnemyShipAttack.Configs CreateAttackConfigs()
  {
    return (new EnemyShipAttack.Configs {
      ShootDelay = this.shootDelay,
      ShootRange = this.shootRange,
      ShootDamage = this.shootDamage,
      ProjectileSpeed = this.projectileSpeed,
      ProjectileLifetTime = this.projectileLifeTime
    });
  }

  EnemyShipMovement InitMovement()
  {
    var movement = new EnemyShipMovement(
      body: this.body,
      rigidbody: this.rb,
      configs: this.CreateMovementConfigs(),
      initialState: this.initialState
    );
    if (this.target != null) {
      movement.SetTarget(this.target.transform);
    }
    return (movement);
  }

  EnemyShipMovement.Configs CreateMovementConfigs()
  {
    return (new EnemyShipMovement.Configs {
      ChasingRange = (this.distToStopChase, this.distToStartChase),
      Acceleration = this.acceleration,
      MaxSpeed = this.maxSpeed,
      ChasingTimeWhenAttacked = this.chasingTimeWhenAttacked,
    });
  }

  void Start()
  {
    this.health.OnTakeDamage += this.OnTakeDamage;
    this.health.OnDestroyed += this.OnDestroyed;
    if (this.HpBar != null && this.health != null) {
      this.HpBar.WatchingIntValue = this.health.Hp;
      //this.HpBar.gameObject.SetActive(false);
    }
    if (this.HpBarPointerHandler != null) {
      this.HpBarPointerHandler.AddEvent(PointerHandler.PointerEvent.Click, this.OnClickHpbar);
    }
  }

  void Update()
  {
    this.UpdateTargetDistance();
    this.movement.Update(Time.deltaTime);
    this.attack.Update(Time.deltaTime);
  }

  void OnValidate()
  {
    if (this.movement != null) {
      this.movement.configs = this.CreateMovementConfigs();
      if (this.target != null ) {
        this.movement.SetTarget(this.target.transform);
      }
    }
    if (this.attack != null) {
      this.attack.configs = this.CreateAttackConfigs();
      if (this.target != null) {
        this.attack.SetTarget(this.target.GetComponent<IDamagable>());
      }
    }
    if (this.detectTrigger != null) {
      this.detectTrigger.radius = this.detectRange;
    }
  }

  void OnTriggerEnter(Collider collider)
  {
    if (collider.gameObject.layer == this.playerMotherShipLayer ||
        (this.target == null && 
         collider.gameObject.layer == this.playerBorneCraftLayer)) {
      var damagable = collider.gameObject.GetComponent<IDamagable>();
      if (damagable != null) {
        this.StartCombatWith(damagable);
      }
    }
  }

  void RemoveTarget()
  {
    this.target = null;
    this.movement.RemoveTarget();
    this.attack.RemoveTarget();
    this.targetDistance = float.MaxValue;
  }

  void UpdateTargetDistance()
  {
    if (this.target != null) {
      this.targetDistance = Vector3.Distance(
        this.body.position,
        this.target.transform.position
      );
      if (!this.target.activeSelf) {
        this.RemoveTarget();
      }
    }
    else {
      this.targetDistance = float.MaxValue;
    }
    this.movement.TargetDistance = this.targetDistance;
    this.attack.TargetDistance = this.targetDistance;
  }

  void OnTakeDamage(int damage, Transform attacker, Nullable<Vector3> attackedPosition)
  {
    if (this.HpBar != null) {
      this.HpBar.gameObject.SetActive(true);
    }
    var attackerDamagble = attacker.gameObject.GetComponent<IDamagable>();
    if (this.target == null && attackerDamagble != null) {
      this.StartCombatWith(attackerDamagble);
    }
  }

  void StartCombatWith(IDamagable target)
  {
    this.target = target.gameObject;
    this.movement.StartChasing(this.target.transform);
    this.attack.SetTarget(target);
    target.OnDisabled += this.FinishCombatWith;
    target.OnDestroyed += this.FinishCombatWith;
  }

  void FinishCombatWith(IDamagable target)
  {
    if (this.target == target.gameObject) {
      this.RemoveTarget();
      target.OnDestroyed -= this.FinishCombatWith;
      target.OnDisabled -= this.FinishCombatWith;
    }
  }

  void OnDestroyed(IDamagable health)
  {
    var effect = Instantiate(this.destoryedEffect).GetComponent<BaseExplosion>();
    effect.transform.position = this.body.position;
    effect.LifeTime = EnemyShip.DESTORYED_EXPLOSION_LIFETIME;
    effect.transform.localScale = EnemyShip.DESTORY_EXPLOSION_SCALE; 
    effect.gameObject.SetActive(true);
    var sfx = AudioManager.Shared.GetSfxController();
    sfx.transform.position = this.body.position;
    sfx.SetVolume(EnemyShip.DESTORYED_SOUND_VOLUME);
    sfx.PlaySound(AudioManager.SmallExposionSound);
    this.RemoveTarget();
  }

  void OnClickHpbar(PointerEventData eventData)
  {
    if (eventData.button != PointerEventData.InputButton.Middle) {
      CombatManager.Shared.OnSelectEnemy(this.health,
          eventData.button == PointerEventData.InputButton.Left);
    }
  }
}
