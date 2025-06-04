using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Architecture;

public class CombatUI : MonoBehaviour 
{
  public const string PREFAB_NAME = "CombatUI";
  public bool IsShowing { get; private set; }
  public RenderTexture[] CraftshipTextures => this.hpView.CraftshipTextures;
  public StatusController.Field SelectedField => StatusController.ALL_FIELDS[this.statusControlView.CursorIndex];
  ObservableValue<(int, int)> motherShipHp;
  ObservableValue<(int, int)>[] borneCraftshipsHp;
  const string CONTAINER_NAME = "combatUI-container";
  VisualElement root;
  PlayerHpView hpView;
  StatusControlView statusControlView;
  TimeView timeView;

  public void Show() 
  { 
    this.root.visible = true;
    this.IsShowing = true;
    this.root.BringToFront();
  }

  public void Hide() 
  { 
    this.root.visible = false;
    this.IsShowing = false;
    this.root.SendToBack();
  }

  public void SetTime(int seconds) => this.timeView.SetTime(seconds);

  public void SetBooster(ObservableValue<float> booster) 
  {
    booster.OnChanged += this.OnMotherShipBoosterChanged;
    this.OnMotherShipBoosterChanged(booster.Value);
  }

  public void SetBattery(ObservableValue<(float, float)> battery)
  {
    battery.OnChanged += this.OnMotherShipBatteryChanged;
    this.OnMotherShipBatteryChanged(battery.Value);
  }

  public void SetPowerDistribution(StatusController.PowerDistribution distribution)
  {
    distribution.MotherShipBarrier.OnChanged += this.OnBarrierPowerChanged;
    this.OnBarrierPowerChanged(distribution.MotherShipBarrier.Value);
    distribution.MotherShipBooster.OnChanged += this.OnBoosterPowerChanged;
    this.OnBoosterPowerChanged(distribution.MotherShipBooster.Value);
    distribution.MotherShipSpeed.OnChanged += this.OnSpeedPowerChanged;
    this.OnSpeedPowerChanged(distribution.MotherShipSpeed.Value);
    distribution.CraftshipBattery.OnChanged += this.OnBatteryPowerChanged;
    this.OnBatteryPowerChanged(distribution.CraftshipBattery.Value);

    distribution.ExtraPower.OnChanged += this.OnExtraPowerChanged;
    this.OnExtraPowerChanged(distribution.ExtraPower.Value);
  }

  void OnBarrierPowerChanged(int power) =>
    this.statusControlView.SetFieldValue(StatusController.Field.Barrier, power);
  
  void OnBoosterPowerChanged(int power) =>
    this.statusControlView.SetFieldValue(StatusController.Field.Booster, power);

  void OnSpeedPowerChanged(int power) =>
    this.statusControlView.SetFieldValue(StatusController.Field.Speed, power);

  void OnBatteryPowerChanged(int power) =>
    this.statusControlView.SetFieldValue(StatusController.Field.Battery, power);

  void OnExtraPowerChanged(int power) =>
    this.statusControlView.SetExtraValue(power);

  public void SetCraftshipPortraitVisible(int index, bool visible)
  {
    var (portrait, placeholder) = this.hpView.CraftshipPortrait[index];
    this.hpView.SetHiddenTo(portrait, !visible);
    this.hpView.SetHiddenTo(placeholder, visible);
  }

  public void SetHp(
      (ObservableValue<(int, int)> hp, ObservableValue<(int, int)> barrier) motherShip,
      (ObservableValue<(int, int)> hp, ObservableValue<(int, int)> barrier)[] borneCraftships)
  {
    this.hpView.CreateMotherShipStatus();
    this.hpView.CreateBronecraftStatus(borneCraftships.Length);
    motherShip.hp.OnChanged += this.OnMotherShipHpChanged;
    motherShip.barrier.OnChanged += this.OnMotherShipBarrierChanged;

    for (int i = 0; i < borneCraftships.Length; ++i) {
      var index = i;
      borneCraftships[i].hp.OnChanged += (hp) => this.OnCraftshipHpChanged(index, hp);
      borneCraftships[i].barrier.OnChanged += (barrier) => this.OnCraftshipBarrierChanged(index, barrier);
    }
  }

  void Awake()
  {
    this.Init();
    this.CreateUI();
    this.Hide();
    UserInputManager.Shared.NavigateDirection.OnChanged += this.OnNavigationChanged;
  }

  void OnNavigationChanged(Nullable<Direction> dir)
  {
    if (dir == Direction.Left) {
      this.statusControlView.MoveCursorToPrev();
    }
    else if (dir == Direction.Right) {
      this.statusControlView.MoveCursorToNext();
    }
  }

  void CreateUI()
  {
    this.timeView = new();
    this.root.Add(this.timeView);
    this.hpView = new();
    this.root.Add(this.hpView);
    this.statusControlView = new();
    this.root.Add(this.statusControlView);
  }

  // Update is called once per frame
  void Update()
  {
    if (this.IsShowing) {

    }
  }


  void Init()
  {
    this.root = this.GetComponent<UIDocument>().rootVisualElement;
    this.root.name = CombatUI.CONTAINER_NAME;
    this.root.style.width = Length.Percent(100);
    this.root.style.height = Length.Percent(100);
  }

  void OnMotherShipBoosterChanged(float booster) {
    this.hpView.SetValue( 
      this.hpView.MotherShipHandle.booster,
      booster/ 100f 
    );
  }

  void OnMotherShipBatteryChanged((float current, float max) battery)
  {
    this.hpView.SetValue(
      this.hpView.MotherShipHandle.battery, battery.current / battery.max);
  }

  void OnMotherShipHpChanged((int current, int max) hp)
  {
    float percentage = (float)hp.current / (float)hp.max;
    this.hpView.SetValue(this.hpView.MotherShipHandle.hp, percentage);
  }

  void OnMotherShipBarrierChanged((int current, int max) barrier)
  {
    float percentage = (float)barrier.current / (float)barrier.max;
    this.hpView.SetValue(this.hpView.MotherShipHandle.barrier, percentage);
  }

  void OnCraftshipHpChanged(int index, (int current, int max) hp)
  {
    float percentage = (float)hp.current / (float)hp.max;
    this.hpView.SetValue(this.hpView.CraftshipHpHandles[index].hp, percentage);
  }

  void OnCraftshipBarrierChanged(int index, (int current, int max) barrier)
  {
    float percentage = (float)barrier.current / (float)barrier.max;
    this.hpView.SetValue(this.hpView.CraftshipHpHandles[index].barrier, percentage);
  }
}
