using System;
using System.Collections.Generic;
using UnityEngine;
using Architecture;

public class StatusController : MonoBehaviour
{
  public enum Field
  {
    Barrier,
    Speed,
    Booster,
    Battery
  }
  public static readonly Field[] ALL_FIELDS = (Field[])Enum.GetValues(typeof(Field));
  public const int MAX_FIELD_VALUE = 5;

  public class PowerDistribution
  {

    public int MaxPower;
    public ObservableValue<int> MotherShipBarrier { get; private set; }
    public ObservableValue<int> MotherShipSpeed { get; private set; }
    public ObservableValue<int> MotherShipBooster { get; private set; }
    public ObservableValue<int> CraftshipBattery { get; private set; }
    public ObservableValue<int> ExtraPower { get; private set; }

    public PowerDistribution(
      int maxPower,
      int motherShipBarrier,
      int motherShipSpeed,
      int motherShipBooster,
      int craftshipBattery
    )
    {
      this.MaxPower = maxPower;
      this.MotherShipBarrier = new (motherShipBarrier);
      this.MotherShipSpeed = new (motherShipSpeed);
      this.MotherShipBooster = new (motherShipBooster);
      this.CraftshipBattery = new (craftshipBattery);
      this.ExtraPower = new (maxPower - motherShipBarrier - motherShipSpeed - motherShipBooster - craftshipBattery
      );
      if (this.ExtraPower.Value < 0) {
        throw new ApplicationException("ExtraPower less than zero");
      }
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
      craftshipBattery: this.distributionConfigs.CraftshipBattery
    );
  }

  // Start is called before the first frame update
  void Start()
  {
    UserInputManager.Shared.NavigateDirection.OnChanged += this.OnNavigationChanged;
    UIManager.Shared.SetDistribution(this.distribution);
  }

  void OnNavigationChanged(Nullable<Direction> direction)
  {
    if (direction == null || 
        direction == Direction.Left ||
        direction == Direction.Right) {
      return ;
    }
    var selected = UIManager.Shared.SelectedField;
    var field = selected switch  {
      Field.Barrier => this.distribution.MotherShipBarrier,
      Field.Speed => this.distribution.MotherShipSpeed,
      Field.Booster => this.distribution.MotherShipBooster,
      Field.Battery => this.distribution.CraftshipBattery,
      _ => throw new NotImplementedException()
    };
    var extraPower = this.distribution.ExtraPower.Value;
    if (field.Value > 0 && direction == Direction.Down) {
      field.Value -= 1;
      this.distribution.ExtraPower.Value += 1;
    }
    else if (field.Value < StatusController.MAX_FIELD_VALUE &&
      direction == Direction.Up && extraPower > 0) {
      field.Value += 1;
      this.distribution.ExtraPower.Value -= 1;
    }
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
      this.Distribution.CraftshipBattery.Value = this.distributionConfigs.CraftshipBattery;
    }
  }

  [Serializable]
  public class DistributionConfigs 
  {
    public int Max;
    public int MotherShipBarrier; 
    public int MotherShipSpeed;
    public int MotherShipBooster;
    public int CraftshipBattery;
  }
}
