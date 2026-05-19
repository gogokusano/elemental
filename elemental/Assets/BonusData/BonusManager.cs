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

    private PlayerManager playerManager;

    void Start()
    {
        playerManager = Object.FindFirstObjectByType<PlayerManager>();

        if (EventPoolManager.Instance != null)
        {
            // ★ここが通常イベントと違います！GetRandomBonus()を呼ぶ
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

        if (playerManager != null && option.hpChange != 0)
        {
            if (option.hpChange < 0)
            {
                playerManager.TakeDamage(-option.hpChange);
            }
            else
            {
                // ★ボーナスなので回復処理（必要に応じてplayerManager.Healなどを実装）
                playerManager.currentHp = Mathf.Min(playerManager.currentHp + option.hpChange, playerManager.maxHp);
            }
        }

        // 最大HPの増減
        if (option.maxHpChange != 0 && PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.maxHp += option.maxHpChange;
            // 最大HPが増えた分、現在HPも回復させるなどの処理を入れても良いです
            PlayerDataManager.Instance.currentHp += Mathf.Max(0, option.maxHpChange);
        }
        
        // 奇物の獲得
        if (option.rewardRelic != null && PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.AddRelic(option.rewardRelic);
        }

        ReturnToMap();
    }

    void ReturnToMap()
    {
        Debug.Log("マップシーンへ帰還します。");
        SceneManager.LoadScene("Map"); // 環境に合わせて "TempMapScene" などに変更してください
    }
}