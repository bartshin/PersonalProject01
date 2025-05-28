using System;
using System.Collections.Generic;
using UnityEngine;

public class BorneCraftMovement 
{
  public struct ElipseOrbit
  {
    public float Length;
    public float Width;
  }

  public struct Configs
  {
    public float Speed;
    public float MinAngle;
    public float MaxAngle;
    public float PrepareSortieTime;
    
    public Configs(float speed, float minAngle, float maxAngle, float prepareSortieTime)
    {
      this.Speed = speed;
      this.MinAngle = minAngle * 2 * MathF.PI/ 360f;
      this.MaxAngle = maxAngle * 2 * MathF.PI/ 360f;
      this.PrepareSortieTime = prepareSortieTime;
    }
  }

  public Action OnReturnToShip;
  public Action OnSortie;
  public Configs configs;
  ElipseOrbit orbit;
  float currentOrgbitAngle;
  Rigidbody rb;
  Transform transform;
  float waitToSortie;
  bool isSortied;

  public BorneCraftMovement(Rigidbody rigidbody, Transform transform, Configs configs)
  {
    this.rb = rigidbody;
    this.transform = transform;
    this.configs = configs; 
    this.orbit = new ElipseOrbit { Length = 5f, Width = 2f };
    this.isSortied = false;
    this.currentOrgbitAngle = this.configs.MaxAngle;
  }

  public void Update(float deltaTime)
  {
    if (!isSortied && this.waitToSortie <= 0) {
      this.Sortie();
    }
    if (this.isSortied) {
      this.currentOrgbitAngle =
        this.currentOrgbitAngle + this.configs.Speed * deltaTime;
      if (this.currentOrgbitAngle > this.configs.MaxAngle) {
        this.ReturnToMotherShip();
      }
      else {
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
    this.currentOrgbitAngle = this.configs.MinAngle;
    this.transform.localPosition = new Vector3(
      MathF.Cos(this.currentOrgbitAngle) * this.orbit.Width,
      this.transform.localPosition.y,
      MathF.Sin(this.currentOrgbitAngle) * this.orbit.Length
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

  void UpdatePosition(float deltaTime)
  {
    this.transform.localPosition = Vector3.Lerp(
      this.transform.localPosition,
      new Vector3(
        MathF.Cos(this.currentOrgbitAngle) * this.orbit.Width,
        this.transform.localPosition.y,
        MathF.Sin(this.currentOrgbitAngle) * this.orbit.Length
      ),
      0.5f
    );
  }

  void OnWaitToSortie(float deltaTime)
  {
    this.waitToSortie -= deltaTime;
  }
}
