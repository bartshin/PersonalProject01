using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Field = StatusController.Field;

public class StatusControlView: VisualElement
{
  /********************* USS Selector ***************************/
  public const string PREFIX = "status-control-";
  public const string CONTAINER_NAME = StatusControlView.PREFIX + "view-container";
  public const string ALL_FIELDS_CONTAINER = StatusControlView.PREFIX + "all-fields-container";
  public const string FIELD_CONTAINER = StatusControlView.PREFIX + "field-container";
  public const string EXTRA_CONTAINER = StatusControlView.PREFIX + "extra-container";
  public const string GUAGE_CONTAINER = StatusControlView.PREFIX + "gauge-container";
  public const string HORIZONTAL_GAUGE = StatusControlView.PREFIX + "horizontal-gauge";
  public const string VERTICAL_GAUGE = StatusControlView.PREFIX + "vertical-gauge";
  public const string GUAGE_BLOCK = StatusControlView.PREFIX + "gauge-block";
  public const string GAUGE_ICON = StatusControlView.PREFIX + "gauge-icon";

  /*************************** Constants **************************/
  const int EXTRA_BLOCK_COUNT = 10;
  const int FIELD_BLOCK_COUNT = StatusController.MAX_FIELD_VALUE;
  static readonly Color EXTRA_GAUGE_COLOR = new Color(0f, 202f/255f, 1f);
  static string[] FIELD_ICON_PATH;
  const string EXTRA_ICON = "icons/power";
  (VisualElement container, VisualElement[] blocks)[] fields;
  (VisualElement container, VisualElement[] blocks) extra;

  public int CursorIndex 
  { 
    get => this.cursorIndex;
    set {
      this.OnCursorChanged(this.cursorIndex, value);
      this.cursorIndex = value;
    }
  } 
  public int cursorIndex { get; private set; } 

  static StatusControlView()
  {
    var paths = new string[StatusController.ALL_FIELDS.Length];
    paths[(int)Field.Barrier] = "icons/barrier";
    paths[(int)Field.Speed] = "icons/speed";
    paths[(int)Field.Booster] = "icons/booster";
    paths[(int)Field.Battery] = "icons/craftship_battery";
    StatusControlView.FIELD_ICON_PATH = paths;
  }
  
  public StatusControlView()
  {
    this.name = StatusControlView.CONTAINER_NAME;
    this.CreateUI();
  }

  public void SetFieldValue(Field field, int value)
  {
    var (_, blocks) = this.fields[(int)field];
    this.UpdateBlocks(blocks, value);
  }

  public void SetExtraValue(int value) 
  {
    this.UpdateBlocks(this.extra.blocks, value);
  }

  public void UpdateBlocks(VisualElement[] blocks, int value)
  {
    for (int i = 0; i < blocks.Length; i++) {
      if (i >= value && blocks[i].ClassListContains("filled")) {
        blocks[i].RemoveFromClassList("filled");
      } 
      else if (i < value && !blocks[i].ClassListContains("filled")) {
        blocks[i].AddToClassList("filled");
      }
    }
  }

  public void MoveCursorToNext()
  {
    var nextIndex = this.CursorIndex + 1;
    if (nextIndex >= this.fields.Length) {
      nextIndex = 0;
    }
    this.CursorIndex = nextIndex;
  }

  public void MoveCursorToPrev()
  {
    var prevIndex = this.CursorIndex - 1;
    if (prevIndex < 0) {
      prevIndex = this.fields.Length - 1;
    }
    this.CursorIndex = prevIndex;
  }

  void CreateUI()
  {
    var fieldsView = this.CreateFieldsView();
    this.Add(fieldsView);
    var extraView = this.CreateExtraView();
    this.Add(extraView);
  }

  void OnCursorChanged(int prev, int next)
  {
    if (prev < this.fields.Length &&
        next < this.fields.Length) {
      if (prev != next) {
        this.fields[prev].container.RemoveFromClassList("active");
        this.fields[next].container.AddToClassList("active");
      }
    }
    else {
      throw (new ArgumentOutOfRangeException());
    }
  }

  VisualElement CreateFieldsView()
  {
    this.fields = new (VisualElement, VisualElement[])[
      StatusController.ALL_FIELDS.Length];
    var container = new VisualElement();
    container.name = StatusControlView.ALL_FIELDS_CONTAINER;
    for (int i = 0; i < StatusController.ALL_FIELDS.Length; ++i) {
      var fieldContainer = new VisualElement();
      fieldContainer.AddToClassList(StatusControlView.FIELD_CONTAINER);
      var gauge = this.CreateGauge(
        count: StatusControlView.FIELD_BLOCK_COUNT,
        blocks: out VisualElement[] blocks,
        color: Color.white);
      gauge.AddToClassList(StatusControlView.VERTICAL_GAUGE);
      fieldContainer.Add(gauge);
      var icon = new Image();
      var field = StatusController.ALL_FIELDS[i];
      icon.image = Resources.Load<Texture2D>(
        StatusControlView.FIELD_ICON_PATH[(int)field]);
      icon.AddToClassList(StatusControlView.GAUGE_ICON);
      fieldContainer.Add(icon);
      container.Add(fieldContainer);
      this.fields[i] = (
        container: fieldContainer,
        blocks: blocks);
    }
    this.fields[this.CursorIndex].container.AddToClassList("active");
    return (container);
  }

  VisualElement CreateExtraView()
  {
    var container = new VisualElement();
    container.name = StatusControlView.EXTRA_CONTAINER; 
    var icon = new Image();
    icon.image = Resources.Load<Texture2D>(StatusControlView.EXTRA_ICON);
    icon.AddToClassList(StatusControlView.GAUGE_ICON);
    container.Add(icon);
    var gauge = this.CreateGauge(
        count: StatusControlView.EXTRA_BLOCK_COUNT,
        blocks: out VisualElement[] blocks,
        color: StatusControlView.EXTRA_GAUGE_COLOR); 
    gauge.AddToClassList(StatusControlView.HORIZONTAL_GAUGE);
    container.Add(gauge);
    this.extra = (container, blocks);
    return (container);
  }
  
  VisualElement CreateGauge(int count, out VisualElement[] blocks, Color color)
  {
    var container = new VisualElement();
    container.AddToClassList(StatusControlView.GUAGE_CONTAINER);
    blocks = new VisualElement[count];
    for (int i = 0; i < count; ++i) {
      var block = new VisualElement();
      block.AddToClassList(StatusControlView.GUAGE_BLOCK);
      block.style.backgroundColor = new StyleColor(color);
      container.Add(block);
      blocks[count - 1 - i] = block;
    }
    return (container);
  }
}
