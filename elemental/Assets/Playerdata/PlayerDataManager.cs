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

    // ★追加：全カードのデータベース（GetRewardCardsでカードを抽出するために必要です）
    [Header("全カードのデータベース（すべてのカードを登録）")]
    public List<CardData> allAvailableCards = new List<CardData>();

    private int defaultMaxHp;
    private int defaultGold;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
            
            // ★起動時の初期ステータスを記憶しておく
            defaultMaxHp = maxHp;
            defaultGold = gold;

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

    public void AddRandomRelicsAdvanced(int count, List<Rarity> allowedRarities, List<RelicCategory> allowedCategories, bool useType, RelicType type, bool canUpgrade, float upgradeChance, Rarity upgradedRarity)
    {
        for (int i = 0; i < count; i++)
        {
            List<Rarity> currentTargetRarities = new List<Rarity>();

            if (canUpgrade && Random.value <= upgradeChance)
            {
                currentTargetRarities.Add(upgradedRarity);
                Debug.Log($"<color=yellow>奇物がアップグレードされた！ レアリティ: {upgradedRarity}</color>");
            }
            else if (allowedRarities != null && allowedRarities.Count > 0)
            {
                currentTargetRarities.AddRange(allowedRarities);
            }

            List<RelicData> pool = new List<RelicData>(unownedRelics);

            // レアリティでの絞り込み
            if (currentTargetRarities.Count > 0)
            {
                pool = pool.FindAll(r => currentTargetRarities.Contains(r.rarity));
            }

            // ★追加：カテゴリー（有利・不利など）での絞り込み
            if (allowedCategories != null && allowedCategories.Count > 0)
            {
                pool = pool.FindAll(r => allowedCategories.Contains(r.relicCategory));
            }

            // タイプでの絞り込み
            if (useType)
            {
                pool = pool.FindAll(r => r.relicType == type);
            }

            if (pool.Count == 0)
            {
                Debug.LogWarning("条件に一致する未所持の奇物が残っていません！");
                break; 
            }

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
    

    // ==========================================
    // ▼ ここから今回追加した報酬用のメソッド ▼
    // ==========================================

    // ゴールドを加算するメソッド
    public void AddGold(int amount)
    {
        gold += amount;
        Debug.Log($"ゴールド獲得: {amount} / 現在のゴールド: {gold}");
    }

    // 報酬画面用に、重複しないN枚のカードをランダムに取得するメソッド
    public List<CardData> GetRewardCards(int count = 3)
    {
        List<CardData> rewardCards = new List<CardData>();
        
        // エラー回避：もしインスペクターでallAvailableCardsに何も登録されていなかった場合
        if (allAvailableCards == null || allAvailableCards.Count == 0)
        {
            Debug.LogError("全カードリスト(allAvailableCards)が設定されていないか、空です！インスペクターからカードを登録してください。");
            return rewardCards;
        }

        // 全カードリストのコピーを作成（本体を破壊しないため）
        List<CardData> pool = new List<CardData>(allAvailableCards); 

        for (int i = 0; i < count; i++)
        {
            if (pool.Count == 0) break;

            int randomIndex = Random.Range(0, pool.Count);
            rewardCards.Add(pool[randomIndex]);
            
            // 選ばれたカードをプールから削除し、重複提示を防ぐ
            pool.RemoveAt(randomIndex); 
        }

        return rewardCards;

    }

    public void ResetAllData()
    {
        Debug.Log("<color=red>すべてのプレイヤーデータとマップ進捗を初期化します。</color>");

        // 1. ステータスとゴールドの初期化
        maxHp = defaultMaxHp;
        gold = defaultGold;
        currentHp = maxHp;
        hasCounter = false;

        // 2. デッキを初期デッキの内容で再読み込み
        deckCards = new List<CardData>(startingDeck);

        // 3. 所持奇物のクリアと山札の再構築
        ownedRelics.Clear();
        unownedRelics = new List<RelicData>(allAvailableRelics);

        // 4. マップ進行度（PlayerPrefs）の完全削除
        PlayerPrefs.DeleteKey("LastClearedNode");
        PlayerPrefs.DeleteKey("CurrentChallengingNode");
        PlayerPrefs.DeleteKey("MapSavedX");
        PlayerPrefs.DeleteKey("MapSeed");
        PlayerPrefs.Save();
    }
}