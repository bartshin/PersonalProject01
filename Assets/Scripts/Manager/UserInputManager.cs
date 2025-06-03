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

  public ObservableValue<Nullable<Vector2>> PrimarySelectedScreenPosition { get; private set; }
  public ObservableValue<Nullable<Vector2>> SecondarySelectedScreenPosition { get; private set; }
  public ObservableValue<Nullable<Direction>> NavigateDirection { get; private set; }
  public Vector3 DirectionInput { get; private set; }
  public Vector2 PointerDelta { get; private set; }
  public bool IsBoosting { get; private set; }
  public bool IsUsingPointer 
  { 
    get => !this.isTrackingPointerDelta;
    set {
      this.isTrackingPointerDelta = !value;
      Cursor.visible = value;
    }
  }
  public bool hasMainActionTrigged;
  public Operation MainOperation { get; private set; }
  public Operation SubOperation { get; private set; }
  bool isTrackingPointerDelta;
  InputAction move;
  InputAction primarySelect;
  InputAction secondarySelect;
  InputAction speedUp;
  InputAction look;
  InputAction navigate;

  void Awake()
  {
    base.OnAwake();
    this.IsUsingPointer = true;
    this.NavigateDirection = new (null);
    this.move = InputSystem.actions.FindAction("Move");
    this.primarySelect = InputSystem.actions.FindAction("PrimarySelect");
    this.secondarySelect = InputSystem.actions.FindAction("SecondarySelect");
    this.speedUp = InputSystem.actions.FindAction("SpeedUp");
    this.look = InputSystem.actions.FindAction("Look");
    this.MainOperation = new Operation(
      InputSystem.actions.FindAction("MainAction"));
    this.SubOperation = new Operation(InputSystem.actions.FindAction("SubAction"));
    this.PrimarySelectedScreenPosition = new (null);
    this.navigate = InputSystem.actions.FindAction("Navigate");
    this.SecondarySelectedScreenPosition = new (null);
  }

  void OnEnable()
  {
  }

  void Update()
  {
    if (this.IsUsingPointer) {
      if (this.primarySelect.WasPressedThisFrame()) {
        this.PrimarySelectedScreenPosition.Value = Pointer.current.position.ReadValue();
      }
      if (this.secondarySelect.WasPressedThisFrame()) {
        this.SecondarySelectedScreenPosition.Value = Pointer.current.position.ReadValue(); 
      }
    }
    else {
      this.PointerDelta = this.look.ReadValue<Vector2>();
    }
    this.MainOperation.Update();
    this.SubOperation.Update();
    this.DirectionInput = this.move.ReadValue<Vector3>();
    this.IsBoosting = this.speedUp.IsPressed();
    var navigateInput = this.navigate.ReadValue<Vector2>();
    this.SetNavigateDirection(navigateInput);
  }

  void SetNavigateDirection(Vector2 input)
  {
    if (input == Vector2.zero) { 
      this.NavigateDirection.Value = null;
      return ;
    }
    if (input.x > float.Epsilon) {
      this.NavigateDirection.Value = Direction.Right; 
    }
    else if (input.x < -float.Epsilon) {
      this.NavigateDirection.Value = Direction.Left; 
    }
    else if (input.y > float.Epsilon) {
      this.NavigateDirection.Value = Direction.Up;
    }
    else if (input.y < -float.Epsilon) {
      this.NavigateDirection.Value = Direction.Down;
    }
  }

  void OnDisable()
  {
  }

  void OnDestory()
  {
    this.OnDestroyed();
  }
}
