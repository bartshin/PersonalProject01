using System;
using System.Collections.Generic;
using UnityEngine;
using Architecture;
using Cinemachine;

public class CameraManager : SingletonBehaviour<CameraManager>
{
  [SerializeField]
  CinemachineVirtualCamera topviewCamera;
  [SerializeField]
  CinemachineVirtualCamera sideviewCamera;
  [SerializeField]
  Transform topviewFollow;
  [SerializeField]
  Transform topviewLookAt;
  [SerializeField]
  Vector3 topviewFollowOffset;
  Transform playerShip;
  [SerializeField]
  public Vector3 TopviewLookAtOffsetDest
  {
    get => this.topviewLookAtOffsetDest;
    set {
      this.topviewLookAtOffsetDest = value;
      this.isLookAtOffsetMoving = true;
    }
  }
  [SerializeField]
  float topviewLookAtOffsetLerp;
  [SerializeField]
  float topviewLookAtOffsetThreshold;
  Vector3 topviewLookAtOffsetDest;
  bool isLookAtOffsetMoving;

  void Awake()
  {
    base.OnAwake();
  }
  // Start is called before the first frame update
  void Start()
  {
    this.topviewFollow.position = this.topviewFollowOffset;
    CombatManager.Shared.SelectedEnemy.OnChanged += this.OnSelectedEnemyChanged;
  }

  public void SetPlayerShip(Transform player)
  {
    this.playerShip = player;
    this.sideviewCamera.Follow = player;
  }

  public void UnsetPlayerShip()
  {
    this.playerShip = null;
    this.sideviewCamera.Follow = null;
  }

  void LateUpdate()
  {
    if (this.playerShip != null) {
      var playerPosition = this.playerShip.position;
      this.topviewFollow.position = playerPosition + this.topviewFollowOffset;
      if (!this.isLookAtOffsetMoving) {
        this.topviewLookAt.position = new Vector3( 
          playerPosition.x + this.TopviewLookAtOffsetDest.x,
          playerPosition.y + this.TopviewLookAtOffsetDest.y,
          playerPosition.z + this.TopviewLookAtOffsetDest.z
        );
      }
      else {
        this.topviewLookAt.position = Vector3.Lerp(
          this.topviewLookAt.position,
          playerPosition + this.topviewLookAtOffsetDest,
          this.topviewLookAtOffsetLerp * Time.smoothDeltaTime
        );
        var dist = Vector3.Distance(
          this.topviewLookAt.position,
          playerPosition + this.TopviewLookAtOffsetDest
        );
        if (dist < this.topviewLookAtOffsetThreshold) {
          this.isLookAtOffsetMoving = false;
        }
      }
    }
  }

  void OnDestroy()
  {
    base.OnDestroyed();
  }
  
  void OnSelectedEnemyChanged(IDamagable enemy) 
  {
    if (enemy != null && this.playerShip != null) {
      var offset = new Vector2(
        enemy.gameObject.transform.position.x - this.playerShip.position.x ,
        enemy.gameObject.transform.position.z - this.playerShip.position.z );
      this.TopviewLookAtOffsetDest = new Vector3(
        offset.x * 0.25f, 0, offset.y * 0.25f
      );
      this.sideviewCamera.LookAt = enemy.gameObject.transform;
    }
    else if (enemy == null) {
      this.sideviewCamera.LookAt = null;
      this.TopviewLookAtOffsetDest = Vector3.zero;
    }
  }
}
