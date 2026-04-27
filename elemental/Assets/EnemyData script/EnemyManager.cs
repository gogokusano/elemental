using UnityEngine;
using TMPro;
using UnityEngine.UI; // ★Imageを使うために必要です

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
    public Image elementIconDisplay; // ★追加：属性アイコンを表示する画像枠
    public ElementType currentElement = ElementType.None; // ★追加：現在の付与属性
    public bool isFrozen = false;       // ★追加：凍結フラグ
    public bool isPhysicalWeak = false; // ★追加：物理弱体フラグ

    void Start() { SetupEnemy(); }

    public void SetupEnemy()
    {
        if (enemyData != null)
        {
            currentHP = enemyData.maxHP;
            currentBlock = 0;
            if (enemyImage != null) enemyImage.sprite = enemyData.enemyImage;
            UpdateUI();
        }
    }

    public void ExecuteAction()
    {
        if (enemyData == null || enemyData.actionList.Count == 0) return;

        // ★追加：凍結状態の判定（50%の確率で行動をスキップして終了）
        if (isFrozen)
        {
            isFrozen = false; // 1回判定したら解除
            if (Random.value <= 0.5f) {
                Debug.Log("<color=cyan>敵は凍結していて動けない！</color>");
                return; 
            }
        }

        int randomIndex = Random.Range(0, enemyData.actionList.Count);
        EnemyAction chosenAction = enemyData.actionList[randomIndex];

        switch (chosenAction.actionType)
        {
            case EnemyActionType.Attack:
                PlayerManager player = Object.FindFirstObjectByType<PlayerManager>();
                if (player != null) player.TakeDamage(chosenAction.value);
                break;
            case EnemyActionType.Defend:
                currentBlock += chosenAction.value;
                break;
            case EnemyActionType.AddStatusCard:
                DeckManager dm = Object.FindFirstObjectByType<DeckManager>();
                if (dm != null && chosenAction.statusCard != null) 
                {
                    dm.AddCardToDrawPile(chosenAction.statusCard); 
                }
                break;
        }
        UpdateUI();
    }

    // ★新規追加：カードの属性を受け取ってダメージやコンボを計算する処理
    public void ProcessAttack(CardData card)
    {
        float damageFloat = card.damage;
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
            // 蒸発・溶解 (火×水, 火×氷)
            if (IsCombo(ElementType.Fire, ElementType.Water, currentElement, incomingElement) ||
                IsCombo(ElementType.Fire, ElementType.Ice, currentElement, incomingElement))
            {
                damageFloat *= 2.0f;
                comboTriggered = true;
                Debug.Log("<color=red>蒸発/溶解！ダメージ2.0倍！</color>");
            }
            // 凍結 (水×氷)
            else if (IsCombo(ElementType.Water, ElementType.Ice, currentElement, incomingElement))
            {
                isFrozen = true;
                comboTriggered = true;
                Debug.Log("<color=cyan>凍結！敵が次に50%の確率で行動不能！</color>");
            }
            // 物理弱体 (氷×雷)
            else if (IsCombo(ElementType.Ice, ElementType.Thunder, currentElement, incomingElement))
            {
                isPhysicalWeak = true;
                comboTriggered = true;
                Debug.Log("<color=yellow>物理弱体付与！次のNormal攻撃が2.5倍！</color>");
            }
            // カウンター準備 (岩×岩)
            else if (currentElement == ElementType.Rock && incomingElement == ElementType.Rock)
            {
                PlayerManager player = Object.FindFirstObjectByType<PlayerManager>();
                if (player != null) player.hasCounter = true;
                comboTriggered = true;
                Debug.Log("<color=orange>カウンター準備完了！次の敵の攻撃を2.0倍で跳ね返す！</color>");
            }
            // 爆発・感電 (火×雷, 水×雷)
            else if (IsCombo(ElementType.Fire, ElementType.Thunder, currentElement, incomingElement) ||
                     IsCombo(ElementType.Water, ElementType.Thunder, currentElement, incomingElement))
            {
                float bonus = damageFloat * 0.8f;
                damageFloat += bonus; // 攻撃対象への追加ダメージ (80%)
                
                // 他の敵全員に波及ダメージ (追加ダメージの50%)
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

        // コンボが発動したら属性を消す、発動しなかったら新しい属性を付与する
        if (comboTriggered) {
            SetElement(ElementType.None);
        } else if (incomingElement != ElementType.None && incomingElement != ElementType.Normal) {
            SetElement(incomingElement);
        }

        // 最終的なダメージを与える
        TakeDamage(Mathf.RoundToInt(damageFloat));
    }

    // ★新規追加：組み合わせの判定を簡単にするヘルパー関数
    private bool IsCombo(ElementType a, ElementType b, ElementType current, ElementType incoming) {
        return (current == a && incoming == b) || (current == b && incoming == a);
    }

    // ★新規追加：アイコンの表示と色（または画像）を切り替える処理
    private void SetElement(ElementType el)
    {
        currentElement = el;
        if (elementIconDisplay != null) {
            elementIconDisplay.gameObject.SetActive(el != ElementType.None && el != ElementType.Normal);
            
            // アイコン画像を用意するまでの仮の色分けです。後で画像を差し替える処理に変更できます。
            if (el == ElementType.Fire) elementIconDisplay.color = Color.red;
            else if (el == ElementType.Water) elementIconDisplay.color = Color.blue;
            else if (el == ElementType.Ice) elementIconDisplay.color = Color.cyan;
            else if (el == ElementType.Thunder) elementIconDisplay.color = Color.yellow;
            else if (el == ElementType.Rock) elementIconDisplay.color = new Color(0.5f, 0.3f, 0.1f);
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