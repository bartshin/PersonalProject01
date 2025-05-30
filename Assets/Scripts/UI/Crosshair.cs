using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Architecture;

public class Crosshair : MonoBehaviour
{
  Image icon;
  Material iconMaterial;
  const float FADE_ALPHA_STEP = 1.8f;
  const float ENLARGE_STEP = 0.5f;
  const float MIN_SIZE_VALUE = 0.1f;
  const float MAX_SIZE_VALUE = 0.2f;
  bool isFadeIn;
  bool isExpanding;
  bool isShrinking;
  bool isWaving;
  bool isActive;
  Color defaultColor = new Color(0, 255f/256f, 156f/256f, 1f);
  Color transparentAlpha = new Color(0, 255f/256f, 156f/256f, 0f);

  void Awake()
  {
    this.icon = this.GetComponentInChildren<Image>();
    this.iconMaterial = this.icon.material;
    this.iconMaterial.SetVector("_Color", this.defaultColor);
  }

  void Start()
  {
    this.Hide();
  }

  void OnEnable()
  {
    this.AddEventListeners();
  }

  void OnDisable()
  {
    this.RemoveEventListeners();
  }

  void AddEventListeners()
  {
    CameraManager.Shared.ActiveSideCamera.OnChanged += this.OnSideCameraChanged;
    UserInputManager.Shared.MainOperation.OnTriggered += this.OnAction;
  }

  void RemoveEventListeners()
  {
    CameraManager.Shared.ActiveSideCamera.OnChanged -= this.OnSideCameraChanged;
    UserInputManager.Shared.MainOperation.OnTriggered -= this.OnAction;
  }

  void OnSideCameraChanged(Nullable<Direction> direction) 
  {
    if (direction != null) {
      this.Show();
    }
    else {
      this.Hide();
    }
  }

  // Update is called once per frame
  void Update()
  {
    var animateValues = this.iconMaterial.GetVector("_AnimateValues");
    bool isUpdated = false;
    if (this.isFadeIn) {
      animateValues.w += FADE_ALPHA_STEP * Time.deltaTime;
      this.isFadeIn = animateValues.w < 1.0f;
      isUpdated = true;
    }
    if (this.isShrinking) {
      animateValues.x = Mathf.Max(
          animateValues.x - ENLARGE_STEP * Time.deltaTime, MIN_SIZE_VALUE);
      this.isShrinking = animateValues.x > MIN_SIZE_VALUE;
      isUpdated = true;
    }
    if (isUpdated) {
      this.iconMaterial.SetVector("_AnimateValues", animateValues);
    }
  }

  void OnChangedAiming(bool isAiming) 
  {
    if (isAiming) {
      this.Show();
    }
    else {
      this.Hide();
    }
  }

  void OnAction()
  {
    if (this.isActive) {
      this.OnShooting();
    }
  }

  void OnShooting()
  {
    var currentValues = this.iconMaterial.GetVector("_AnimateValues");
    currentValues.x = MAX_SIZE_VALUE;
    currentValues.w = 1f;
    this.iconMaterial.SetVector(
        "_AnimateValues",
        currentValues);
    this.isShrinking = true;
    this.isExpanding = false;
    this.isFadeIn = false;
  }

  void OnChangedMoving(bool isMoving) {
    var currentValues = this.iconMaterial.GetVector("_AnimateValues");
    currentValues.y = isMoving ? 1: 0;
    this.iconMaterial.SetVector("_AnimateValues", currentValues);
  }

  void Show()
  {
    this.isActive = true;
    this.isFadeIn = true;
    this.isShrinking = true;
    this.isExpanding = false;
  }

  void Hide()
  {
    this.isActive = false;
    var currentValues = this.iconMaterial.GetVector("_AnimateValues");
    currentValues.w = 0;
    this.iconMaterial.SetVector(
        "_AnimateValues",
        currentValues
        );
    this.isFadeIn = false;
    this.isExpanding = false;
    this.isShrinking = false;
  }

  void Expand()
  {
    this.isShrinking = false;
    this.isExpanding = true;
  }

  void Shrink()
  {
    this.isShrinking = true;
    this.isExpanding = false;
  }
}
