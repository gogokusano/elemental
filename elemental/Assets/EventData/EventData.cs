using UnityEngine;

[System.Serializable]
public class EventOption
{
    [TextArea(2,5)]
    public string buttonText; // ボタンに表示するテキスト

    [Header("ステータス変動")]
    public int hpChange;      //HP増減値（マイナスならダメージ。後々アイテム獲得などに拡張できます）
    public int maxHpChange;   // ★追加：最大HPの増減（+で増加、-で減少）
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