using System;
using System.Collections.Generic;
using UnityEngine;

public class BorneCraftMovement 
{
  const float Y_OFFSET_FROM_MOTHERSHIP = -10f;
  public static readonly  Vector3 BASE_POSITION = new Vector3(
    0, Y_OFFSET_FROM_MOTHERSHIP, 0
  );
  public struct EclipseOrbit
  {
    public float Length;
    public float Width;
  }

  public struct Configs
  {
    public float Speed;
    public (float min, float max) MoveAngle;
    public float HalfMoveAngle;
    public float MaxShootAngle;
    public float PrepareSortieTime;
    public float ShootRange;
    public float MaxDistToSortie;
    
    public Configs(
        float speed, 
        (float min, float max) moveAngles,
        float maxShootAngle,
        float shootRange,
        float prepareSortieTime,
        float maxDistToSortie)
    {
      this.Speed = speed;
      this.MoveAngle = (moveAngles.min * 2 * MathF.PI/ 360f, 
          moveAngles.max * 2 * MathF.PI/ 360f);
      this.HalfMoveAngle = (this.MoveAngle.min + this.MoveAngle.max) * 0.5f;
      this.MaxShootAngle = maxShootAngle * 2 * MathF.PI / 360f;
      this.ShootRange = shootRange;
      this.PrepareSortieTime = prepareSortieTime;
      this.MaxDistToSortie = maxDistToSortie;
    }
  }

  public bool IsPowerDown;
  public Action OnReturnToShip;
  public Action OnSortie;
  public Configs configs;
  public float TargetDistance;
  public bool IsShootable { get; private set; }
  EclipseOrbit orbit;
  float angleSpeed;
  float currentOrbitAngle;
  Rigidbody rb;
  Transform transform;
  Transform container;
  float waitToSortie;
  float returnThreshold;
  bool isSortied;
  Transform target;
  float maxSortiedTime = 8f;
  float currentSortedTime;

  public BorneCraftMovement(Transform container, Rigidbody rigidbody, Transform transform, Configs configs, float waitOffset = 0f)
  {
    this.container = container;
    this.rb = rigidbody;
    this.transform = transform;
    this.configs = configs; 
    this.isSortied = false;
    this.waitToSortie = waitOffset;
  }
  
  public void SetTarget(Transform target, float waitTime = 0)
  {
    this.target = target;
    this.returnThreshold = Math.Clamp(
      Vector3.Distance(
        this.container.transform.position, target.position) * 0.05f,
      0.5f,
      2f
    );
    this.currentOrbitAngle = this.configs.MoveAngle.max;
    this.waitToSortie = waitTime;
    this.SetOrbit(target.position);
  }

  public void RemoveTarget()
  {
    this.target = null;
  }

  public void Update(float deltaTime)
  {
    if (this.target != null &&
        !isSortied && this.waitToSortie <= 0 &&
        this.TargetDistance < this.configs.MaxDistToSortie &&
        !this.IsPowerDown
        ) {
      this.Sortie();
    }
    if (this.target != null && !this.IsPowerDown) {
      this.IsShootable = this.currentOrbitAngle < this.configs.MaxShootAngle;
    }
    else {
      this.IsShootable = false;
    }
    if (this.isSortied) {
      this.currentSortedTime += deltaTime;
      this.UpdatePosition(deltaTime);
      this.UpdateRotation(deltaTime);
      if ((this.currentOrbitAngle > this.configs.HalfMoveAngle &&
          Vector2.Distance(
            this.transform.position, this.container.position) < 
          this.returnThreshold) || 
          this.currentSortedTime > this.maxSortiedTime) {
        this.ReturnToMotherShip();
      }
    }
    else if (!this.IsPowerDown) {
      OnWaitToSortie(deltaTime);
    }
  }

  void Sortie()
  {
    this.isSortied = true;
    this.currentSortedTime = 0;
    this.currentOrbitAngle = this.configs.MoveAngle.min;
    this.SetOrbit(this.target.position);
    this.transform.localPosition = Vector3.Lerp(
      BorneCraftMovement.BASE_POSITION,
      new Vector3(
        MathF.Cos(this.currentOrbitAngle) * this.orbit.Width,
        this.transform.localPosition.y,
        MathF.Sin(this.currentOrbitAngle) * this.orbit.Length
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
   if (this.currentOrbitAngle < this.configs.MoveAngle.max) {
      this.currentOrbitAngle =
        this.currentOrbitAngle + this.angleSpeed * deltaTime;
      this.transform.localPosition = Vector3.Lerp(
        this.transform.localPosition,
        new Vector3(
          MathF.Cos(this.currentOrbitAngle) * this.orbit.Width,
          this.transform.localPosition.y,
          MathF.Sin(this.currentOrbitAngle) * this.orbit.Length
        ),
        1f * deltaTime
      );
   }
   else {
     this.transform.localPosition = Vector3.Lerp(
        this.transform.localPosition,
        BorneCraftMovement.BASE_POSITION,
        1.5f * deltaTime
     );
   }
  }

  void UpdateRotation(float deltaTime)
  {
    if (this.IsShootable) {
      this.transform.LookAt(this.target);
    }
    else if (this.currentOrbitAngle < this.configs.MoveAngle.max * 0.85) {
      var containerDir = (this.container.position - this.transform.position).normalized;
      this.transform.rotation = Quaternion.Lerp(
        this.transform.rotation,
        Quaternion.LookRotation(containerDir, this.transform.up),
        5f * deltaTime
      );
    }
  }

  void OnWaitToSortie(float deltaTime)
  {
    this.waitToSortie -= deltaTime;
  }
}
