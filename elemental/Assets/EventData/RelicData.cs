using UnityEngine;

[CreateAssetMenu(fileName = "NewRelic", menuName = "CardGame/RelicData")]
public class RelicData : ScriptableObject
{
    public string relicName;      // 奇物の名前
    [TextArea]
    public string description;    // 効果の説明文
    public Sprite relicIcon;      // 画面に表示するアイコン
    
    // 必要であれば「レアリティ」や「売却価格」なども追加できます
    public Rarity rarity;
}