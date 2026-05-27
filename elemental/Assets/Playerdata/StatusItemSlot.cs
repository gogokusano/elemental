using UnityEngine;
using UnityEngine.UI;

public class StatusItemSlot : MonoBehaviour
{
    [Header("UI要素の割り当て")]
    public Image iconImage;
    public Button slotButton;

    private CardData targetCard;
    private RelicData targetRelic;

    void Awake()
    {
        if (slotButton == null) slotButton = GetComponent<Button>();
        slotButton.onClick.AddListener(OnSlotClicked);
    }

    // カードとしてセットアップ
    public void SetupCard(CardData card)
    {
        targetCard = card;
        targetRelic = null;
        if (iconImage != null && card != null)
        {
            iconImage.sprite = card.cardImage;
        }
    }

    // 奇物としてセットアップ
    public void SetupRelic(RelicData relic)
    {
        targetRelic = relic;
        targetCard = null;
        if (iconImage != null && relic != null)
        {
            iconImage.sprite = relic.relicIcon;
        }
    }

    private void OnSlotClicked()
    {
        if (targetCard != null)
        {
            StatusPanelManager.Instance.ShowCardDetail(targetCard);
        }
        else if (targetRelic != null)
        {
            StatusPanelManager.Instance.ShowRelicDetail(targetRelic);
        }
    }
}