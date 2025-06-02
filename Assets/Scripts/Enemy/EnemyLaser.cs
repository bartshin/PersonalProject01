using System;
using System.Collections.Generic;
using UnityEngine;
using Architecture;
using UnityEngine.VFX;

public class EnemyLaser : EnemyProjectile
{
  [SerializeField]
  VisualEffect laserEffect;
  public override float LifeTime 
  { 
    get => this.lifeTime;
    set {
      this.lifeTime = value;
      this.laserEffect.SetFloat("duration", this.LifeTime);
    }
  }
  float lifeTime;
}
