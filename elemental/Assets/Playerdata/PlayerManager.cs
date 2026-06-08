using UnityEngine;
using TMPro;
using UnityEngine.UI; 

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance; 

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

    [Header("エフェクト設定")]
    public DamageText damageTextPrefab; // プレイヤー用のダメージテキストプレハブ
    public Transform damageTextParent;    // ★追加：テキストを表示させたいUIオブジェクト（HPバーなど）

    void Awake()
    {
        Instance = this; 
    }

    void Start()
    {
        currentBlock = 0;

        if (PlayerDataManager.Instance != null)
        {
            if (hpSlider != null) hpSlider.maxValue = PlayerDataManager.Instance.maxHp;
            if (blockSlider != null) blockSlider.maxValue = PlayerDataManager.Instance.maxHp;
        }

        UpdateUI();
    }

    public void AddBlock(int amount)
    {
        if (PlayerDebuffManager.Instance != null)
        {
            int penalty = PlayerDebuffManager.Instance.GetWeakenModifier(); 
            amount -= penalty;
            if (amount < 0) amount = 0; 
        }

        currentBlock += amount;
        UpdateUI();
    }

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

        if (PlayerDebuffManager.Instance != null)
        {
            int penalty = PlayerDebuffManager.Instance.GetWeakenModifier();
            finalDamage -= penalty;
            if (finalDamage < 0) finalDamage = 0; 
        }
        
        return Mathf.RoundToInt(finalDamage); 
    }

    public void TakeDamage(int damage)
    {
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

        // ★修正：指定された親UIの子供として生成し、さらに最前面に持ってくる
        if (damage > 0 && damageTextPrefab != null)
        {
            // damageTextParentが指定されていればそこ、無ければ自分自身を親にする
            Transform parentTransform = damageTextParent != null ? damageTextParent : transform;
            DamageText textObj = Instantiate(damageTextPrefab, parentTransform);
            
            // 他のUI（背景など）の後ろに隠れないように、強制的に最前面に並び替える処理
            textObj.transform.SetAsLastSibling();
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            if (textRect != null)
            {
                // 親UIの中心から、少し上（Y座標を100ピクセル上）に表示
                textRect.anchoredPosition = new Vector2(0, 100f); 
            }
            
            textObj.Setup(damage);
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

    public void TakeDirectDamage(int damage)
    {
        if (PlayerDataManager.Instance != null && damage > 0)
        {
            // ★修正：毒などの直接ダメージ時にも、指定された親で最前面に表示
            if (damageTextPrefab != null)
            {
                Transform parentTransform = damageTextParent != null ? damageTextParent : transform;
                DamageText textObj = Instantiate(damageTextPrefab, parentTransform);
                
                textObj.transform.SetAsLastSibling();

                RectTransform textRect = textObj.GetComponent<RectTransform>();
                if (textRect != null)
                {
                    textRect.anchoredPosition = new Vector2(0, 100f);
                }
                textObj.Setup(damage);
            }

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
        if (hpText != null && PlayerDataManager.Instance != null) 
        {
            hpText.text = PlayerDataManager.Instance.currentHp + " / " + PlayerDataManager.Instance.maxHp;
        }

        if (hpSlider != null && PlayerDataManager.Instance != null)
        {
            hpSlider.value = PlayerDataManager.Instance.currentHp;
        }
        
        if (blockSlider != null && PlayerDataManager.Instance != null)
        {
            blockSlider.value = PlayerDataManager.Instance.currentHp + currentBlock; 
            blockSlider.gameObject.SetActive(currentBlock > 0);
        }

        if (blockText != null)
        {
            blockText.text = "Block: " + currentBlock;
            blockText.gameObject.SetActive(currentBlock > 0);
        }
    }
}