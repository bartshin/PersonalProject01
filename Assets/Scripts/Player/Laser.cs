using System;
using System.Collections.Generic;
using UnityEngine;
using Architecture;
using UnityEngine.VFX;

public class Laser : PlayerBullet 
{
  [SerializeField]
  VisualEffect laserEffect;

  protected void Start()
  {
    this.laserEffect.SetFloat("duration", this.LifeTime);
  }
}
