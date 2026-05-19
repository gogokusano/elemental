using UnityEngine;

// [CreateAssetMenu] を追加してエディタ上から作成できるようにしています
[CreateAssetMenu(fileName = "NewRelic", menuName = "CardGame/RelicData")]
public class RelicData : ScriptableObject
{
    [Header("基本情報")]
    public string relicName;      // 奇物の名前
    [TextArea]
    public string description;    // 効果の説明文
    public Sprite relicIcon;      // 画面に表示するアイコン
    public Rarity rarity;         // レアリティ（既存のものをそのまま使用）

    // ==========================================
    // 効果を発動するタイミング（フック）
    // 派生先で上書き（override）して使います。
    // ==========================================

    /// <summary>
    /// 奇物を手に入れた瞬間に一度だけ実行される効果（最大HPアップなど）
    /// </summary>
    public virtual void OnAcquire() { }

    /// <summary>
    /// 戦闘が始まった瞬間に実行される効果（初期シールド付与、バフ付与など）
    /// </summary>
    public virtual void OnBattleStart() { }

    /// <summary>
    /// 自分のターンが始まった時に実行される効果
    /// </summary>
    public virtual void OnTurnStart() { }

    /// <summary>
    /// 自分のターンが終わった時に実行される効果（シールド付与など）
    /// </summary>
    public virtual void OnTurnEnd() { }

    /// <summary>
    /// 敵を倒した瞬間に実行される効果（コスト回復など）
    /// </summary>
    public virtual void OnEnemyKilled() { }

    /// <summary>
    /// 与えるダメージを計算する時に書き換える効果（すべてのダメージ+5など）
    /// </summary>
    public virtual float OnModifyModifyDamage(float baseDamage) { return baseDamage; }

    /// <summary>
    /// 受けるダメージを計算する時に書き換える効果（受けるダメージ軽減など）
    /// </summary>
    public virtual int OnModifyTakeDamage(int incomingDamage) { return incomingDamage; }
}