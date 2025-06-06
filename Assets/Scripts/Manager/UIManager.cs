using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Architecture;

public class UIManager : SingletonBehaviour<UIManager>
{
  public static readonly Color CRAFTSHIP_PORTRAIT_BACKGROUND_COLOR = new Color(
      51f/255f, 51f/255f, 51f/255f, 0.5f);
  CombatUI combatUI;
  GreetingUI greetingUI;

  public enum PopupUI
  {
    ItemSelect,
    LevelUp,
    StageEnd
  }
  public StatusController.Field SelectedField => this.combatUI.SelectedField;
  GameObject combatUIPrefab;
  GameObject greetingUIPrefab;

  public RenderTexture[] CraftshipTextures => this.combatUI.CraftshipTextures;
  public void SetTime(int seconds) => this.combatUI?.SetTime(seconds);

  public void SetBooster(ObservableValue<float> booster) => this.combatUI.SetBooster(booster);

  public void SetBattery(ObservableValue<(float, float)> battery) => this.combatUI.SetBattery(battery);

  public void SetHp(
      (ObservableValue<(int, int)> hp, ObservableValue<(int, int)> barrier) motherShip,
      (ObservableValue<(int, int)> hp, ObservableValue<(int, int)> barrier)[] borneCraftships
      ) => this.combatUI.SetHp(motherShip, borneCraftships);

  public void SetDistribution(StatusController.PowerDistribution distribution) => this.combatUI.SetPowerDistribution(distribution);

  public void ShowCraftshipPortrait(int index) => this.combatUI.SetCraftshipPortraitVisible(index, true);

  public void HideCraftshipPortrait(int index) => this.combatUI.SetCraftshipPortraitVisible(index, false);

  public void SetVisbleCombatUI(bool visible) {
    if (visible) {
      this.combatUI?.Show();
    }
    else {
      this.combatUI?.Hide();
    }
  }

  public void SetGameOverUI(bool isWin) => this.greetingUI.SetGameOverUI(isWin);

  public void SetVisbleGreetingUI(bool visible) {
    if (visible) {
      this.greetingUI?.Show();
    }
    else {
      this.greetingUI?.Hide();
    }
  }

  void Awake()
  {
    base.OnAwake();
    this.Init();
  }

  void Init()
  {
    this.greetingUIPrefab = ((GameObject)Resources.Load("Prefabs/GreetingUI"));
    this.greetingUI = Instantiate(this.greetingUIPrefab).GetComponent<GreetingUI>();
    this.greetingUI.transform.parent = this.transform;
    this.combatUIPrefab = ((GameObject)Resources.Load("Prefabs/" + CombatUI.PREFAB_NAME));
    this.combatUI = Instantiate(this.combatUIPrefab).GetComponent<CombatUI>();
    this.combatUI.transform.parent = this.transform;
  }
}
