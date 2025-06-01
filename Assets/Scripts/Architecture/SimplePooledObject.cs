using System;
using UnityEngine;
using Architecture;

class SimplePooledObject : MonoBehaviour, IPooedObject
{
  public Action<IPooedObject> OnDisabled 
  { 
    get => this.onDisabled as Action<IPooedObject>;
    set => this.onDisabled = value;
  }
  Action<SimplePooledObject> onDisabled;
  public float LifeTime;

  void Update()
  {
    this.LifeTime -= Time.deltaTime;
    if (this.LifeTime <= 0) {
      this.gameObject.SetActive(false);
    }
  }

  void OnDisable()
  {
    if (this.onDisabled != null) {
      this.onDisabled.Invoke(this);
    }
  }

}

