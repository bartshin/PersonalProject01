using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Architecture;

public class CombatManager : SingletonBehaviour<CombatManager>
{
  public enum AttackMode
  {
    Select,
    Aim
  }
  // TODO: Change to Deque
  public List<IDamagable> Targets { get; private set; }
  public ObservableValue<IDamagable> SelectedDamagable { get; private set; }
  public (GameObject gameObject, IDamagable damagable) LastHitEnemy;
  public AttackMode CurrentAttackMode = AttackMode.Select;
  public Dictionary<IDamagable, EnemyShip> enemies;
  int enemyLayer; 
  int maxTargetCount = 3;

  public void CancelAttack()
  {
    if (this.SelectedDamagable.Value != null) {
      this.AddTargetToFront(this.SelectedDamagable.Value);
      if (SelectedDamagable.Value != null) {
        this.OnDeselectDamagable(SelectedDamagable.Value);
      }
      this.SelectedDamagable.Value = null;
    }
  }

  public void ExecuteAttack()
  {
    if (this.Targets.Count > 0) {
      var first = this.Targets[0];
      this.SelectedDamagable.Value = first;
      if (this.Targets.Count == 0) {
        return ;
      }
      this.Targets.RemoveAt(0);
    }
  }

  public void OnSelectDamagable(IDamagable damagable, bool isPrimaryButton)
  {
    if (isPrimaryButton) {
      this.OnPrimarySelected(damagable);
    }
    else {
      this.OnSecondarySelected(damagable);
    }
  }

  void OnDeselectDamagable(IDamagable damagable)
  {
    if (this.enemies.TryGetValue(damagable, out EnemyShip enemy)) {
      enemy.OnDeselected();
    }
  }

  void Awake()
  {
    base.OnAwake();
    this.enemyLayer = (1 << LayerMask.NameToLayer("Enemy"));
    this.SelectedDamagable = new (null);
    this.LastHitEnemy = (null, null);
    this.enemies = new ();
    this.Targets = new (this.maxTargetCount + 1);
    this.SelectedDamagable.WillChange += (enemy) => enemy.OnDestroyed -= this.OnDamagableDestroyed;
    this.SelectedDamagable.OnChanged += (enemy) => {
      if (enemy != null) {
        enemy.OnDestroyed += this.OnDamagableDestroyed;
      }
    };
  }

  // Start is called before the first frame update
  void Start()
  {
    UserInputManager.Shared.PrimarySelectedScreenPosition.OnChanged += this.HandlePrimarySelect;
    UserInputManager.Shared.SecondarySelectedScreenPosition.OnChanged += this.HandleSecondarySelect;
  }

  void Update()
  {
    //FIXME: Remove test *********************************************
    if (Input.GetKeyDown(KeyCode.Alpha4)) {
      for (int i = 0; i < this.Targets.Count; i++) {
        Debug.Log($"{i}: {this.Targets[i].gameObject.name}"); 
      }
    }
    if (Input.GetKeyDown(KeyCode.Return)) {
      this.ExecuteAttack();
    }
    if (Input.GetKeyDown(KeyCode.Escape)) {
      this.CancelAttack();
    }
    //****************************************************************
  }

  void OnDestroy()
  {
    UserInputManager.Shared.PrimarySelectedScreenPosition.OnChanged -= this.HandlePrimarySelect;
    UserInputManager.Shared.SecondarySelectedScreenPosition.OnChanged -= this.HandleSecondarySelect;
    base.OnDestroyed();
  }

  void HandlePrimarySelect(Nullable<Vector2> position) => this.HandleSelectScreenPosition(position, true);

  void HandleSecondarySelect(Nullable<Vector2> position) => this.HandleSelectScreenPosition(position, false);

  void HandleSelectScreenPosition(Nullable<Vector2> position, bool isPrimaryButton)
  {
    if (position != null &&
        this.CurrentAttackMode == AttackMode.Select) {
      var damagable = this.FindEnemy(position.Value); 
      if (damagable == null) {
        return ;
      }
      if (!this.enemies.TryGetValue(damagable, out EnemyShip enemy)) {
        enemy = damagable.gameObject.GetComponent<EnemyShip>();
        if (enemy != null) {
          this.enemies.Add(damagable, enemy);
        }
      }
      if (enemy != null && this.Targets.IndexOf(damagable) == -1) {
        enemy.OnSelected();
      }
      this.OnSelectDamagable(damagable, isPrimaryButton);
    }
  }

  void SwapTargetToFirst(int index)
  {
    if (index == 0 || this.Targets.Count == 1) {
      return ;
    }
    var first = this.Targets[0];
    this.Targets[0] = this.Targets[index];
    this.Targets[index] = first;
  }

  void AddTargetToFront(IDamagable target)
  {
    this.Targets.Insert(0, target);
    while (this.Targets.Count == this.maxTargetCount) {
      this.Targets.RemoveAt(this.Targets.Count - 1);
    }
  }

  void OnPrimarySelected(IDamagable damagable)
  {
    var index = this.Targets.IndexOf(damagable);
    if (index != -1) {
      this.OnDeselectDamagable(damagable);
      this.Targets.RemoveAt(index);
    }
    else {
      this.Targets.Add(damagable);
    }
  }

  void OnSecondarySelected(IDamagable damagable)
  {
    var index = this.Targets.IndexOf(damagable);
    if (index != -1) {
      this.Targets.RemoveAt(index);
    }
    else if (this.SelectedDamagable.Value != null &&
        damagable != this.SelectedDamagable.Value) {
      this.AddTargetToFront(this.SelectedDamagable.Value); 
    }
    if (damagable != this.SelectedDamagable.Value) {
      this.SelectedDamagable.Value = damagable;
    }
    else {
      if (this.enemies.TryGetValue(damagable, out EnemyShip enemy)) {
        enemy.OnDeselected();
      }
      this.SelectedDamagable.Value = null;
    }
  }

  IDamagable FindEnemy(Vector2 position)
  {
    Ray ray = Camera.main.ScreenPointToRay(new Vector3(position.x, position.y, 0));
    if (
        !EventSystem.current.IsPointerOverGameObject() &&
        Physics.Raycast(
          ray: ray, 
          hitInfo: out RaycastHit hit,
          maxDistance: Mathf.Infinity,
          layerMask: this.enemyLayer
          )) {
      return (hit.collider.GetComponent<IDamagable>());
    }
    else {
      return (null);
    }
  }

  void OnDamagableDestroyed(IDamagable damagable)
  {
    this.enemies.Remove(damagable);
    if (this.SelectedDamagable.Value == damagable) {
      if (this.Targets.Count > 0) {
        this.OnSecondarySelected(this.Targets[0]);
      }
      else {
        this.SelectedDamagable.Value = null;
      }
    }
  }
}
