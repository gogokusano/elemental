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

    [Header("レアリティ別 背景画像設定")]
    public Sprite bgCommon;
    public Sprite bgUncommon;
    public Sprite bgRare;
    public Sprite bgEpic;
    public Sprite bgLegendary;
    public Sprite bgSpecial;
    public Sprite bgNegative;
    
    [Header("★レアリティ星画像設定")]
    public Sprite starRare;       // ★ (1つ星)
    public Sprite starEpic;       // ★★ (2つ星)
    public Sprite starLegendary;  // ★★★ (3つ星)
    public Sprite starNegative;   // ★不利奇物専用のアイコン（ドクロやヒビ割れた星など）
    [Header("詳細パネル内のUI (追加)")]
    public Image detailStarImage;

    [Header("詳細ポップアップ用UI")]
    public TextMeshProUGUI detailNameText;
    public TextMeshProUGUI detailDescriptionText;
    public Image detailImageView;
    public Image detailBackgroundImage;

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

    public Sprite GetRelicBackground(RelicData relic)
    {
        if (relic == null) return null;

        // ★カテゴリがNegative(不利奇物)なら、レアリティ設定を無視して専用背景を返す
        if (relic.relicCategory == RelicCategory.Negative)
        {
            return bgNegative;
        }

        // それ以外はレアリティに応じて返す
        switch (relic.rarity)
        {
            case Rarity.Common:    return bgCommon;
            case Rarity.Uncommon:  return bgUncommon;
            case Rarity.Rare:      return bgRare;
            case Rarity.Epic:      return bgEpic;
            case Rarity.Legendary: return bgLegendary;
            case Rarity.Special:   return bgSpecial;
            default:               return bgCommon;
        }
    }

    public Sprite GetRelicStarSprite(RelicData relic)
    {
        if (relic == null) return null;

        // ★不利奇物の場合は、レアリティ設定を無視して専用アイコンを返す
        if (relic.relicCategory == RelicCategory.Negative)
        {
            return starNegative;
        }

        // それ以外はレアリティに応じて星画像を返す
        switch (relic.rarity)
        {
            case Rarity.Rare:      return starRare;
            case Rarity.Epic:      return starEpic;
            case Rarity.Legendary: return starLegendary;
            default:               return null;
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
        
        // ★修正：カードを開いたときは奇物のレアリティ背景を非表示にする
        if (detailBackgroundImage != null)
        {
            detailBackgroundImage.gameObject.SetActive(false);
        }
        
        // ★必要に応じて、詳細画面での画像アスペクト比も調整する（ImageのPreserve AspectをONにしておくと便利です）

        detailPopupPanel.SetActive(true);
    }

    public void ShowRelicDetail(RelicData relic)
    {
        if (detailPopupPanel == null || relic == null) return;

        detailNameText.text = relic.relicName;
        detailDescriptionText.text = relic.description;
        detailImageView.sprite = relic.relicIcon;

        if (detailBackgroundImage != null)
        {
            Sprite bgSprite = GetRelicBackground(relic);
            if (bgSprite != null)
            {
                detailBackgroundImage.gameObject.SetActive(true);
                detailBackgroundImage.sprite = bgSprite;
            }
            else
            {
                detailBackgroundImage.gameObject.SetActive(false);
            }
        }

        if (detailStarImage != null)
        {
            Sprite starSprite = GetRelicStarSprite(relic);
            if (starSprite != null)
            {
                detailStarImage.sprite = starSprite;
                detailStarImage.gameObject.SetActive(true);
            }
            else
            {
                // 星画像が設定されていない（Commonなど）場合は非表示にする
                detailStarImage.gameObject.SetActive(false);
            }
        }

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