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


  void OnDisable()
  {
    if (this.onDisabled != null) {
      this.onDisabled.Invoke(this);
    }
  }
}

