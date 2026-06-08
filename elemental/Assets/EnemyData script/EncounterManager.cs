using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

// 敵ごとの出現確率を設定するための構造体
[System.Serializable]
public struct EnemySpawnWeight
{
    public EnemyData enemyData;
    [Tooltip("この敵の出現しやすさ（数字が大きいほど出やすい）")]
    public int weight;
}

public class EncounterManager : MonoBehaviour
{
    [Header("敵の基本プレハブ (EnemyObject)")]
    public GameObject enemyPrefab;

    [Header("敵を配置する画面上のエリア（Canvas内の空オブジェクト）")]
    public Transform enemySpawnArea;

    [Header("▼ 敵の配置間隔")]
    public float horizontalSpacing = 180f;
    public float verticalSpacing = 100f;

    [Header("▼ 出現対数（1体、2体、3体）それぞれの重み(Weight)")]
    [Tooltip("1体、2体、3体が出現する確率の比率。例: 1, 4, 5 にすると 1体=10%, 2体=40%, 3体=50%")]
    public int weightFor1Enemy = 1;
    public int weightFor2Enemies = 4;
    public int weightFor3Enemies = 5;

    [Header("▼ 現在の進行度（自動取得）")]
    public int currentMap = 1;
    public int currentFloor = 1;

    [Header("① 1階層目確定の敵データ（スライムなど）")]
    public EnemyData firstFloorEnemy;

    [Header("② 第1マップ：道中の通常敵プール（Weight付き）")]
    public List<EnemySpawnWeight> map1NormalPool = new List<EnemySpawnWeight>();

    [Header("②-Ex 第1マップ：1体出現時限定の追加レア・強力敵プール")]
    public List<EnemySpawnWeight> map1SingleOnlyPool = new List<EnemySpawnWeight>();

    [Header("③ 第2マップ：道中の通常敵プール（Weight付き）")]
    public List<EnemySpawnWeight> map2NormalPool = new List<EnemySpawnWeight>();

    [Header("③-Ex 第2マップ：1体出現時限定の追加レア・強力敵プール")]
    public List<EnemySpawnWeight> map2SingleOnlyPool = new List<EnemySpawnWeight>();

    // 今回出現させる敵のリスト
    private List<EnemyData> enemiesToSpawn = new List<EnemyData>();

    void Awake()
    {
        string nodeName = PlayerPrefs.GetString("CurrentChallengingNode", "");

        if (!string.IsNullOrEmpty(nodeName))
        {
            Match match = Regex.Match(nodeName, @"Floor([0-9]+)");
            if (match.Success)
            {
                if (int.TryParse(match.Groups[1].Value, out int floorNumber))
                {
                    currentFloor = floorNumber;
                }
            }
        }

        Debug.Log($"<color=green>【EncounterManager】現在の階層を自動取得しました: {currentFloor} 層目 (読み取ったノード名: {nodeName})</color>");
    }

    void Start()
    {
        GenerateBattle();
    }

    public void GenerateBattle()
    {
        if (enemySpawnArea != null)
        {
            foreach (Transform child in enemySpawnArea)
            {
                Destroy(child.gameObject);
            }
        }

        DetermineEnemies();
        SpawnEnemies();
    }

    private void DetermineEnemies()
    {
        enemiesToSpawn.Clear();

        // 1. 現在のマップに応じて、ベースとなる通常プールと、1体時限定のレアプールを切り替える
        List<EnemySpawnWeight> basePool = (currentMap == 2) ? map2NormalPool : map1NormalPool;
        List<EnemySpawnWeight> singleOnlyPool = (currentMap == 2) ? map2SingleOnlyPool : map1SingleOnlyPool;

        // 実際に今回の戦闘の抽選に使う「最終的なプール」の土台を作成
        List<EnemySpawnWeight> finalLotteryPool = new List<EnemySpawnWeight>(basePool);

        // 【例外安全】万が一通常プールが空だった場合のバックアップ
        if (finalLotteryPool.Count == 0)
        {
            if (firstFloorEnemy != null) enemiesToSpawn.Add(firstFloorEnemy);
            return;
        }

        // --- ルール①：1層目の最初の戦闘 ---
        bool isFirstBattleOfRun = currentFloor <= 1 && string.IsNullOrEmpty(PlayerPrefs.GetString("LastClearedNode", ""));

        if (isFirstBattleOfRun)
        {
            Debug.Log("【EncounterManager】1層目の『初回戦闘』のため固定エネミー構成にします。");
            if (firstFloorEnemy != null) enemiesToSpawn.Add(firstFloorEnemy);
            enemiesToSpawn.Add(GetRandomEnemyFromPool(finalLotteryPool));
        }
        // --- ルール②：それ以降のすべての戦闘 ---
        else
        {
            // 1. 出現する「対数（1~3体）」をWeightに基づいて抽選
            int enemyCount = RollEnemyCount();
            Debug.Log($"【EncounterManager】通常戦闘。抽選された出現対数: {enemyCount} 体");

            // ========================================================
            // ★核心：もし「1体出現」が選ばれたなら、限定のレアプールを抽選対象に必ず合流させる！
            // ========================================================
            if (enemyCount == 1 && singleOnlyPool != null && singleOnlyPool.Count > 0)
            {
                Debug.Log("<color=orange>【EncounterManager】1体出現が選ばれたため、レア・強力エネミープールを抽選対象に合流させました！</color>");
                finalLotteryPool.AddRange(singleOnlyPool);
            }

            // 2. 決まった数だけ、最終的なプールからWeightに基づいて毎回個別抽選
            for (int i = 0; i < enemyCount; i++)
            {
                enemiesToSpawn.Add(GetRandomEnemyFromPool(finalLotteryPool));
            }
        }
    }

    /// <summary>
    /// 設定されたWeight(重み)を元に、1～3体の出現数をランダム決定する
    /// </summary>
    private int RollEnemyCount()
    {
        int totalWeight = weightFor1Enemy + weightFor2Enemies + weightFor3Enemies;
        if (totalWeight <= 0) return 2; // 安全弁

        int rolledValue = Random.Range(0, totalWeight);

        if (rolledValue < weightFor1Enemy)
        {
            return 1;
        }
        else if (rolledValue < weightFor1Enemy + weightFor2Enemies)
        {
            return 2;
        }
        else
        {
            return 3;
        }
    }

    /// <summary>
    /// エネミープールの中から、それぞれのWeight(重み)に基づいて1体を抽選する
    /// </summary>
    private EnemyData GetRandomEnemyFromPool(List<EnemySpawnWeight> pool)
    {
        int totalWeight = 0;
        foreach (var enemySpawn in pool)
        {
            totalWeight += Mathf.Max(1, enemySpawn.weight); // 0以下の値は1として扱う安全策
        }

        int rolledValue = Random.Range(0, totalWeight);
        int currentWeightSum = 0;

        for (int i = 0; i < pool.Count; i++)
        {
            currentWeightSum += Mathf.Max(1, pool[i].weight);
            if (rolledValue < currentWeightSum)
            {
                return pool[i].enemyData;
            }
        }

        return pool[0].enemyData;
    }

    private void SpawnEnemies()
    {
        if (enemiesToSpawn.Count == 0) return;

        int totalCount = enemiesToSpawn.Count;

        for (int i = 0; i < totalCount; i++)
        {
            EnemyData currentEnemyData = enemiesToSpawn[i];
            if (currentEnemyData == null) continue;

            GameObject newEnemy = Instantiate(enemyPrefab, enemySpawnArea);
            EnemyManager manager = newEnemy.GetComponent<EnemyManager>();

            if (manager != null)
            {
                manager.enemyData = currentEnemyData;
                manager.SetupEnemy();
            }

            newEnemy.transform.localPosition = GetPosition(totalCount, i);
        }
    }

    private Vector3 GetPosition(int totalCount, int index)
    {
        if (totalCount == 1) return Vector3.zero;

        if (totalCount == 2)
        {
            if (index == 0) return new Vector3(-horizontalSpacing, 0, 0);
            if (index == 1) return new Vector3(horizontalSpacing, 0, 0);
        }
        else if (totalCount == 3)
        {
            if (index == 0) return new Vector3(0, -verticalSpacing, 0);
            if (index == 1) return new Vector3(-horizontalSpacing, verticalSpacing, 0);
            if (index == 2) return new Vector3(horizontalSpacing, verticalSpacing, 0);
        }
        return Vector3.zero;
    }
}