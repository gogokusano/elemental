using UnityEngine;
using System.Collections.Generic;

public enum EventPenaltyType { HpLoss, MaxHpLoss, GoldLoss }

public enum TargetSelectionMethod { None, Random, Select }

[System.Serializable]
public class EventOption
{
    [TextArea(2,5)]
    public string buttonText;

    [Header("ステータス変動 (通常時)")]
    public int hpChange;
    public int maxHpChange;

    [Header("奇物獲得 (固定指定)")]
    public RelicData rewardRelic; 

    // ==========================================
    // ★統合：ランダム奇物獲得の万能設定
    // ==========================================
    [Header("ランダム奇物獲得")]
    public bool giveRandomRelic = false;
    public int minRelicCount = 1; // ★最低獲得数
    public int maxRelicCount = 1; // ★最大獲得数
    
    [Tooltip("許可するレアリティ（不利奇物を弾く場合は Common, Rare 等を指定）")]
    public List<Rarity> allowedRarities = new List<Rarity>();

    [Header("絞り込み：タイプ")]
    public bool filterByType = false;
    public RelicType targetRelicType;

    [Header("確率アップグレード")]
    public bool canUpgradeRarity = false;
    [Range(0f, 1f)]
    public float upgradeChance = 0.5f; 
    public Rarity upgradedRarity; 

    [Header("奇物の喪失")]
    public int loseRelicCount = 0;
    public TargetSelectionMethod loseRelicMethod = TargetSelectionMethod.None;

    [Header("カードの獲得")]
    public int gainCardCount = 0;
    public TargetSelectionMethod gainCardMethod = TargetSelectionMethod.None;
    [Tooltip("特定条件のカードをプールから引っ張る設定")]
    public bool filterCardByElement = false;
    public ElementType targetCardElement;
    public bool filterCardByCost = false;
    public int targetCardCost;
    public bool filterCardByRarity = false;
    public Rarity targetCardRarity;

    [Header("カードの削除")]
    public int removeCardCount = 0;
    public TargetSelectionMethod removeCardMethod = TargetSelectionMethod.None;

    [Header("カードの変化")]
    public int transformCardCount = 0;
    public TargetSelectionMethod transformCardMethod = TargetSelectionMethod.None;

    [Header("カードのコピー")]
    public int duplicateCardCount = 0;
    public TargetSelectionMethod duplicateCardMethod = TargetSelectionMethod.None;

    [Header("確率（ギャンブル）設定")]
    public bool isGambleOption = false;
    [Range(0f, 1f)]
    public float gambleSuccessChance = 0.5f;
    public EventPenaltyType penaltyType;
    public int penaltyValue;
}

[CreateAssetMenu(fileName = "NewEvent", menuName = "CardGame/EventData")]
public class EventData : ScriptableObject
{
    public string eventName;
    [TextArea(3, 5)]
    public string eventText;
    public Sprite eventImage;
    public EventOption[] options; 
}