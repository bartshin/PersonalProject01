using System;
using System.Collections.Generic;
using UnityEngine;
using Architecture;
using Cinemachine;

public class CameraManager : SingletonBehaviour<CameraManager>
{
  new public static void CreateInstance()  
  {
    GameObject prefab = Resources.Load<GameObject>("Prefabs/CameraManager"); 
    var gameObject = Instantiate(prefab);
    DontDestroyOnLoad(gameObject);
  }
  public ObservableValue<Nullable<Direction>> ActiveSideCamera; 
  [SerializeField]
  public Vector3 TopviewLookAtOffsetDest
  {
    get => this.topviewLookAtOffsetDest;
    set {
      this.topviewLookAtOffsetDest = value;
      this.isLookAtOffsetMoving = true;
    }
  }
  public Vector3 SideviewOffset;

  [SerializeField]
  CinemachineVirtualCamera topviewCamera;
  [SerializeField]
  CinemachineVirtualCamera sideviewCamera;
  [SerializeField]
  Transform topviewFollow;
  [SerializeField]
  Transform topviewLookAt;
  [SerializeField]
  Transform sideviewFollow;
  [SerializeField]
  Transform sideviewLookAt;
  [SerializeField]
  Vector3 topviewFollowOffset;
  Transform playerShip;
  [SerializeField]
  float topviewLookAtOffsetLerp;
  [SerializeField]
  float topviewLookAtOffsetThreshold;
  Vector3 topviewLookAtOffsetDest;
  Quaternion sideviewDirDest;
  bool isLookAtOffsetMoving;
  int playershipMask;
  int playerInteriorMask;
  float sideviewRadius = 10f;

  void Awake()
  {
    base.OnAwake();
    this.playershipMask = (1 << LayerMask.NameToLayer("Player"));
    this.playerInteriorMask = (1 << LayerMask.NameToLayer("PlayerInterior"));
    this.ActiveSideCamera = new (null);
    this.ActiveSideCamera.OnChanged += this.OnSideCameraChanged;
    this.ActiveSideCamera.WillChange += this.WillSideCameraChanged;
  }
  // Start is called before the first frame update
  void Start()
  {
    this.topviewFollow.position = this.topviewFollowOffset;
    CombatManager.Shared.SelectedDamagable.OnChanged += this.OnSelectedEnemyChanged;
  }

  public void SetPlayerShip(Transform player)
  {
    this.playerShip = player;
    this.sideviewFollow.position = player.position + this.SideviewOffset;
    this.topviewFollow.position = player.position+ this.topviewFollowOffset;
  }

  public void UnsetPlayerShip()
  {
    this.playerShip = null;
    this.sideviewCamera.Follow = null;
  }

  public void SetSideViewDir(Quaternion dir)
  {
    this.sideviewDirDest = dir;
  }

  void Update()
  {
    //FIXME: Make UI************************
    if (Input.GetKeyDown(KeyCode.Alpha1)) {
      this.ActiveSideCamera.Value = Direction.Left;
    }
    if (Input.GetKeyDown(KeyCode.Alpha2)) {
      this.ActiveSideCamera.Value = Direction.Right;
    }
    if (Input.GetKeyDown(KeyCode.Alpha3)) {
      this.ActiveSideCamera.Value = null;
    }
    //***************************************
  }

  void LateUpdate()
  {
    if (this.playerShip != null) {
      if (this.ActiveSideCamera.Value != null) {
        this.sideviewFollow.position = this.playerShip.position + this.SideviewOffset;
        this.sideviewLookAt.position = this.sideviewFollow.position +
          this.sideviewDirDest * Vector3.forward;
        this.sideviewLookAt.rotation = this.sideviewDirDest;
      }
      else {
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
    }
    else if (enemy == null) {
      this.TopviewLookAtOffsetDest = Vector3.zero;
    }
  }

  void SetCullingMask(Nullable<Direction> activeSideCamera)
  {
    var currentMask = Camera.main.cullingMask;
    if (activeSideCamera == null) {
      Camera.main.cullingMask |= this.playershipMask;
      Camera.main.cullingMask &= ~this.playerInteriorMask;
    }
    else {
      Camera.main.cullingMask &= ~this.playershipMask;
      Camera.main.cullingMask |= this.playerInteriorMask;
    }
  }

  void WillSideCameraChanged(Nullable<Direction> activeSide)
  {
    if (activeSide != null) {
      this.sideviewCamera.Priority = 0;
      this.SetCullingMask(null);
    }
  }

  void OnSideCameraChanged(Nullable<Direction> activeSide)
  {
    this.sideviewCamera.Priority = activeSide != null ? 2: 0;
    this.SetCullingMask(activeSide);
  }
}
