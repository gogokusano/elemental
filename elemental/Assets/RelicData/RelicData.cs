using UnityEngine;

public enum RelicType { None, Attack, Defense, Special }
public enum RelicCategory { Normal, Subtle, Negative }

[CreateAssetMenu(fileName = "NewRelic", menuName = "CardGame/RelicData")]
public class RelicData : ScriptableObject
{
    [HideInInspector]
    public string relicID; // ★自動的にアセット名（kibutu10など）が格納される固有ID

    [Header("基本情報")]
    public string relicName;      // 奇物の名前
    [TextArea]
    public string description;    // 効果の説明文
    public Sprite relicIcon;      // 画面に表示するアイコン
    public Rarity rarity;         // レアリティ（★Rare、★★Epic、★★★legendary）

    [Header("ゲーム上の設定")]
    public RelicType relicType;
    public RelicCategory relicCategory; // 有利・微妙・不利の区別用

    // Unityエディタ上でアセット名が変わるか、作成された時に自動でIDを同期させる
    protected virtual void OnValidate()
    {
        if (string.IsNullOrEmpty(relicID) || relicID != name)
        {
            relicID = name; // アセットのファイル名（例: kibutu10）をそのままIDにする
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }

    // ==========================================
    // 効果を発動するタイミング（フック）
    // ==========================================

    /// <summary>
    /// 奇物を手に入れた瞬間に一度だけ実行される効果
    /// </summary>
    public virtual void OnAcquire() { }

    /// <summary>
    /// 戦闘が始まった瞬間に実行される効果
    /// </summary>
    public virtual void OnBattleStart() { }

    /// <summary>
    /// 自分のターンが始まった時に実行される効果
    /// </summary>
    public virtual void OnTurnStart() { }

    /// <summary>
    /// 自分のターンが終わった時に実行される効果
    /// </summary>
    public virtual void OnTurnEnd() { }

    /// <summary>
    /// 敵を倒した瞬間に実行される効果
    /// </summary>
    public virtual void OnEnemyKilled() { }

    /// <summary>
    /// 与えるダメージを計算する時に書き換える効果
    /// </summary>
    public virtual float OnModifyModifyDamage(float baseDamage, CardData card) { return baseDamage; }

    /// <summary>
    /// 受けるダメージを計算する時に書き換える効果
    /// </summary>
    public virtual int OnModifyTakeDamage(int incomingDamage) { return incomingDamage; }

    /// <summary>
    /// 属性反応が発生した瞬間に呼ばれるフック
    /// </summary>
    public virtual void OnElementReaction() { }

    /// <summary>
    /// ターンのドロー枚数を書き換えるフック
    /// </summary>
    public virtual int OnModifyDrawAmount(int baseAmount) { return baseAmount; }

    /// <summary>
    /// ゴールドを獲得する時に獲得量を書き換えるフック
    /// </summary>
    public virtual int OnModifyGainGold(int amount) { return amount; }
}