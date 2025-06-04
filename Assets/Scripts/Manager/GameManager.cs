using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architecture;

public class GameManager : SingletonBehaviour<GameManager>
{
  [SerializeField]
  GameObject[] enemyPrefabs;
  [SerializeField]
  float[] enemyChances;
  [SerializeField]
  float enemySpawnMinRange;
  [SerializeField]
  float enemySpawnMaxRange;
  [SerializeField]
  int maxEnemyCount;
  [SerializeField]
  int minEnemyCount;
  [SerializeField]
  int defaultHeal;
  [SerializeField]
  int healIncrease;
  [SerializeField]
  float clearTime;
  [SerializeField]
  float enemySpawnInterval;

  const float MAX_ENEMY_DIST = 500f;
  const int MAX_ENEMY_COUNT = 50;

  public int ClearTimeInSeconds => (int)this.clearTime;
  float startTime;
  float passedTime => Time.time - this.startTime;
  YieldInstruction WaitToCheckTime = new WaitForSeconds(0.5f);
  YieldInstruction WaitToSpawnEnemy = new WaitForSeconds(1f);
  ObservableValue<int> TimeRemain;
  public Action OnClear;
  float nextEnemySpawn = float.MaxValue;

  Dictionary<IDamagable, EnemyShip> currentEnemies;
  public Action<int> OnHealPlayer;
  bool isClear = false;
  Coroutine timeRoutine;
  Coroutine spawnEnemyRoutine;
  bool positionIsPositive;

  System.Random rand;
  float GetRandomPercentage() => (float)this.rand.Next(0, 100) / 100f;

  public enum GameState 
  {
    Spawning,
    InCombat
  }
  public GameState State { get; private set; }

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
  new public static void CreateInstance()  
  {
    GameObject prefab = Resources.Load<GameObject>("Prefabs/GameManager");
    var gameObject = Instantiate(prefab);
    DontDestroyOnLoad(gameObject);
    CombatManager.CreateInstance();
    CameraManager.CreateInstance();
    UserInputManager.CreateInstance();
    AudioManager.CreateInstance();
    UIManager.CreateInstance();
  }

  public void GameOver()
  {
    UIManager.Shared.SetVisbleCombatUI(false);  
    UIManager.Shared.SetVisbleGreetingUI(true);  
    UIManager.Shared.SetGameOverUI(false);
    foreach (var (_, enemy) in this.currentEnemies) {
      Destroy(enemy.gameObject);
    }

    this.currentEnemies.Clear();
  }

  public void OnClickStart()
  {
    var prefab = Resources.Load<GameObject>("Prefabs/PlayerMotherShip");
    Instantiate(prefab);
    UIManager.Shared.SetVisbleCombatUI(true);
  }

  void Awake()
  {
    base.OnAwake();
    this.rand = new();
    this.TimeRemain = new(this.ClearTimeInSeconds);
    this.currentEnemies = new();
  }

  public void StartGame()
  {
    this.isClear = false;
    this.startTime = Time.time;
    this.timeRoutine = this.StartCoroutine(this.Tick());
    this.TimeRemain.Value = this.ClearTimeInSeconds;
    this.TimeRemain.OnChanged += this.OnTimeChanged;
  }

  void OnTimeChanged(int seconds)
  {
    UIManager.Shared.SetTime(seconds);
  }

  IEnumerator Tick()
  {
    while (!this.isClear) {
      this.TimeRemain.Value = this.ClearTimeInSeconds - (int)(this.passedTime);
      if (this.nextEnemySpawn < Time.time && 
          this.currentEnemies.Count < GameManager.MAX_ENEMY_COUNT) {
        this.SpawnEnemies();
      }
      if (this.passedTime > this.clearTime) {
        this.isClear = true;
        UIManager.Shared.SetVisbleCombatUI(false);  
        UIManager.Shared.SetVisbleGreetingUI(true);  
        UIManager.Shared.SetGameOverUI(true);
        foreach (var (_, enemy) in this.currentEnemies) {
          Destroy(enemy.gameObject);
        }

        this.currentEnemies.Clear();
        if (this.OnClear != null) {
          this.OnClear.Invoke();
        }
      }
      yield return (this.WaitToCheckTime);
    }
  }

  void CheckEnemy()
  {
    var player = GameObject.FindGameObjectWithTag("Player");
    if (player != null) {
      foreach (var (damagable, ship) in this.currentEnemies) {
        var dist = Vector3.Distance( 
            ship.transform.position,
            player.transform.position);
        if (dist < GameManager.MAX_ENEMY_DIST) { 
          continue;
        }
        ship.transform.position = this.GetRandomPosition(player.transform.position);
        ship.AddPatrolPoint(player.transform.position);
      }
    }

  }

  void SpawnEnemies()
  {
    var gameObject = GameObject.FindGameObjectWithTag("Player");
    this.StartSpwanEnemies(gameObject.transform);
  }

  public void StartSpwanEnemies(Transform player)
  {
    this.nextEnemySpawn = Time.time + this.enemySpawnInterval;
    this.State = GameState.Spawning;
    if (this.spawnEnemyRoutine != null) {
      this.StopCoroutine(this.spawnEnemyRoutine);
    }
    var count = UnityEngine.Random.Range(
      this.minEnemyCount, this.maxEnemyCount
    );
    this.spawnEnemyRoutine = this.StartCoroutine(
      this.SpawnEnemiesRoutine(count, player)); 
  }

  IEnumerator SpawnEnemiesRoutine(int count, Transform player)
  {
    var spawnedCount = 0;
    while (spawnedCount < count) {
      var enemy = this.SpawnEnemy(
        this.GetRandomPosition(player.position), this.PickPrefab());
      var chance = this.GetRandomPercentage();
      if (chance < 0.3f) {
        enemy.AddPatrolPoint(player.position);
      }
      else {
        enemy.AddPatrolPoint(this.GetRandomPosition(player.position));
      }
      spawnedCount += 1;
      yield return (this.WaitToSpawnEnemy);
    }
    this.State = GameState.InCombat;
  }

  EnemyShip SpawnEnemy(Vector3 position, GameObject prefab)
  {
    var gameObject = Instantiate(prefab, position, Quaternion.identity);
    var enemy = gameObject.GetComponent<EnemyShip>(); 
    var damagable = IDamagable.GetDamagable(gameObject);
    damagable.OnDestroyed += this.OnEnemyDestroyed;
    this.currentEnemies.TryAdd(damagable, enemy);
    return (enemy);
  }

  void OnEnemyDestroyed(IDamagable damagable)
  {
    if (this.currentEnemies.TryGetValue(damagable, out EnemyShip enemy)) {
      this.currentEnemies.Remove(damagable);
      int healAmount = this.defaultHeal;
      if (enemy.name.Contains("Large")) {
        healAmount += this.healIncrease * 2;
      }
      else if (enemy.name.Contains("Medium")) {
        healAmount += this.healIncrease;
      }
      if (this.OnHealPlayer != null) {
        this.OnHealPlayer.Invoke(healAmount);
      }
    }
  }

  Vector3 GetRandomPosition(Vector3 center)
  {
    var x = UnityEngine.Random.Range(this.enemySpawnMinRange, this.enemySpawnMaxRange);
    var y = UnityEngine.Random.Range(0f, 0.5f);
    var z = UnityEngine.Random.Range(this.enemySpawnMinRange, this.enemySpawnMaxRange);
    return (new Vector3(
      this.GetRandomSignedValue(x),
      this.GetRandomSignedValue(y), 
      this.GetRandomSignedValue(z)));
  }

  float GetRandomSignedValue(float value) {
    this.positionIsPositive = !this.positionIsPositive;
    if (!this.positionIsPositive) {
      return (-value);
    }
    return (value);
  }

  GameObject PickPrefab()
  {
    var percentage = this.GetRandomPercentage();
    for (int i = 0; i < this.enemyChances.Length; i++) {
       if (percentage < this.enemyChances[i]) {
          return (this.enemyPrefabs[i]);
       }
    }
    return (this.enemyPrefabs[this.enemyPrefabs.Length - 1]);
  }
}
