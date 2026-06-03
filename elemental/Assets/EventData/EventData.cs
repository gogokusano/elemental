using UnityEngine;
using System.Collections.Generic;

public enum EventPenaltyType { HpLoss, MaxHpLoss, GoldLoss }

public enum TargetSelectionMethod { None, Random, Select }

public enum ShopActionType { None, BuyRelic, BuyCard, RemoveCard, Leave }

[System.Serializable]
public class RarityPrice
{
    public Rarity rarity;
    public int price;
}

[System.Serializable]
public class ShopConfig
{
    [Header("陳列数")]
    public int relicCount = 3;
    public int cardCount = 3;

    [Header("ショップに並ぶレアリティ (チェックしたものだけ出現)")]
    public List<Rarity> allowedRelicRarities = new List<Rarity>() { Rarity.Common, Rarity.Rare, Rarity.Epic };
    public List<Rarity> allowedCardRarities = new List<Rarity>() { Rarity.Common, Rarity.Rare, Rarity.Epic };

    [Header("奇物のレアリティ別価格")]
    public List<RarityPrice> relicPrices = new List<RarityPrice>();

    [Header("カードのレアリティ別価格")]
    public List<RarityPrice> cardPrices = new List<RarityPrice>();
}

[System.Serializable]
public class EventOption
{
    [TextArea(2,5)]
    public string buttonText;

    [Header("ショップ設定")]
    public ShopActionType shopAction = ShopActionType.None;
    public int removeCardPrice = 75;

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

    [Header("★ショップ専用設定 (ショップマスの時のみ有効)")]
    public ShopConfig shopConfig;
}