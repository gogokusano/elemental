using UnityEngine;

[System.Serializable]
public class EventOption
{
    public string buttonText; // ボタンに表示するテキスト
    public int hpChange;      // 例：HP増減値（マイナスならダメージ。後々アイテム獲得などに拡張できます）
}

[CreateAssetMenu(fileName = "NewEvent", menuName = "CardGame/EventData")]
public class EventData : ScriptableObject
{
    public string eventName;
    [TextArea(3, 5)]
    public string eventText;
    public Sprite eventImage;
    public EventOption[] options; // 選択肢（Sentaku1〜3に合わせて最大3つを想定）
}