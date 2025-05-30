using System;
using System.Collections.Generic;
using UnityEngine;
using Architecture;

public interface IProjectile
{
  public Vector3 TargetPosition { get; set; }
  public int Damage { get; set; }
  public float InitialSpeed { get; set; }
  public float LifeTime { get; set; } 
  public GameObject FiredShip { get; set; }
  public IDamagable Target { get; set; }
  public Vector3 Direction { get; set; } 
}
