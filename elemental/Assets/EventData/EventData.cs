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

    [Header("カードの削除上限回数")]
    public int maxCardRemoveCount = 1;

    [Header("ショップに並ぶレアリティ (空ならすべて出現)")]
    public List<Rarity> allowedRelicRarities = new List<Rarity>() { Rarity.Common, Rarity.Rare, Rarity.Epic };
    public List<Rarity> allowedCardRarities = new List<Rarity>() { Rarity.Common, Rarity.Rare, Rarity.Epic };

    [Header("ショップに並ぶ奇物のカテゴリー (空ならすべて出現)")]
    [Tooltip("Normal, Subtle などを指定。Negativeを外せば不利奇物は並ばない")]
    public List<RelicCategory> allowedRelicCategories = new List<RelicCategory>() { RelicCategory.Normal, RelicCategory.Subtle };

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
    [Tooltip("HP割合変動 (最大HPを基準。例: 0.3で+30%, -0.2で-20%)")]
    public float hpPercentChange;
    public int maxHpChange;
    [Tooltip("最大HP割合変動 (例: 0.1で+10%, -0.1で-10%)")]
    public float maxHpPercentChange;
    [Header("ゴールド変動 (通常時)")]
    [Tooltip("正の値で獲得、負の値で減少")]
    public int goldChange;

    [Header("奇物獲得 (固定指定)")]
    public RelicData rewardRelic; 

    // ==========================================
    // ★追加・修正：ランダム奇物獲得（枠1）
    // ==========================================
    [Header("ランダム奇物獲得 (枠1)")]
    public bool giveRandomRelic = false;
    public int minRelicCount = 1; 
    public int maxRelicCount = 1; 
    
    [Tooltip("許可するレアリティ（空ならすべて許可）")]
    public List<Rarity> allowedRarities = new List<Rarity>();

    [Tooltip("★許可するカテゴリー（Normal, Subtle, Negative）")]
    public List<RelicCategory> allowedCategories = new List<RelicCategory>() { RelicCategory.Normal };

    [Header("絞り込み：タイプ")]
    public bool filterByType = false;
    public RelicType targetRelicType;

    [Header("確率アップグレード")]
    public bool canUpgradeRarity = false;
    [Range(0f, 1f)]
    public float upgradeChance = 0.5f; 
    public Rarity upgradedRarity; 

    // ==========================================
    // ★新規追加：ランダム奇物獲得（枠2・デメリット用など）
    // ==========================================
    [Header("追加のランダム奇物獲得 (枠2)")]
    public bool giveSecondaryRandomRelic = false;
    public int minSecondaryRelicCount = 1;
    public int maxSecondaryRelicCount = 1;
    
    [Tooltip("許可するレアリティ（空ならすべて許可）")]
    public List<Rarity> secondaryAllowedRarities = new List<Rarity>();

    [Tooltip("★許可するカテゴリー（Negativeなどを指定）")]
    public List<RelicCategory> secondaryAllowedCategories = new List<RelicCategory>() { RelicCategory.Negative };


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