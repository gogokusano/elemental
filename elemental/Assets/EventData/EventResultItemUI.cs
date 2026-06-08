using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EventResultItemUI : MonoBehaviour
{
    [Header("UI参照 (画像関連)")]
    public Image backgroundImage;   // 奇物・ゴールド用の背景枠
    public Image iconImage;         // カード・奇物・ゴールドの画像
    public Image rarityStarImage;   // 奇物の星・不利奇物用マーク

    [Header("★UI参照 (テキスト関連・新規追加)")]
    public TextMeshProUGUI nameText;        // アイテム名 (トライアングル、飛び散る塵など)
    public TextMeshProUGUI descriptionText; // 効果説明文
    public TextMeshProUGUI countText;       // 獲得数・減少数 (36など)

    // ==========================================
    // 奇物の表示セットアップ
    // ==========================================
    public void SetupRelic(RelicData relic, int count, bool isLost)
    {
        if (relic == null) return;

        // 背景、アイコン、星の設定
        if (backgroundImage != null && StatusPanelManager.Instance != null)
        {
            backgroundImage.sprite = StatusPanelManager.Instance.GetRelicBackground(relic);
            backgroundImage.gameObject.SetActive(true);
        }
        if (iconImage != null)
        {
            iconImage.sprite = relic.relicIcon;
            iconImage.gameObject.SetActive(true);
        }
        if (rarityStarImage != null && StatusPanelManager.Instance != null)
        {
            Sprite starSprite = StatusPanelManager.Instance.GetRelicStarSprite(relic);
            if (starSprite != null)
            {
                rarityStarImage.sprite = starSprite;
                rarityStarImage.gameObject.SetActive(true);
            }
            else rarityStarImage.gameObject.SetActive(false);
        }

        // ★テキストの設定
        if (nameText != null) nameText.text = relic.relicName;
        if (descriptionText != null) descriptionText.text = relic.description;
        
        if (countText != null)
        {
            string prefix = isLost ? "<color=#FF5555>喪失</color> " : "";
            countText.text = prefix + (count > 1 ? $"x{count}" : "");
            countText.gameObject.SetActive(count > 1 || isLost); // 1個獲得の時はスッキリさせるため非表示
        }
    }

    // ==========================================
    // カードの表示セットアップ
    // ==========================================
    public void SetupCard(CardData card, int count, bool isLost)
    {
        if (card == null) return;

        if (backgroundImage != null) backgroundImage.gameObject.SetActive(false);
        if (rarityStarImage != null) rarityStarImage.gameObject.SetActive(false);

        if (iconImage != null)
        {
            iconImage.sprite = card.cardImage;
            iconImage.gameObject.SetActive(true);
        }

        // ★テキストの設定
        if (nameText != null) nameText.text = card.cardName;
        // 説明文は cardTextS があればそちらを優先、なければ description を表示
        if (descriptionText != null) descriptionText.text = string.IsNullOrEmpty(card.cardTextS) ? card.description : card.cardTextS;

        if (countText != null)
        {
            string prefix = isLost ? "<color=#FF5555>削除</color> " : "";
            countText.text = prefix + (count > 1 ? $"x{count}" : "");
            countText.gameObject.SetActive(count > 1 || isLost);
        }
    }

    // ==========================================
    // ゴールドの表示セットアップ
    // ==========================================
    public void SetupGold(int amount, Sprite goldIcon, Sprite goldBg, bool isLost)
    {
        if (backgroundImage != null)
        {
            if (goldBg != null)
            {
                backgroundImage.sprite = goldBg;
                backgroundImage.gameObject.SetActive(true);
            }
            else backgroundImage.gameObject.SetActive(false);
        }
        if (rarityStarImage != null) rarityStarImage.gameObject.SetActive(false);
        
        if (iconImage != null && goldIcon != null)
        {
            iconImage.sprite = goldIcon;
            iconImage.gameObject.SetActive(true);
        }

        // ★テキストの設定
        if (nameText != null) nameText.text = "宇宙の欠片"; // お好みの名前に変更してください
        if (descriptionText != null) descriptionText.gameObject.SetActive(false); // ゴールドに説明文は不要なので消す

        if (countText != null)
        {
            if (isLost)
                countText.text = $"<color=#FF5555>- {Mathf.Abs(amount)}</color>";
            else
                countText.text = $"{amount}"; // スクショのように数値だけ表示
            
            countText.gameObject.SetActive(true);
        }
    }
}