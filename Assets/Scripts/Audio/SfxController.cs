using System;
using System.Collections.Generic;
using UnityEngine;
using Architecture;

public class SfxController : MonoBehaviour, IPooedObject
{
  public Action<IPooedObject> OnDisabled { get; set; }
  AudioSource source;
  static readonly System.Random VOLUME_RAND = new ();

  float remainingPlayTime;

  public void PlaySound(AudioClip clip, Vector3 position, (int min, int max) volumeRange)
  {
    var volume = SfxController.VOLUME_RAND.Next(
      volumeRange.min,
      volumeRange.max
    );
    this.transform.position = position;
    this.SetVolume(volume);
    this.PlaySound(clip);
  }

  public void PlaySound(AudioClip clip)
  {
    this.SetSound(clip);
    this.source.Play();
  }

  public void Pause()
  {
    this.source.Pause();
  }

  public void PlayBack()
  {
    if (this.source.isPlaying && this.source.time != 0) {
      this.source.UnPause();
    }
    else {
      this.source.Play();
    }
  }

  public void SetVolume(float volume)
  {
    this.source.volume = volume;
  }

  public void SetSound(AudioClip clip)
  {
    this.source.Stop();
    this.source.clip = clip;
    this.remainingPlayTime = clip.length;
  }

  public void SetLoop(bool loop)
  {
    this.source.loop = loop;
  }

  void Awake()
  {
    this.source = this.GetComponent<AudioSource>();
  }

  // Update is called once per frame
  void Update()
  {
    if (this.source.loop)  {
      return ;
    }
    this.remainingPlayTime -= Time.deltaTime;
    if (this.remainingPlayTime <= 0) {
      this.source.Stop();
      this.source.clip = null;
      this.transform.parent = AudioManager.Shared.transform;
      this.transform.position = Vector3.zero;
      this.gameObject.SetActive(false);
    }
  }

  void OnDisable()
  {
    this.source.loop = false;
    if (this.OnDisabled != null) {
      this.OnDisabled.Invoke(this);
    }
  }
}
