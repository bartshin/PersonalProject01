using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Architecture;

public class UIManager : SingletonBehaviour<UIManager>
{

  CombatUI combatUI;

  public enum PopupUI
  {
    ItemSelect,
    LevelUp,
    StageEnd
  }
  GameObject combatUIPrefab;
  GameObject loadingUIPrefab;

  public void SetHp(
      (ObservableValue<(int, int)> hp, ObservableValue<(int, int)> barrier) motherShip,
      (ObservableValue<(int, int)> hp, ObservableValue<(int, int)> barrier)[] borneCraftShips
      ) => this.combatUI.SetHp(motherShip, borneCraftShips);

  void Awake()
  {
    base.OnAwake();
    this.Init();
  }

  void Init()
  {
    this.combatUIPrefab = ((GameObject)Resources.Load("Prefabs/" + CombatUI.PREFAB_NAME));
    this.combatUI = Instantiate(this.combatUIPrefab).GetComponent<CombatUI>();
    this.combatUI.transform.parent = this.transform;
  }
}
