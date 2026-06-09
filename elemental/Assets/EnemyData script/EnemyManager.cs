using System.Collections.Generic; 
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EnemyManager : MonoBehaviour
{
    public EnemyData enemyData;
    public int currentHP;
    public int currentBlock;

    [Header("中ボス設定")]
    public bool isMidBoss = false; 

    // ==========================================
    // ★ボス専用設定
    // ==========================================
    [Header("ボス設定")]
    public bool isBoss = false; 
    public int barrierCount = 0; // 9層のバリア
    public TextMeshProUGUI barrierText; // バリア数を表示するテキスト
    public float bossDamageMultiplier = 1.0f; // 与ダメージ倍率（初期1.0倍）
    public CardData bossWoundCard; // 捨て札に混ぜる「負傷」カードのデータ
    private int bossActionStep = 0; // 行動パターンのカウント
    // ==========================================

    [Header("UI設定")]
    public TextMeshProUGUI hpText;
    public Image enemyImage;
    public TextMeshProUGUI blockText;
    public Slider hpSlider; 
    public Slider blockSlider; 

    [Header("属性・状態異常システム")]
    public Image elementIconDisplay; 
    public ElementType currentElement = ElementType.None; 
    public bool isFrozen = false;       
    public bool isPhysicalWeak = false; 

    [Header("属性アイコン画像")]
    public Sprite fireIcon;
    public Sprite waterIcon;
    public Sprite iceIcon;
    public Sprite thunderIcon;
    public Sprite rockIcon;

    [Header("行動予測(Intent)設定")]
    public Image intentIconDisplay;     
    public TextMeshProUGUI intentText;  
    public Sprite attackIntentIcon;     
    public Sprite defendIntentIcon;     
    public Sprite statusIntentIcon;     
    public Sprite buffIntentIcon; // バフ用のアイコン

    [Header("必殺技(Ultimate)ギミック設定")]
    public bool hasUltimate = false;            
    public int ultimateTriggerTurn = 15;        
    public int ultimateDamage = 999;           
    public Sprite chargeIntentIcon;            

    private int turnCount = 1;                 
    private bool isCharging = false;           
    private bool isReleasing = false;          

    [Header("エフェクト設定")]
    public DamageText damageTextPrefab; 

    private EnemyAction nextAction;     
    private EnemyAction lastAction1; 
    private EnemyAction lastAction2; 
    private List<EnemyAction> usedOneTimeActions = new List<EnemyAction>(); 

    void Start() { SetupEnemy(); }

    public void SetupEnemy()
    {
        if (enemyData != null)
        {
            currentHP = enemyData.maxHP;
            currentBlock = 0;
            if (enemyImage != null) enemyImage.sprite = enemyData.enemyImage;
            
            lastAction1 = null;
            lastAction2 = null;
            usedOneTimeActions.Clear();

            turnCount = 1;
            isCharging = false;
            isReleasing = false;
            bossActionStep = 0;

            if (hpSlider != null) hpSlider.maxValue = enemyData.maxHP;
            if (blockSlider != null) blockSlider.maxValue = enemyData.maxHP; 

            DetermineNextAction();
            UpdateUI();
        }
    }

    public void DetermineNextAction()
    {
        if (hasUltimate)
        {
            if (turnCount == ultimateTriggerTurn)
            {
                isCharging = true;
                isReleasing = false;
                nextAction = null; 
                UpdateUltimateIntentUI(true);
                return;
            }
            else if (isCharging)
            {
                isCharging = false;
                isReleasing = true;
                nextAction = null; 
                UpdateUltimateIntentUI(false);
                return;
            }
        }

        isCharging = false;
        isReleasing = false;

        // ★ボスの場合：固定ローテーションで行動を決定
        if (isBoss)
        {
            UpdateBossIntentUI();
            return;
        }

        // --- 通常敵・中ボスの行動決定 ---
        if (enemyData == null || enemyData.actionList.Count == 0) return;

        List<EnemyAction> availableActions = new List<EnemyAction>();
        float hpPercentage = (float)currentHP / enemyData.maxHP;

        foreach (var action in enemyData.actionList)
        {
            if (action.isPhase2Only && hpPercentage > 0.5f) continue;
            if (action.isOneTimeOnly && usedOneTimeActions.Contains(action)) continue;
            if (lastAction1 == action) continue;
            availableActions.Add(action);
        }

        if (availableActions.Count == 0) availableActions.Add(enemyData.actionList[0]);

        int randomIndex = Random.Range(0, availableActions.Count);
        nextAction = availableActions[randomIndex];
        UpdateIntentUI();
    }

    private void UpdateBossIntentUI()
    {
        if (intentIconDisplay == null || intentText == null) return;
        intentIconDisplay.gameObject.SetActive(true);
        intentText.gameObject.SetActive(true);

        int currentStep = bossActionStep % 4;

        switch (currentStep)
        {
            case 0: // 行動1: 8ダメージ
                intentIconDisplay.sprite = attackIntentIcon;
                intentIconDisplay.color = Color.red;
                intentText.text = Mathf.RoundToInt(8 * bossDamageMultiplier).ToString();
                break;
            case 1: // 行動2: 7ダメージ × 2
                intentIconDisplay.sprite = attackIntentIcon;
                intentIconDisplay.color = Color.red;
                intentText.text = Mathf.RoundToInt(7 * bossDamageMultiplier) + "x2";
                break;
            case 2: // 行動3: 25ダメージ ＋ 負傷3枚
                intentIconDisplay.sprite = attackIntentIcon; 
                intentIconDisplay.color = new Color(0.8f, 0, 0.8f); 
                intentText.text = Mathf.RoundToInt(25 * bossDamageMultiplier).ToString();
                break;
            case 3: // 行動4: 与ダメージ1.5倍バフ
                intentIconDisplay.sprite = buffIntentIcon != null ? buffIntentIcon : statusIntentIcon;
                intentIconDisplay.color = Color.yellow;
                intentText.text = "強化";
                break;
        }
    }

    private void UpdateUltimateIntentUI(bool charging)
    {
        if (intentIconDisplay == null || intentText == null) return;
        intentIconDisplay.gameObject.SetActive(true);
        intentText.gameObject.SetActive(true);

        if (charging)
        {
            intentIconDisplay.sprite = chargeIntentIcon != null ? chargeIntentIcon : statusIntentIcon;
            intentIconDisplay.color = new Color(1.0f, 0.5f, 0.0f); 
            intentText.text = ""; 
        }
        else
        {
            intentIconDisplay.sprite = attackIntentIcon;
            intentIconDisplay.color = Color.red; 
            intentText.text = ultimateDamage.ToString(); 
        }
    }

    private void UpdateIntentUI()
    {
        if (intentIconDisplay == null || intentText == null || nextAction == null) return;
        intentIconDisplay.gameObject.SetActive(true);
        intentText.gameObject.SetActive(true);

        switch (nextAction.actionType)
        {
            case EnemyActionType.Attack:
                intentIconDisplay.sprite = attackIntentIcon;
                intentIconDisplay.color = Color.red; 
                intentText.text = nextAction.value.ToString(); 
                break;
            case EnemyActionType.Defend:
                intentIconDisplay.sprite = defendIntentIcon;
                intentIconDisplay.color = Color.blue; 
                intentText.text = nextAction.value.ToString(); 
                break;
            case EnemyActionType.AddStatusCard:
                intentIconDisplay.sprite = statusIntentIcon;
                intentIconDisplay.color = Color.green; 
                intentText.text = ""; 
                break;
            case EnemyActionType.ApplyDebuff:
                intentIconDisplay.sprite = statusIntentIcon; 
                intentText.text = ""; 
                switch (nextAction.debuffType)
                {
                    case DebuffType.Poison: intentIconDisplay.color = new Color(0.2f, 0.8f, 0.2f); break;
                    case DebuffType.Weaken: intentIconDisplay.color = new Color(0.4f, 0.4f, 0.4f); break;
                    case DebuffType.Bleed: intentIconDisplay.color = new Color(1.0f, 0.27f, 0.0f); break;
                    case DebuffType.Paralysis: intentIconDisplay.color = new Color(0.58f, 0.0f, 0.82f); break;
                    case DebuffType.Confusion: intentIconDisplay.color = new Color(1.0f, 0.0f, 1.0f); break;
                    default: intentIconDisplay.color = new Color(0.6f, 0.0f, 0.8f); break;
                }
                break;
        }
    }

    public void ExecuteAction()
    {
        if (hasUltimate && (isCharging || isReleasing))
        {
            if (isReleasing)
            {
                PlayerManager player = Object.FindFirstObjectByType<PlayerManager>();
                if (player != null) player.TakeDamage(ultimateDamage);
            }
            UpdateUI();
            turnCount++; 
            DetermineNextAction(); 
            return;
        }

        if (isFrozen)
        {
            isFrozen = false; 
            if (Random.value <= 0.5f) { turnCount++; DetermineNextAction(); return; }
        }

        PlayerManager p = Object.FindFirstObjectByType<PlayerManager>();

        // ★ボスの場合の固定行動実行
        if (isBoss)
        {
            int currentStep = bossActionStep % 4;

            // ★2連続攻撃(行動2)のときだけ、時間差コルーチンに処理を逃がして終了する
            if (currentStep == 1)
            {
                StartCoroutine(BossMultiAttackCoroutine(p));
                return;
            }

            switch (currentStep)
            {
                case 0:
                    if (p != null) p.TakeDamage(Mathf.RoundToInt(8 * bossDamageMultiplier));
                    break;
                // case 1 (連続攻撃) は下にコルーチンとして分離しました
                case 2:
                    if (p != null) p.TakeDamage(Mathf.RoundToInt(25 * bossDamageMultiplier));
                    DeckManager dm = Object.FindFirstObjectByType<DeckManager>();
                    if (dm != null && bossWoundCard != null)
                    {
                        // 山札に3枚追加
                        for (int i=0; i<3; i++) dm.AddCardToDrawPile(bossWoundCard);
                        Debug.Log("<color=red>ボスが山札に負傷カードを3枚追加した！</color>");
                    }
                    break;
                case 3:
                    bossDamageMultiplier *= 1.5f;
                    Debug.Log($"<color=yellow>ボスが強化された！現在の倍率: {bossDamageMultiplier}倍</color>");
                    break;
            }
            bossActionStep++;
            turnCount++;
            UpdateUI();
            DetermineNextAction();
            return;
        }

        // --- 通常敵・中ボスの行動実行 ---
        if (nextAction == null) DetermineNextAction();

        switch (nextAction.actionType)
        {
            case EnemyActionType.Attack:
                if (p != null) p.TakeDamage(nextAction.value);
                break;
            case EnemyActionType.Defend:
                currentBlock += nextAction.value;
                break;
            case EnemyActionType.AddStatusCard:
                DeckManager dm = Object.FindFirstObjectByType<DeckManager>();
                if (dm != null && nextAction.statusCard != null) { dm.AddCardToDrawPile(nextAction.statusCard); }
                break;
            case EnemyActionType.ApplyDebuff:
                if (PlayerDebuffManager.Instance != null) { PlayerDebuffManager.Instance.ApplyDebuff(nextAction.debuffType, nextAction.debuffDuration, nextAction.debuffValue); }
                break;
        }
        
        UpdateUI();
        if (nextAction.isOneTimeOnly) usedOneTimeActions.Add(nextAction);
        
        lastAction2 = lastAction1;
        lastAction1 = nextAction;
        turnCount++; 
        DetermineNextAction();
    }

    // ★新設：ボスの時間差2連続攻撃用コルーチン
    private System.Collections.IEnumerator BossMultiAttackCoroutine(PlayerManager player)
    {
        if (player != null)
        {
            int multiDmg = Mathf.RoundToInt(7 * bossDamageMultiplier);

            // 1回目の攻撃
            player.TakeDamage(multiDmg);

            // 0.25秒待つ（トントンッの絶妙な間隔。お好みで数値を調整してください）
            yield return new WaitForSeconds(0.25f);

            // 2回目の攻撃
            player.TakeDamage(multiDmg);
        }

        // ダメージを与え終わったら、本来行うはずだったターン終了処理を実行する
        bossActionStep++;
        turnCount++;
        UpdateUI();
        DetermineNextAction();
    }

    public void ProcessAttack(CardData card)
    {
        PlayerManager player = Object.FindFirstObjectByType<PlayerManager>();
        float damageFloat = card.damage;

        if (player != null) damageFloat = player.CalculateFinalDamage(Mathf.RoundToInt(damageFloat), card);
        
        ElementType incomingElement = card.elementType; 

        if (PlayerDebuffManager.Instance != null && PlayerDebuffManager.Instance.HasConfusion())
        {
            incomingElement = ElementType.Normal;
        }

        if (isPhysicalWeak && incomingElement == ElementType.Normal) { damageFloat *= 2.5f; isPhysicalWeak = false; }

        bool comboTriggered = false;
        Color textCustomColor = Color.white; 

        if (incomingElement == ElementType.Fire) textCustomColor = new Color(1f, 0.25f, 0.25f); 
        else if (incomingElement == ElementType.Water) textCustomColor = new Color(0.25f, 0.6f, 1f); 
        else if (incomingElement == ElementType.Ice) textCustomColor = new Color(0.4f, 0.9f, 1f); 
        else if (incomingElement == ElementType.Thunder) textCustomColor = new Color(1f, 0.9f, 0.2f); 
        else if (incomingElement == ElementType.Rock) textCustomColor = new Color(0.65f, 0.45f, 0.3f); 

        if (currentElement != ElementType.None && currentElement != ElementType.Normal &&
            incomingElement != ElementType.None && incomingElement != ElementType.Normal)
        {
            if (IsCombo(ElementType.Fire, ElementType.Water, currentElement, incomingElement) ||
                IsCombo(ElementType.Fire, ElementType.Ice, currentElement, incomingElement))
            {
                damageFloat *= 2.0f; comboTriggered = true; textCustomColor = new Color(1f, 0.5f, 0f); 
            }
            else if (IsCombo(ElementType.Water, ElementType.Ice, currentElement, incomingElement))
            {
                isFrozen = true; comboTriggered = true; textCustomColor = new Color(0.5f, 0.8f, 1f); 
            }
            else if (IsCombo(ElementType.Ice, ElementType.Thunder, currentElement, incomingElement))
            {
                isPhysicalWeak = true; comboTriggered = true; textCustomColor = new Color(0.7f, 0.4f, 1f); 
            }
            else if (currentElement == ElementType.Rock && incomingElement == ElementType.Rock)
            {
                if (player != null) player.hasCounter = true; comboTriggered = true; textCustomColor = new Color(0.8f, 0.6f, 0.4f);
            }
            else if (IsCombo(ElementType.Fire, ElementType.Thunder, currentElement, incomingElement) ||
                     IsCombo(ElementType.Water, ElementType.Thunder, currentElement, incomingElement))
            {
                float bonus = damageFloat * 0.8f; damageFloat += bonus; 
                EnemyManager[] allEnemies = Object.FindObjectsByType<EnemyManager>(FindObjectsSortMode.None);
                foreach(var e in allEnemies) {
                    if (e != this && e.gameObject.activeSelf) { e.TakeDamageWithColor(Mathf.RoundToInt(bonus * 0.5f), new Color(1f, 0.9f, 0.2f)); }
                }
                comboTriggered = true; textCustomColor = new Color(1f, 0.8f, 0f); 
            }
        }

        if (PlayerDataManager.Instance != null)
        {
            foreach (RelicData relic in PlayerDataManager.Instance.ownedRelics) { relic.OnElementReaction(); }
        }

        if (comboTriggered) { SetElement(ElementType.None); } 
        else if (incomingElement != ElementType.None && incomingElement != ElementType.Normal) { SetElement(incomingElement); }

        TakeDamageWithColor(Mathf.RoundToInt(damageFloat), textCustomColor);
    }

    private bool IsCombo(ElementType a, ElementType b, ElementType current, ElementType incoming) { return (current == a && incoming == b) || (current == b && incoming == a); }

    public void SetElement(ElementType el)
    {
        currentElement = el;
        if (elementIconDisplay != null) {
            elementIconDisplay.gameObject.SetActive(el != ElementType.None && el != ElementType.Normal);
            if (el == ElementType.Fire) { elementIconDisplay.sprite = fireIcon; elementIconDisplay.color = Color.red; }
            else if (el == ElementType.Water) { elementIconDisplay.sprite = waterIcon; elementIconDisplay.color = Color.blue; }
            else if (el == ElementType.Ice) { elementIconDisplay.sprite = iceIcon; elementIconDisplay.color = Color.cyan; }
            else if (el == ElementType.Thunder) { elementIconDisplay.sprite = thunderIcon; elementIconDisplay.color = Color.yellow; }
            else if (el == ElementType.Rock) { elementIconDisplay.sprite = rockIcon; elementIconDisplay.color = new Color(0.5f, 0.3f, 0.1f); }
        }
    }

    public void TakeDamage(int damage) { TakeDamageWithColor(damage, new Color(1f, 0.3f, 0.3f)); }

    public void TakeDamageWithColor(int damage, Color textColor)
    {
        if (damage <= 0) return;

        // ★ボスのバリア処理！ダメージを受ける直前に判定
        if (isBoss && barrierCount > 0)
        {
            barrierCount--; 
            damage = 1;     
            Debug.Log($"<color=cyan>ボスのバリアが発動！ ダメージが1にされた！ 残りバリア: {barrierCount}</color>");
            textColor = Color.cyan; 
        }

        if (currentBlock > 0)
        {
            if (currentBlock >= damage) { currentBlock -= damage; damage = 0; }
            else { damage -= currentBlock; currentBlock = 0; }
        }

        if (damage > 0 && damageTextPrefab != null)
        {
            DamageText textObj = Instantiate(damageTextPrefab, transform);
            textObj.transform.SetAsLastSibling();
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            if (textRect != null) { textRect.anchoredPosition = new Vector2(0, 100f); }
            textObj.Setup(damage, textColor); 
        }

        currentHP -= damage;
        
        if (currentHP <= 0) 
        { 
            currentHP = 0; 
            gameObject.SetActive(false); 

            if (PlayerDataManager.Instance != null)
            {
                foreach (RelicData relic in PlayerDataManager.Instance.ownedRelics) { relic.OnEnemyKilled(); }
            }

            // ==========================================
            // ★ボス・中ボス・通常敵で死んだ時の処理を完全に分ける
            // ==========================================
            EnemyManager[] allEnemies = Object.FindObjectsByType<EnemyManager>(FindObjectsSortMode.None);
            bool isAnyEnemyAlive = false;
            bool wasMidBossBattle = this.isMidBoss;
            bool wasBossBattle = this.isBoss; 

            foreach (var e in allEnemies)
            {
                if (e != null && e.isMidBoss) wasMidBossBattle = true; 
                if (e != null && e.isBoss) wasBossBattle = true; 
                if (e != null && e.gameObject.activeSelf) isAnyEnemyAlive = true;
            }

            if (!isAnyEnemyAlive)
            {
                GameManager gm = Object.FindFirstObjectByType<GameManager>();
                if (gm != null) gm.WinGame();

                if (wasBossBattle)
                {
                    Debug.Log("<color=yellow>ボス撃破！！ 後でここにゲーム全体のリザルト画面を繋げます！</color>");
                }
                else
                {
                    RewardManager rm = Object.FindFirstObjectByType<RewardManager>();
                    if (rm != null) rm.ShowReward(); 
                }
            }
        }
        else { UpdateUI(); }
    }

    public void ResetBlock() { currentBlock = 0; UpdateUI(); }

    private void UpdateUI()
    {
        if (hpText != null && enemyData != null) hpText.text = currentHP + " / " + enemyData.maxHP; 
        if (hpSlider != null) hpSlider.value = currentHP;
        if (blockSlider != null) { blockSlider.value = currentHP + currentBlock; blockSlider.gameObject.SetActive(currentBlock > 0); }
        if (blockText != null) { blockText.text = "Block: " + currentBlock; blockText.gameObject.SetActive(currentBlock > 0); }

        // ★ボスのバリアUIの更新
        if (isBoss && barrierText != null)
        {
            barrierText.gameObject.SetActive(barrierCount > 0);
            barrierText.text = $"バリア: {barrierCount}";
        }
    }
}