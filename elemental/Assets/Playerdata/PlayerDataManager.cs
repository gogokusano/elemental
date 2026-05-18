using UnityEngine;
using System.Collections.Generic;

public class PlayerDataManager : MonoBehaviour
{
    // どこからでもアクセスできるようにするためのインスタンス
    public static PlayerDataManager Instance { get; private set; }

    [Header("永続ステータス")]
    public int maxHp = 50;
    public int currentHp;
    public int gold = 100;
    public bool hasCounter = false; // イベントで付与されたカウンター状態など

    [Header("デッキ情報")]
    public List<CardData> deckCards = new List<CardData>(); // 所持カードリスト
    public List<CardData> startingDeck = new List<CardData>(); // 最初から持っているカード（設定用）

    [Header("所持奇物")]
    public List<RelicData> ownedRelics = new List<RelicData>();

    private void Awake()
    {
        // シングルトンの設定
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーン遷移しても壊さない
            InitializeData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ゲーム開始時の初期化
    private void InitializeData()
    {
        currentHp = maxHp;
        // 開始時のデッキをコピー
        deckCards = new List<CardData>(startingDeck);
        ownedRelics.Clear();
    }

    // HPの変更を保存する（イベントや戦闘後に呼ぶ）
    public void SaveHp(int hp)
    {
        currentHp = Mathf.Clamp(hp, 0, maxHp);
    }
    
    // カードを追加する
    public void AddCard(CardData newCard)
    {
        deckCards.Add(newCard);
        Debug.Log($"カード獲得: {newCard.cardName} / 現在の枚数: {deckCards.Count}");
    }
}