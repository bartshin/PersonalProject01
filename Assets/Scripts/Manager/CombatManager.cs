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
  public ObservableValue<IDamagable> SelectedEnemy { get; private set; }
  public (GameObject gameObject, IDamagable damagable) LastHitEnemy;
  public AttackMode CurrentAttackMode = AttackMode.Select;
  int enemyLayer; 
  int maxTargetCount = 3;

  public void CancelAttack()
  {
    if (this.SelectedEnemy.Value != null) {
      this.AddTargetToFront(this.SelectedEnemy.Value);
      this.SelectedEnemy.Value = null;
    }
  }

  public void ExecuteAttack()
  {
    if (this.Targets.Count > 0) {
      var first = this.Targets[0];
      this.SelectedEnemy.Value = first;
      if (this.Targets.Count == 0) {
        return ;
      }
      this.Targets.RemoveAt(0);
    }
  }

  public void OnSelectEnemy(IDamagable enemy, bool isPrimaryButton)
  {
    if (isPrimaryButton) {
      var index = this.Targets.IndexOf(enemy);
      if (index != -1) {
        this.Targets.RemoveAt(index);
      }
      else {
        this.Targets.Add(enemy);
      }
    }
    else {
      this.StartAttack(enemy);
    }
  }

  void Awake()
  {
    base.OnAwake();
    this.enemyLayer = (1 << LayerMask.NameToLayer("Enemy"));
    this.SelectedEnemy = new (null);
    this.LastHitEnemy = (null, null);
    this.Targets = new (this.maxTargetCount + 1);
    this.SelectedEnemy.WillChange += (enemy) => enemy.OnDestroyed -= this.OnEnemyDestroyed;
    this.SelectedEnemy.OnChanged += (enemy) => {
      if (enemy != null) {
        enemy.OnDestroyed += this.OnEnemyDestroyed;
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
      var selectedEnemy = this.FindEnemy(position.Value); 
      if (selectedEnemy == null) {
        return ;
      }
      this.OnSelectEnemy(selectedEnemy, isPrimaryButton);
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

  void StartAttack(IDamagable target)
  {
    var index = this.Targets.IndexOf(target);
    if (index != -1) {
      this.Targets.RemoveAt(index);
    }
    else if (this.SelectedEnemy.Value != null &&
        target != this.SelectedEnemy.Value) {
      this.AddTargetToFront(this.SelectedEnemy.Value); 
    }
    if (target != this.SelectedEnemy.Value) {
      this.SelectedEnemy.Value = target;
    }
    else {
      this.SelectedEnemy.Value = null;
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

  void OnEnemyDestroyed(IDamagable enemy)
  {
    if (this.SelectedEnemy.Value == enemy) {
      if (this.Targets.Count > 0) {
        this.StartAttack(this.Targets[0]);
      }
      else {
        this.SelectedEnemy.Value = null;
      }
    }
  }
}
