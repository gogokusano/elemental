using System.Collections.Generic; 
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EnemyManager : MonoBehaviour
{
    public EnemyData enemyData;
    public int currentHP;
    public int currentBlock;

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

    [Header("必殺技(Ultimate)ギミック設定")]
    public bool hasUltimate = false;            
    public int ultimateTriggerTurn = 5;        
    public int ultimateDamage = 999;           
    public Sprite chargeIntentIcon;            

    private int turnCount = 1;                 
    private bool isCharging = false;           
    private bool isReleasing = false;          

    [Header("エフェクト設定")]
    public DamageText damageTextPrefab; // ダメージテキストのプレハブ

    private EnemyAction nextAction;     
    private EnemyAction lastAction1; 
    private EnemyAction lastAction2; 
    private List<EnemyAction> usedOneTimeActions = new List<EnemyAction>(); 

    void Start() 
    { 
        SetupEnemy(); 
    }

    public void SetupEnemy()
    {
        if (enemyData != null)
        {
            currentHP = enemyData.maxHP;
            isPhysicalWeak = false;
            currentBlock = 0;
            if (enemyImage != null) enemyImage.sprite = enemyData.enemyImage;
            
            lastAction1 = null;
            lastAction2 = null;
            usedOneTimeActions.Clear();

            turnCount = 1;
            isCharging = false;
            isReleasing = false;

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
                    case DebuffType.Poison:
                        intentIconDisplay.color = new Color(0.2f, 0.8f, 0.2f); 
                        break;
                    case DebuffType.Weaken:
                        intentIconDisplay.color = new Color(0.4f, 0.4f, 0.4f); 
                        break;
                    case DebuffType.Bleed:
                        intentIconDisplay.color = new Color(1.0f, 0.27f, 0.0f); 
                        break;
                    case DebuffType.Paralysis:
                        intentIconDisplay.color = new Color(0.58f, 0.0f, 0.82f); 
                        break;
                    case DebuffType.Confusion:
                        intentIconDisplay.color = new Color(1.0f, 0.0f, 1.0f); 
                        break;
                    default:
                        intentIconDisplay.color = new Color(0.6f, 0.0f, 0.8f); 
                        break;
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

        if (nextAction == null) DetermineNextAction();

        if (isFrozen)
        {
            isFrozen = false; 
            if (Random.value <= 0.5f) {
                turnCount++;
                DetermineNextAction(); 
                return; 
            }
        }

        switch (nextAction.actionType)
        {
            case EnemyActionType.Attack:
                PlayerManager player = Object.FindFirstObjectByType<PlayerManager>();
                if (player != null) player.TakeDamage(nextAction.value);
                break;
            case EnemyActionType.Defend:
                currentBlock += nextAction.value;
                break;
            case EnemyActionType.AddStatusCard:
                DeckManager dm = Object.FindFirstObjectByType<DeckManager>();
                if (dm != null && nextAction.statusCard != null) 
                {
                    dm.AddCardToDrawPile(nextAction.statusCard); 
                }
                break;
            case EnemyActionType.ApplyDebuff:
                if (PlayerDebuffManager.Instance != null)
                {
                    PlayerDebuffManager.Instance.ApplyDebuff(nextAction.debuffType, nextAction.debuffDuration, nextAction.debuffValue);
                }
                break;
        }
        
        UpdateUI();

        if (nextAction.isOneTimeOnly) usedOneTimeActions.Add(nextAction);
        
        lastAction2 = lastAction1;
        lastAction1 = nextAction;

        turnCount++; 
        DetermineNextAction();
    }

    public void ProcessAttack(CardData card)
    {
        PlayerManager player = Object.FindFirstObjectByType<PlayerManager>();
        float damageFloat = card.damage;

        if (player != null)
        {
            damageFloat = player.CalculateFinalDamage(Mathf.RoundToInt(damageFloat), card);
        }
        
        ElementType incomingElement = card.elementType; 

        if (PlayerDebuffManager.Instance != null && PlayerDebuffManager.Instance.HasConfusion())
        {
            incomingElement = ElementType.Normal;
            Debug.Log("<color=magenta>混乱により、属性の付与が無効化された！</color>");
        }

        if (isPhysicalWeak && incomingElement == ElementType.Normal)
        {
            damageFloat *= 2.5f;
            isPhysicalWeak = false;
        }

        bool comboTriggered = false;

        if (currentElement != ElementType.None && currentElement != ElementType.Normal &&
            incomingElement != ElementType.None && incomingElement != ElementType.Normal)
        {
            if (IsCombo(ElementType.Fire, ElementType.Water, currentElement, incomingElement) ||
                IsCombo(ElementType.Fire, ElementType.Ice, currentElement, incomingElement))
            {
                damageFloat *= 2.0f;
                comboTriggered = true;
            }
            else if (IsCombo(ElementType.Water, ElementType.Ice, currentElement, incomingElement))
            {
                isFrozen = true;
                comboTriggered = true;
            }
            else if (IsCombo(ElementType.Ice, ElementType.Thunder, currentElement, incomingElement))
            {
                isPhysicalWeak = true;
                comboTriggered = true;
            }
            else if (currentElement == ElementType.Rock && incomingElement == ElementType.Rock)
            {
                if (player != null) player.hasCounter = true;
                comboTriggered = true;
            }
            else if (IsCombo(ElementType.Fire, ElementType.Thunder, currentElement, incomingElement) ||
                     IsCombo(ElementType.Water, ElementType.Thunder, currentElement, incomingElement))
            {
                float bonus = damageFloat * 0.8f;
                damageFloat += bonus; 
                
                EnemyManager[] allEnemies = Object.FindObjectsByType<EnemyManager>(FindObjectsSortMode.None);
                foreach(var e in allEnemies) {
                    if (e != this && e.gameObject.activeSelf) {
                        e.TakeDamage(Mathf.RoundToInt(bonus * 0.5f));
                    }
                }
                comboTriggered = true;
            }
        }

        if (PlayerDataManager.Instance != null)
        {
            foreach (RelicData relic in PlayerDataManager.Instance.ownedRelics)
            {
                relic.OnElementReaction(); 
            }
        }

        if (comboTriggered) {
            SetElement(ElementType.None);
        } else if (incomingElement != ElementType.None && incomingElement != ElementType.Normal) {
            SetElement(incomingElement);
        }

        TakeDamage(Mathf.RoundToInt(damageFloat));
    }

    private bool IsCombo(ElementType a, ElementType b, ElementType current, ElementType incoming) {
        return (current == a && incoming == b) || (current == b && incoming == a);
    }

    public void SetElement(ElementType el)
    {
        currentElement = el;
        if (elementIconDisplay != null) {
            elementIconDisplay.gameObject.SetActive(el != ElementType.None && el != ElementType.Normal);
            
            if (el == ElementType.Fire) 
            {
                elementIconDisplay.sprite = fireIcon;
                elementIconDisplay.color = Color.red;
            }
            else if (el == ElementType.Water) 
            {
                elementIconDisplay.sprite = waterIcon;
                elementIconDisplay.color = Color.blue;
            }
            else if (el == ElementType.Ice) 
            {
                elementIconDisplay.sprite = iceIcon;
                elementIconDisplay.color = Color.cyan;
            }
            else if (el == ElementType.Thunder) 
            {
                elementIconDisplay.sprite = thunderIcon;
                elementIconDisplay.color = Color.yellow;
            }
            else if (el == ElementType.Rock) 
            {
                elementIconDisplay.sprite = rockIcon;
                elementIconDisplay.color = new Color(0.5f, 0.3f, 0.1f); 
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0) return;

        if (currentBlock > 0)
        {
            if (currentBlock >= damage) { currentBlock -= damage; damage = 0; }
            else { damage -= currentBlock; currentBlock = 0; }
        }

        // ★修正：UI（Canvas）の階層構造に対応した生成処理
        if (damage > 0 && damageTextPrefab != null)
        {
            // 敵のUI（this.transform）の「子供」として生成する
            DamageText textObj = Instantiate(damageTextPrefab, transform);
            
            // 敵UIの中心から、少し上（Y座標を100ピクセル上）に初期位置をずらす
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            if (textRect != null)
            {
                textRect.anchoredPosition = new Vector2(0, 100f);
            }
            
            textObj.Setup(damage); // ダメージ数値を渡す
        }

        currentHP -= damage;
        
        if (currentHP <= 0) 
        { 
            currentHP = 0; 
            
            gameObject.SetActive(false); 

            if (PlayerDataManager.Instance != null)
            {
                foreach (RelicData relic in PlayerDataManager.Instance.ownedRelics)
                {
                    relic.OnEnemyKilled();
                }
            }

            EnemyManager[] remainingEnemies = Object.FindObjectsByType<EnemyManager>(FindObjectsSortMode.None);
            bool isAnyEnemyAlive = false;
            foreach (var e in remainingEnemies)
            {
                if (e != null && e.gameObject.activeSelf)
                {
                    isAnyEnemyAlive = true;
                    break; 
                }
            }

            if (!isAnyEnemyAlive)
            {
                GameManager gm = Object.FindFirstObjectByType<GameManager>();
                if (gm != null) gm.WinGame();

                RewardManager rm = Object.FindFirstObjectByType<RewardManager>();
                if (rm != null) rm.ShowReward();
            }
        }
        else
        {
            UpdateUI();
        }
    }

    public void ResetBlock() { currentBlock = 0; UpdateUI(); }

    private void UpdateUI()
    {
        if (hpText != null && enemyData != null)
            hpText.text = currentHP + " / " + enemyData.maxHP; 

        if (hpSlider != null) hpSlider.value = currentHP;
        
        if (blockSlider != null)
        {
            blockSlider.value = currentHP + currentBlock; 
            blockSlider.gameObject.SetActive(currentBlock > 0);
        }

        if (blockText != null) {
            blockText.text = "Block: " + currentBlock;
            blockText.gameObject.SetActive(currentBlock > 0);
        }
    }
}