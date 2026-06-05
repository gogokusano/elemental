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
            currentBlock = 0;
            if (enemyImage != null) enemyImage.sprite = enemyData.enemyImage;
            
            lastAction1 = null;
            lastAction2 = null;
            usedOneTimeActions.Clear();

            if (hpSlider != null) hpSlider.maxValue = enemyData.maxHP;
            if (blockSlider != null) blockSlider.maxValue = enemyData.maxHP; 

            DetermineNextAction();
            
            UpdateUI();
        }
    }

    public void DetermineNextAction()
    {
        if (enemyData == null || enemyData.actionList.Count == 0) return;

        List<EnemyAction> availableActions = new List<EnemyAction>();
        float hpPercentage = (float)currentHP / enemyData.maxHP;

        foreach (var action in enemyData.actionList)
        {
            if (action.isPhase2Only && hpPercentage > 0.5f) continue;
            if (action.isOneTimeOnly && usedOneTimeActions.Contains(action)) continue;
            if (lastAction1 == action && lastAction2 == action) continue;

            availableActions.Add(action);
        }

        if (availableActions.Count == 0) availableActions.Add(enemyData.actionList[0]);

        int randomIndex = Random.Range(0, availableActions.Count);
        nextAction = availableActions[randomIndex];

        UpdateIntentUI();
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
        }
    }

    public void ExecuteAction()
    {
        if (nextAction == null) DetermineNextAction();

        if (isFrozen)
        {
            isFrozen = false; 
            if (Random.value <= 0.5f) {
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
        }
        
        UpdateUI();

        if (nextAction.isOneTimeOnly) usedOneTimeActions.Add(nextAction);
        lastAction2 = lastAction1;
        lastAction1 = nextAction;

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

    // ========================================================
    // ★修正箇所：敵が全滅した時だけリザルトを表示するように変更！
    // ========================================================
    public void TakeDamage(int damage)
    {
        if (damage <= 0) return;

        if (currentBlock > 0)
        {
            if (currentBlock >= damage) { currentBlock -= damage; damage = 0; }
            else { damage -= currentBlock; currentBlock = 0; }
        }
        currentHP -= damage;
        
        if (currentHP <= 0) 
        { 
            currentHP = 0; 
            
            // ★1. 先にこの敵を非表示（死んだ扱い）にする
            gameObject.SetActive(false); 

            if (PlayerDataManager.Instance != null)
            {
                foreach (RelicData relic in PlayerDataManager.Instance.ownedRelics)
                {
                    relic.OnEnemyKilled();
                }
            }

            // ★2. 画面内にまだ生きている敵がいるか探す
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

            // ★3. もし生きている敵が1体もいなければ（全滅したら）勝利画面へ！
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
    // ========================================================

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