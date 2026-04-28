using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EventManager : MonoBehaviour
{
    [Header("イベントデータのリスト")]
    public List<EventData> eventDatabase; // インスペクターから全てのEventDataを登録する

    [Header("UI参照")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public Image eventImageView;
    public GameObject buttonPrefab;       // 選択肢ボタンのプレハブ
    public Transform buttonContainer;     // ボタンを並べる親オブジェクト

    void Start()
    {
        // 初期ロード時にはランダムなイベントを表示
        GenerateRandomEvent();
    }

    void GenerateRandomEvent()
    {
        if (eventDatabase.Count == 0) return;

        // ランダムに1つイベントを選ぶ
        int randomIndex = Random.Range(0, eventDatabase.Count);
        EventData selectedEvent = eventDatabase[randomIndex];

        // UIに反映させる
        DisplayEvent(selectedEvent);
    }

    void DisplayEvent(EventData ev)
    {
        titleText.text = ev.eventTitle;
        descriptionText.text = ev.eventDescription;
        if (ev.eventImage != null)
        {
            eventImageView.sprite = ev.eventImage;
        }

        // 既存のボタンをクリア
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        // 選択肢の数だけボタンを生成
        foreach (EventOption option in ev.options)
        {
            GameObject btnObj = Instantiate(buttonPrefab, buttonContainer);
            
            // ボタンのテキストを設定
            TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            
            // 効果の説明を取得して、ボタンテキストを構築
            string effectDescription = "";
            if (option.effect != null)
            {
                effectDescription = option.effect.GetDescription();
            }
            
            btnText.text = $"{option.buttonTextPrefix} {effectDescription}";

            // ボタンを押したときの処理を追加
            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() => OnOptionSelected(option));
        }
    }

    void OnOptionSelected(EventOption option)
    {
        Debug.Log($"'{option.buttonTextPrefix}' が選ばれました！");

        // 効果を発動
        if (option.effect != null)
        {
            option.effect.ApplyEffect();
        }

        // ここに効果の処理（HP減少、カード獲得など）と、マップへの帰還処理を書く
        // SceneManager.LoadScene("MapScene");
    }
}