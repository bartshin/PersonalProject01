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
  ObservableValue<(int, int)> motherShipHp;
  ObservableValue<(int, int)>[] borneCraftshipsHp;
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

  public void SetCraftshipPortraitVisible(int index, bool visible)
  {
    var portrait = this.hpView.CraftshipPortrait[index];
    this.hpView.SetHiddenTo(portrait, !visible);
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
