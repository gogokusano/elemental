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
        currentBlock = 0;
        UpdateUI();
    }

    public void AddBlock(int amount)
    {
        currentBlock += amount;
        UpdateUI();
    }

    // ==========================================
    // ★追加：敵にダメージを与える時、奇物の効果（筋力など）を乗せるための関数
    // ==========================================
public int CalculateFinalDamage(int baseDamage, CardData card)
    {
        float finalDamage = baseDamage;

        if (PlayerDataManager.Instance != null)
        {
            foreach (RelicData relic in PlayerDataManager.Instance.ownedRelics)
            {
                // 引数に card を渡すように変更
                finalDamage = relic.OnModifyModifyDamage(finalDamage, card);
            }
        }
        
        return Mathf.RoundToInt(finalDamage); 
    }

    public void TakeDamage(int damage)
    {
        // ==========================================
        // ★追加：1. 一番最初に、持っている奇物の効果で受けるダメージを軽減する
        // ==========================================
        if (PlayerDataManager.Instance != null)
        {
            foreach (RelicData relic in PlayerDataManager.Instance.ownedRelics)
            {
                damage = relic.OnModifyTakeDamage(damage);
            }
        }

        if (damage <= 0) return;

        // 2. カウンター判定
        if (hasCounter) 
        {
            hasCounter = false;
            EnemyManager enemy = Object.FindFirstObjectByType<EnemyManager>();
            if (enemy != null) {
                Debug.Log("<color=orange>カウンター発動！ダメージを無効化して2.0倍にして返した！</color>");
                enemy.TakeDamage(damage * 2); 
            }
            return; 
        }

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

        // 3. まずブロックで受ける
        if (currentBlock > 0)
        {
            if (currentBlock >= damage) { currentBlock -= damage; damage = 0; }
            else { damage -= currentBlock; currentBlock = 0; }
        }

        // 4. 残ったダメージがあればHPを減らす
        if (damage > 0 && PlayerDataManager.Instance != null)
        {
            int newHp = PlayerDataManager.Instance.currentHp - damage;
            PlayerDataManager.Instance.SaveHp(newHp); 
        }
        
        // 5. 死亡判定
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.currentHp <= 0) 
        {
            GameManager gm = Object.FindFirstObjectByType<GameManager>();
            if (gm != null) gm.LoseGame();
            
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