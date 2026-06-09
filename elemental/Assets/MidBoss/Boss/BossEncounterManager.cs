using UnityEngine;

public class BossEncounterManager : MonoBehaviour
{
    [Header("ボスのプレハブ (EnemyObject)")]
    public GameObject bossPrefab;

    [Header("ボスを配置するエリア（EnemySpawnGroup）")]
    public Transform enemySpawnArea;

    [Header("ボスのデータ（1体固定）")]
    public EnemyData bossData;

    void Start()
    {
        GenerateBoss();
    }

    public void GenerateBoss()
    {
        if (enemySpawnArea != null)
        {
            foreach (Transform child in enemySpawnArea) Destroy(child.gameObject);
        }

        if (bossData == null || bossPrefab == null)
        {
            Debug.LogError("ボスのデータまたはプレハブが設定されていません！");
            return;
        }

        GameObject newBoss = Instantiate(bossPrefab, enemySpawnArea);
        EnemyManager manager = newBoss.GetComponent<EnemyManager>();

        if (manager != null)
        {
            manager.enemyData = bossData;
            manager.isBoss = true; // ★ボスフラグをON！
            manager.barrierCount = 9; // ★初期バリアを9層に設定
            
            // ボスは15ターン目に必殺技準備（16ターン目に発動）
            manager.hasUltimate = true;
            manager.ultimateTriggerTurn = 15;
            manager.ultimateDamage = 999;

            manager.SetupEnemy();
        }

        newBoss.transform.localPosition = Vector3.zero;
        newBoss.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f); // ラスボスの威圧感
        Debug.Log($"<color=red>【ボス出現】{bossData.name} が立ちはだかった！</color>");
    }
}