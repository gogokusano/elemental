using UnityEngine;

// イベントの選択肢を定義するクラス
[System.Serializable]
public class EventOption
{
    public string buttonText; // ボタンに表示するテキスト（例：「黄金の像を奪う」）
    // 実際にはここに「HPを-10する」「レリックを獲得する」などの効果（Effect）を紐づけます
}

// イベント本体のデータを定義するScriptableObject
[CreateAssetMenu(fileName = "NewEvent", menuName = "CardGame/EventData")]
public class EventData : ScriptableObject
{
    public string eventTitle;          // イベント名
    [TextArea(3, 5)]
    public string eventDescription;    // イベントの本文
    public Sprite eventImage;          // イベントの背景や挿絵
    public EventOption[] options;      // 選択肢（通常1〜3個）
}