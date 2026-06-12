using UnityEngine;

[CreateAssetMenu(fileName = "NewRelicCore", menuName = "CardGame/Relics/RelicCore (汎用奇物)")]
public class RelicCore : RelicData
{
    [Header("【取得時効果】")]
    public int maxHpBonus = 0;        // ★不滅のハート（10を設定）
    public int initialGoldBonus = 0;

    [Header("【戦闘開始時効果】")]
    public int startBlock = 0;        // ★古びた地図（10を設定）
    public bool applyRandomElementOnStart = false; 
    public int preventDebuffCount = 0; // ★頑丈なロープ（1を設定）
    
    [Tooltip("戦闘開始時にすべての敵に特定の属性を付着するフラグ（★元素の魔導書用）")]
    public bool applyWaterElementOnStart = false; 

    [Header("【毎ターン効果】")]
    public int turnStartHeal = 0;
    public int turnStartBlock = 0;  
    public int turnEndBlock = 0;
    public int turnEndHeal = 0;     

    [Header("【パッシブ効果】(ダメージ・防御関連)")]
    public float flatDamageBonus = 0f;
    public float damageMultiplier = 1f;
    public int damageReduction = 0;
    public int flatDamageTakenIncrease = 0; 

    [Tooltip("火（炎）属性カードでの攻撃時に追加される固定ダメージ（★祈りのキャンドル用：3を設定）")]
    public int fireCardDamageBonus = 0;

    [Header("【特殊条件パッシブ】")]
    public float firstTurnDamageBonus = 0f; 
    public float cost3DamageBonus = 0f;    
    public int drawAmountBonus = 0;         
    public int elementReactionBlock = 0;    
    public bool killEnemyManaRecover = false; 
    public bool doubleDiscard = false;      

    [Header("【ゴールド関連パッシブ】")]
    public float goldGainMultiplier = 1.0f;
    public int goldThresholdForDamage = 0;
    public float damageBonusPerGoldRatio = 0f;

    [Header("【イベント/システム関連パッシブ】")]
    public float eventHpLossMultiplier = 1.0f; // ★癒しのウール（0.5を設定）
    public int hpHealOnDiscardCardEvent = 0;   // ★鍛冶屋のハンマー（1を設定）
    public int maxHandSizeBonus = 0;           // ★冒険者のベルト（1を設定）

    [Header("【戦闘終了/ボス関連】")]
    public float bossGoldMultiplier = 1.0f;     // ★黄金の鍵（2.0を設定）
    public int battleEndMaxHpBonus = 0;        // ★黄金の鍵（2を設定）
    
    [Tooltip("戦闘終了時に回復するHPの量（★黄金の鍵用：2を設定）")]
    public int battleEndHealAmount = 0;        // ★黄金の鍵（2を設定）

    [Header("【属性反応コスト回復】")]
    [Range(0, 100)] public int elementReactionManaRecoverChance = 0; // ★導きのランタン（20を設定）
    public int elementReactionManaRecoverAmount = 0;                 // ★導きのランタン（1を設定）

    // 戦闘中やゲームプレイ中の個別状態管理用
    [System.NonSerialized] private bool isFirstTurn = true;
    [System.NonSerialized] private int currentPreventDebuffCount = 0;

    public override void OnAcquire()
    {
        if (PlayerDataManager.Instance != null)
        {
            if (maxHpBonus != 0)
            {
                PlayerDataManager.Instance.maxHp += maxHpBonus;
                PlayerDataManager.Instance.currentHp += maxHpBonus;
                Debug.Log($"{relicName} の効果: 最大HPが {maxHpBonus} 上昇しました。");
            }
            if (initialGoldBonus != 0)
            {
                PlayerDataManager.Instance.gold += initialGoldBonus;
            }
        }
    }

    public override void OnBattleStart()
    {
        isFirstTurn = true; 

        // ★古びた地図の効果
        if (startBlock > 0)
        {
            PlayerManager pm = Object.FindFirstObjectByType<PlayerManager>();
            if (pm != null)
            {
                pm.AddBlock(startBlock);
                Debug.Log($"{relicName} の効果: 戦闘開始時にシールドを {startBlock} 獲得！");
            }
        }

        // ★頑丈なロープの効果
        if (preventDebuffCount > 0)
        {
            currentPreventDebuffCount = preventDebuffCount;
            Debug.Log($"{relicName} の効果: デバフ防御バリアを展開！");
        }

        // 📘 ★元素の魔導書の効果（すべての敵に水属性を付着）
        if (applyWaterElementOnStart)
        {
            GameObject awaiter = new GameObject("WaterElementTriggerHelper");
            var helper = awaiter.AddComponent<MushroomTriggerHelper>();
            helper.Initialize(this, true); 
        }
    }

    public bool TryPreventDebuff()
    {
        if (currentPreventDebuffCount > 0)
        {
            currentPreventDebuffCount--;
            Debug.Log($"{relicName} の効果: デバフを防ぎました。（残り: {currentPreventDebuffCount}）");
            return true;
        }
        return false;
    }

    public override void OnTurnStart()
    {
        if (PlayerDataManager.Instance != null)
        {
            if (turnStartHeal > 0)
            {
                PlayerDataManager.Instance.SaveHp(PlayerDataManager.Instance.currentHp + turnStartHeal);
            }
            if (turnStartBlock > 0)
            {
                PlayerManager pm = Object.FindFirstObjectByType<PlayerManager>();
                if (pm != null) pm.AddBlock(turnStartBlock);
            }
        }

        if (applyRandomElementOnStart && isFirstTurn)
        {
            GameObject awaiter = new GameObject("MushroomElementTriggerHelper");
            var helper = awaiter.AddComponent<MushroomTriggerHelper>();
            helper.Initialize(this, false);
        }
    }

    public override void OnTurnEnd()
    {
        PlayerManager pm = Object.FindFirstObjectByType<PlayerManager>();
        if (pm != null && turnEndBlock > 0) pm.AddBlock(turnEndBlock);

        if (turnEndHeal > 0 && PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.SaveHp(PlayerDataManager.Instance.currentHp + turnEndHeal);
        }
        isFirstTurn = false; 
    }

    public override float OnModifyModifyDamage(float baseDamage, CardData card)
    {
        float finalDamage = baseDamage;
        finalDamage = (finalDamage + flatDamageBonus) * damageMultiplier;

        // 🕯️ ★祈りのキャンドルの効果
        if (card != null && fireCardDamageBonus > 0)
        {
            if ((card.elementType == ElementType.Fire || card.elementType == ElementType.Thunder) && card.cardType == CardType.Attack)
            {
                finalDamage += fireCardDamageBonus;
                Debug.Log($"{relicName} の効果: 火属性攻撃のダメージが +{fireCardDamageBonus} されました！");
            }
        }

        if (goldThresholdForDamage > 0 && damageBonusPerGoldRatio != 0f && PlayerDataManager.Instance != null)
        {
            int goldCount = PlayerDataManager.Instance.gold / goldThresholdForDamage;
            if (goldCount > 0) finalDamage *= (1f + (goldCount * damageBonusPerGoldRatio));
        }

        if (isFirstTurn && card != null && card.cardType == CardType.Attack)
        {
            finalDamage += firstTurnDamageBonus;
        }

        if (card != null && card.cost == 3)
        {
            finalDamage += cost3DamageBonus;
        }

        return finalDamage;
    }

    public override int OnModifyTakeDamage(int incomingDamage)
    {
        int finalDamage = incomingDamage;
        if (damageReduction > 0) finalDamage = Mathf.Max(0, finalDamage - damageReduction);
        if (flatDamageTakenIncrease > 0) finalDamage += flatDamageTakenIncrease;
        return finalDamage;
    }

    public override void OnElementReaction()
    {
        if (elementReactionBlock > 0)
        {
            PlayerManager pm = Object.FindFirstObjectByType<PlayerManager>();
            if (pm != null) pm.AddBlock(elementReactionBlock);
        }

        // ★導きのランタンの効果
        if (elementReactionManaRecoverChance > 0 && elementReactionManaRecoverAmount > 0)
        {
            if (Random.Range(0, 100) < elementReactionManaRecoverChance)
            {
                ManaManager mm = Object.FindFirstObjectByType<ManaManager>();
                if (mm != null)
                {
                    mm.currentMana = Mathf.Min(mm.maxMana, mm.currentMana + elementReactionManaRecoverAmount);
                    mm.UpdateManaUI();
                    Debug.Log($"{relicName} の効果: コストが {elementReactionManaRecoverAmount} 回復！");
                }
            }
        }
    }

    public override void OnEnemyKilled()
    {
        if (killEnemyManaRecover)
        {
            ManaManager mm = Object.FindFirstObjectByType<ManaManager>();
            if (mm != null)
            {
                mm.currentMana = Mathf.Min(mm.maxMana, mm.currentMana + 1);
                mm.UpdateManaUI();
            }
        }
    }

    public override int OnModifyDrawAmount(int baseAmount)
    {
        return baseAmount + drawAmountBonus;
    }

    public int OnModifyGainGoldAdvanced(int amount, bool isBossBattle)
    {
        if (isBossBattle && bossGoldMultiplier != 1.0f) amount = Mathf.RoundToInt(amount * bossGoldMultiplier);
        if (goldGainMultiplier != 1.0f) amount = Mathf.RoundToInt(amount * goldGainMultiplier);
        return amount;
    }

    public override int OnModifyGainGold(int amount)
    {
        return OnModifyGainGoldAdvanced(amount, false);
    }

    /// <summary>
    /// 戦闘終了時の処理（★黄金の鍵の効果用）
    /// </summary>
    public void OnBattleEnd()
    {
        if (PlayerDataManager.Instance != null)
        {
            // 1. 最大HPの上昇
            if (battleEndMaxHpBonus > 0)
            {
                PlayerDataManager.Instance.maxHp += battleEndMaxHpBonus;
                PlayerDataManager.Instance.currentHp += battleEndMaxHpBonus; // 最大HP増加分、現在値も引き上げ
                Debug.Log($"{relicName} の効果: 最大HPが +{battleEndMaxHpBonus} されました！");
            }

            // 2. 体力の回復処理（追加部分）
            if (battleEndHealAmount > 0)
            {
                // 最大HPを超えないように安全に回復して保存
                int targetHp = Mathf.Min(PlayerDataManager.Instance.maxHp, PlayerDataManager.Instance.currentHp + battleEndHealAmount);
                PlayerDataManager.Instance.SaveHp(targetHp);
                Debug.Log($"{relicName} の効果: 戦闘終了により体力が {battleEndHealAmount} 回復しました！（現在HP: {PlayerDataManager.Instance.currentHp}/{PlayerDataManager.Instance.maxHp}）");
            }
        }
    }

    public int ModifyEventHpLoss(int originalLoss)
    {
        if (eventHpLossMultiplier != 1.0f) return Mathf.RoundToInt(originalLoss * eventHpLossMultiplier);
        return originalLoss;
    }

    public void OnDiscardCardInEvent()
    {
        if (hpHealOnDiscardCardEvent > 0 && PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.SaveHp(PlayerDataManager.Instance.currentHp + hpHealOnDiscardCardEvent);
        }
    }
}

public class MushroomTriggerHelper : MonoBehaviour
{
    private RelicCore relic;
    private bool forceWater;

    public void Initialize(RelicCore relicCore, bool waterMode)
    {
        relic = relicCore;
        forceWater = waterMode;
        StartCoroutine(WaitAndApplyElement());
    }

    private System.Collections.IEnumerator WaitAndApplyElement()
    {
        EnemyManager[] enemies = null;
        int frameCount = 0;
        while (frameCount < 30)
        {
            enemies = Object.FindObjectsByType<EnemyManager>(FindObjectsSortMode.None);
            if (enemies != null && enemies.Length > 0) break;
            frameCount++;
            yield return null;
        }
        if (enemies != null && enemies.Length > 0)
        {
            System.Array elements = System.Enum.GetValues(typeof(ElementType));
            foreach (EnemyManager enemy in enemies)
            {
                if (enemy == null) continue;
                
                if (forceWater)
                {
                    enemy.SetElement(ElementType.Water);
                    Debug.Log($"{relic.relicName}: {enemy.name} に「水」属性を付着しました。");
                }
                else
                {
                    int randomIndex = UnityEngine.Random.Range(2, elements.Length);
                    ElementType randomElement = (ElementType)elements.GetValue(randomIndex);
                    enemy.SetElement(randomElement);
                }
            }
        }
        Destroy(gameObject);
    }
}