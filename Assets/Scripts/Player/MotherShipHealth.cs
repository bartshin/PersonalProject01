using System;
using System.Collections.Generic;
using UnityEngine;
using Architecture;

public class MotherShipHealth : ShipHealth
{

  [Header("Configs")]
  [SerializeField]
  int maxBarrier;

  int remainingBarrier;

  override protected void Awake()
  {
    base.Awake();
    this.remainingBarrier = this.maxBarrier;
  }

  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  override protected void Update()
  {

  }
}
