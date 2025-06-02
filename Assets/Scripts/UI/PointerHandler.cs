using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PointerHandler : 
  MonoBehaviour, IPointerClickHandler, IPointerUpHandler, IPointerDownHandler, IPointerExitHandler, IPointerMoveHandler, IPointerEnterHandler
{

  public enum PointerEvent
  {
    Click,
    Down,
    Up,
    Move,
    Enter,
    Exit,
    BeginDrag,
    EndDrag
  }

  [SerializeField]
  PointerEvent[] ActivatedEvents;
  Action<PointerEventData>[] allEvents;
  int[] eventTypeIndices;

  public void AddEvent(PointerEvent eventType, Action<PointerEventData> callback)
  {
    var index = this.eventTypeIndices[(int)eventType];
    if (index != -1) {
      this.allEvents[index] += callback;
    }
    else {
      throw new ArgumentException($"{eventType} is not activated");
    }
  }

  public void RemoveEvent(PointerEvent eventType, Action<PointerEventData> callback) 
  {
    var index = this.eventTypeIndices[(int)eventType];
    if (index != -1) {
      this.allEvents[index] -= callback;
    }
    else {
      throw new ArgumentException($"{eventType} is not activated");
    }
  }

  void OnEventTriggered(PointerEvent eventType, PointerEventData eventData)
  {
    var index = this.eventTypeIndices[(int)eventType];
    if (index != -1 && this.allEvents[index] != null) {
      this.allEvents[index].Invoke(eventData);
    }
  }

  void Awake()
  {
    var eventTypeCount = Enum.GetValues(typeof(PointerEvent)).Length;
    this.allEvents = new Action<PointerEventData>[this.ActivatedEvents.Length];
    this.eventTypeIndices = new int[eventTypeCount];
    for (int i = 0; i < eventTypeCount; ++i){
      var pointerEvent = (PointerEvent)i;
      var index = Array.IndexOf(this.ActivatedEvents, pointerEvent);
      this.eventTypeIndices[i] = index;
    }
  }

  /**************************** Interface ******************************/

  public void OnPointerClick(PointerEventData eventData) => this.OnEventTriggered((int)PointerEvent.Click, eventData);

  public void OnPointerDown(PointerEventData eventData) => this.OnEventTriggered(PointerEvent.Down, eventData);

  public void OnPointerUp(PointerEventData eventData) => this.OnEventTriggered(PointerEvent.Down, eventData);

  public void OnPointerEnter(PointerEventData eventData) => this.OnEventTriggered(PointerEvent.Down, eventData);

  public void OnPointerMove(PointerEventData eventData) => this.OnEventTriggered(PointerEvent.Down, eventData);

  public void OnPointerExit(PointerEventData eventData) => this.OnEventTriggered(PointerEvent.Down, eventData);
}
