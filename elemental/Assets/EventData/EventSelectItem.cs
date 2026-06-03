using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EventSelectItem : MonoBehaviour
{
    [Header("UI設定")]
    public Image iconImage;

    private CardData targetCard;
    private RelicData targetRelic;
    private EventManager manager;

    public void SetupCard(CardData card, EventManager mgr)
    {
        targetCard = card;
        targetRelic = null;
        manager = mgr;
        
        if (iconImage != null && card.cardImage != null) iconImage.sprite = card.cardImage;
        
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void SetupRelic(RelicData relic, EventManager mgr)
    {
        targetRelic = relic;
        targetCard = null;
        manager = mgr;
        
        if (iconImage != null && relic.relicIcon != null) iconImage.sprite = relic.relicIcon;
        
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        if (targetCard != null) manager.OnCardSelected(targetCard);
        else if (targetRelic != null) manager.OnRelicSelected(targetRelic);
    }
}