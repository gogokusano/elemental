using UnityEngine;
using TMPro;
using UnityEngine.UI; 

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance; // ★追加：他のスクリプトから呼び出しやすくするためのInstance

    [Header("プレイヤーステータス (※HPはPlayerDataManagerで管理)")]
    public int currentBlock;

    // ★カウンター状態を判定するフラグ
    public bool hasCounter = false; 

    [Header("UI設定")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI blockText;
    
    // ★追加：プレイヤー用のHPバーとブロックバー
    public Slider hpSlider; 
    public Slider blockSlider; // シールド（ブロック）用のスライダー

    void Awake()
    {
        Instance = this; // ★追加
    }

    void Start()
    {
        currentBlock = 0;

        // ★追加：戦闘開始時に、スライダーの「最大値」をプレイヤーの最大HPに設定する
        if (PlayerDataManager.Instance != null)
        {
            if (hpSlider != null) hpSlider.maxValue = PlayerDataManager.Instance.maxHp;
            if (blockSlider != null) blockSlider.maxValue = PlayerDataManager.Instance.maxHp;
        }

        UpdateUI();
    }

    public void AddBlock(int amount)
    {
        // ==========================================
        // ★デバフ処理：弱体化がかかっていたらブロック獲得量を減らす
        // ==========================================
        if (PlayerDebuffManager.Instance != null)
        {
            int penalty = PlayerDebuffManager.Instance.GetWeakenModifier(); // 弱体化の数値を引っ張ってくる
            amount -= penalty;
            if (amount < 0) amount = 0; // マイナスにはならないようにする
        }

        currentBlock += amount;
        UpdateUI();
    }

    // ==========================================
    // ★敵にダメージを与える時、奇物の効果（筋力など）を乗せるための関数
    // ==========================================
    public int CalculateFinalDamage(int baseDamage, CardData card)
    {
        float finalDamage = baseDamage;

        if (PlayerDataManager.Instance != null)
        {
            foreach (RelicData relic in PlayerDataManager.Instance.ownedRelics)
            {
                finalDamage = relic.OnModifyModifyDamage(finalDamage, card);
            }
        }

        // ==========================================
        // ★デバフ処理：弱体化がかかっていたら与えるダメージを減らす
        // ==========================================
        if (PlayerDebuffManager.Instance != null)
        {
            int penalty = PlayerDebuffManager.Instance.GetWeakenModifier();
            finalDamage -= penalty;
            if (finalDamage < 0) finalDamage = 0; // マイナスにはならないようにする
        }
        
        return Mathf.RoundToInt(finalDamage); 
    }

    public void TakeDamage(int damage)
    {
        // ==========================================
        // ★1. 一番最初に、持っている奇物の効果で受けるダメージを軽減する
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
        CheckDeath();

        UpdateUI();
    }

    // ==========================================
    // ★追加：毒や一部イベント用！ブロックを無視して直接HPを削る関数
    // ==========================================
    public void TakeDirectDamage(int damage)
    {
        if (PlayerDataManager.Instance != null && damage > 0)
        {
            int newHp = PlayerDataManager.Instance.currentHp - damage;
            PlayerDataManager.Instance.SaveHp(newHp); 
            
            Debug.Log($"<color=purple>直接ダメージ（毒など）を {damage} 受けた！</color>");
            CheckDeath();
            UpdateUI();
        }
    }

    private void CheckDeath()
    {
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.currentHp <= 0) 
        {
            GameManager gm = Object.FindFirstObjectByType<GameManager>();
            if (gm != null) gm.LoseGame();
            
            gameObject.SetActive(false);
        }
    }

    public void ResetBlock()
    {
        currentBlock = 0;
        UpdateUI();
    }

    void UpdateUI()
    {
        // 1. テキストの更新
        if (hpText != null && PlayerDataManager.Instance != null) 
        {
            hpText.text = PlayerDataManager.Instance.currentHp + " / " + PlayerDataManager.Instance.maxHp;
        }

        // 2. HPスライダーの値を更新
        if (hpSlider != null && PlayerDataManager.Instance != null)
        {
            hpSlider.value = PlayerDataManager.Instance.currentHp;
        }
        
        // 3. ブロックスライダーの値を更新（敵と同じで HP + ブロック の長さにする）
        if (blockSlider != null && PlayerDataManager.Instance != null)
        {
            blockSlider.value = PlayerDataManager.Instance.currentHp + currentBlock; 
            blockSlider.gameObject.SetActive(currentBlock > 0);
        }

        // 4. ブロックテキストの更新
        if (blockText != null)
        {
            blockText.text = "Block: " + currentBlock;
            blockText.gameObject.SetActive(currentBlock > 0);
        }
    }
}