using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architecture;

public class AudioManager : SingletonBehaviour<AudioManager>
{
  public static AudioClip SmallExposionSound { get; private set; }
  static string[] bgms;

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
    AudioManager.SmallExposionSound = Resources.Load<AudioClip>("Audio/ship_explosion_short");
    AudioManager.bgms = new string[3] {
      "Audio/Space Threat  (Slow Ambient Version, Looped)",
      "Audio/Space Threat (Electronic Dramatic Version, Looped)",
      "Audio/Space Threat (Energetic Powerful Version, Looped)"
    };
    this.PlayBgm();
  }

  void PlayBgm()
  {
    var index = new System.Random().Next(0, 3);
    var bgm = Resources.Load<AudioClip>(AudioManager.bgms[index]);
    var sfx = this.GetSfxController();
    sfx.SetLoop(true);
    sfx.SetVolume(0.3f);
    sfx.PlaySound(bgm);
  }
}
