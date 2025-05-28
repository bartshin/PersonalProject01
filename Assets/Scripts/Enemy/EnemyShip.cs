using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShip : MonoBehaviour
{
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

  [Header("Configs")]
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
  [SerializeField]
  EnemyShipMovement.State initialState;

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
    return (new EnemyShipAttack());
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

  // Start is called before the first frame update
  void Start()
  {
    this.health.OnTakeDamage += this.OnTakeDamage;
  }

  // Update is called once per frame
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
    if (this.detectTrigger != null) {
      this.detectTrigger.radius = this.detectRange;
    }
  }

  void OnTriggerEnter(Collider collider)
  {
    if (collider.gameObject.layer == this.playerMotherShipLayer ||
        (this.target == null && 
         collider.gameObject.layer == this.playerBorneCraftLayer)) {
      this.target = collider.gameObject;
      this.movement.StartChasing(this.target.transform);
      this.attack.SetTarget(this.target.GetComponent<IDamagable>());
    }
  }

  void RemoveTarget()
  {
    if (this.movement != null) {
      this.movement.RemoveTarget();
    }
  }

  void UpdateTargetDistance()
  {
    if (this.target != null) {
      this.targetDistance = Vector3.Distance(
        this.body.position,
        this.target.transform.position
      );
      if (!this.target.activeSelf) {
        this.target = null;
        this.RemoveTarget();
        this.targetDistance = float.MaxValue;
      }
    }
    else {
      this.targetDistance = float.MaxValue;
    }
    this.movement.TargetDistance = this.targetDistance;
  }

  void OnTakeDamage(int damage, Transform attacker)
  {
    if (this.target == null) {
      this.target = attacker.gameObject;
      this.movement.StartChasing(this.target.transform);
    }
  }
}
