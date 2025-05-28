using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotherShip : MonoBehaviour
{
  [Header("References")]
  [SerializeField]
  Rigidbody rb;

  [Header("Configs")]
  [SerializeField]
  float turningSpeed;
  [SerializeField]
  float acceleration;
  [SerializeField]
  float verticalAcceleration;
  [SerializeField]
  float maxSpeed;
  [SerializeField]
  float maxVerticalSpeed;
  [SerializeField]
  float angularVelocityThreshold;
  [SerializeField]
  float angularVelocityDecreseRatio;

  MotherShipMovement movement;

  void Awake()
  {
    if (this.rb == null) {
      this.rb = this.GetComponent<Rigidbody>();
    }
    this.movement = this.InitMovement();
  }

  MotherShipMovement InitMovement()
  {
    return (new MotherShipMovement(
      rigidbody: this.rb,
      transform: this.transform,
      configs: this.CreateConfigs()
     )
    );
  }

  MotherShipMovement.Configs CreateConfigs()
  {
    return (new MotherShipMovement.Configs {
      turningSpeed = this.turningSpeed,
      acceleration = this.acceleration,
      verticalAcceleration = this.verticalAcceleration,
      maxSpeed = this.maxSpeed,
      angularVelocityThreshold = this.angularVelocityThreshold,
      angularVelocityDecreseRatio = this.angularVelocityDecreseRatio
    });
  }

  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {
    this.movement.Update(Time.deltaTime);
  }

  void OnValidate()
  {
    if (this.movement != null) {
      this.movement.configs = this.CreateConfigs();
    }
  }
}
