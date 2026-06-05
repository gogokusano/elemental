using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class EncounterManager : MonoBehaviour
{
    [Header("敵の基本プレハブ (EnemyObject)")]
    public GameObject enemyPrefab;

    [Header("敵を配置する画面上のエリア（Canvas内の空オブジェクト）")]
    public Transform enemySpawnArea;

    [Header("▼ 敵の配置間隔（インスペクターから数字を変更できます）")]
    public float horizontalSpacing = 180f;
    public float verticalSpacing = 100f;

    [Header("▼ 現在の進行度（自動取得）")]
    public int currentMap = 1;
    public int currentFloor = 1;

    [Header("① 1階層目確定の敵データ（スライムなど）")]
    public EnemyData firstFloorEnemy;

    [Header("② 第1マップ：道中の通常敵プール")]
    public List<EnemyData> map1NormalPool = new List<EnemyData>();

    [Header("③ 第2マップ：道中の通常敵プール")]
    public List<EnemyData> map2NormalPool = new List<EnemyData>();

    private EnemyData chosenEnemy;

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

        DetermineEnemyData();
        SpawnEnemies();
    }

    private void DetermineEnemyData()
    {
        if (currentMap == 1)
        {
            if (currentFloor <= 1)
            {
                chosenEnemy = firstFloorEnemy;
            }
            else
            {
                if (map1NormalPool != null && map1NormalPool.Count > 0)
                {
                    int randomIndex = Random.Range(0, map1NormalPool.Count);
                    chosenEnemy = map1NormalPool[randomIndex];
                }
            }
        }
        else if (currentMap == 2)
        {
            if (map2NormalPool != null && map2NormalPool.Count > 0)
            {
                int randomIndex = Random.Range(0, map2NormalPool.Count);
                chosenEnemy = map2NormalPool[randomIndex];
            }
        }

        if (chosenEnemy == null)
        {
            Debug.LogWarning("【安全装置】敵プールから取得できなかったため、1層目の敵を出現させます。");
            chosenEnemy = firstFloorEnemy;
        }
    }

    private void SpawnEnemies()
    {
        if (chosenEnemy == null) return;

        // ========================================================
        // ★修正：敵の出現数を 1 ～ 3 体の間で毎回ランダムに決定する
        // ========================================================
        int count = Random.Range(1, 4);

        Debug.Log($"【EncounterManager】{chosenEnemy.enemyName} が {count} 体抽選されました。");

        for (int i = 0; i < count; i++)
        {
            GameObject newEnemy = Instantiate(enemyPrefab, enemySpawnArea);
            EnemyManager manager = newEnemy.GetComponent<EnemyManager>();

            if (manager != null)
            {
                manager.enemyData = chosenEnemy;
                manager.SetupEnemy();
            }

            // 動的に決まった「count」に基づいて、綺麗に整列した座標を取得して適用
            newEnemy.transform.localPosition = GetPosition(count, i);
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
            // 3体時のきれいなV字配置（あるいは逆V字）
            if (index == 0) return new Vector3(0, -verticalSpacing, 0);
            if (index == 1) return new Vector3(-horizontalSpacing, verticalSpacing, 0);
            if (index == 2) return new Vector3(horizontalSpacing, verticalSpacing, 0);
        }
        return Vector3.zero;
    }
}