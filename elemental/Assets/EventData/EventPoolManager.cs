using System.Collections.Generic;
using UnityEngine;

public class EventPoolManager : MonoBehaviour
{
    public static EventPoolManager Instance { get; private set; }

    [Header("通常イベントのリスト")]
    public List<EventData> allEvents;
    private List<EventData> remainingEvents = new List<EventData>();

    // ★追加：ボーナスイベントのリスト
    [Header("ボーナスイベントのリスト")]
    public List<EventData> allBonusEvents;
    private List<EventData> remainingBonusEvents = new List<EventData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // ★初期化時に両方の山札を作る
            ResetPool();
            ResetBonusPool(); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 通常イベントの山札リセット
    public void ResetPool()
    {
        remainingEvents = new List<EventData>(allEvents);
    }

    // ★追加：ボーナスイベントの山札リセット
    public void ResetBonusPool()
    {
        remainingBonusEvents = new List<EventData>(allBonusEvents);
    }

    // 通常イベントを引く
    public EventData GetRandomEvent()
    {
        if (allEvents == null || allEvents.Count == 0) return null;

        if (remainingEvents.Count == 0)
        {
            Debug.Log("すべての通常イベントが出尽くしました。山札をリセットします。");
            ResetPool();
        }

        int randomIndex = Random.Range(0, remainingEvents.Count);
        EventData selectedEvent = remainingEvents[randomIndex];
        remainingEvents.RemoveAt(randomIndex);

        return selectedEvent;
    }

    // ★追加：ボーナスイベントを引く
    public EventData GetRandomBonus()
    {
        if (allBonusEvents == null || allBonusEvents.Count == 0) return null;

        if (remainingBonusEvents.Count == 0)
        {
            Debug.Log("すべてのボーナスが出尽くしました。山札をリセットします。");
            ResetBonusPool();
        }

        int randomIndex = Random.Range(0, remainingBonusEvents.Count);
        EventData selectedBonus = remainingBonusEvents[randomIndex];
        remainingBonusEvents.RemoveAt(randomIndex);

        return selectedBonus;
    }
}