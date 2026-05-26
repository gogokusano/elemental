using UnityEngine;

// メニューからこの「汎用奇物」を作成できるようにします
[CreateAssetMenu(fileName = "NewRelicCore", menuName = "CardGame/Relics/RelicCore (汎用奇物)")]
public class RelicCore : RelicData
{
    [Header("【取得時効果】(1回だけ)")]
    public int maxHpBonus = 0;       // 最大HPの増加量
    public int initialGoldBonus = 0; // 獲得時の追加ゴールド

    [Header("【戦闘開始時効果】")]
    public int startBlock = 0;       // 戦闘開始時に得るブロック

    [Header("【毎ターン効果】")]
    public int turnStartHeal = 0;    // 自分のターン開始時のHP回復量
    public int turnEndBlock = 0;     // 自分のターン終了時に得るブロック

    [Header("【パッシブ効果】(常時)")]
    public float flatDamageBonus = 0f; // 与えるダメージの固定値アップ (+2など)
    public float damageMultiplier = 1f; // 与えるダメージの倍率 (1ならそのまま、1.5なら1.5倍)
    public int damageReduction = 0;    // 受けるダメージの固定値カット

    // =====================================================
    // 以下、設定された数値が 0 (または 1) でなければ効果を発動する処理
    // =====================================================

    public override void OnAcquire()
    {
        if (PlayerDataManager.Instance != null)
        {
            if (maxHpBonus != 0)
            {
                PlayerDataManager.Instance.maxHp += maxHpBonus;
                PlayerDataManager.Instance.currentHp += maxHpBonus;
                Debug.Log($"{relicName} の効果: 最大HPが {maxHpBonus} 増えた！");
            }

            if (initialGoldBonus != 0)
            {
                PlayerDataManager.Instance.gold += initialGoldBonus;
                Debug.Log($"{relicName} の効果: ゴールドを {initialGoldBonus} 獲得！");
            }
        }
    }

    public override void OnBattleStart()
    {
        if (startBlock > 0)
        {
            PlayerManager pm = Object.FindFirstObjectByType<PlayerManager>();
            if (pm != null)
            {
                pm.AddBlock(startBlock);
                Debug.Log($"{relicName} の効果: 戦闘開始時にブロック {startBlock} を獲得！");
            }
        }
    }

    public override void OnTurnStart()
    {
        if (turnStartHeal > 0 && PlayerDataManager.Instance != null)
        {
            int newHp = PlayerDataManager.Instance.currentHp + turnStartHeal;
            PlayerDataManager.Instance.SaveHp(newHp);
            Debug.Log($"{relicName} の効果: ターン開始時にHPを {turnStartHeal} 回復！");
        }
    }

    public override void OnTurnEnd()
    {
        if (turnEndBlock > 0)
        {
            PlayerManager pm = Object.FindFirstObjectByType<PlayerManager>();
            if (pm != null)
            {
                pm.AddBlock(turnEndBlock);
                Debug.Log($"{relicName} の効果: ターン終了時にブロック {turnEndBlock} を獲得！");
            }
        }
    }

    public override float OnModifyModifyDamage(float baseDamage)
    {
        // まず固定値を足して、そのあとに倍率を掛ける計算式
        float finalDamage = (baseDamage + flatDamageBonus) * damageMultiplier;
        
        if (flatDamageBonus != 0 || damageMultiplier != 1f)
        {
            Debug.Log($"{relicName} の効果: ダメージが {baseDamage} から {finalDamage} に変化！");
        }
        
        return finalDamage;
    }

    public override int OnModifyTakeDamage(int incomingDamage)
    {
        if (damageReduction > 0)
        {
            int reduced = Mathf.Max(0, incomingDamage - damageReduction);
            Debug.Log($"{relicName} の効果: 受けるダメージを {damageReduction} 軽減！");
            return reduced;
        }
        return incomingDamage;
    }
}