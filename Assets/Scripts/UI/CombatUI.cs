using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Architecture;

public class CombatUI : MonoBehaviour 
{
  public const string PREFAB_NAME = "CombatUI";
  public bool IsShowing { get; private set; }
  ObservableValue<(int, int)> motherShipHp;
  ObservableValue<(int, int)>[] borneCraftShipsHp;
  const string CONTAINER_NAME = "combatUI-container";
  VisualElement root;
  PlayerHpView hpView;

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

  public void SetHp(
      (ObservableValue<(int, int)> hp, ObservableValue<(int, int)> barrier) motherShip,
      (ObservableValue<(int, int)> hp, ObservableValue<(int, int)> barrier)[] borneCraftShips)
  {
    this.hpView.CreateMotherShipStatus();
    this.hpView.CreateBronecraftStatus(borneCraftShips.Length);
    motherShip.hp.OnChanged += this.OnMotherShipHpChanged;
    motherShip.barrier.OnChanged += this.OnMotherShipBarrierChanged;

    for (int i = 0; i < borneCraftShips.Length; ++i) {
      var index = i;
      borneCraftShips[i].hp.OnChanged += (hp) => this.OnBornecraftShipHpChanged(index, hp);
      borneCraftShips[i].barrier.OnChanged += (barrier) => this.OnBornecraftShipBarrierChanged(index, barrier);
    }
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

  void OnBornecraftShipHpChanged(int index, (int current, int max) hp)
  {
    float percentage = (float)hp.current / (float)hp.max;
    this.hpView.SetValue(this.hpView.BornecraftShipHpHandles[index].hp, percentage);
  }

  void OnBornecraftShipBarrierChanged(int index, (int current, int max) barrier)
  {
    float percentage = (float)barrier.current / (float)barrier.max;
    this.hpView.SetValue(this.hpView.BornecraftShipHpHandles[index].barrier, percentage);
  }

  void Awake()
  {
    this.Init();
    this.CreateUI();
  }

  void Start()
  {
  }

  void CreateUI()
  {
    this.hpView = new();
    this.root.Add(this.hpView);
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
}
