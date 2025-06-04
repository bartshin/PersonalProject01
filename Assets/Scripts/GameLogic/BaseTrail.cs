using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseTrail : MonoBehaviour
{
  List<TrailRenderer> trails;
  public float LifeTime = 4f;
  static readonly YieldInstruction WAIT_FRAME = new WaitForEndOfFrame();
  
  void Awake()
  {
    this.trails = new (5);
    var trailSelf = this.GetComponent<TrailRenderer>();
    if (trailSelf != null) {
      this.trails.Add(trailSelf);
    }
    foreach (var trail in this.GetComponentsInChildren<TrailRenderer>()) {
      if (trail != null) {
        this.trails.Add(trail);
      }
    }
  }

  void OnEnable()
  {
    this.StartCoroutine(this.ResetTrails());
  }

  void OnDisable()
  {
    foreach (var trail in this.trails) {
      trail.enabled = false;
    }
  }


  IEnumerator ResetTrails()
  {
    foreach (var trail in this.trails) {
      trail.Clear();
      trail.time = -1;
    }

    yield return (BaseTrail.WAIT_FRAME);

    foreach (var trail in this.trails) {
      trail.enabled = true;
      trail.time = this.LifeTime;
    }
  }
}
