using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerHpView : VisualElement
{
  static readonly Color HP_COLOR = new Color(234f/255f, 45f/255f, 20f/255f);
  static readonly Color BARRIER_COLOR = new Color(0f, 202f/255f, 1f);
  /****************** Text ***********************************/
  const string MOTHERSHIP_LABEL_TEXT = "Mother Ship";
  const string CRAFTSHIP_LABEL_TEXT = "Sub Ship";
  /****************** USS Selector ***************************/
  const string CONTAINER = "player-hp-view-container";
  const string CRAFTSHIPS_STATUS_VIEW = "bornecraft-ships-status-view";
  const string MOTHERSHIP_STATUS_CONTAINER = "mothership-status-container";
  const string CRAFTSHIP_STATUS_CONTAINER = "bornecraft-ship-status-container";
  const string CRAFT_PORTRAIT_IMAGE = "bornecraft-portrait-image";
  const string STATUS_CONTAINER_LABEL = "status-container-label";
  const string CRAFT_CONTEINR_LABEL = "bornecraft-container-label";
  const string BAR_CONTAINER = "bar-container";
  const string BAR_LABEL = "bar-label";
  const string BAR = "bar";
  const string BAR_BACKGROUND = "bar-background";
  const string BAR_FILL = "bar-fill";
  const string BAR_MASK = "bar-mask";
  const string CRAFTSHIP_HP_BAR_CONTAINER = "craftship-hp-bar-container";
  const string CRAFTSHIP_BARRIER_BAR_CONTAINER = "craftship-barrier-bar-container";
  /******************** Constants ***************************/
  readonly static Vector2Int CRAFTSHIP_TEXTURE_SIZE = new (128, 128);

  public RenderTexture[] CraftshipTextures { get; private set; }
  public VisualElement[] CraftshipPortrait { get; private set; }
  public (VisualElement hp, VisualElement barrier) MotherShipHandle { get; private set; }
  public (VisualElement hp, VisualElement barrier)[] CraftshipHpHandles { get; private set; }

  public PlayerHpView()
  {
    this.name = PlayerHpView.CONTAINER;
  }

  public void SetHiddenTo(VisualElement element, bool hidden)
  {
    if (hidden) {
      element.AddToClassList("hidden");
    }
    else {
      element.RemoveFromClassList("hidden");
    }
  }

  public void CreateMotherShipStatus()
  {
    var motherShipHpContainer = this.CreateMotherShipStatusView();
    this.Add(motherShipHpContainer);
  }

  public void CreateBronecraftStatus(int bornecraftshipCount)
  {
    this.CreateBronecraftTextures(bornecraftshipCount);
    this.CraftshipHpHandles = new (VisualElement, VisualElement)[bornecraftshipCount];
    this.CraftshipPortrait = new VisualElement[bornecraftshipCount];
    var bornecraftshipsHpView = new VisualElement();
    bornecraftshipsHpView.name = PlayerHpView.CRAFTSHIPS_STATUS_VIEW;
    for (int i = 0; i < bornecraftshipCount; ++i) {
      bornecraftshipsHpView.Add(this.CreateBornecraftsStatusView(i)); 
    } 
    this.Add(bornecraftshipsHpView);
  }

  public void SetValue(VisualElement handle, float percentage)
  {
    handle.style.translate = new StyleTranslate(
      new Translate(new Length(percentage * 100f, LengthUnit.Percent),
        new Length())
    );
  }

  void CreateBronecraftTextures(int count)
  {
    this.CraftshipTextures = new RenderTexture[count];
    for (int i = 0; i < this.CraftshipTextures.Length; ++i) {
      this.CraftshipTextures[i] = new RenderTexture(
        width: PlayerHpView.CRAFTSHIP_TEXTURE_SIZE.x,
        height: PlayerHpView.CRAFTSHIP_TEXTURE_SIZE.y,
        depth: 16,
        format: RenderTextureFormat.ARGB32
      ); 
    }
  }

  VisualElement CreateMotherShipStatusView()
  {
    var container = new VisualElement();
    container.name = PlayerHpView.MOTHERSHIP_STATUS_CONTAINER;

    var containerLabel = new Label(PlayerHpView.MOTHERSHIP_LABEL_TEXT);
    containerLabel.AddToClassList(PlayerHpView.STATUS_CONTAINER_LABEL);
    container.Add(containerLabel);
    var hpBar = this.CreateBar("HP", out VisualElement hpHandle, PlayerHpView.HP_COLOR);

    var barrierBar = this.CreateBar("Shield", out VisualElement barrierHandle, PlayerHpView.BARRIER_COLOR);
    
    this.MotherShipHandle = (hp: hpHandle, barrier: barrierHandle);
    container.Add(hpBar);
    container.Add(barrierBar);
    return (container);
  }

  VisualElement CreateBornecraftsStatusView(int number)
  {
    var container = new VisualElement();
    container.AddToClassList(PlayerHpView.CRAFTSHIP_STATUS_CONTAINER);
    var containerLabel = new Label($"{PlayerHpView.CRAFTSHIP_LABEL_TEXT} {number + 1}");
    containerLabel.AddToClassList(PlayerHpView.STATUS_CONTAINER_LABEL);
    containerLabel.AddToClassList(PlayerHpView.CRAFT_CONTEINR_LABEL);
    container.Add(containerLabel);

    var portraitImage = new VisualElement();
    portraitImage.AddToClassList(PlayerHpView.CRAFT_PORTRAIT_IMAGE);
    portraitImage.AddToClassList("hidden");
    portraitImage.style.backgroundImage = new StyleBackground(
      Background.FromRenderTexture(this.CraftshipTextures[number])
    );
    container.Add(portraitImage);
    this.CraftshipPortrait[number] = portraitImage;

    var hpBar = this.CreateBar(
       labelText: null,
       handle: out VisualElement hpHandle,
       color: PlayerHpView.HP_COLOR);
    hpBar.AddToClassList(PlayerHpView.CRAFTSHIP_HP_BAR_CONTAINER);

    var barrierBar = this.CreateBar(
        labelText: null,
        handle: out VisualElement barrierHandle,
        color: PlayerHpView.BARRIER_COLOR);
    barrierBar.AddToClassList(PlayerHpView.CRAFTSHIP_BARRIER_BAR_CONTAINER); 

    this.CraftshipHpHandles[number] = (hp: hpHandle,barrier: barrierHandle);
    container.Add(hpBar);
    container.Add(barrierBar);
    return (container);
  }

  VisualElement CreateBar(in string labelText, out VisualElement handle, Color color )
  {
    var container = new VisualElement();
    container.AddToClassList(PlayerHpView.BAR_CONTAINER);

    if (labelText != null) {
      var label = new Label(labelText);
      label.AddToClassList(PlayerHpView.BAR_LABEL);
      label.style.color = color;
      container.Add(label);
    }

    var hpBar = new VisualElement();
    hpBar.AddToClassList(PlayerHpView.BAR);
    this.SetBorderColor(hpBar, color);
    container.Add(hpBar);

    var background = new VisualElement();
    background.AddToClassList(PlayerHpView.BAR_BACKGROUND);
    hpBar.Add(background);

    var fill = new VisualElement();
    fill.AddToClassList(PlayerHpView.BAR_FILL);
    fill.style.unityBackgroundImageTintColor = color;
    background.Add(fill);

    var mask = new VisualElement();
    mask.AddToClassList(PlayerHpView.BAR_MASK);
    mask.style.translate = new StyleTranslate(
      new Translate(new Length(100f, LengthUnit.Percent), new Length())
    );
    background.Add(mask);
    handle = mask;

    return (container);
  }

  void SetBorderColor(VisualElement element, Color color) 
  {
    element.style.borderTopColor = color;
    element.style.borderBottomColor = color;
    element.style.borderLeftColor = color;
    element.style.borderRightColor = color;
  }
}
