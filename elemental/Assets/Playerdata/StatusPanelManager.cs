using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusPanelManager : MonoBehaviour
{
    public static StatusPanelManager Instance { get; private set; }

    [Header("パネル本体のGameObject")]
    public GameObject mainPanel;          // ステータス画面全体の親
    public GameObject detailPopupPanel;   // 詳細画面の親

    [Header("プレイヤー情報UI")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI goldText;

    [Header("スクロールリスト設定")]
    public Transform cardContentArea;     // カードアイコンを並べるGridの親
    public Transform relicContentArea;    // 奇物アイコンを並べるGridの親
    
    // ★ここを2つに分けました！
    public GameObject cardSlotPrefab;     // 長方形のカード用プレハブ
    public GameObject relicSlotPrefab;    // 正方形の奇物用プレハブ

    [Header("詳細ポップアップ用UI")]
    public TextMeshProUGUI detailNameText;
    public TextMeshProUGUI detailDescriptionText;
    public Image detailImageView;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (mainPanel != null) mainPanel.SetActive(false);
            if (detailPopupPanel != null) detailPopupPanel.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void ResetCamera()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            canvas.worldCamera = Camera.main;
        }
    }
    public void TogglePanel()
    {
        if (mainPanel == null) return;

        bool isActive = !mainPanel.activeSelf;
        mainPanel.SetActive(isActive);

        if (isActive)
        {
            if (detailPopupPanel != null) detailPopupPanel.SetActive(false);
            RefreshUI();
        }
    }

    public void RefreshUI()
    {
        if (PlayerDataManager.Instance == null) return;

        if (hpText != null)
        {
            hpText.text = $"HP: {PlayerDataManager.Instance.currentHp} / {PlayerDataManager.Instance.maxHp}";
        }
        if (goldText != null)
        {
            goldText.text = $"GOLD: {PlayerDataManager.Instance.gold}";
        }

        // リストの子要素を削除
        foreach (Transform child in cardContentArea) Destroy(child.gameObject);
        foreach (Transform child in relicContentArea) Destroy(child.gameObject);

        // ★カードリストの生成（カード用プレハブを使用）
        if (cardSlotPrefab != null)
        {
            foreach (CardData card in PlayerDataManager.Instance.deckCards)
            {
                if (card == null) continue;
                GameObject slotObj = Instantiate(cardSlotPrefab, cardContentArea);
                StatusItemSlot slot = slotObj.GetComponent<StatusItemSlot>();
                if (slot != null) slot.SetupCard(card);
            }
        }

        // ★奇物リストの生成（奇物用プレハブを使用）
        if (relicSlotPrefab != null)
        {
            foreach (RelicData relic in PlayerDataManager.Instance.ownedRelics)
            {
                if (relic == null) continue;
                GameObject slotObj = Instantiate(relicSlotPrefab, relicContentArea);
                StatusItemSlot slot = slotObj.GetComponent<StatusItemSlot>();
                if (slot != null) slot.SetupRelic(relic);
            }
        }
    }

    public void ShowCardDetail(CardData card)
    {
        if (detailPopupPanel == null || card == null) return;

        detailNameText.text = string.IsNullOrEmpty(card.cardNameS) ? card.cardName : card.cardNameS;
        detailDescriptionText.text = string.IsNullOrEmpty(card.cardTextS) ? card.description : card.cardTextS;
        detailImageView.sprite = card.cardImage;
        
        // ★必要に応じて、詳細画面での画像アスペクト比も調整する（ImageのPreserve AspectをONにしておくと便利です）

        detailPopupPanel.SetActive(true);
    }

    public void ShowRelicDetail(RelicData relic)
    {
        if (detailPopupPanel == null || relic == null) return;

        detailNameText.text = relic.relicName;
        detailDescriptionText.text = relic.description;
        detailImageView.sprite = relic.relicIcon;

        detailPopupPanel.SetActive(true);
    }

    public void CloseDetailPopup()
    {
        if (detailPopupPanel != null)
        {
            detailPopupPanel.SetActive(false);
        }
    }
}