using System;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class MotherShip : MonoBehaviour
{
  [Header("References")]
  [SerializeField]
  Rigidbody rb;
  [SerializeField]
  GameObject sideAttackProjectilePrefab;

  [Header("BorneCraft")]
  public GameObject[] bornCraftPrefabs;
  public BorneCraft.Configs[] bornCraftConfigs;

  [Header("Movement Configs")]
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
  [SerializeField]
  float boosterPower;
  [SerializeField]
  float boosterRestore;
  [SerializeField]
  float boosterConsume;

  [Header("Sideattack Configs")]
  [SerializeField]
  float sideAttackDelay;
  [SerializeField]
  float sideAttackProjectileSpeed;
  [SerializeField]
  float sideAttackPower;
  [SerializeField]
  float sideAttackProjectileLifeTime;

  MotherShipMovement movement;
  MotherShipSideAttack sideAttack;
  List<BorneCraft> borneCrafts;
  IDamagable currentCraftTarget;

  void Awake()
  {
    if (this.rb == null) {
      this.rb = this.GetComponent<Rigidbody>();
    }
    this.movement = this.InitMovement();
    this.sideAttack = this.InitSideAttack();
    this.InitBornCrafts();
  }

  void InitBornCrafts()
  {
    this.borneCrafts = new ();
    for (int i = 0; i < this.bornCraftPrefabs.Length; ++i) {
      this.borneCrafts.Add(
        this.SpawnBornCraft(
          this.bornCraftPrefabs[i], this.bornCraftConfigs[i])); 
    }
  }

  BorneCraft SpawnBornCraft(GameObject prefab, BorneCraft.Configs configs)
  {
    var gameObject = Instantiate(prefab);
    gameObject.transform.parent = this.transform;
    gameObject.transform.position = this.transform.position;
    var craft = gameObject.GetComponent<BorneCraft>();
    craft.CraftConfigs = configs;
    return (craft);
  }

  MotherShipSideAttack InitSideAttack()
  {
    var sideAttack = new MotherShipSideAttack(
      ship: this.transform,
      projectilePrefab: this.sideAttackProjectilePrefab,
      configs: this.CreateSideAttackConfigs()
    );
    return (sideAttack); 
  }

  MotherShipSideAttack.Configs CreateSideAttackConfigs()
  {
    return (new MotherShipSideAttack.Configs {
      Delay = this.sideAttackDelay,
      Power = this.sideAttackPower,
      ProjectileSpeed = this.sideAttackProjectileSpeed,
      LifeTime = this.sideAttackProjectileLifeTime,
    });
  }

  MotherShipMovement InitMovement()
  {
    return (new MotherShipMovement(
      rigidbody: this.rb,
      transform: this.transform,
      configs: this.CreateMovementConfigs()
     )
    );
  }

  MotherShipMovement.Configs CreateMovementConfigs()
  {
    return (new MotherShipMovement.Configs {
      TurningSpeed = this.turningSpeed,
      Acceleration = this.acceleration,
      VerticalAcceleration = this.verticalAcceleration,
      MaxSpeed = this.maxSpeed,
      AngularVelocityThreshold = this.angularVelocityThreshold,
      AngularVelocityDecreseRatio = this.angularVelocityDecreseRatio,
      BoosterPower = this.boosterPower,
      BoosterRestore = this.boosterRestore,
      BoosterConsume = this.boosterConsume,
    });
  }

  // Start is called before the first frame update
  void Start()
  {
  }

  void OnEnable()
  {
    CombatManager.Shared.SelectedEnemy.OnChanged += this.OnSelectedEnemyChanged;
    CameraManager.Shared.ActiveSideCamera.OnChanged += this.OnSideCameraChanged;
    CameraManager.Shared.SetPlayerShip(this.transform);
  }

  void OnDisable()
  {
    CombatManager.Shared.SelectedEnemy.OnChanged -= this.OnSelectedEnemyChanged;
    CameraManager.Shared.ActiveSideCamera.OnChanged -= this.OnSideCameraChanged;
    CameraManager.Shared.UnsetPlayerShip();
  }

  // Update is called once per frame
  void Update()
  {
    this.movement.Update(Time.deltaTime);
    this.sideAttack.Update(Time.deltaTime);
  }

  void OnValidate()
  {
    if (this.movement != null) {
      this.movement.configs = this.CreateMovementConfigs();
    }
    if (this.sideAttack != null) {
      this.sideAttack.configs = this.CreateSideAttackConfigs();
    }
  }

  void OnDestroy()
  {
    if (CameraManager.Shared != null) {
      CameraManager.Shared.UnsetPlayerShip();
    }
  }

  void OnSelectedEnemyChanged(IDamagable enemy) 
  {
    if (enemy != null) {
      if (this.currentCraftTarget != null) {
        this.currentCraftTarget.OnDestroyed -= this.OnEnemyDestroyed;
        this.OnEnemyDeselected();
      }
      this.currentCraftTarget = enemy;
      enemy.OnDestroyed += this.OnEnemyDestroyed;
      foreach (var craft in this.borneCrafts) {
        craft.SelectEnemy(enemy); 
      }
    }
  }

  void OnEnemyDeselected()
  {
    this.currentCraftTarget = null;
    foreach (var craft in this.borneCrafts) {
      craft.DeselectEnemy(); 
    }
  }

  void OnEnemyDestroyed(IDamagable enemy)
  {
    if (enemy == this.currentCraftTarget) {
      this.OnEnemyDeselected();
      enemy.OnDestroyed -= this.OnEnemyDestroyed;
    }
  }

  void OnSideCameraChanged(Nullable<Direction> direction) 
  {
    UserInputManager.Shared.IsTrackingMouse = direction != null;
    if (direction != null) {
      if (direction.Value == Direction.Left) {
        this.sideAttack.AimDirection = Quaternion.LookRotation(this.transform.right * -1f);
      }
      else {
        this.sideAttack.AimDirection = Quaternion.LookRotation(this.transform.right);
      }
      this.sideAttack.AttackDirection = direction.Value;
    }
    this.sideAttack.IsActive = direction != null;
  }
}
