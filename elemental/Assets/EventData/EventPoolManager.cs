using System.Collections.Generic;
using UnityEngine;

public class EventPoolManager : MonoBehaviour
{
    // どこからでもアクセスできるようにする（シングルトン）
    public static EventPoolManager Instance { get; private set; }

    [Header("ゲーム内に登場する全イベントリスト")]
    public List<EventData> allEvents;

    // 現在の「山札」（引けるイベントの残り）
    private List<EventData> remainingEvents = new List<EventData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーン遷移しても消さない
            ResetPool(); // 初期化時に山札を作る
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 山札を初期化する（全イベントを補充）
    public void ResetPool()
    {
        remainingEvents = new List<EventData>(allEvents);
    }

    // ランダムなイベントを引き、山札から除外する
    public EventData GetRandomEvent()
    {
        if (allEvents == null || allEvents.Count == 0) return null;

        // 山札が空になったら、すべてのイベントを補充してリセット
        if (remainingEvents.Count == 0)
        {
            Debug.Log("すべてのイベントが出尽くしました。山札をリセットします。");
            ResetPool();
        }

        // 残っている中からランダムに1つ選ぶ
        int randomIndex = Random.Range(0, remainingEvents.Count);
        EventData selectedEvent = remainingEvents[randomIndex];

        // 選ばれたイベントを山札から削除（次回以降出なくする）
        remainingEvents.RemoveAt(randomIndex);

        return selectedEvent;
    }
}