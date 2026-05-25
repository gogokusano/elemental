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

    // ==========================================
    // ★追加：ランダム奇物獲得のためのプール管理
    // ==========================================
    [Header("奇物の山札（kibutu0～9をすべて登録してください）")]
    public List<RelicData> allAvailableRelics = new List<RelicData>(); 
    private List<RelicData> unownedRelics = new List<RelicData>(); // まだ持っていない奇物リスト

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

        // ★追加：ゲーム開始時に、すべての奇物を「未所持リスト」にセットする
        unownedRelics = new List<RelicData>(allAvailableRelics);
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

    public void RemoveCard(CardData cardToRemove)
    {
        if (deckCards.Contains(cardToRemove))
        {
            deckCards.Remove(cardToRemove);
            Debug.Log($"カード削除: {cardToRemove.cardName} / 現在の枚数: {deckCards.Count}");
        }
    }

    // 奇物を追加する
    public void AddRelic(RelicData newRelic)
    {
        if (newRelic == null) return;
        
        ownedRelics.Add(newRelic);
        Debug.Log($"<color=cyan>奇物獲得: {newRelic.relicName}</color>");

        // ★追加：特定の奇物を指定して獲得した場合も、ランダムプールから消して重複を防ぐ
        if (unownedRelics.Contains(newRelic))
        {
            unownedRelics.Remove(newRelic);
        }
        
        // 獲得した瞬間の固有効果（最大HPアップなど）があれば即座に実行する
        newRelic.OnAcquire();
    }

    // ==========================================
    // ★追加：ランダムな奇物を1つ付与する関数
    // ==========================================
    public void AddRandomRelic()
    {
        // すべての奇物を取り尽くしている場合の安全策
        if (unownedRelics.Count == 0)
        {
            Debug.LogWarning("すべての奇物を獲得済みです！");
            // ※必要であれば、ここで「代わりにHPを5回復する」などの処理を入れられます。
            return;
        }

        // まだ持っていない奇物の中からランダムに1つ選ぶ
        int randomIndex = Random.Range(0, unownedRelics.Count);
        RelicData selectedRelic = unownedRelics[randomIndex];

        // プレイヤーに付与する（AddRelic関数の中でプールからの削除も自動で行われます）
        AddRelic(selectedRelic);
    }
}