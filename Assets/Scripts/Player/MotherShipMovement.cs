using System;
using System.Collections.Generic;
using UnityEngine;

public class MotherShipMovement
{
  public struct UserInput
  {
    public Vector2 Moving;
    public float Altitude;
    public bool IsBoosting;
    public bool IsTuring => Math.Abs(this.Moving.x) > float.Epsilon;
    public bool IsAccelerating => Math.Abs(this.Moving.y) > float.Epsilon;
    public bool IsChaningAltitude => Math.Abs(this.Altitude) > float.Epsilon;
  }

  public struct Configs
  {
    public float TurningSpeed;
    public float Acceleration;
    public float VerticalAcceleration;
    public float MaxSpeed;
    public float MaxVerticalSpeed;
    public float AngularVelocityThreshold;
    public float AngularVelocityDecreseRatio;
    public float BoosterPower;
    public float BoosterRestore;
    public float BoosterConsume;
  }

  public Configs configs;
  public bool IsRotatable;
  Rigidbody rb;
  Transform transform;
  float currentBooster;
  bool isBoosting;
  float minBoostingGauge = 10f;

  public MotherShipMovement(Rigidbody rigidbody, Transform transform, Configs configs)
  {
    this.rb = rigidbody;
    this.transform = transform;
    this.configs = configs;
    this.isBoosting = false;
    this.currentBooster = 50f;
    this.IsRotatable = true;
  }

  public void Update(float deltaTime)
  {
    UserInput input = this.GetInput(); 
    this.isBoosting = input.IsBoosting && this.currentBooster > this.minBoostingGauge;
    if (!this.isBoosting) {
      this.currentBooster = Math.Min(this.currentBooster + this.configs.BoosterRestore * Time.deltaTime, 100f);
    }
    else {
      this.currentBooster = Math.Max(this.currentBooster - this.configs.BoosterConsume * Time.deltaTime, 0f);
      if (this.currentBooster < 0) {
        this.isBoosting = false;
      }
    }
    if (this.IsRotatable) {
      this.UpdateDirection(input.IsTuring, input.Moving.x, deltaTime);
    }
    if (input.IsAccelerating) {
      this.Move(input.Moving.y, deltaTime);
    }
    if (input.IsChaningAltitude) {
      this.UpdateAltitude(input.Altitude, deltaTime);
    }
  }

  void Move(float acceleratingInput, float deltaTime)
  {
    var acceleration = this.configs.Acceleration;
    if (this.isBoosting) {
      acceleration += this.configs.BoosterPower;
    }
    this.rb.velocity += this.transform.forward * acceleratingInput * acceleration * deltaTime;
    var velocity = new Vector2(this.rb.velocity.x, this.rb.velocity.z);
    if (!this.isBoosting && velocity.magnitude > this.configs.MaxSpeed) {
      velocity *= this.configs.MaxSpeed / this.rb.velocity.magnitude;
      this.rb.velocity = new Vector3(
        velocity.x, this.rb.velocity.y, velocity.y);
    }
  }

  void UpdateDirection(bool isTurning, float directionInput, float deltaTime)
  {
    if (isTurning) {
      this.rb.angularVelocity += new Vector3(
          0, directionInput * this.configs.TurningSpeed * deltaTime, 0);
    }
    if (this.rb.angularVelocity.magnitude > float.Epsilon) {
      this.ForceMoveFoward();
      if (!isTurning) {
        this.DecreseAngularVelocity(deltaTime);
      }
    }
  }

  void UpdateAltitude(float altitudeInput, float deltaTime)
  {
    this.rb.velocity += this.transform.up * altitudeInput * this.configs.VerticalAcceleration * deltaTime;
    if (Math.Abs(this.rb.velocity.y) > this.configs.MaxVerticalSpeed) {
      this.rb.velocity = new Vector3(
        this.rb.velocity.x,
        Math.Clamp(
          this.rb.velocity.y, 
          -this.configs.MaxVerticalSpeed,
          this.configs.MaxVerticalSpeed
          ),
        this.rb.velocity.z
      );
    }
  }

  void ForceMoveFoward()
  {
    var velocity = new Vector2(this.rb.velocity.x, this.rb.velocity.z);
    var speed = velocity.magnitude;
    var newVelocity = new Vector2(this.transform.forward.x, this.transform.forward.z) * speed;
    this.rb.velocity = new Vector3(newVelocity.x, this.rb.velocity.y, newVelocity.y);
  }

  void DecreseAngularVelocity(float deltaTime)
  {
    if (this.rb.angularVelocity.magnitude > this.configs.AngularVelocityThreshold) {
      this.rb.angularVelocity = Vector3.Lerp(
        this.rb.angularVelocity,
        Vector3.zero,
        this.configs.AngularVelocityDecreseRatio * deltaTime);
    }
    else {
      this.rb.angularVelocity = Vector3.zero;
    }
  }

  UserInput GetInput()
  {
    Vector2 movingInput = new Vector2(
      UserInputManager.Shared.DirectionInput.x,
      UserInputManager.Shared.DirectionInput.z
    );
    float altitude = UserInputManager.Shared.DirectionInput.y;
    bool isBoosting = UserInputManager.Shared.IsBoosting;
    return (new UserInput{ 
      Moving = movingInput,
      Altitude = altitude,
      IsBoosting = isBoosting
    });
  }
}
