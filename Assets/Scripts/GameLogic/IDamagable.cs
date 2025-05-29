using System;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable
{
  private static Dictionary<GameObject, IDamagable> damagables = new();

  public int TakeDamage(int attackDamage);
  public int TakeDamage(int attackDamage, Transform attacker);
  public Action<IDamagable> OnDestroyed { get; set; }
  public Action<IDamagable> OnDisabled { get; set; }
  public GameObject gameObject { get; }

  protected static void Register(GameObject gameObject, IDamagable damagable) {
    if (!IDamagable.damagables.TryAdd(gameObject, damagable)) {
      Debug.LogError($"Duplicate damagable for {gameObject.name}");
    }
  }

  public static IDamagable GetDamagable(GameObject gameObject)
  {
    if (IDamagable.damagables.TryGetValue(gameObject, out IDamagable damagable)) {
      return (damagable);
    }
    return (null);
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
