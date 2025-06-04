using System;
using UnityEngine;
using UnityEngine.UIElements;

public class GreetingUI: MonoBehaviour
{
  VisualElement root;
  public bool IsShowing { get; private set; }
  VisualElement synopsis;
  VisualElement goal;
  VisualElement howTo;
  VisualElement startButton;
  VisualElement gameOver;
  Label gameOverText;

  public void Show() 
  { 
    this.root.visible = true;
    this.IsShowing = true;
    this.root.BringToFront();
  }

  public void Hide() 
  { 
    this.root.visible = false;
    this.IsShowing = false;
    this.root.SendToBack();
  }

  public void SetGameOverUI(bool isWin)
  {
    this.gameOver.visible = true; 
    this.gameOverText.text = isWin ? "Contraturation ! You win!":
      "Fail to defense You lose!";
  }

  void Awake()
  {
    this.root = this.GetComponent<UIDocument>().rootVisualElement;
    this.root.name = "greetingUI-container";
    this.root.style.width = Length.Percent(100);
    this.root.style.height = Length.Percent(100);
    this.CreateUI();
    this.Show();
  }

  void CreateUI()
  {
    this.gameOver = this.CreateGameOverView();
    this.gameOver.visible = false;
    this.synopsis = this.CreateSynopsys();
    this.goal = this.CreateGoal();
    this.howTo = this.CreateHowTo();
    this.startButton = this.CreateStartButton();
    this.root.Add(synopsis);
    this.root.Add(goal);
    this.root.Add(howTo);
    this.root.Add(startButton);
  }

  void CreateGameOver()
  {
  }

  Button CreateStartButton()
  {
    Button button = new Button();
    button.name = "start-button";
    Label label = new ("Start");
    label.name = "start-button-label";
    button.Add(label);
    button.RegisterCallback<ClickEvent>(this.OnClickStartButton);
    return (button);
  }

  void OnClickStartButton(ClickEvent e)
  {
    this.Hide(); 
    GameManager.Shared.OnClickStart();
  }

  VisualElement CreateHowTo()
  {
    var container = new VisualElement();
    container.AddToClassList("greetingUI-text-container");
    var label = new Label();
    label.AddToClassList("greetingUI-text-label");
    label.text = HOW_TO_PLAY;
    container.Add(label);
    return (container);
  }

  VisualElement CreateGameOverView()
  {
    var container = new VisualElement();
    container.AddToClassList("greetingUI-text-container");
    var label = new Label();
    label.text = "";
    label.AddToClassList("greetingUI-text-label");
    this.gameOverText = label;
    container.Add(label);
    return (container);
  }

  VisualElement CreateSynopsys()
  {
    var container = new VisualElement();
    container.AddToClassList("greetingUI-text-container");
    var label = new Label();
    label.text = SYNOPSIS;
    label.AddToClassList("greetingUI-text-label");
    container.Add(label);
    return (container);
  }

  VisualElement CreateGoal()
  {
    var container = new VisualElement();
    container.AddToClassList("greetingUI-text-container");
    var label = new Label();
    label.text = GOAL;
    label.AddToClassList("greetingUI-text-label");
    container.Add(label);
    return (container);
  }

  const string SYNOPSIS = "Defend against waves of enemy ships in space!\n Take control of your battle cruiser and fight back.\n Use powerful weapons and upgrades to defeat enemies.\n Quick thinking and strategy will lead you to victory.\n Save the galaxy and protect the peace!";

  const string GOAL = "Enemies will constantly attack you! Survive 5 minute!";

  const string HOW_TO_PLAY = "Camera Change\npress number 1 key: top view\npress number 2: left view\npress number 3: right view\n\tAttack\n\tside view attack\n\tmouse left click: fire gun\n\tright click: launch missile\n\ttop view attack\n\tmouse left click: select enemy to attack queue\n\tmouse right click: attack enemy now\n\tenter key: attack enemy in queue";
}
