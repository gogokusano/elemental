using System.Collections.Generic; // Listを使うために必要
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

    private EnemyAction nextAction;     // 次の行動

    // AI履歴用の変数
    private EnemyAction lastAction1; // 1ターン前の行動
    private EnemyAction lastAction2; // 2ターン前の行動
    private List<EnemyAction> usedOneTimeActions = new List<EnemyAction>(); // 使用済みの行動

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
            
            // 行動履歴の初期化
            lastAction1 = null;
            lastAction2 = null;
            usedOneTimeActions.Clear();

            // ゲーム開始時に最初の行動を決定する
            DetermineNextAction();
            
            UpdateUI();
        }
    }

    // 次のターンの行動を決定し、UIを更新するメソッド
    public void DetermineNextAction()
    {
        if (enemyData == null || enemyData.actionList.Count == 0) return;

        List<EnemyAction> availableActions = new List<EnemyAction>();
        float hpPercentage = (float)currentHP / enemyData.maxHP;

        foreach (var action in enemyData.actionList)
        {
            // ① HP50%以下専用の技なのに、まだHPが50%より多い場合は候補から外す
            if (action.isPhase2Only && hpPercentage > 0.5f) continue;

            // ② 1回限りの技で、すでに使用済みの場合は候補から外す
            if (action.isOneTimeOnly && usedOneTimeActions.Contains(action)) continue;

            // ③ まったく同じ行動を3回連続で行うのを防ぐ
            if (lastAction1 == action && lastAction2 == action) continue;

            availableActions.Add(action);
        }

        // 候補がなくなってしまったら、リストの最初の行動を強制的に選ぶ
        if (availableActions.Count == 0) availableActions.Add(enemyData.actionList[0]);

        int randomIndex = Random.Range(0, availableActions.Count);
        nextAction = availableActions[randomIndex];

        UpdateIntentUI();
    }

    // ★修正：行動予測のUI表示と色を更新する
    private void UpdateIntentUI()
    {
        if (intentIconDisplay == null || intentText == null || nextAction == null) return;

        intentIconDisplay.gameObject.SetActive(true);
        intentText.gameObject.SetActive(true);

        switch (nextAction.actionType)
        {
            case EnemyActionType.Attack:
                intentIconDisplay.sprite = attackIntentIcon;
                intentIconDisplay.color = Color.red; // Attackアイコンは赤色
                intentText.text = nextAction.value.ToString(); 
                break;
            case EnemyActionType.Defend:
                intentIconDisplay.sprite = defendIntentIcon;
                intentIconDisplay.color = Color.blue; // ブロックアイコンは青色
                intentText.text = nextAction.value.ToString(); 
                break;
            case EnemyActionType.AddStatusCard:
                intentIconDisplay.sprite = statusIntentIcon;
                intentIconDisplay.color = Color.green; // 異常アイコンは緑色
                intentText.text = ""; // 状態異常などは数値を出さない
                break;
        }
    }

    // 敵の行動を実行する
    public void ExecuteAction()
    {
        if (nextAction == null) DetermineNextAction();

        // 凍結状態の判定
        if (isFrozen)
        {
            isFrozen = false; 
            if (Random.value <= 0.5f) {
                Debug.Log("<color=cyan>敵は凍結していて動けない！</color>");
                DetermineNextAction(); // 行動をスキップしても次の行動は決める
                return; 
            }
        }

        // 予告していた行動の実行
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

        // 行動履歴の更新
        if (nextAction.isOneTimeOnly) usedOneTimeActions.Add(nextAction);
        lastAction2 = lastAction1;
        lastAction1 = nextAction;

        // 次のターンの行動を決定する
        DetermineNextAction();
    }

    // プレイヤーからの攻撃（カード）を処理する
    public void ProcessAttack(CardData card)
    {
        PlayerManager player = Object.FindFirstObjectByType<PlayerManager>();
        float damageFloat = card.damage;

        if (player != null)
        {
            damageFloat = player.CalculateFinalDamage(Mathf.RoundToInt(damageFloat), card);
        }
        
        ElementType incomingElement = card.elementType; 

        // 物理弱体（氷×雷）の消費判定
        if (isPhysicalWeak && incomingElement == ElementType.Normal)
        {
            damageFloat *= 2.5f;
            isPhysicalWeak = false;
            Debug.Log("<color=yellow>物理弱体が発動！Normalダメージが2.5倍！</color>");
        }

        bool comboTriggered = false;

        // コンボ判定（None/Normal以外同士で判定）
        if (currentElement != ElementType.None && currentElement != ElementType.Normal &&
            incomingElement != ElementType.None && incomingElement != ElementType.Normal)
        {
            if (IsCombo(ElementType.Fire, ElementType.Water, currentElement, incomingElement) ||
                IsCombo(ElementType.Fire, ElementType.Ice, currentElement, incomingElement))
            {
                damageFloat *= 2.0f;
                comboTriggered = true;
                Debug.Log("<color=red>蒸発/溶解！ダメージ2.0倍！</color>");
            }
            else if (IsCombo(ElementType.Water, ElementType.Ice, currentElement, incomingElement))
            {
                isFrozen = true;
                comboTriggered = true;
                Debug.Log("<color=cyan>凍結！敵が次に50%の確率で行動不能！</color>");
            }
            else if (IsCombo(ElementType.Ice, ElementType.Thunder, currentElement, incomingElement))
            {
                isPhysicalWeak = true;
                comboTriggered = true;
                Debug.Log("<color=yellow>物理弱体付与！次のNormal攻撃が2.5倍！</color>");
            }
            else if (currentElement == ElementType.Rock && incomingElement == ElementType.Rock)
            {
                if (player != null) player.hasCounter = true;
                comboTriggered = true;
                Debug.Log("<color=orange>カウンター準備完了！次の敵の攻撃を2.0倍で跳ね返す！</color>");
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
                Debug.Log("<color=magenta>爆発/感電発生！周囲にもダメージ！</color>");
            }
        }

        // 奇物による属性反応時の効果発動
        if (PlayerDataManager.Instance != null)
        {
            foreach (RelicData relic in PlayerDataManager.Instance.ownedRelics)
            {
                relic.OnElementReaction(); 
            }
        }

        // コンボ処理による属性のクリア/付与
        if (comboTriggered) {
            SetElement(ElementType.None);
        } else if (incomingElement != ElementType.None && incomingElement != ElementType.Normal) {
            SetElement(incomingElement);
        }

        // 最終的なダメージを与える
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
        currentHP -= damage;
        
        if (currentHP <= 0) 
        { 
            currentHP = 0; 

            // 敵死亡時の奇物効果発動
            if (PlayerDataManager.Instance != null)
            {
                foreach (RelicData relic in PlayerDataManager.Instance.ownedRelics)
                {
                    relic.OnEnemyKilled();
                }
            }

            GameManager gm = Object.FindFirstObjectByType<GameManager>();
            if (gm != null) gm.WinGame();
            gameObject.SetActive(false); 
        }
        UpdateUI();
    }

    public void ResetBlock() { currentBlock = 0; UpdateUI(); }

    private void UpdateUI()
    {
        if (hpText != null && enemyData != null)
            hpText.text = "Enemy HP: " + currentHP + " / " + enemyData.maxHP;
        if (blockText != null) {
            blockText.text = "Block: " + currentBlock;
            blockText.gameObject.SetActive(currentBlock > 0);
        }
    }
}