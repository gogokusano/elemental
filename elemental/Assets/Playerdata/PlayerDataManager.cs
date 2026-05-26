using UnityEngine;
using System.Collections.Generic;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    [Header("永続ステータス")]
    public int maxHp = 50;
    public int currentHp;
    public int gold = 100;
    public bool hasCounter = false; 

    [Header("デッキ情報")]
    public List<CardData> deckCards = new List<CardData>(); 
    public List<CardData> startingDeck = new List<CardData>(); 

    [Header("所持奇物")]
    public List<RelicData> ownedRelics = new List<RelicData>();

    [Header("奇物の山札（kibutu0～9をすべて登録してください）")]
    public List<RelicData> allAvailableRelics = new List<RelicData>(); 
    private List<RelicData> unownedRelics = new List<RelicData>(); 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
            InitializeData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeData()
    {
        currentHp = maxHp;
        deckCards = new List<CardData>(startingDeck);
        ownedRelics.Clear();
        unownedRelics = new List<RelicData>(allAvailableRelics);
    }

    public void SaveHp(int hp)
    {
        currentHp = Mathf.Clamp(hp, 0, maxHp);
    }
    
    public void AddCard(CardData newCard)
    {
        deckCards.Add(newCard);
        Debug.Log($"カード獲得: {newCard.cardName} / 現在の枚数: {deckCards.Count}");
    }

    public void RemoveCard(CardData cardToRemove)
    {
        if (deckCards.Contains(cardToRemove))
        {
            deckCards.Remove(cardToRemove);
            Debug.Log($"カード削除: {cardToRemove.cardName} / 現在の枚数: {deckCards.Count}");
        }
    }

    public void AddRelic(RelicData newRelic)
    {
        if (newRelic == null) return;
        
        ownedRelics.Add(newRelic);
        Debug.Log($"<color=cyan>奇物獲得: {newRelic.relicName}</color>");

        if (unownedRelics.Contains(newRelic))
        {
            unownedRelics.Remove(newRelic);
        }
        
        newRelic.OnAcquire();
    }

    public void AddRandomRelic()
    {
        if (unownedRelics.Count == 0)
        {
            Debug.LogWarning("すべての奇物を獲得済みです！");
            return;
        }

        int randomIndex = Random.Range(0, unownedRelics.Count);
        RelicData selectedRelic = unownedRelics[randomIndex];

        AddRelic(selectedRelic);
    }
}