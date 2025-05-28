using System;
using System.Collections.Generic;
using UnityEngine;

public class BorneCraft : MonoBehaviour
{
  [Header("References")]
  [SerializeField]
  GameObject body;
  [SerializeField]
  GameObject projectile;
  [SerializeField]
  Rigidbody rb;
  [SerializeField]
  Transform target;

  [Header("Configs")]
  [SerializeField]
  float speed;
  [SerializeField]
  float minMoveAngle;
  [SerializeField]
  float maxMoveAngle;
  [SerializeField]
  float maxShootAngle;
  [SerializeField]
  float prepareSortieTime;
  [SerializeField]
  float shootRange;
  [SerializeField]
  float shootDelay;
  [SerializeField]
  int shootDamage;
  [SerializeField]
  float waitOffset;

  BorneCraftMovement movement;
  BorneCraftAttack attack;
  bool isSortie;
  float targetDistance;
  (float min, float max) containerOffset = (1f, 10f);

  public void SetTarget(Transform target)
  {
    this.target = target;
    this.movement.SetTarget(target, this.waitOffset);
    this.attack.SetTarget(target);
  }

  void Awake()
  {
    if (this.rb == null) {
      this.rb = this.GetComponent<Rigidbody>();
    }
    this.movement = this.InitMovement();
    this.attack = this.InitAttack();
  }

  BorneCraftAttack InitAttack()
  {
    var attack = new BorneCraftAttack(
      body: this.body.transform,
      projectile: this.projectile,
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
      Damage = this.shootDamage
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
      prepareSortieTime: this.prepareSortieTime
    ));
  }

  // Start is called before the first frame update
  void Start()
  {
    CombatManager.Shared.SelectedEnemy.OnChanged += this.OnEnemeySelected;
  }

  // Update is called once per frame
  void Update()
  {
    if (this.target != null) {
      this.targetDistance = Vector3.Distance(
          this.target.position, this.body.transform.position);
      this.movement.TargetDistance = this.targetDistance;
      this.UpdateContainer();
    }
    this.movement.Update(Time.deltaTime);
    if (this.movement.IsShootable) {
      this.attack.Update(Time.deltaTime);
    }
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

  void OnEnemeySelected(IDamagable enemy) 
  {
    if (enemy != null) {
      this.body.SetActive(true);
      this.SetTarget(enemy.gameObject.transform);
    }
  }

  void OnShoot()
  {
  }

  void OnReturnToShip()
  {
    this.body.SetActive(false);
    this.isSortie = false;
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
}
