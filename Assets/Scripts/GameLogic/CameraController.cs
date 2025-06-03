using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraController : MonoBehaviour
{
  public Action<ScriptableRenderContext> OnRendered;
  public Camera cam;

  void Awake()
  {
    this.cam = this.gameObject.GetComponent<Camera>();
  }

  void OnCameraRendered(ScriptableRenderContext context, Camera cam)
  {
    if (this.OnRendered == null || cam != this.cam) {
      return ;
    }
    this.OnRendered.Invoke(context);
  }
  // Start is called before the first frame update
  void OnEnable()
  {
    //RenderPipelineManager.endCameraRendering += this.OnCameraRendered;
  }

  void OnDisable()
  {
    //RenderPipelineManager.endCameraRendering -= this.OnCameraRendered;
  }

  // Update is called once per frame
  void Update()
  {

  }
}
