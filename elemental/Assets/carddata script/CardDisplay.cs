using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    // ★マナ消費などで自分のデータを参照するために記憶しておく
    public CardData cardData; 

    [Header("UI参照")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI descriptionText;
    public Image cardImage;     // イラスト用
    public Image frameImage;    // 枠（レアリティ）用

    public void Setup(CardData data)
    {
        cardData = data; // ★渡されたデータを記憶する！

        // 1. 基本テキストの反映
        nameText.text = data.cardName;
        costText.text = data.cost.ToString();
        descriptionText.text = data.description;

        // 2. イラストの反映
        if (cardImage != null && data.cardImage != null)
        {
            cardImage.sprite = data.cardImage;
        }

        // 3. レアリティによる枠の色の変更
        if (frameImage != null)
        {
            switch (data.rarity)
            {
                case Rarity.Common: frameImage.color = Color.white; break;
                case Rarity.Uncommon: frameImage.color = Color.cyan; break;
                case Rarity.Rare: frameImage.color = Color.yellow; break;
                case Rarity.Special: frameImage.color = Color.magenta; break;
            }
        }

        // 4. 属性（エレメント）による名前の色の変更
        switch (data.elementType)
        {
            case ElementType.Fire:
                nameText.color = Color.red;
                break;
            case ElementType.Water:
                nameText.color = Color.blue;
                break;
            case ElementType.Ice:
                nameText.color = Color.cyan;
                break;
            case ElementType.Thunder:
                nameText.color = Color.yellow;
                break;
            case ElementType.Rock:
                nameText.color = new Color(0.6f, 0.4f, 0.2f); // 茶色
                break;
            default:
                nameText.color = Color.white; // 属性なしは白
                break;
        }
    }
}