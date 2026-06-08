using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EventSelectItem : MonoBehaviour
{
    [Header("UI設定")]
    public Image iconImage;
    public Image backgroundImage;
    public Image rarityStarImage;
    public TextMeshProUGUI priceText;

    private CardData targetCard;
    private RelicData targetRelic;
    private EventManager manager;

    // ★引数に int price = -1 を追加
    public void SetupCard(CardData card, EventManager mgr, int price = -1) 
    {
        targetCard = card;
        targetRelic = null;
        manager = mgr;
        
        if (iconImage != null && card.cardImage != null) iconImage.sprite = card.cardImage;
        
        // カードの場合は背景や星を消す
        if (backgroundImage != null) backgroundImage.gameObject.SetActive(false);
        if (rarityStarImage != null) rarityStarImage.gameObject.SetActive(false);

        // ★価格の表示設定
        if (priceText != null)
        {
            if (price >= 0)
            {
                priceText.text = price.ToString() + " G";
                priceText.gameObject.SetActive(true);
            }
            else
            {
                priceText.gameObject.SetActive(false); // ショップ以外では隠す
            }
        }
        
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    // ★引数に int price = -1 を追加
    public void SetupRelic(RelicData relic, EventManager mgr, int price = -1) 
    {
        targetRelic = relic;
        targetCard = null;
        manager = mgr;
        
        if (relic != null)
        {
            if (iconImage != null && relic.relicIcon != null) iconImage.sprite = relic.relicIcon;
            
            if (backgroundImage != null && StatusPanelManager.Instance != null)
            {
                backgroundImage.sprite = StatusPanelManager.Instance.GetRelicBackground(relic);
                backgroundImage.gameObject.SetActive(true);
            }

            if (rarityStarImage != null && StatusPanelManager.Instance != null)
            {
                Sprite starSprite = StatusPanelManager.Instance.GetRelicStarSprite(relic);
                if (starSprite != null)
                {
                    rarityStarImage.sprite = starSprite;
                    rarityStarImage.gameObject.SetActive(true);
                }
                else
                {
                    rarityStarImage.gameObject.SetActive(false);
                }
            }
        }

        // ★価格の表示設定
        if (priceText != null)
        {
            if (price >= 0)
            {
                priceText.text = price.ToString() + " G";
                priceText.gameObject.SetActive(true);
            }
            else
            {
                priceText.gameObject.SetActive(false);
            }
        }

        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        if (targetCard != null) manager.OnCardSelected(targetCard);
        else if (targetRelic != null) manager.OnRelicSelected(targetRelic);
    }
}