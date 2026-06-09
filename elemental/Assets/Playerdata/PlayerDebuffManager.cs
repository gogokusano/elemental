using System.Collections.Generic;
using UnityEngine;

// 現在かかっているデバフの情報を記憶するクラス
[System.Serializable]
public class ActiveDebuff
{
    public DebuffType type;
    public int value;
    public int duration;
}

public class PlayerDebuffManager : MonoBehaviour
{
    public static PlayerDebuffManager Instance;

    [Header("現在プレイヤーにかかっているデバフ")]
    public List<ActiveDebuff> activeDebuffs = new List<ActiveDebuff>();

    [Header("出血用：負傷カードのデータ")]
    public CardData woundCardData; // インスペクターで負傷カードをセットする

    void Awake()
    {
        Instance = this;
    }

    // 敵がデバフ攻撃をしてきた時に呼ばれる関数
    public void ApplyDebuff(DebuffType type, int duration, int value)
    {
        // 既に同じデバフがかかっている場合はターン数と数値を更新・合算する処理などをここに入れますが、
        // 今回はシンプルに新しく追加します。
        ActiveDebuff newDebuff = new ActiveDebuff
        {
            type = type,
            duration = duration,
            value = value
        };
        
        activeDebuffs.Add(newDebuff);
        Debug.Log($"<color=purple>プレイヤーに {type} が {duration} ターン付与された！</color>");
    }

    // ★ プレイヤーのターン終了時に呼び出す関数
    public void OnPlayerTurnEnd()
    {
        for (int i = activeDebuffs.Count - 1; i >= 0; i--)
        {
            ActiveDebuff debuff = activeDebuffs[i];

            // デバフごとのターン終了時効果を発動
            switch (debuff.type)
            {
                case DebuffType.Poison: // 毒：ブロック無視ダメージ
                    Debug.Log($"毒ダメージを {debuff.value} 受けた！");
                    if (PlayerManager.Instance != null) 
                    {
                        PlayerManager.Instance.TakeDirectDamage(debuff.value); // ★実際に直接ダメージを与える！
                    }
                    break;

                case DebuffType.Bleed: // 出血：捨て札に負傷カードを追加
                    Debug.Log("出血により、捨て札に負傷カードが追加された！");
                    if (DeckManager.Instance != null && woundCardData != null)
                    {
                        DeckManager.Instance.SendToDiscard(woundCardData); // ★実際に捨て札に送る！
                    }
                    break;
            }

            // ターンを1減らし、0になったら解除
            debuff.duration--;
            if (debuff.duration <= 0)
            {
                Debug.Log($"{debuff.type} が解除された！");
                activeDebuffs.RemoveAt(i);
            }
        }
    }

    // --- 以下は他のスクリプトから「今デバフかかってる？」と確認するための便利関数 ---

    // 弱体化による増減値を取得（与ダメージやブロック計算時に使う）
    public int GetWeakenModifier()
    {
        int totalPenalty = 0;
        foreach (var d in activeDebuffs)
        {
            if (d.type == DebuffType.Weaken) totalPenalty += d.value; // 例：-3など
        }
        return totalPenalty; // マイナスの値が返る想定だったが、今回はそのまま数値を返す（PlayerManager側で引いているため）
    }

    // 金縛りによるマナ減少値を取得（ターン開始時のマナ回復計算に使う）
    public int GetParalysisManaPenalty()
    {
        int penalty = 0;
        foreach (var d in activeDebuffs)
        {
            if (d.type == DebuffType.Paralysis) penalty -= d.value; // 例：-2など（マイナスにして返す）
        }
        return penalty;
    }

    // 混乱がかかっているかチェック（属性付与の判定に使う）
    public bool HasConfusion()
    {
        foreach (var d in activeDebuffs)
        {
            if (d.type == DebuffType.Confusion) return true;
        }
        return false;
    }
}