using System;
using System.Collections.Generic;
using UnityEngine;

public class BorneCraftMovement 
{
  public struct EclipseOrbit
  {
    public float Length;
    public float Width;
  }

  public struct Configs
  {
    public float Speed;
    public (float min, float max) MoveAngle;
    public float MaxShootAngle;
    public float PrepareSortieTime;
    public float ShootRange;
    
    public Configs(
        float speed, 
        (float min, float max) moveAngles,
        float maxShootAngle,
        float shootRange,
        float prepareSortieTime)
    {
      this.Speed = speed;
      this.MoveAngle = (moveAngles.min * 2 * MathF.PI/ 360f, 
          moveAngles.max * 2 * MathF.PI/ 360f);
      this.MaxShootAngle = maxShootAngle * 2 * MathF.PI / 360f;
      this.ShootRange = shootRange;
      this.PrepareSortieTime = prepareSortieTime;
    }
  }

  public Action OnReturnToShip;
  public Action OnSortie;
  public Configs configs;
  public float TargetDistance;
  public bool IsShootable { get; private set; }
  EclipseOrbit orbit;
  float angleSpeed;
  float currentOrgbitAngle;
  Rigidbody rb;
  Transform transform;
  Transform container;
  float waitToSortie;
  bool isSortied;
  Transform target;

  public BorneCraftMovement(Transform container, Rigidbody rigidbody, Transform transform, Configs configs)
  {
    this.container = container;
    this.rb = rigidbody;
    this.transform = transform;
    this.configs = configs; 
    this.isSortied = false;
  }
  
  public void SetTarget(Transform target)
  {
    this.target = target;
    this.currentOrgbitAngle = this.configs.MoveAngle.max;
    this.SetOrbit(target.position);
  }

  public void Update(float deltaTime)
  {
    if (this.target != null &&
        !isSortied && this.waitToSortie <= 0) {
      this.Sortie();
    }
    if (this.target != null) {
      this.IsShootable = this.currentOrgbitAngle < this.configs.MaxShootAngle;
    }
    else {
      this.IsShootable = false;
    }
    if (this.isSortied) {
      this.currentOrgbitAngle =
        this.currentOrgbitAngle + this.angleSpeed * deltaTime;
      if (this.currentOrgbitAngle > this.configs.MoveAngle.max) {
        this.ReturnToMotherShip();
      }
      else {
        this.UpdateRotation(deltaTime);
        this.UpdatePosition(deltaTime);
      }
    }
    else {
      OnWaitToSortie(deltaTime);
    }
  }

  void Sortie()
  {
    this.isSortied = true;
    this.currentOrgbitAngle = this.configs.MoveAngle.min;
    this.SetOrbit(this.target.position);
    this.transform.localPosition = Vector3.Lerp(
      Vector3.zero,
      new Vector3(
        MathF.Cos(this.currentOrgbitAngle) * this.orbit.Width,
        this.transform.localPosition.y,
        MathF.Sin(this.currentOrgbitAngle) * this.orbit.Length
        ),
      0.2f
    );
    if (this.OnSortie != null) {
      this.OnSortie.Invoke();
    }
  }

  void ReturnToMotherShip()
  {
    this.isSortied = false;
    this.waitToSortie = this.configs.PrepareSortieTime;
    if (this.OnReturnToShip != null) {
      this.OnReturnToShip.Invoke();
    }
  }

  void SetOrbit(Vector3 targetPosition)
  {
    this.orbit = new EclipseOrbit { 
      Length = this.TargetDistance,
      Width = this.TargetDistance * 0.15f 
    };
    this.angleSpeed = this.configs.Speed * ( 0.5f + 0.5f / this.TargetDistance) ;
  }

  void UpdatePosition(float deltaTime)
  {
   if (this.currentOrgbitAngle < this.configs.MoveAngle.max * 0.8) {
      this.transform.localPosition = Vector3.Lerp(
        this.transform.localPosition,
        new Vector3(
          MathF.Cos(this.currentOrgbitAngle) * this.orbit.Width,
          this.transform.localPosition.y,
          MathF.Sin(this.currentOrgbitAngle) * this.orbit.Length
        ),
        1f * deltaTime
      );
   }
   else {
     this.transform.localPosition = Vector3.Lerp(
        this.transform.localPosition,
        Vector3.zero,
        1.5f * deltaTime
     );
   }
  }

  void UpdateRotation(float deltaTime)
  {
    if (this.IsShootable) {
      this.transform.LookAt(this.target);
    }
    else if (this.currentOrgbitAngle < this.configs.MoveAngle.max * 0.85) {
      var containerDir = (this.container.position - this.transform.position).normalized;
      this.transform.rotation = Quaternion.Lerp(
        this.transform.rotation,
        Quaternion.LookRotation(containerDir, this.transform.up),
        deltaTime
      );
    }
  }

  void OnWaitToSortie(float deltaTime)
  {
    this.waitToSortie -= deltaTime;
  }
}
