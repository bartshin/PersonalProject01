using System;
using System.Collections.Generic;
using UnityEngine;
using Architecture;

public class GameManager : SingletonBehaviour<GameManager>
{
  public enum GameState 
  {
  }

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
  new public static void CreateInstance()  
  {
    SingletonBehaviour<GameManager>.CreateInstance();
  }
  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {

  }
}
