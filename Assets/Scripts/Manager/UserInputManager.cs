using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Architecture;

public class UserInputManager : SingletonBehaviour<UserInputManager>
{
  public class Operation
  {
    InputAction Action;
    public bool HasRegistered {
      get => this.hasRegistered;
      set {
        this.hasRegistered = value;
        this.hasTriggered = false;
      }
    }
    public bool HasTriggered {
      get => this.hasTriggered;
      set {
        this.hasTriggered= value;
        if (value && this.OnTriggered != null) {
          this.OnTriggered.Invoke();
        }
      }
    }
    public Action OnTriggered;
    bool hasRegistered;
    bool hasTriggered;

    public void Update()
    {
      this.HasRegistered  = this.Action.IsPressed();
    }

    public Operation(InputAction action)
    {
      this.Action = action;
      this.HasRegistered = false;      
      this.HasTriggered = false;
    }
  }

  public ObservableValue<Nullable<Vector2>> SelectedScreenPosition;
  public Vector3 DirectionInput { get; private set; }
  public Vector2 PointerDelta { get; private set; }
  public bool IsBoosting { get; private set; }
  public bool IsUsingPointer 
  { 
    get => this.isTrackingMouse;
    set {
      this.isTrackingMouse = value;
      Cursor.visible = !value;
    }
  }
  public bool hasMainActionTrigged;
  public Operation MainOperation { get; private set; }
  public Operation SubOperation { get; private set; }
  bool isTrackingMouse;
  InputAction move;
  InputAction select;
  InputAction speedUp;
  InputAction look;

  void Awake()
  {
    base.OnAwake();
    this.move = InputSystem.actions.FindAction("Move");
    this.select = InputSystem.actions.FindAction("Select");
    this.speedUp = InputSystem.actions.FindAction("SpeedUp");
    this.look = InputSystem.actions.FindAction("Look");
    this.MainOperation = new Operation(
      InputSystem.actions.FindAction("MainAction"));
    this.SubOperation = new Operation(InputSystem.actions.FindAction("SubAction"));
    this.SelectedScreenPosition = new (null);
  }

  void OnEnable()
  {
    this.move.Enable();
    this.select.Enable();
  }

  void Update()
  {
    if (this.IsUsingPointer) {
      this.PointerDelta = this.look.ReadValue<Vector2>();
    }
    else if (this.select.WasPressedThisFrame()) {
      this.SelectedScreenPosition.Value = Mouse.current.position.ReadValue();
    }
    this.MainOperation.Update();
    this.SubOperation.Update();
    this.DirectionInput = this.move.ReadValue<Vector3>();
    this.IsBoosting = this.speedUp.IsPressed();
  }

  void OnDisable()
  {
    this.move.Disable();
    this.select.Disable();
  }

  void OnDestory()
  {
    this.OnDestroyed();
  }
}
