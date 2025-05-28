using System;
using UnityEngine;

namespace Architecture
{
  public class ObservableValue<T>
  {
    public T Value {
      get => this.innerValue;
      set {
        if (!this.innerValue.Equals(value)) {
          if (this.WillChange != null) {
            this.WillChange(value);
          }
          this.innerValue = value;
          if (this.OnChanged != null) {
            this.OnChanged.Invoke(value);
          }
        }
      }
    }

    public ObservableValue(T initialValue = default)
    {
      this.innerValue = initialValue;
    }

    public void DestorySelf()
    {
      if (this.OnDestory != null) {
        this.OnDestory.Invoke();
      }
    }

    public Action<T> OnChanged;
    public Action<T> WillChange;
    public Action OnDestory;

    T innerValue;
  }
}
