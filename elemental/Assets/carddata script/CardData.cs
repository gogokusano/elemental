using UnityEngine;

// Attack, Skill に加えて Heal, Special を追加
public enum CardType { Attack, Skill, Heal, Special }
public enum Rarity { Common, Uncommon, Rare, Epic, Legendary, Special } 
// 属性定義：ご提示いただいたリストを使用します
public enum ElementType { None, Normal, Fire, Water, Wood, Light, Dark, Ice, Thunder, Rock } 

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
    public int cardDraw; // ★ここに追加：カードをドローする枚数
    
    [Header("レアリティ・属性")]
    public Rarity rarity;
    public ElementType elementType; // ★EnemyManager側もこれに合わせます

    [Header("見た目")]
    [TextArea]
    public string description;
    public Sprite cardImage;

    [Header("特殊設定")]
    public bool isUnusable; 
}