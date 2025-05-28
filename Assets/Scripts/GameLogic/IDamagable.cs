using System;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable
{
  private static Dictionary<GameObject, IDamagable> damagables = new();

  public int TakeDamage(int attackDamage);
  public int TakeDamage(int attackDamage, Transform attacker);
  public GameObject gameObject { get; }

  virtual protected void Register(GameObject gameObject) {
    if (!IDamagable.damagables.TryAdd(
        gameObject, this
        )) {

    }
  }

  static IDamagable FindIDamagableFrom(GameObject gameObject)
  {
    var found = gameObject.GetComponent<IDamagable>();
    if (found != null) {
      return (found);
    }
    return (gameObject.GetComponentInParent<IDamagable>());
  }
}
