using System;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class MotherShip : MonoBehaviour
{
  [Header("References")]
  [SerializeField]
  StatusController status;
  [SerializeField]
  Rigidbody rb;
  [SerializeField]
  GameObject laserPrefab;
  [SerializeField]
  GameObject missilePrefab;

  [Header("BorneCraft")]
  public GameObject[] bornCraftPrefabs;
  public BorneCraft.Configs[] bornCraftConfigs;

  [Header("BorneCraft Configs")]
  [SerializeField]
  float craftShipBatteryEffiency;
  [SerializeField]
  float craftShipBatteryMaxEffiency;
  
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
  [SerializeField]
  float accelerationEffiency;
  [SerializeField]
  float maxSpeedEffiency;
  [SerializeField]
  float boosterRestoreEffiency;
  [SerializeField]
  float boosterPowerEffiency;

  [Header("Sideattack Configs")]
  [SerializeField]
  float laserDelay;
  [SerializeField]
  float laserSpeed;
  [SerializeField]
  int laserPower;
  [SerializeField]
  float laserLifeTime;
  [SerializeField]
  float missileDelay;
  [SerializeField]
  float missileInitialSpeed;
  [SerializeField]
  float missileAcceleration;
  [SerializeField]
  int missilePower;
  [SerializeField]
  float missileLifeTime;

  MotherShipMovement movement;
  MotherShipSideAttack sideAttack;
  List<BorneCraft> borneCrafts;
  IDamagable currentCraftTarget;
  float craftShipBattery;
  float craftShipBatteryMax;
  float craftShipBatteryCharge;

  void Awake()
  {
    if (this.rb == null) {
      this.rb = this.GetComponent<Rigidbody>();
    }
    if (this.status == null) {
      this.status = this.GetComponent<StatusController>();
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
    gameObject.transform.localPosition = BorneCraftMovement.BASE_POSITION;
    var craft = gameObject.GetComponent<BorneCraft>();
    craft.CraftConfigs = configs;
    craft.OnReturned += this.OnCraftShipReturned;
    return (craft);
  }

  MotherShipSideAttack InitSideAttack()
  {
    var sideAttack = new MotherShipSideAttack(
      ship: this.gameObject,
      laserPrefab: this.laserPrefab,
      missilePrefab: this.missilePrefab,
      configs: this.CreateSideAttackConfigs()
    );
    return (sideAttack); 
  }

  MotherShipSideAttack.Configs CreateSideAttackConfigs()
  {
    return (new MotherShipSideAttack.Configs {
      LaserDelay = this.laserDelay,
      LaserPower = this.laserPower,
      LaserSpeed = this.laserSpeed,
      LaserLifeTime = this.laserLifeTime,
      MissileDelay = this.missileDelay,
      MissilePower = this.missilePower,
      MissileInitialSpeed = this.missileInitialSpeed,
      MissileAcceleration = this.missileAcceleration,
      MissileLifeTime = this.missileLifeTime,
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

  void Start()
  {
    this.SetMovementSpeedPower(this.status.Distribution.MotherShipSpeed.Value);
    this.status.Distribution.MotherShipSpeed.OnChanged += this.SetMovementSpeedPower;
    this.SetBoosterPower(this.status.Distribution.MotherShipBooster.Value);
    this.status.Distribution.MotherShipBooster.OnChanged += this.SetBoosterPower;
    this.SetCraftShipBarrierPower(this.status.Distribution.CraftShipBarrier.Value);
    this.status.Distribution.MotherShipBarrier.OnChanged += this.SetCraftShipBarrierPower;
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
    //FIXME: Remove Test code ***********************
    if (Input.GetKeyDown(KeyCode.Alpha6)) {
      Debug.Log($"Battery: {this.craftShipBattery}/{this.craftShipBatteryMax}");
    }
    //***********************************************
    this.UpdateCraftShipBattery(Time.deltaTime);
    this.movement.Update(Time.deltaTime);
    this.sideAttack.Update(Time.deltaTime);
  }

  void OnValidate()
  {
    if (this.movement != null) {
      this.movement.configs = this.CreateMovementConfigs();
      if (this.status != null && this.status.Distribution != null) {
        this.SetMovementSpeedPower(this.status.Distribution.MotherShipSpeed.Value);
      }
    }
    if (this.sideAttack != null) {
      this.sideAttack.configs = this.CreateSideAttackConfigs();
    }
    if (this.status != null && this.status.Distribution != null) {
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
    if (this.currentCraftTarget != null) {
      this.currentCraftTarget.OnDestroyed -= this.OnEnemyDestroyed;
      this.OnEnemyDeselected();
    }
    if (enemy != null) {
      this.currentCraftTarget = enemy;
      enemy.OnDestroyed += this.OnEnemyDestroyed;
      foreach (var craft in this.borneCrafts) {
        craft.SelectEnemy(enemy); 
      }
    }
    else {
      this.currentCraftTarget = null;
      foreach (var craft in this.borneCrafts) {
        craft.DeselectEnemy(); 
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
    UserInputManager.Shared.IsUsingPointer = direction == null;
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
    CombatManager.Shared.CurrentAttackMode = direction == null ? 
      CombatManager.AttackMode.Select: CombatManager.AttackMode.Aim;
  }

  void SetMovementSpeedPower(int power)  
  {
    this.movement.configs.MaxSpeed = this.maxSpeedEffiency * power;
    this.movement.configs.Acceleration = this.accelerationEffiency * power;
  }

  void SetBoosterPower(int power)
  {
    this.movement.configs.BoosterPower = this.boosterPowerEffiency * power;
    this.movement.configs.BoosterRestore = this.boosterRestoreEffiency * power;
  }

  void SetCraftShipBarrierPower(int power) 
  {
    this.craftShipBatteryMax = this.craftShipBatteryMaxEffiency * power;
    this.craftShipBattery = Math.Min(
      this.craftShipBattery, this.craftShipBatteryMax);
    this.craftShipBatteryCharge = this.craftShipBatteryEffiency * power;
  }

  void UpdateCraftShipBattery(float deltaTime)
  {
    this.craftShipBattery = Math.Min(
      this.craftShipBattery + this.craftShipBatteryCharge * deltaTime,
      this.craftShipBatteryMax
    );
  }

  void OnCraftShipReturned(BorneCraft craftShip) 
  {
    var restored = craftShip.RestoreBarrier((int)this.craftShipBattery);
    this.craftShipBattery -= (float)restored;
  }
}
