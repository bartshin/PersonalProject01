using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architecture
{

  public class SingletonBehaviour<T>: MonoBehaviour where T: MonoBehaviour
  {
    static T instance;

    public static T Shared => SingletonBehaviour<T>.instance;
  
    public static void Destroy()
    {
      if (SingletonBehaviour<T>.instance != null) {
        Destroy(SingletonBehaviour<T>.Shared.gameObject);
        SingletonBehaviour<T>.instance = null;
      }
    }

    public static void Create()
    {
      SingletonBehaviour<T>.instance = FindObjectOfType<T>() ?? 
        SingletonBehaviour<T>.CreateInstance();
    }

    protected static T CreateInstance() 
    {
      var gameObject = new GameObject(typeof(T).Name);
      DontDestroyOnLoad(gameObject);
      return (gameObject.AddComponent<T>());
    }

    protected void OnAwake()
    {
      if (SingletonBehaviour<T>.instance == null) {
        SingletonBehaviour<T>.instance = this as T;
      }
      else if (SingletonBehaviour<T>.instance != this) {
        Destroy(this.gameObject);
      }
    }
  }
}
