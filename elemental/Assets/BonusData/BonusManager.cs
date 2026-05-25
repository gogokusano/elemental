using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class BonusManager : MonoBehaviour
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
            EventData currentBonus = EventPoolManager.Instance.GetRandomBonus();
            if (currentBonus != null)
            {
                SetupEventUI(currentBonus);
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
        Debug.Log($"ボーナス選択肢「{option.buttonText}」が選ばれました！");

        if (PlayerDataManager.Instance != null)
        {
            // 1. 最大HPの増減
            if (option.maxHpChange != 0)
            {
                PlayerDataManager.Instance.maxHp += option.maxHpChange;
                if (PlayerDataManager.Instance.maxHp < 1) PlayerDataManager.Instance.maxHp = 1; // 0以下防止
                
                if (option.maxHpChange > 0)
                {
                    PlayerDataManager.Instance.currentHp += option.maxHpChange;
                }
            }

            // 2. 現在HPの増減
            if (option.hpChange != 0)
            {
                int newHp = PlayerDataManager.Instance.currentHp + option.hpChange;
                PlayerDataManager.Instance.SaveHp(newHp);
            }

            // 3. 奇物の獲得（特定の奇物が指定されている場合）
            if (option.rewardRelic != null)
            {
                PlayerDataManager.Instance.AddRelic(option.rewardRelic);
            }
            // ★追加：ランダムな奇物を獲得する設定になっている場合
            else if (option.giveRandomRelic)
            {
                PlayerDataManager.Instance.AddRandomRelic();
            }
        }
        else
        {
            Debug.LogError("PlayerDataManagerが存在しません！");
        }

        ReturnToMap();
    }

    void ReturnToMap()
    {
        Debug.Log("マップシーンへ帰還します。");
        SceneManager.LoadScene("Map"); 
    }
}