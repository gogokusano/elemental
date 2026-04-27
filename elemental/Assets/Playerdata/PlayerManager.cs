using UnityEngine;
using TMPro;

public class PlayerManager : MonoBehaviour
{
    [Header("プレイヤーステータス")]
    public int maxHp = 50;
    public int currentHp;
    public int currentBlock;

    // ★追加：カウンター状態を判定するフラグ
    public bool hasCounter = false; 

    [Header("UI設定")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI blockText;

    void Start()
    {
        currentHp = maxHp;
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
        // ★追加：ダメージを受ける前に、まずカウンターが発動するか判定
        if (hasCounter) 
        {
            hasCounter = false;
            EnemyManager enemy = Object.FindFirstObjectByType<EnemyManager>();
            if (enemy != null) {
                Debug.Log("<color=orange>カウンター発動！ダメージを無効化して2.0倍にして返した！</color>");
                enemy.TakeDamage(damage * 2); 
            }
            return; // カウンター発動時はここで処理を終了し、自分はダメージや画面揺れを受けない
        }

        if (damage <= 0) return;

        // 元々の実装：カメラシェイク演出
        GameObject container = GameObject.Find("ShakeContainer");
        if (container != null)
        {
            CameraShake shake = container.GetComponent<CameraShake>();
            if (shake != null) 
            {
                shake.StartCoroutine(shake.Shake(0.2f, 20.0f));
            }
        }

        // 1. まずブロックで受ける
        if (currentBlock > 0)
        {
            if (currentBlock >= damage) { currentBlock -= damage; damage = 0; }
            else { damage -= currentBlock; currentBlock = 0; }
        }

        // 2. 残ったダメージがあればHPを減らす
        if (damage > 0)
        {
            currentHp -= damage;
        }
        
        if (currentHp <= 0) 
        {
            currentHp = 0;
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
        if (hpText != null) hpText.text = "Player HP: " + currentHp + " / " + maxHp;
        if (blockText != null)
        {
            blockText.text = "Block: " + currentBlock;
            blockText.gameObject.SetActive(currentBlock > 0);
        }
    }
}