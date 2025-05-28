using System;
using System.Collections.Generic;
using UnityEngine;
using Architecture;

public class EnemyHealth : MonoBehaviour, IDamagable
{
  [Header("Configs")]
  [SerializeField]
  int maxHp;
  [SerializeField]
  int defense;
  public Action<EnemyHealth> OnDestroyed;

  public ObservableValue<(int current, int max)> Hp;

  void Awake()
  {
    this.Hp = new((this.maxHp, this.maxHp));
  }

  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {

  }

  public int TakeDamage(int attackDamage)
  {
    var damage = Math.Max(attackDamage - this.defense, 0);
    var damageTaken = Math.Min(
      damage,
      this.Hp.Value.current
    );
    var (current, max) = this.Hp.Value;
    this.Hp.Value = (current - damageTaken, max);
    if (current > 0 && this.Hp.Value.current <= 0) {
      this.DestroySelf();
    }
    Debug.Log($"TakeDamage : {damageTaken}");
    return (damageTaken);
  }

  public int TakeDamage(int attackDamage, Transform attacker)
  {
    throw new NotImplementedException();
  }

  void DestroySelf()
  {
    if (this.OnDestroyed != null) {
      this.OnDestroyed(this);
    }
    Destroy(this.gameObject);
  }
}
