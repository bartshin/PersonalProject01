using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BorneCraft : MonoBehaviour
{
  [Header("References")]
  [SerializeField]
  GameObject body;
  [SerializeField]
  Rigidbody rb;

  [Header("Configs")]
  [SerializeField]
  float speed;
  [SerializeField]
  float minAngle;
  [SerializeField]
  float maxAngle;
  [SerializeField]
  float prepareSortieTime;

  BorneCraftMovement movement;
  bool isSortie;

  void Awake()
  {
    if (this.rb == null) {
      this.rb = this.GetComponent<Rigidbody>();
    }
    this.movement = this.InitMovement();
  }

  BorneCraftMovement InitMovement()
  {
    var movement = (new BorneCraftMovement(
      rigidbody: this.rb,
      transform: this.transform,
      configs: this.CreateConfigs() 
    ));
    movement.OnSortie += this.OnSorite;
    movement.OnReturnToShip += this.OnReturnToShip;
    return (movement);
  }

  BorneCraftMovement.Configs CreateConfigs()
  {
    return (new BorneCraftMovement.Configs(
      speed: this.speed,
      minAngle: this.minAngle,
      maxAngle: this.maxAngle,
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
    this.movement.Update(Time.deltaTime);
  }

  void OnValidate()
  {
    if (this.movement != null) {
      this.movement.configs = this.CreateConfigs();
    }
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
}
