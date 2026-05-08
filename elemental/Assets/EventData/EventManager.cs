using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class EventManager : MonoBehaviour
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
        // あなたのコードに合わせて、シーン内のPlayerManagerを取得
        playerManager = Object.FindFirstObjectByType<PlayerManager>();

        // EventPoolManagerからランダムなイベントを1つ引く
        if (EventPoolManager.Instance != null)
        {
            EventData currentEvent = EventPoolManager.Instance.GetRandomEvent();
            if (currentEvent != null)
            {
                SetupEventUI(currentEvent);
            }
        }
        else
        {
            Debug.LogWarning("EventPoolManagerが見つかりません。（エディタ上で直接このシーンを再生した場合など）");
        }
    }

    void SetupEventUI(EventData ev)
    {
        // テキストと画像の設定
        eventNameText.text = ev.eventName;
        eventDescriptionText.text = ev.eventText;
        if (ev.eventImage != null)
        {
            eventImageView.sprite = ev.eventImage;
        }

        // 一旦すべてのボタンを非表示＆クリック処理リセット
        foreach (var btn in optionButtons)
        {
            btn.gameObject.SetActive(false);
            btn.onClick.RemoveAllListeners();
        }

        // EventDataで設定した選択肢の数だけボタンを有効化
        for (int i = 0; i < ev.options.Length; i++)
        {
            if (i >= optionButtons.Length) break; // 枠（3つ）を超えたら無視

            EventOption option = ev.options[i];
            Button btn = optionButtons[i];
            
            // TextMeshProのテキストを書き換え
            TextMeshProUGUI btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.text = option.buttonText;
            }

            // クリックされた時の処理を登録
            btn.onClick.AddListener(() => OnOptionSelected(option));
            
            btn.gameObject.SetActive(true); // ボタンを表示
        }
    }

    // いずれかの選択肢が選ばれた時の処理
    void OnOptionSelected(EventOption option)
    {
        Debug.Log($"選択肢「{option.buttonText}」が選ばれました！");

        // PlayerManagerへの影響を適用する例
        if (playerManager != null && option.hpChange != 0)
        {
            if (option.hpChange < 0)
            {
                // hpChangeがマイナスならダメージを与える
                playerManager.TakeDamage(-option.hpChange);
            }
            else
            {
                // 回復処理が必要な場合はここに追加（playerManager.Heal(...)など）
            }
        }

        // ここにマップ画面へ戻る処理を書きます
        // Object.FindFirstObjectByType<GameManager>().BackToTitle(); // とりあえずの遷移例
        ReturnToMap();
    }

    void ReturnToMap()
    {
        Debug.Log("マップシーンへ帰還します。");
        // "Map" の部分は、実際の仮Mapシーンの名前に合わせて変更してください
        SceneManager.LoadScene("KariMap"); 
    }
}