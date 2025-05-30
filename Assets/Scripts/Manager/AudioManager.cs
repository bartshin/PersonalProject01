using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architecture;

public class AudioManager : SingletonBehaviour<AudioManager>
{

  new public static void CreateInstance()  
  {
    GameObject prefab = Resources.Load<GameObject>("Prefabs/AudioManager");
    var gameObject = Instantiate(prefab);
    DontDestroyOnLoad(gameObject);
  }

  const int DEFAULT_SFX_POOL_SIZE = 30;
  [SerializeField]
  GameObject sfxControllerPrefab;
  ObjectPool<SfxController> sfxPool;

  public SfxController GetSfxController()
  {
    return (this.sfxPool.Get());
  }

  void Awake()
  {
    base.OnAwake();
    this.sfxPool = new MonoBehaviourPool<SfxController>(
        poolSize: DEFAULT_SFX_POOL_SIZE,
        prefab: sfxControllerPrefab);
  }
}
