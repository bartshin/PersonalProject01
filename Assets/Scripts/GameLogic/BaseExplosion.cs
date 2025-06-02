using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseExplosion : SimplePooledObject
{
  [SerializeField]
  ParticleSystem effect;

  public override float LifeTime 
  { 
    get => this.lifeTime;
    set {
      this.lifeTime = value;
      if (this.effect != null) {
        this.SetEffectLifeTime(value);
      }
    }
  }

  float lifeTime;
  
  void SetEffectLifeTime(float lifeTime)
  {
    var particle = this.effect.main;
    particle.startLifetime = lifeTime;
  }
}
