using UnityEngine;

// Attack, Skill に加えて Heal, Special を追加
public enum CardType { Attack, Skill, Heal, Special }
public enum Rarity { Common, Uncommon, Rare, Epic, Legendary, Special }

// 属性定義
public enum ElementType
{
    None,
    Normal,
    Fire,
    Water,
    Wood,
    Light,
    Dark,
    Ice,
    Thunder,
    Rock
}

[CreateAssetMenu(fileName = "NewCard", menuName = "Card/CardData")]
public class CardData : ScriptableObject
{
    [Header("基本設定")]
    public string cardName;
    public int cost;
    public CardType cardType;
    public int damage;
    public int block;
    public int heal;
    public int cardDraw;

    [Header("レアリティ・属性")]
    public Rarity rarity;
    public ElementType elementType;

    [Header("カード画像用")]
    [TextArea]
    public string description;
    public Sprite cardImage;

    // ★追加
    [Header("詳細画面用")]
    public string cardNameS;

    [TextArea(3, 10)]
    public string cardTextS;

    [Header("特殊設定")]
    public bool isUnusable;
}