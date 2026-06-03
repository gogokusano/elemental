using System.Collections.Generic;
using UnityEngine;

public class EventPoolManager : MonoBehaviour
{
    public static EventPoolManager Instance { get; private set; }

    [Header("通常イベントのリスト")]
    public List<EventData> allEvents;
    private List<EventData> remainingEvents = new List<EventData>();

    [Header("ボーナスイベントのリスト")]
    public List<EventData> allBonusEvents;
    private List<EventData> remainingBonusEvents = new List<EventData>();

    // ★追加：休息イベント（Camp）のリスト
    [Header("休息イベントのリスト")]
    public List<EventData> allCampEvents;
    private List<EventData> remainingCampEvents = new List<EventData>();

    [Header("異常イベントのリスト")]
    public List<EventData> allAnomalyEvents;
    private List<EventData> remainingAnomalyEvents = new List<EventData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // ★初期化時にすべての山札を作る
            ResetPool();
            ResetBonusPool(); 
            ResetCampPool();
            ResetAnomalyPool();
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

    // ボーナスイベントの山札リセット
    public void ResetBonusPool()
    {
        remainingBonusEvents = new List<EventData>(allBonusEvents);
    }

    // ★追加：休息イベントの山札リセット
    public void ResetCampPool()
    {
        remainingCampEvents = new List<EventData>(allCampEvents);
    }

    public void ResetAnomalyPool()
    {
        remainingAnomalyEvents = new List<EventData>(allAnomalyEvents);
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

    // ボーナスイベントを引く
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
    
    public EventData GetRandomCamp()
    {
        if (allCampEvents == null || allCampEvents.Count == 0) return null;

        if (remainingCampEvents.Count == 0)
        {
            Debug.Log("すべての休息イベントが出尽くしました。山札をリセットします。");
            ResetCampPool();
        }

        int randomIndex = Random.Range(0, remainingCampEvents.Count);
        EventData selectedCamp = remainingCampEvents[randomIndex];
        remainingCampEvents.RemoveAt(randomIndex);

        return selectedCamp;
    }

    public EventData GetRandomAnomaly()
    {
        if (allAnomalyEvents == null || allAnomalyEvents.Count == 0) return null;

        if (remainingAnomalyEvents.Count == 0)
        {
            Debug.Log("すべての異常イベントが出尽くしました。山札をリセットします。");
            ResetAnomalyPool();
        }

        int randomIndex = Random.Range(0, remainingAnomalyEvents.Count);
        EventData selectedAnomaly = remainingAnomalyEvents[randomIndex];
        remainingAnomalyEvents.RemoveAt(randomIndex);

        return selectedAnomaly;
    }
}