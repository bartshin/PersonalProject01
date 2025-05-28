using System;
using System.Collections.Generic;
using UnityEngine;

public class MotherShipMovement
{
  struct UserInput
  {
    public Vector2 Moving;
    public float Altitude;
    public bool IsTuring => Math.Abs(this.Moving.x) > float.Epsilon;
    public bool IsAccelerating => Math.Abs(this.Moving.y) > float.Epsilon;
    public bool IsChaningAltitude => Math.Abs(this.Altitude) > float.Epsilon;
  }

  public struct Configs
  {
    public float turningSpeed;
    public float acceleration;
    public float verticalAcceleration;
    public float maxSpeed;
    public float maxVerticalSpeed;
    public float angularVelocityThreshold;
    public float angularVelocityDecreseRatio;
  }

  Rigidbody rb;
  Transform transform;
  public Configs configs;

  KeyCode upwardKey = KeyCode.E;
  KeyCode downwardKey = KeyCode.Q;

  public MotherShipMovement(Rigidbody rigidbody, Transform transform, Configs configs)
  {
    this.rb = rigidbody;
    this.transform = transform;
    this.configs = configs;
  }

  public void Update(float deltaTime)
  {
    UserInput input = this.GetInput(); 
    this.UpdateDirection(input.IsTuring, input.Moving.x, deltaTime);
    if (input.IsAccelerating) {
      this.Move(input.Moving.y, deltaTime);
    }
    if (input.IsChaningAltitude) {
      this.UpdateAltitude(input.Altitude, deltaTime);
    }
  }

  void Move(float acceleratingInput, float deltaTime)
  {
    this.rb.velocity += this.transform.forward * acceleratingInput * this.configs.acceleration * deltaTime;
    var velocity = new Vector2(this.rb.velocity.x, this.rb.velocity.z);
    if (velocity.magnitude > this.configs.maxSpeed) {
      velocity *= this.configs.maxSpeed / this.rb.velocity.magnitude;
      this.rb.velocity = new Vector3(
        velocity.x, this.rb.velocity.y, velocity.y);
    }
  }

  void UpdateDirection(bool isTurning, float directionInput, float deltaTime)
  {
    if (isTurning) {
      this.rb.angularVelocity += new Vector3(
          0, directionInput * this.configs.turningSpeed * deltaTime, 0);
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
    this.rb.velocity += this.transform.up * altitudeInput * this.configs.verticalAcceleration * deltaTime;
    if (Math.Abs(this.rb.velocity.y) > this.configs.maxVerticalSpeed) {
      this.rb.velocity = new Vector3(
        this.rb.velocity.x,
        Math.Clamp(
          this.rb.velocity.y, 
          -this.configs.maxVerticalSpeed,
          this.configs.maxVerticalSpeed
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
    if (this.rb.angularVelocity.magnitude > this.configs.angularVelocityThreshold) {
      this.rb.angularVelocity = Vector3.Lerp(
        this.rb.angularVelocity,
        Vector3.zero,
        this.configs.angularVelocityDecreseRatio * deltaTime);
    }
    else {
      this.rb.angularVelocity = Vector3.zero;
    }
  }

  UserInput GetInput()
  {
    Vector2 movingInput = new Vector2(
      Input.GetAxisRaw("Horizontal"),
      Input.GetAxisRaw("Vertical")
    );
    float altitude = 0f;
    if (Input.GetKey(this.upwardKey)) {
      altitude += 1f;
    }
    if (Input.GetKey(this.downwardKey)) {
      altitude -= 1f; 
    }
    return (new UserInput{ Moving = movingInput, Altitude = altitude });
  }
}
