using System;
using System.Collections.Generic;
using UnityEngine;
using Architecture;

public class StatusController : MonoBehaviour
{
  public class PowerDistribution
  {

    public int MaxPower;
    public ObservableValue<int> MotherShipBarrier { get; private set; }
    public ObservableValue<int> MotherShipSpeed { get; private set; }
    public ObservableValue<int> MotherShipBooster { get; private set; }
    public ObservableValue<int> CraftShipBarrier { get; private set; }

    public PowerDistribution(
      int maxPower,
      int motherShipBarrier,
      int motherShipSpeed,
      int motherShipBooster,
      int craftShipBarrier
    )
    {
      this.MaxPower = maxPower;
      this.MotherShipBarrier = new (motherShipBarrier);
      this.MotherShipSpeed = new (motherShipSpeed);
      this.MotherShipBooster = new (motherShipBooster);
      this.CraftShipBarrier = new (craftShipBarrier);
    }

    public void SetPower(int power)
    {
      // TODO: Check current usage
      this.MaxPower = power;
    }
  }

  public PowerDistribution Distribution 
  { 
    get => this.distribution;
    private set {
      this.distribution = value;
    }
  }
  [Header("References")]
  [SerializeField]
  MotherShipHealth health;

  [Header("Configs")]
  [SerializeField]
  DistributionConfigs distributionConfigs;
  PowerDistribution distribution;

  void Awake()
  {
    if (this.health == null) {
      this.health = this.GetComponent<MotherShipHealth>();
    }
    this.Distribution = new (
      maxPower: this.distributionConfigs.Max,
      motherShipBarrier: this.distributionConfigs.MotherShipBarrier,
      motherShipSpeed: this.distributionConfigs.MotherShipSpeed,
      motherShipBooster: this.distributionConfigs.MotherShipBooster,
      craftShipBarrier: this.distributionConfigs.CraftShipBarrier
    );
  }

  // Start is called before the first frame update
  void Start()
  {
  }

  // Update is called once per frame
  void Update()
  {
  }

  void OnValidate()
  {
    if (this.Distribution != null && this.Distribution.MotherShipSpeed != null) {
      this.Distribution.MaxPower = this.distributionConfigs.Max;
      this.Distribution.MotherShipBarrier.Value = this.distributionConfigs.MotherShipBarrier;
      this.Distribution.MotherShipSpeed.Value = this.distributionConfigs.MotherShipSpeed;
      this.Distribution.MotherShipBooster.Value = this.distributionConfigs.MotherShipBooster;
      this.Distribution.CraftShipBarrier.Value = this.distributionConfigs.CraftShipBarrier;
    }
  }

  [Serializable]
  public class DistributionConfigs 
  {
    public int Max;
    public int MotherShipBarrier; 
    public int MotherShipSpeed;
    public int MotherShipBooster;
    public int CraftShipBarrier;
  }
}
