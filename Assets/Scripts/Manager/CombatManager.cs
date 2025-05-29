using System;
using System.Collections.Generic;
using UnityEngine;
using Architecture;

public class CombatManager : SingletonBehaviour<CombatManager>
{
  public ObservableValue<IDamagable> SelectedEnemy { get; private set; }
  public (GameObject gameObject, IDamagable damagable) LastHitEnemy;
  int enemyLayer; 

  void Awake()
  {
    base.OnAwake();
    this.enemyLayer = (1 << LayerMask.NameToLayer("Enemy"));
    this.SelectedEnemy = new (null);
    this.LastHitEnemy = (null, null);
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
    UserInputManager.Shared.SelectedAttackPosition.OnChanged += this.HandleSelectScreen;
  }

  void HandleSelectScreen(Nullable<Vector2> position)
  {
    if (position != null) {
      var selectedEnemy = this.FindEnemy(position.Value); 
      if (selectedEnemy != null) {
        this.SelectedEnemy.Value = selectedEnemy;
      }
    }
  }

  IDamagable FindEnemy(Vector2 position)
  {
    Ray ray = Camera.main.ScreenPointToRay(new Vector3(position.x, position.y, 0));
    if (Physics.Raycast(
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
      this.SelectedEnemy.Value = null;
    }
  }
}
