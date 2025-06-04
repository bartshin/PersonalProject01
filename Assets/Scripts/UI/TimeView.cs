using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TimeView: VisualElement
{
  const string CONTAINER_NAME = "timeview-container";
  const string TIME_LABEL = "timeview-time-label";
  (int min, int seconds) displayedTime;
  StringBuilder stringBuilder = new StringBuilder();
  Label timeLabel;

  public TimeView()
  {
    this.name = TimeView.CONTAINER_NAME;
    this.displayedTime = (0, 0);
    this.CreateUI();
  }

  public void SetTime(int seconds)
  {
    this.displayedTime = (seconds / 60, seconds % 60);
    this.UpdateLabel();
  }

  void CreateUI()
  {
    var label = new Label();
    label.text = this.GetLabelText(); 
    label.name = TimeView.TIME_LABEL;
    this.Add(label);
    this.timeLabel = label;
  }

  string GetLabelText()
  {
    this.stringBuilder.Clear();
    if (this.displayedTime.min < 10) {
      this.stringBuilder.Append($"0{this.displayedTime.min}");
    }
    else {
      this.stringBuilder.Append($"{this.displayedTime.min}");
    }
    this.stringBuilder.Append(" : ");
    if (this.displayedTime.seconds < 10) {
      this.stringBuilder.Append($"0{this.displayedTime.seconds }");
    }
    else {
      this.stringBuilder.Append($"{this.displayedTime.seconds }");
    }
    return (this.stringBuilder.ToString());
  }

  void UpdateLabel()
  {
    this.timeLabel.text = this.GetLabelText();
  }
}
