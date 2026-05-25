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
            // EventPoolManagerに実装されている（または今後追加する）GetRandomCampからデータを取得
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
        Debug.Log($"休息選択肢「{option.buttonText}」が選ばれました！");

        if (PlayerDataManager.Instance != null)
        {
            // 1. 最大HPの増減（「最大HPを+5して肉体を強化する」などの選択肢用）
            if (option.maxHpChange != 0)
            {
                PlayerDataManager.Instance.maxHp += option.maxHpChange;
                if (PlayerDataManager.Instance.maxHp < 1) PlayerDataManager.Instance.maxHp = 1; // 0以下防止
                
                // 最大HPが増えた場合は、その分現在HPも回復させる
                if (option.maxHpChange > 0)
                {
                    PlayerDataManager.Instance.currentHp += option.maxHpChange;
                }
            }

            // 2. 現在HPの増減（「焚き火で休む：HPを15回復する」などのメイン処理用）
            if (option.hpChange != 0)
            {
                int newHp = PlayerDataManager.Instance.currentHp + option.hpChange;
                PlayerDataManager.Instance.SaveHp(newHp);
            }

            // 3. 奇物の獲得（「落ちていたお守りを拾う」などの選択肢用）
            if (option.rewardRelic != null)
            {
                PlayerDataManager.Instance.AddRelic(option.rewardRelic);
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
        Debug.Log("マップシーン（安息の地）から帰還します。");
        SceneManager.LoadScene("Map"); 
    }
}