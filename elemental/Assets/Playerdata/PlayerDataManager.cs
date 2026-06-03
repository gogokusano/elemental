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

    [Header("全カードのデータベース（すべてのカードを登録）")]
    public List<CardData> allAvailableCards = new List<CardData>();

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

    public void AddRandomRelicsAdvanced(int count, List<Rarity> allowedRarities, bool useType, RelicType type, bool canUpgrade, float upgradeChance, Rarity upgradedRarity)
    {
        for (int i = 0; i < count; i++)
        {
            // 1回引くごとに、対象とするレアリティを決定する
            List<Rarity> currentTargetRarities = new List<Rarity>();

            // アップグレード判定
            if (canUpgrade && Random.value <= upgradeChance)
            {
                currentTargetRarities.Add(upgradedRarity);
                Debug.Log($"<color=yellow>奇物がアップグレードされた！ レアリティ: {upgradedRarity}</color>");
            }
            // アップグレードしなかった場合は、インスペクターで指定したレアリティ群を採用
            else if (allowedRarities != null && allowedRarities.Count > 0)
            {
                currentTargetRarities.AddRange(allowedRarities);
            }

            // プールの作成（未所持のものすべてからスタート）
            List<RelicData> pool = new List<RelicData>(unownedRelics);

            // レアリティでの絞り込み
            if (currentTargetRarities.Count > 0)
            {
                pool = pool.FindAll(r => currentTargetRarities.Contains(r.rarity));
            }

            // タイプでの絞り込み
            if (useType)
            {
                pool = pool.FindAll(r => r.relicType == type);
            }

            // 条件に合う奇物がない場合はループを終了する
            if (pool.Count == 0)
            {
                Debug.LogWarning("条件（レアリティ・タイプ）に一致する未所持の奇物がもう残っていません！");
                break; 
            }

            // 絞り込まれたプールの中からランダムに1つ選んで獲得
            RelicData chosenRelic = pool[Random.Range(0, pool.Count)];
            AddRelic(chosenRelic);
        }
    }

    public void LoseRandomRelics(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (ownedRelics.Count > 0)
            {
                int idx = Random.Range(0, ownedRelics.Count);
                RelicData relic = ownedRelics[idx];
                ownedRelics.RemoveAt(idx);
                unownedRelics.Add(relic); // 未所持に戻す
                Debug.Log($"<color=red>奇物を失った: {relic.relicName}</color>");
            }
        }
    }

    // フィルターに従ってランダムなカードを獲得する
    public void AddRandomCardsFiltered(int count, bool useElement, ElementType element, bool useCost, int cost, bool useRarity, Rarity rarity)
    {
        List<CardData> pool = new List<CardData>(allAvailableCards);

        if (useElement) pool = pool.FindAll(c => c.elementType == element);
        if (useCost) pool = pool.FindAll(c => c.cost == cost);
        if (useRarity) pool = pool.FindAll(c => c.rarity == rarity);

        if (pool.Count == 0)
        {
            Debug.LogWarning("条件に合致するカードが存在しません！フィルターなしでランダム取得します。");
            pool = new List<CardData>(allAvailableCards);
        }

        for (int i = 0; i < count; i++)
        {
            if (pool.Count > 0)
            {
                CardData chosen = pool[Random.Range(0, pool.Count)];
                AddCard(chosen);
            }
        }
    }
    
    // デッキからランダムにカードを削除する
    public void RemoveRandomCards(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (deckCards.Count > 0)
            {
                int idx = Random.Range(0, deckCards.Count);
                CardData card = deckCards[idx];
                RemoveCard(card);
            }
        }
    }

    public void LoseRelic(RelicData relic)
    {
        if (ownedRelics.Contains(relic))
        {
            ownedRelics.Remove(relic);
            unownedRelics.Add(relic); // 山札に戻す
            Debug.Log($"<color=red>奇物を失った: {relic.relicName}</color>");
        }
    }
}