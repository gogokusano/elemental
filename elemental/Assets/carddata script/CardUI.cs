using UnityEngine;
using UnityEngine.UI; // ImageやButtonを使うために必要

public class CardUI : MonoBehaviour
{
    public Image cardImage;     // カードの絵を表示する枠
    public Button selectButton; // 押すためのボタン

    private CardData currentCardData; // このUIが持っているカードデータ

    // 報酬画面などでカードデータを渡されたときに呼ばれる関数
    public void SetupCard(CardData data)
    {
        currentCardData = data;

        // データに画像がセットされていれば、それを白い枠に貼り付ける
        if (data.cardImage != null)
        {
            cardImage.sprite = data.cardImage;
        }
    }
}