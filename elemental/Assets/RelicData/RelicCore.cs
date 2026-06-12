using UnityEngine;

[CreateAssetMenu(fileName = "NewRelicCore", menuName = "CardGame/Relics/RelicCore (汎用奇物)")]
public class RelicCore : RelicData
{
    [Header("【取得時効果】")]
    public int maxHpBonus = 0;
    public int initialGoldBonus = 0;

    [Header("【戦闘開始時効果】")]
    public int startBlock = 0;
    public bool applyRandomElementOnStart = false; // ★4.妖精の胞子キノコ用

    [Header("【毎ターン効果】")]
    public int turnStartHeal = 0;
    public int turnStartBlock = 0;  // ★3.エメンタール用
    public int turnEndBlock = 0;
    public int turnEndHeal = 0;     // ★1.生命の赤リンゴ用

    [Header("【パッシブ効果】(ダメージ関連)")]
    public float flatDamageBonus = 0f;
    public float damageMultiplier = 1f;
    public int damageReduction = 0;
    public int flatDamageTakenIncrease = 0; // ★5.諸刃の秘薬用（受けるダメージUP）

    [Header("【特殊条件パッシブ】")]
    public float firstTurnDamageBonus = 0f; // ★2.豪傑の麦酒用
    public float cost3DamageBonus = 0f;    // ★7.必中の古矢用
    public int drawAmountBonus = 0;         // ★10.知識の巻物用
    public int elementReactionBlock = 0;    // ★8.魔術師の古帽子用
    public bool killEnemyManaRecover = false; // ★9.完熟の青リンゴ用
    public bool doubleDiscard = false;      // ★6.幸運の古銭用（処理の予約用フラグ）
    
    [Header("【ゴールド関連パッシブ】")]
    [Tooltip("ゴールド獲得量の倍率 (1.0でそのまま、1.2で+20%、0.8で-20%)")]
    public float goldGainMultiplier = 1.0f;
    
    [Tooltip("所持ゴールド n個 につきダメージ増加（0にすると無効）")]
    public int goldThresholdForDamage = 0;
    
    [Tooltip("n個満たす毎のダメージ増減率 (0.1で+10%、-0.1で-10%)")]
    public float damageBonusPerGoldRatio = 0f;

    // 戦闘中の個別状態管理用（ScriptableObjectの一時変数）
    [System.NonSerialized] private bool isFirstTurn = true;

    public override void OnAcquire()
    {
        if (PlayerDataManager.Instance != null)
        {
            if (maxHpBonus != 0)
            {
                PlayerDataManager.Instance.maxHp += maxHpBonus;
                PlayerDataManager.Instance.currentHp += maxHpBonus;
            }
            if (initialGoldBonus != 0)
            {
                PlayerDataManager.Instance.gold += initialGoldBonus;
            }
        }
    }

    public override void OnBattleStart()
    {
        isFirstTurn = true; // 戦闘開始時に最初のターンフラグをリセット

        if (startBlock > 0)
        {
            PlayerManager pm = Object.FindFirstObjectByType<PlayerManager>();
            if (pm != null) pm.AddBlock(startBlock);
        }
    }

    public override void OnTurnStart()
    {
        if (PlayerDataManager.Instance != null)
        {
            if (turnStartHeal > 0)
            {
                PlayerDataManager.Instance.SaveHp(PlayerDataManager.Instance.currentHp + turnStartHeal);
            }
            // ★3.エメンタール効果
            if (turnStartBlock > 0)
            {
                PlayerManager pm = Object.FindFirstObjectByType<PlayerManager>();
                if (pm != null) pm.AddBlock(turnStartBlock);
            }
        }

        // 🍄 ★4.妖精の胞子キノコ効果
        if (applyRandomElementOnStart && isFirstTurn)
        {
            GameObject awaiter = new GameObject("MushroomElementTriggerHelper");
            var helper = awaiter.AddComponent<MushroomTriggerHelper>();
            helper.Initialize(this);
        }
    }

    public override void OnTurnEnd()
    {
        PlayerManager pm = Object.FindFirstObjectByType<PlayerManager>();
        if (pm != null && turnEndBlock > 0)
        {
            pm.AddBlock(turnEndBlock);
        }

        // ★1.生命の赤リンゴ効果
        if (turnEndHeal > 0 && PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.SaveHp(PlayerDataManager.Instance.currentHp + turnEndHeal);
            Debug.Log($"{relicName} の効果: ターン終了時にHPを {turnEndHeal} 回復！");
        }

        isFirstTurn = false; // ターン終了時に最初のターンではなくなる
    }

    public override float OnModifyModifyDamage(float baseDamage, CardData card)
    {
        float finalDamage = baseDamage;

        // 全カード共通の固定値と倍率
        finalDamage = (finalDamage + flatDamageBonus) * damageMultiplier;

        // 所持ゴールドn個につきダメージ増加
        if (goldThresholdForDamage > 0 && damageBonusPerGoldRatio != 0f && PlayerDataManager.Instance != null)
        {
            int goldCount = PlayerDataManager.Instance.gold / goldThresholdForDamage;
            if (goldCount > 0)
            {
                float bonusMultiplier = 1f + (goldCount * damageBonusPerGoldRatio);
                finalDamage *= bonusMultiplier;
                Debug.Log($"{relicName}の効果: 所持ゴールドによりダメージが {bonusMultiplier} 倍に！");
            }
        }

        // ★2.豪傑の麦酒効果 (最初のターンかつ攻撃カードのみ)
        if (isFirstTurn && card != null && card.cardType == CardType.Attack)
        {
            finalDamage += firstTurnDamageBonus;
            Debug.Log($"{relicName} の効果(初ターン攻撃): ダメージが +{firstTurnDamageBonus} された！");
        }

        // ★7.必中の古矢効果 (コスト3カードのみ)
        if (card != null && card.cost == 3)
        {
            finalDamage += cost3DamageBonus;
            Debug.Log($"{relicName} の効果(コスト3): ダメージが +{cost3DamageBonus} された！");
        }

        return finalDamage;
    }

    public override int OnModifyTakeDamage(int incomingDamage)
    {
        int finalDamage = incomingDamage;

        if (damageReduction > 0)
        {
            finalDamage = Mathf.Max(0, finalDamage - damageReduction);
        }

        // ★5.諸刃の秘薬効果 (受けるダメージ一律増加)
        if (flatDamageTakenIncrease > 0)
        {
            finalDamage += flatDamageTakenIncrease;
            Debug.Log($"{relicName} の効果(諸刃): 受けるダメージが +{flatDamageTakenIncrease} された！");
        }

        return finalDamage;
    }

    // ★8.魔術師の古帽子効果
    public override void OnElementReaction()
    {
        if (elementReactionBlock > 0)
        {
            PlayerManager pm = Object.FindFirstObjectByType<PlayerManager>();
            if (pm != null)
            {
                pm.AddBlock(elementReactionBlock);
                Debug.Log($"{relicName} の効果: 属性反応によりシールドを {elementReactionBlock} 獲得！");
            }
        }
    }

    // ★9.完熟の青リンゴ効果
    public override void OnEnemyKilled()
    {
        if (killEnemyManaRecover)
        {
            ManaManager mm = Object.FindFirstObjectByType<ManaManager>();
            if (mm != null)
            {
                mm.currentMana = Mathf.Min(mm.maxMana, mm.currentMana + 1);
                mm.UpdateManaUI();
                Debug.Log($"{relicName} の効果: 敵撃破によりマナが 1 回復した！");
            }
        }
    }

    // ★10.知識の巻物効果
    public override int OnModifyDrawAmount(int baseAmount)
    {
        return baseAmount + drawAmountBonus;
    }

    public override int OnModifyGainGold(int amount)
    {
        if (goldGainMultiplier != 1.0f)
        {
            amount = Mathf.RoundToInt(amount * goldGainMultiplier);
            Debug.Log($"{relicName}の効果: ゴールド獲得量が {amount}G に変化！");
        }
        return amount;
    }
}

// 🍏 敵の生成を安全に待機して、見つかり次第【すべての敵にそれぞれ】属性を付与するヘルパー
public class MushroomTriggerHelper : MonoBehaviour
{
    private RelicCore relic;

    public void Initialize(RelicCore relicCore)
    {
        relic = relicCore;
        StartCoroutine(WaitAndApplyElement());
    }

    private System.Collections.IEnumerator WaitAndApplyElement()
    {
        EnemyManager[] enemies = null;
        int frameCount = 0;

        // 最大30フレーム（約0.5秒）、敵が生成されるのを毎フレーム監視して待ちます
        while (frameCount < 30)
        {
            enemies = Object.FindObjectsByType<EnemyManager>(FindObjectsSortMode.None);
            if (enemies != null && enemies.Length > 0)
            {
                break;
            }
            frameCount++;
            yield return null;
        }

        // 敵が見つかったら【全員ループ】で個別にランダム属性を付与
        if (enemies != null && enemies.Length > 0)
        {
            System.Array elements = System.Enum.GetValues(typeof(ElementType));

            foreach (EnemyManager enemy in enemies)
            {
                if (enemy == null) continue;

                // 敵1体ごとにランダムな属性を決定（全員バラバラの属性になる可能性があります）
                int randomIndex = UnityEngine.Random.Range(2, elements.Length);
                ElementType randomElement = (ElementType)elements.GetValue(randomIndex);

                enemy.SetElement(randomElement);
                Debug.Log($"【大成功】{relic.relicName}: 敵の生成を待ってから {enemy.name} に {randomElement} 属性を付着しました！");
            }
        }
        else
        {
            Debug.LogError($"【エラー】{relic.relicName}: 30フレーム待機しましたが敵が見つかりませんでした。");
        }

        Destroy(gameObject); // 用が済んだら自動削除
    }
}