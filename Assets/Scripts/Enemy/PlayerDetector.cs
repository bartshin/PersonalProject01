using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDetector : MonoBehaviour
{
  public Action<Collider> OnDetect;

  [SerializeField]
  SphereCollider trigger;

  public void SetRange(float range)
  {
    if (this.trigger != null) {
      this.trigger.radius = range;
    }
  }

  void Awake()
  {
    if (this.trigger == null) {
      this.trigger = this.GetComponent<SphereCollider>();
    }
  }

  void OnTriggerEnter(Collider collider)
  {
    if (this.OnDetect != null) {
      this.OnDetect.Invoke(collider);
    }
  }
}
