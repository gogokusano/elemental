using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CampManager : MonoBehaviour
{
    [Header("UIの割り当て")]
    public TextMeshProUGUI eventNameText;
    public TextMeshProUGUI eventDescriptionText;
    public Image eventImageView;

    [Header("選択肢ボタン (Sentaku1, 2, 3を登録)")]
    public Button[] optionButtons; 

    void Start()
    {
        if (EventPoolManager.Instance != null)
        {
            EventData currentCamp = EventPoolManager.Instance.GetRandomCamp();
            if (currentCamp != null)
            {
                SetupEventUI(currentCamp);
            }
        }
        else
        {
            Debug.LogWarning("EventPoolManagerが見つかりません。");
        }
    }

    void SetupEventUI(EventData ev)
    {
        eventNameText.text = ev.eventName;
        eventDescriptionText.text = ev.eventText;
        if (ev.eventImage != null)
        {
            eventImageView.sprite = ev.eventImage;
        }

        foreach (var btn in optionButtons)
        {
            btn.gameObject.SetActive(false);
            btn.onClick.RemoveAllListeners();
        }

        for (int i = 0; i < ev.options.Length; i++)
        {
            if (i >= optionButtons.Length) break; 

            EventOption option = ev.options[i];
            Button btn = optionButtons[i];
            
            TextMeshProUGUI btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.text = option.buttonText;
            }

            btn.onClick.AddListener(() => OnOptionSelected(option));
            btn.gameObject.SetActive(true); 
        }
    }

    void OnOptionSelected(EventOption option)
    {
        Debug.Log($"選択肢「{option.buttonText}」が選ばれました！");

        if (PlayerDataManager.Instance != null)
        {
            // ギャンブルの判定
            bool isSuccess = true;
            if (option.isGambleOption)
            {
                if (Random.value <= option.gambleSuccessChance)
                {
                    isSuccess = true;
                    Debug.Log("<color=green>ギャンブル成功！報酬を獲得します。</color>");
                }
                else
                {
                    isSuccess = false;
                    Debug.Log("<color=red>ギャンブル失敗…デメリットが適用されます。</color>");
                }
            }

            // パターンA：成功、または通常の選択肢
            if (isSuccess)
            {
                if (!option.isGambleOption)
                {
                    if (option.maxHpChange != 0)
                    {
                        PlayerDataManager.Instance.maxHp += option.maxHpChange;
                        if (PlayerDataManager.Instance.maxHp < 1) PlayerDataManager.Instance.maxHp = 1;
                        if (option.maxHpChange > 0) PlayerDataManager.Instance.currentHp += option.maxHpChange;
                    }
                    if (option.hpChange != 0)
                    {
                        PlayerDataManager.Instance.SaveHp(PlayerDataManager.Instance.currentHp + option.hpChange);
                    }
                }

                // ★固定獲得
                if (option.rewardRelic != null)
                {
                    PlayerDataManager.Instance.AddRelic(option.rewardRelic);
                }
                
                // ★統合ランダム獲得（単発も複数も、絞り込みも全てここで処理！）
                if (option.giveRandomRelic)
                {
                    int count = Random.Range(option.minRelicCount, option.maxRelicCount + 1);
                    
                    PlayerDataManager.Instance.AddRandomRelicsAdvanced(
                        count, 
                        option.allowedRarities, 
                        option.filterByType, 
                        option.targetRelicType, 
                        option.canUpgradeRarity, 
                        option.upgradeChance, 
                        option.upgradedRarity
                    );
                }
            }
            // パターンB：ギャンブル失敗時
            else
            {
                switch (option.penaltyType)
                {
                    case EventPenaltyType.HpLoss:
                        PlayerDataManager.Instance.SaveHp(PlayerDataManager.Instance.currentHp - option.penaltyValue);
                        break;

                    case EventPenaltyType.MaxHpLoss:
                        PlayerDataManager.Instance.maxHp -= option.penaltyValue;
                        if (PlayerDataManager.Instance.maxHp < 1) PlayerDataManager.Instance.maxHp = 1;
                        if (PlayerDataManager.Instance.currentHp > PlayerDataManager.Instance.maxHp)
                        {
                            PlayerDataManager.Instance.SaveHp(PlayerDataManager.Instance.maxHp);
                        }
                        break;

                    case EventPenaltyType.GoldLoss:
                        PlayerDataManager.Instance.gold = Mathf.Max(0, PlayerDataManager.Instance.gold - option.penaltyValue);
                        break;
                }
            }
        }

        ReturnToMap();
    }

    void ReturnToMap()
    {
        Debug.Log("マップシーン（安息の地）から帰還します。");
        SceneManager.LoadScene("Map"); 
    }
}