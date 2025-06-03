using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Architecture;

public class GauageImageUI : MonoBehaviour
{
  [SerializeField]
  Image gauageImage;
  public ObservableValue<(float, float)> WatchingFloatValue 
  {
    get => this.watchingFloatValue;
    set {
      if (this.watchingFloatValue != null) {
        this.watchingFloatValue.OnChanged -= this.OnValueChanged;
      }
      this.watchingFloatValue = value;
      if (value != null) {
        value.OnChanged += this.OnValueChanged;
      }
    }
  }
  public ObservableValue<(int, int)> WatchingIntValue
  { 
    get => this.watchingIntValue;
    set {
      if (this.watchingIntValue != null) {
        this.watchingIntValue.OnChanged -= this.OnValueChanged; 
      }
      this.watchingIntValue = value;
      if (value != null) {
        value.OnChanged += this.OnValueChanged;
      }
    }
  }

  ObservableValue<(float, float)> watchingFloatValue;
  ObservableValue<(int, int)> watchingIntValue;
  bool isAnimating;

  float destValue = 1f;
  const float ANIMATE_LERP_STEP = 10f;
  const float ANIMATE_LERP_THRESHOLD = 0.01f;

  // Start is called before the first frame update
  void OnEnable()
  {
    if (this.watchingFloatValue != null) {
      this.watchingFloatValue.OnChanged += this.OnValueChanged;
    }
    else if (this.watchingIntValue != null) {
      this.watchingIntValue.OnChanged -= this.OnValueChanged; 
    }
  }

  void OnDisable()
  {
    if (this.watchingFloatValue != null) {
      this.watchingFloatValue.OnChanged -= this.OnValueChanged;
      this.watchingFloatValue = null;
    }
    else if (this.watchingIntValue != null) {
      this.watchingIntValue.OnChanged -= this.OnValueChanged;
      this.watchingIntValue = null;
    }
  }

  // Update is called once per frame
  void Update()
  {
    if (this.isAnimating) {
      if (Math.Abs(this.gauageImage.fillAmount - this.destValue)
          < ANIMATE_LERP_THRESHOLD) {
        this.gauageImage.fillAmount = this.destValue;
        this.isAnimating = false;
      }
      else {
        this.gauageImage.fillAmount = Mathf.Lerp(
            this.gauageImage.fillAmount,
            this.destValue,
            ANIMATE_LERP_STEP * Time.deltaTime
            );
      }
    }
  }

  void OnValueChanged((int current, int max) value) 
  {
    this.OnValueChanged(((float) value.current, (float) value.max));
  }

  void OnValueChanged((float current, float max) value)
  {
    if (this.gauageImage.fillAmount != this.destValue || 
        value.current <= 0f) {
      this.gauageImage.fillAmount = this.destValue;
    }
    this.destValue = Math.Clamp(
        value.current / value.max, 0, 1);
    this.isAnimating = true;
  }
}
