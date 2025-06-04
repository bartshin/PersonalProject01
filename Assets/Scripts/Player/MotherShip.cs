using System;
using System.Collections.Generic;
using UnityEngine;
using Architecture;

public class MotherShip : MonoBehaviour
{

  [Header("References")]
  [SerializeField]
  GameObject interior;
  [SerializeField]
  StatusController status;
  [SerializeField]
  MotherShipHealth health;
  [SerializeField]
  Rigidbody rb;
  [SerializeField]
  GameObject bulletPrefab;
  [SerializeField]
  GameObject missilePrefab;
  [SerializeField]
  GameObject missileExplosion;
  [SerializeField]
  GameObject muzzleFlashPrefab;

  [Header("BorneCraft")]
  public GameObject[] bornCraftPrefabs;
  public BorneCraft.Configs[] bornCraftConfigs;

  [Header("BorneCraft Configs")]
  [SerializeField]
  float craftshipBatteryEffiency;
  [SerializeField]
  float craftshipBatteryMaxEffiency;
  
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
  float bulletDelay;
  [SerializeField]
  float bulletSpeed;
  [SerializeField]
  int bulletPower;
  [SerializeField]
  float bulletLifeTime;
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
  Dictionary<BorneCraft, (Camera, int)> borneCraftCamera;
  IDamagable currentCraftTarget;
  ObservableValue<(float current, float max)> craftshipBattery;
  float craftshipBatteryCharge;

  void Awake()
  {
    if (this.rb == null) {
      this.rb = this.GetComponent<Rigidbody>();
    }
    if (this.status == null) {
      this.status = this.GetComponent<StatusController>();
    }
    if (this.health == null) {
      this.health = this.GetComponent<MotherShipHealth>();
    }
    this.craftshipBattery = new ((0f, 0f));
    this.movement = this.InitMovement();
    this.sideAttack = this.InitSideAttack();
    this.InitBornCrafts();
    this.borneCraftCamera = new (10);
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
    craft.OnReturned += this.OnCraftshipReturned;
    return (craft);
  }

  MotherShipSideAttack InitSideAttack()
  {
    var sideAttack = new MotherShipSideAttack(
      ship: this.gameObject,
      bulletPrefab: this.bulletPrefab,
      muzzleFlashPrefab: this.muzzleFlashPrefab,
      missilePrefab: this.missilePrefab,
      missileExplosion: this.missileExplosion,
      configs: this.CreateSideAttackConfigs()
    );
    return (sideAttack); 
  }

  MotherShipSideAttack.Configs CreateSideAttackConfigs()
  {
    return (new MotherShipSideAttack.Configs {
      BulletDelay = this.bulletDelay,
      BulletPower = this.bulletPower,
      BulletSpeed = this.bulletSpeed,
      BulletLifeTime = this.bulletLifeTime,
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
    this.SetCraftshipBarrierPower(this.status.Distribution.CraftshipBattery.Value);
    this.status.Distribution.MotherShipBarrier.OnChanged += this.SetCraftshipBarrierPower;
    this.SetCraftshipBarrierPower(this.status.Distribution.MotherShipBarrier.Value);
    this.interior.transform.localPosition = CameraManager.Shared.SideviewOffset;
    this.ShowUI();
  }

  void OnEnable()
  {
    CombatManager.Shared.SelectedDamagable.OnChanged += this.OnSelectedEnemyChanged;
    CameraManager.Shared.ActiveSideCamera.OnChanged += this.OnSideCameraChanged;
    CameraManager.Shared.SetPlayerShip(this.transform);
  }

  void OnDisable()
  {
    CombatManager.Shared.SelectedDamagable.OnChanged -= this.OnSelectedEnemyChanged;
    CameraManager.Shared.ActiveSideCamera.OnChanged -= this.OnSideCameraChanged;
    CameraManager.Shared.UnsetPlayerShip();
  }

  // Update is called once per frame
  void Update()
  {
    //FIXME: Remove Test code ***********************
    if (Input.GetKeyDown(KeyCode.Alpha6)) {
      Debug.Log($"Battery: {this.craftshipBattery.Value.current}/{this.craftshipBattery.Value.max}");
    }
    //***********************************************
    this.UpdateCraftshipBattery(Time.deltaTime);
    this.movement.Update(Time.deltaTime);
    this.sideAttack.Update(Time.deltaTime);
    if (this.sideAttack.IsActive) {
      this.UpdateInterior();
    }
  }

  void ShowUI()
  {
    var borneCraftsStatus = new (
        ObservableValue<(int, int)>,
        ObservableValue<(int, int)>)[this.borneCrafts.Count];
    for (int i = 0; i < this.borneCrafts.Count; ++i) {
      borneCraftsStatus[i] = (this.borneCrafts[i].Hp, this.borneCrafts[i].Barrier);
    }
    UIManager.Shared.SetHp(
      (this.health.Hp, this.health.Barrier),
      borneCraftsStatus 
    );
    var craftshipCullingMask = (1 << (LayerMask.NameToLayer("PlayerBorneCraft")));
    for (int i = 0; i < this.borneCrafts.Count; i++) {
      var texture = UIManager.Shared.CraftshipTextures[i]; 
      var borneCraft = this.borneCrafts[i];
      var camera = CameraManager.Shared.CreateCameraFor(
          texture, borneCraft.Ship); 
      camera.enabled = false;
      camera.cullingMask = craftshipCullingMask;
      camera.clearFlags = CameraClearFlags.SolidColor;
      camera.backgroundColor = UIManager.CRAFTSHIP_PORTRAIT_BACKGROUND_COLOR;
      this.borneCraftCamera[borneCraft] = (camera, i);
      borneCraft.OnSortie += this.OnBorneCraftSortie;
      borneCraft.OnReturned += this.OnBorneCraftReturned;
    }
    UIManager.Shared.SetBooster(this.movement.BoosterGauge);
    UIManager.Shared.SetBattery(this.craftshipBattery);
  }

  void OnBorneCraftSortie(BorneCraft craftship)
  {
    var (cam, index) = this.borneCraftCamera[craftship]; 
    UIManager.Shared.ShowCraftshipPortrait(index);
    CameraManager.Shared.StartRender(cam, 1f);
  }

  void OnBorneCraftReturned(BorneCraft craftship)
  {
    var (cam, index) = this.borneCraftCamera[craftship]; 
    UIManager.Shared.HideCraftshipPortrait(index);
    CameraManager.Shared.StopRender(cam);
  }

  void UpdateInterior()
  {
    var rotation = this.sideAttack.AimDirection.eulerAngles;
    var (x, z) = (rotation.x, rotation.z);
    if (x > 180) {
      x -= 360f;
    }
    if (z > 180) {
      z -= 360f;
    }
    this.interior.transform.rotation = Quaternion.Euler(
        Mathf.Lerp(this.rb.rotation.x, x, 0.8f),
        rotation.y,
        Mathf.Lerp(this.rb.rotation.z, z, 0.8f)
        );
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
    this.movement.IsRotatable = direction == null;
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

  void SetCraftshipBarrierPower(int power) 
  {
    var (current, _) = this.craftshipBattery.Value;
    var newBatteryMax = power * this.craftshipBatteryMaxEffiency;
    this.craftshipBattery.Value = (Math.Min(current, newBatteryMax), newBatteryMax);
    this.craftshipBatteryCharge = this.craftshipBatteryEffiency * power;
  }

  void UpdateCraftshipBattery(float deltaTime)
  {
    var (current, max) = this.craftshipBattery.Value;
    this.craftshipBattery.Value = (Math.Min(
      current + this.craftshipBatteryCharge * deltaTime,
      max
    ), max);
  }

  void OnCraftshipReturned(BorneCraft craftship) 
  {
    var restored = craftship.RestoreBarrier(
        (int)(this.craftshipBattery.Value).current);
    var (current, max) = this.craftshipBattery.Value;
    this.craftshipBattery.Value = (current - (float)restored, max);
  }
}
