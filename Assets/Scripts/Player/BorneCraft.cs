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

  BorneCraftMovement movement;
  BorneCraftAttack attack;
  bool isSortie;
  float targetDistance;
  (float min, float max) containerOffset = (1f, 10f);

  public void SetTarget(Transform target)
  {
    this.movement.SetTarget(target);
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
      ShootDelay = this.shootDelay
    });
  }

  BorneCraftMovement InitMovement()
  {
    var movement = (new BorneCraftMovement(
      container: this.transform,
      rigidbody: this.rb,
      transform: this.body.transform,
      configs: this.CreateMovementConfigs() 
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

  void OnShoot()
  {
    Debug.Log("shoot"); 
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
    this.transform.LookAt(this.target);
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
