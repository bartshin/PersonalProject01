using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Architecture;

public class UserInputManager : SingletonBehaviour<UserInputManager>
{
  public ObservableValue<Nullable<Vector2>> SelectedAttackPosition;
  public Vector3 DirectionInput { get; private set; }
  public Vector2 MouseDelta { get; private set; }
  public bool IsBoosting { get; private set; }
  public bool IsTrackingMouse;
  public Action MainActionCallback;
  public bool HasMainActionPressed { get; set; }
  public bool HasMainActionTrigged
  {
    get => this.hasMainActionTrigged;
    set {
      this.hasMainActionTrigged = value;
      if (value && this.MainActionCallback != null) {
        this.MainActionCallback.Invoke();
      }
    }
  }
  public bool hasMainActionTrigged;
  InputAction move;
  InputAction attack;
  InputAction speedUp;
  InputAction look;
  InputAction mainAction;

  void Awake()
  {
    base.OnAwake();
    this.move = InputSystem.actions.FindAction("Move");
    this.attack = InputSystem.actions.FindAction("Attack");
    this.speedUp = InputSystem.actions.FindAction("SpeedUp");
    this.look = InputSystem.actions.FindAction("Look");
    this.mainAction = InputSystem.actions.FindAction("MainAction");
    this.SelectedAttackPosition = new (null);
  }

  void OnEnable()
  {
    this.move.Enable();
    this.attack.Enable();
  }

  void Update()
  {
    if (this.attack.WasPressedThisFrame()) {
      this.SelectedAttackPosition.Value = Mouse.current.position.ReadValue();
    }
    if (this.IsTrackingMouse) {
      this.MouseDelta = this.look.ReadValue<Vector2>();
    }
    this.HasMainActionTrigged = false;
    this.HasMainActionPressed = this.mainAction.IsPressed();
    this.DirectionInput = this.move.ReadValue<Vector3>();
    this.IsBoosting = this.speedUp.IsPressed();
  }

  void OnDisable()
  {
    this.move.Disable();
    this.attack.Disable();
  }

  void OnDestory()
  {
    this.OnDestroyed();
  }
}
