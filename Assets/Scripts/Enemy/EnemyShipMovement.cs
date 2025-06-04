using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShipMovement
{

  public enum State 
  {
    HoldingPosition,
    ChasingTarget,
    Escaping,
    Patrol
  }

  public struct Configs 
  {
    public (float min, float max) ChasingRange;
    public float Acceleration;
    public float MaxSpeed;
    public float ChasingTimeWhenAttacked;
  }

  public State CurrentState { get; private set; }
  public Configs configs;
  public float TargetDistance;
  List<Vector3> patrolPositions;
  int currentPatrolIndex;
  Transform body;
  Transform target;
  Rigidbody rb;
  float speedToStopThreshold = 0.5f;
  float patrolThreshold = 3f;
  float remainingChasingTime;
  bool IsTargetInChasingRange => (this.TargetDistance < this.configs.ChasingRange.max && this.TargetDistance > this.configs.ChasingRange.min);


  // Start is called before the first frame update
  public EnemyShipMovement(Transform body, Rigidbody rigidbody, Configs configs, State initialState = State.HoldingPosition)
  {
    this.body = body;
    this.rb = rigidbody;
    this.configs = configs;
    this.CurrentState = initialState;
    this.patrolPositions = new (5) {
      this.body.position
    };
  }

  public void AddPatrolPosition(Vector3 position)
  { 
    this.patrolPositions.Add(position);
  }

  public void SetTarget(Transform target)
  {
    this.target = target;
  }

  public void RemoveTarget()
  {
    this.target = null;
    this.CurrentState = State.HoldingPosition;
  }

  public void Update(float deltaTime)
  {
    this.UpdateState();
    this.Move(deltaTime);
  }

  public void StartChasing(Transform target)
  {
    this.SetTarget(target);
    this.CurrentState = State.ChasingTarget;
    this.remainingChasingTime = this.configs.ChasingTimeWhenAttacked;
  }

  void Move(float deltaTime)
  {
    switch (this.CurrentState)
    {
      case State.HoldingPosition:
        if (this.target != null) {
          this.LookTarget(deltaTime);
        }
        if (this.rb.velocity.magnitude > this.speedToStopThreshold) {
          this.DecreaseSpeed(deltaTime);
        }
        else {
          this.rb.velocity = Vector3.zero;
        }
        break;
      case State.ChasingTarget:
        this.LookTarget(deltaTime);
        this.ChaseTarget(deltaTime);
        break;
      case State.Patrol:
        if (this.IsTargetInChasingRange) {
          this.StartChasingTarget();
        }
        else {
          this.MoveToPatrolPosition(deltaTime);
        }
        break;
    }
  }

  void MoveToPatrolPosition(float deltaTime)
  {
    var dist = Vector2.Distance(
        this.body.position,
        this.patrolPositions[this.currentPatrolIndex]);
    Vector3 position;
    if (dist < this.patrolThreshold) {
      position = this.GetNextPatrolPosition();
    }
    else {
      position = this.patrolPositions[this.currentPatrolIndex];
    }
    var dir = (position - this.body.position).normalized;
    this.body.rotation = Quaternion.Lerp(
      this.body.rotation,
      Quaternion.LookRotation(dir),
      1f * deltaTime
    );
    var currentSpeed = this.rb.velocity.magnitude;
    var speed = Math.Min(
        currentSpeed + this.configs.Acceleration,
        this.configs.MaxSpeed
    );
    this.rb.velocity = this.body.forward * speed;
  }

  Vector3 GetNextPatrolPosition()
  {
    var index = this.currentPatrolIndex + 1; 
    if (index >= this.patrolPositions.Count) {
      index = 0;
    }
    this.currentPatrolIndex = index;
    return (this.patrolPositions[index]);
  }

  void UpdateState()
  {
    var isInRange = this.IsTargetInChasingRange;
    if (isInRange) {
      if(this.CurrentState == State.HoldingPosition ||
          this.CurrentState == State.Patrol) {
        this.CurrentState = State.ChasingTarget;
      }
    }
    else if (this.CurrentState == State.ChasingTarget ) {
      if (this.TargetDistance < this.configs.ChasingRange.min) {
        this.CurrentState = State.HoldingPosition;
      }
      else if (this.remainingChasingTime < 0) {
        this.CurrentState = State.Patrol;
      }
    }
  }

  void LookTarget(float deltaTime)
  {
    var dir = (this.target.position - this.body.position).normalized;
    this.body.rotation = Quaternion.Lerp(
      this.body.rotation,
      Quaternion.LookRotation(dir),
      1f * deltaTime
    );
  }

  void ChaseTarget(float deltaTime)
  {
    this.remainingChasingTime -= deltaTime;
    var currentSpeed = this.rb.velocity.magnitude;
    var speed = Math.Min(
        currentSpeed + this.configs.Acceleration,
        this.configs.MaxSpeed
    );
    this.rb.velocity = this.body.forward * speed;
  }

  void DecreaseSpeed(float deltaTime)
  {
    this.rb.velocity = Vector3.Lerp(
      this.rb.velocity,
      Vector3.zero,
      500f * deltaTime
    );
  }

  void StartChasingTarget()
  {
    this.CurrentState = State.ChasingTarget;
  }
}
