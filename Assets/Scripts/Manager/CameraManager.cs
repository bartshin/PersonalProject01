using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Architecture;
using Cinemachine;

public class CameraManager : SingletonBehaviour<CameraManager>
{
  static readonly Vector3 RENDERTEXTURE_CAMERA_OFFSET = new(10f, 10f, 10f);
  static readonly Vector3 SUBCAMERA_RENDER_POSITION = new(1000f, 1000f, 1000f);
  static readonly YieldInstruction SUBCAMERA_RENDER_INTERVAL = new WaitForSeconds(0.3f);
  const int SUB_CAMERA_COUNT = 5;
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
  CameraController mainCamera;
  List<Camera> subCamera;
  Dictionary<Camera, Transform> subCameraTargets;
  List<(Camera cam, Coroutine routine)> subRenders;
  int subCameraLayer;

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

  public Camera CreateCameraFor(RenderTexture renderTexture, Transform target)
  {
    var gameObject = new GameObject("RenderTexture Camera");
    gameObject.transform.position = 
     target.position + CameraManager.RENDERTEXTURE_CAMERA_OFFSET;
    gameObject.transform.LookAt(target);
    var camera = gameObject.AddComponent<Camera>();
    camera.fieldOfView = 60f;
    camera.nearClipPlane = 0.1f;
    camera.farClipPlane = 20f;
    camera.targetTexture = renderTexture;
    this.subCamera.Add(camera);
    this.subCameraTargets[camera] = target;
    return (camera);
  }

  public void StartRender(Camera camera, float duration)
  {
    var index = this.subRenders.FindIndex(elem => elem.cam == camera);
    var target = this.subCameraTargets[camera];
    camera.transform.position = target.position + CameraManager.RENDERTEXTURE_CAMERA_OFFSET;
    camera.transform.LookAt(target);
    if (index != -1) {
      this.StopCoroutine(this.subRenders[index].routine);
    this.subRenders[index] = (
        camera, this.StartCoroutine(this.CreateRenderRoutine(camera, duration, target)));
    }
    else {
      this.subRenders.Add((
        camera, 
        this.StartCoroutine(
          this.CreateRenderRoutine(camera, duration, target))));
    }
  }

  public void StopRender(Camera camera)
  {
    var index = this.subRenders.FindIndex(elem => elem.cam == camera);
    if (index != -1) {
      this.StopCoroutine(this.subRenders[index].routine);
      this.subRenders.RemoveAt(index);
    }
  }

  IEnumerator CreateRenderRoutine(Camera camera, float duration, Transform target)
  {
    float renderedTime = 0;
    var targetLayer = target.gameObject.layer;
    while (renderedTime < duration) {
      renderedTime += Time.deltaTime;   
      //target.gameObject.layer = this.subCameraLayer;
      target.position += CameraManager.SUBCAMERA_RENDER_POSITION;
      camera.transform.position = target.position + CameraManager.RENDERTEXTURE_CAMERA_OFFSET;
      camera.transform.LookAt(target);
      camera.Render();
      target.position -= CameraManager.SUBCAMERA_RENDER_POSITION;
      //target.gameObject.layer = targetLayer;
      yield return (CameraManager.SUBCAMERA_RENDER_INTERVAL);
    }
  }

  void Awake()
  {
    base.OnAwake();
    this.subCameraLayer = LayerMask.NameToLayer("SubCameraOnly");
    this.subCamera = new (CameraManager.SUB_CAMERA_COUNT);
    this.subCameraTargets = new (CameraManager.SUB_CAMERA_COUNT);
    this.subRenders = new (CameraManager.SUB_CAMERA_COUNT);
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
    this.mainCamera = Camera.main.gameObject.GetComponent<CameraController>();
  }

  void Update()
  {
    //TODO: Make UI************************
    if (Input.GetKeyDown(KeyCode.Alpha1)) {
      this.ActiveSideCamera.Value = null;
    }
    else if (Input.GetKeyDown(KeyCode.Alpha2)) {
      this.ActiveSideCamera.Value = Direction.Left;
    }
    else if (Input.GetKeyDown(KeyCode.Alpha3)) {
      this.ActiveSideCamera.Value = Direction.Right;
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
