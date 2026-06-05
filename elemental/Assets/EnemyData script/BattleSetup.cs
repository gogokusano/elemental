using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class BattleSetup : MonoBehaviour
{
    [Header("出現する敵のリスト（階層別）")]
    // ★修正1：最初からリストを初期化しておく（これでリスト自体がNullになるのを防ぐ）
    public List<EnemyData> easyEnemies = new List<EnemyData>();   
    public List<EnemyData> normalEnemies = new List<EnemyData>(); 
    public List<EnemyData> hardEnemies = new List<EnemyData>();   

    void Awake()
    {
        int currentFloor = 1; // デフォルトは1階層

        string nodeName = PlayerPrefs.GetString("CurrentChallengingNode", "");

        if (!string.IsNullOrEmpty(nodeName))
        {
            string[] parts = nodeName.Split('_');
            if (parts.Length >= 2)
            {
                string layerString = parts[1]; 
                string numberOnly = Regex.Replace(layerString, "[^0-9]", "");
                
                if (int.TryParse(numberOnly, out int floorNumber))
                {
                    currentFloor = floorNumber; 
                }
            }
        }

        Debug.Log($"<color=green>現在の階層は {currentFloor} 層目です！</color>");

        List<EnemyData> currentPool = easyEnemies;
        
        if (currentFloor >= 4) currentPool = normalEnemies; 
        if (currentFloor >= 7) currentPool = hardEnemies;

        // ★修正2：リストの中にデータが1つ以上あるかチェック
        if (currentPool != null && currentPool.Count > 0)
        {
            int randomIndex = Random.Range(0, currentPool.Count);
            EnemyData chosenEnemy = currentPool[randomIndex];

            // ★修正3：選ばれた枠が「None」になっていないかチェック
            if (chosenEnemy == null)
            {
                Debug.LogError("【エラー】選ばれた敵のデータが空っぽです！インスペクターのリストに「None」の枠がないか確認してください。");
                return;
            }

            EnemyManager enemyManager = Object.FindFirstObjectByType<EnemyManager>();
            if (enemyManager != null)
            {
                enemyManager.enemyData = chosenEnemy;
                enemyManager.currentHP = chosenEnemy.maxHP; 
                
                Debug.Log($"<color=cyan>階層 {currentFloor}: {chosenEnemy.enemyName} が出現した！</color>");
            }
        }
        else
        {
            Debug.LogWarning("敵のリストが空っぽです！インスペクターでEnemyDataをセットしてください。");
        }
    }
}