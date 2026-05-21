using UnityEngine;
using TMPro;

public class PlayerManager : MonoBehaviour
{
    [Header("プレイヤーステータス (※HPはPlayerDataManagerで管理)")]
    public int currentBlock;

    // ★カウンター状態を判定するフラグ
    public bool hasCounter = false; 

    [Header("UI設定")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI blockText;

    void Start()
    {
        // 以前あった currentHp = maxHp; は削除！（戦闘のたびに全回復してしまうため）
        currentBlock = 0;
        UpdateUI();
    }

    public void AddBlock(int amount)
    {
        currentBlock += amount;
        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        // 1. カウンター判定
        if (hasCounter) 
        {
            hasCounter = false;
            EnemyManager enemy = Object.FindFirstObjectByType<EnemyManager>();
            if (enemy != null) {
                Debug.Log("<color=orange>カウンター発動！ダメージを無効化して2.0倍にして返した！</color>");
                enemy.TakeDamage(damage * 2); 
            }
            return; // カウンター発動時はここで処理を終了
        }

        if (damage <= 0) return;

        // カメラシェイク演出
        GameObject container = GameObject.Find("ShakeContainer");
        if (container != null)
        {
            CameraShake shake = container.GetComponent<CameraShake>();
            if (shake != null) 
            {
                shake.StartCoroutine(shake.Shake(0.2f, 20.0f));
            }
        }

        // 2. まずブロックで受ける
        if (currentBlock > 0)
        {
            if (currentBlock >= damage) { currentBlock -= damage; damage = 0; }
            else { damage -= currentBlock; currentBlock = 0; }
        }

        // 3. 残ったダメージがあればHPを減らす（★PlayerDataManagerを直接操作！）
        if (damage > 0 && PlayerDataManager.Instance != null)
        {
            int newHp = PlayerDataManager.Instance.currentHp - damage;
            PlayerDataManager.Instance.SaveHp(newHp); // マイナスにならないようにSaveHp経由で保存
        }
        
        // 4. 死亡判定（★これもPlayerDataManagerのHPを見る）
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.currentHp <= 0) 
        {
            GameManager gm = Object.FindFirstObjectByType<GameManager>();
            if (gm != null) gm.LoseGame();
            
            // プレイヤー非表示
            gameObject.SetActive(false);
        }

        UpdateUI();
    }

    public void ResetBlock()
    {
        currentBlock = 0;
        UpdateUI();
    }

    void UpdateUI()
    {
        // ★UI表示もPlayerDataManagerから最新の情報を取ってくる
        if (hpText != null && PlayerDataManager.Instance != null) 
        {
            hpText.text = "Player HP: " + PlayerDataManager.Instance.currentHp + " / " + PlayerDataManager.Instance.maxHp;
        }

        if (blockText != null)
        {
            blockText.text = "Block: " + currentBlock;
            blockText.gameObject.SetActive(currentBlock > 0);
        }
    }
}