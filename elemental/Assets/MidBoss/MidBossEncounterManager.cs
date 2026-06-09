using System.Collections.Generic;
using UnityEngine;

public class MidBossEncounterManager : MonoBehaviour
{
    [Header("敵の基本プレハブ (EnemyObject)")]
    public GameObject enemyPrefab;

    [Header("中ボスを配置するエリア（EnemySpawnGroup）")]
    public Transform enemySpawnArea;

    [Header("中ボスのデータプール（ここに3種類入れる）")]
    public List<EnemyData> midBossPool = new List<EnemyData>();

    private EnemyData chosenBoss;

    void Start()
    {
        GenerateMidBoss();
    }

    public void GenerateMidBoss()
    {
        // 念のため古い敵のお掃除
        if (enemySpawnArea != null)
        {
            foreach (Transform child in enemySpawnArea)
            {
                Destroy(child.gameObject);
            }
        }

        // 3体の中からランダムで1体を抽選！
        if (midBossPool != null && midBossPool.Count > 0)
        {
            int randomIndex = Random.Range(0, midBossPool.Count);
            chosenBoss = midBossPool[randomIndex];
            Debug.Log($"<color=red>【中ボス出現】{chosenBoss.name} が抽選されました！</color>");
        }
        else
        {
            Debug.LogError("中ボスのデータが設定されていません！インスペクターを確認してください。");
            return;
        }

        SpawnBoss();
    }

    private void SpawnBoss()
    {
        if (chosenBoss == null) return;

        // 中ボスを生成
        GameObject newBoss = Instantiate(enemyPrefab, enemySpawnArea);
        EnemyManager manager = newBoss.GetComponent<EnemyManager>();

        if (manager != null)
        {
            // ボスのデータを渡してセットアップ
            manager.enemyData = chosenBoss;
            manager.isMidBoss = true; // ★追加：生成された敵に「中ボス」フラグを付与！
            manager.SetupEnemy();
        }

        // 中ボスは1体で堂々と構えるので、ど真ん中（0, 0, 0）に配置
        newBoss.transform.localPosition = Vector3.zero;
        
        // 💡おまけ：威圧感を出すために少しだけ大きくする（不要なら消してOKです）
        newBoss.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
    }
}