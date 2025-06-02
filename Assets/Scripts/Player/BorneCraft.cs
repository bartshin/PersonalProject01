using System;
using System.Collections.Generic;
using UnityEngine;

public class BorneCraft : MonoBehaviour
{
  [Serializable]
  public class Configs
  {
    public float RotateAngle;   
    public float WaitOffset;
  }

  [Header("References")]
  [SerializeField]
  GameObject ship;
  [SerializeField]
  GameObject body;
  [SerializeField]
  GameObject projectile;
  [SerializeField]
  GameObject projectileExplosion;
  [SerializeField]
  Rigidbody rb;
  [SerializeField]
  Transform target;
  [SerializeField]
  CraftShipHealth health;

  [Header("Movement Configs")]
  [SerializeField]
  float speed;
  [SerializeField]
  float minMoveAngle;
  [SerializeField]
  float maxMoveAngle;
  [SerializeField]
  float prepareSortieTime;
  [SerializeField]
  float waitOffset;
  [SerializeField]
  float maxDistToSortie;

  [Header("Attack Configs")]
  [SerializeField]
  float maxShootAngle;
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

  public Action<BorneCraft> OnReturned;
  public Configs CraftConfigs 
  {
    get => this.configs;
    set {
      this.configs = value;
      if (value != null) {
        this.transform.rotation = Quaternion.identity;
        this.transform.Rotate(
          new Vector3(0, 0, this.configs.RotateAngle));
        this.waitOffset = this.configs.WaitOffset;
      }
    }
  }
  Configs configs;

  BorneCraftMovement movement;
  BorneCraftAttack attack;
  bool isSortie;
  float targetDistance;
  (float min, float max) containerOffset = (1f, 10f);

  public int RestoreBarrier(int amount)
  {
    return (this.health.RestoreBarrier(amount));
  }

  public void SetTarget(Transform target)
  {
    this.target = target;
    this.movement.SetTarget(target, this.waitOffset);
    this.attack.SetTarget(target);
  }

  public void SelectEnemy(IDamagable enemy) 
  {
    this.SetTarget(enemy.gameObject.transform);
    this.UpdateTargetDistance();
  }

  public void DeselectEnemy()
  {
    this.target = null;
    this.movement.RemoveTarget();
    this.attack.RemoveTarget();
  }

  void Awake()
  {
    if (this.rb == null) {
      this.rb = this.GetComponent<Rigidbody>();
    }
    if (this.health == null) {
      this.health = this.GetComponent<CraftShipHealth>();
    }
    this.movement = this.InitMovement();
    this.attack = this.InitAttack();
  }

  BorneCraftAttack InitAttack()
  {
    var attack = new BorneCraftAttack(
      ship: this.ship,
      projectile: this.projectile,
      explosion: this.projectileExplosion,
      configs: this.CreateAttackConfigs()
    );
    if (this.target != null) {
      attack.SetTarget(this.target);
    }
    attack.OnShoot += this.OnShoot;
    return (attack);
  }

  BorneCraftAttack.Configs CreateAttackConfigs()
  {
    return (new BorneCraftAttack.Configs {
      ShootDelay = this.shootDelay,
      Damage = this.shootDamage,
      ProjectileSpeed = this.projectileSpeed,
      ProjectileLifeTime = this.projectileLifeTime,
    });
  }

  BorneCraftMovement InitMovement()
  {
    var movement = (new BorneCraftMovement(
      container: this.transform,
      rigidbody: this.rb,
      transform: this.body.transform,
      configs: this.CreateMovementConfigs(),
      waitOffset: this.waitOffset
    ));
    if (this.target != null) {
      movement.SetTarget(this.target);
    }
    movement.OnSortie += this.OnSorite;
    movement.OnReturnToShip += this.OnReturnToShip;
    return (movement);
  }

  BorneCraftMovement.Configs CreateMovementConfigs()
  {
    return (new BorneCraftMovement.Configs(
      speed: this.speed,
      moveAngles: (this.minMoveAngle, this.maxMoveAngle),
      maxShootAngle: this.maxShootAngle,
      shootRange: this.shootRange,
      prepareSortieTime: this.prepareSortieTime,
      maxDistToSortie: this.maxDistToSortie
    ));
  }

  // Start is called before the first frame update
  void Start()
  {
    this.health.OnTakeDamage += this.OnTakeDamageFrom;
  }

  void Update()
  {

    if (this.target != null) {
      this.UpdateTargetDistance();
      this.UpdateContainer();
    }
    this.movement.Update(Time.deltaTime);
    if (this.movement.IsShootable) {
      this.attack.Update(Time.deltaTime);
    }
  }

  void UpdateTargetDistance()
  {
    this.targetDistance = Vector3.Distance(
        this.target.position, this.body.transform.position);
    this.movement.TargetDistance = this.targetDistance;
    this.UpdateContainer();
  }

  void OnValidate()
  {
    if (this.attack == null || this.movement == null) {
      return ;
    }
    this.movement.configs = this.CreateMovementConfigs();
    this.attack.configs = this.CreateAttackConfigs();
    this.SetTarget(this.target);
  }

  void OnShoot()
  {
  }

  void OnReturnToShip()
  {
    this.body.SetActive(false);
    this.isSortie = false;
    if (this.OnReturned != null) {
      this.OnReturned.Invoke(this);
    }
  }

  void OnSorite()
  {
    this.body.SetActive(true);
    this.isSortie = true;
  }

  void UpdateContainer()
  {
    this.transform.localPosition = Vector3.zero;
    var rotation = Quaternion.LookRotation(
      this.target.position - this.transform.position,
      this.transform.up
    );
    this.transform.rotation = rotation;
    var dir = this.transform.localRotation * Vector3.forward;
    var dist = Vector3.Distance(
      this.transform.position,
      this.target.position
    );
    this.transform.localPosition = dir * 
      Math.Clamp(
        dist * 0.1f,
        this.containerOffset.min,
        this.containerOffset.max
      );
  }

  void OnTakeDamageFrom(int damage, Transform attacker) 
  {}
}
