using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public enum EventCategory { Normal, Bonus, Camp, Anomaly, Shop}

public class EventManager : MonoBehaviour
{
    [Header("★イベントのカテゴリ設定 (インスペクターで選択)")]
    public EventCategory eventCategory;

    [Header("UIの割り当て")]
    public TextMeshProUGUI eventNameText;
    public TextMeshProUGUI eventDescriptionText;
    public Image eventImageView;
    public Button[] optionButtons; 

    [Header("★カード選択用UI")]
    public GameObject cardSelectionPanel;       // カード選択画面全体の親
    public TextMeshProUGUI cardSelectionTitle;  // 「捨てるカードを選んでください」等
    public Transform cardContentArea;           // カードを並べるGrid (縦長用)
    public GameObject selectableCardPrefab;     // カード用プレハブ
    
    [Header("★カード右側詳細UI")]
    public GameObject cardDetailPanel;          // カード詳細パネルの親
    public Image cardDetailImage;               // カードの画像表示用
    public TextMeshProUGUI cardDetailText;      // 詳細用の効果テキスト（cardTextSを表示）
    public Button cardConfirmButton;            // カード用の「OK」ボタン
    public Button cardCancelButton;             // ★キャンセルボタン
    public TextMeshProUGUI cardDetailPriceText;

    [Header("★奇物選択用UI")]
    public GameObject relicSelectionPanel;      // 奇物選択画面全体の親
    public TextMeshProUGUI relicSelectionTitle; // 「コピーする奇物を選んでください」等
    public Transform relicContentArea;          // 奇物を並べるGrid (正方形用)
    public GameObject selectableRelicPrefab;    // 奇物用プレハブ

    [Header("★奇物右側詳細UI")]
    public GameObject relicDetailPanel;         // 奇物詳細パネルの親
    public Image relicDetailImage;              // 奇物の画像用
    public TextMeshProUGUI relicDetailName;     // 奇物の名前テキスト
    public TextMeshProUGUI relicDetailText;     // 奇物の効果テキスト
    public Button relicConfirmButton;           // 奇物用の「OK」ボタン
    public Button relicCancelButton;            // ★キャンセルボタン
    public Image relicDetailBackgroundImage;
    public Image relicDetailStarImage;
    public TextMeshProUGUI relicDetailPriceText;

    [Header("★イベント結果ポップアップUI")]
    public GameObject eventResultPanel;          // ポップアップ画面全体の親
    public Transform eventResultContentArea;     // 結果を並べるエリア
    
    // ★変更箇所：プレハブを種類別に3つ用意する
    public GameObject eventResultRelicPrefab;    // 奇物用プレハブ
    public GameObject eventResultCardPrefab;     // カード用プレハブ
    public GameObject eventResultGoldPrefab;     // ゴールド用プレハブ
    public Sprite goldIconSprite;
    public Sprite goldBgSprite;

    public Button eventResultConfirmButton;      // 「マップへ戻る」ボタン




    // 内部管理用変数
    private bool isSelectionWaiting = false;
    private bool isCanceled = false;
    private CardData lastSelectedCard = null;
    private RelicData lastSelectedRelic = null;

    // ショップの商品リストと現在のアクション
    private List<RelicData> shopRelics = new List<RelicData>();
    private List<CardData> shopCards = new List<CardData>();
    private ShopActionType currentShopAction = ShopActionType.None;
    private EventData currentEventData;

    void Start()
    {
        // 最初はすべての選択パネル・詳細パネルを非表示にする
        if (cardSelectionPanel != null) cardSelectionPanel.SetActive(false);
        if (relicSelectionPanel != null) relicSelectionPanel.SetActive(false);
        if (cardDetailPanel != null) cardDetailPanel.SetActive(false);
        if (relicDetailPanel != null) relicDetailPanel.SetActive(false);
        if (eventResultPanel != null) eventResultPanel.SetActive(false);

        // OKボタンのイベントリスナー登録と初期無効化
        if (cardConfirmButton != null)
        {
            cardConfirmButton.onClick.RemoveAllListeners();
            cardConfirmButton.onClick.AddListener(OnConfirmClicked);
            cardConfirmButton.interactable = false;
        }
        if (relicConfirmButton != null)
        {
            relicConfirmButton.onClick.RemoveAllListeners();
            relicConfirmButton.onClick.AddListener(OnConfirmClicked);
            relicConfirmButton.interactable = false;
        }

        // ★キャンセルボタンのリスナー登録（常時クリック可能）
        if (cardCancelButton != null)
        {
            cardCancelButton.onClick.RemoveAllListeners();
            cardCancelButton.onClick.AddListener(OnCancelClicked);
            cardCancelButton.interactable = true;
        }
        if (relicCancelButton != null)
        {
            relicCancelButton.onClick.RemoveAllListeners();
            relicCancelButton.onClick.AddListener(OnCancelClicked);
            relicCancelButton.interactable = true;
        }

        if (EventPoolManager.Instance != null)
        {
            switch (eventCategory)
            {
                case EventCategory.Normal:
                    currentEventData = EventPoolManager.Instance.GetRandomEvent();
                    break;
                case EventCategory.Bonus:
                    currentEventData = EventPoolManager.Instance.GetRandomBonus();
                    break;
                case EventCategory.Camp:
                    currentEventData = EventPoolManager.Instance.GetRandomCamp();
                    break;
                case EventCategory.Anomaly:
                    currentEventData = EventPoolManager.Instance.GetRandomAnomaly();
                    break;
                case EventCategory.Shop: 
                    currentEventData = EventPoolManager.Instance.GetRandomShop(); 
                    if (currentEventData != null) GenerateShopItems(currentEventData.shopConfig); 
                    break;
            }

            if (currentEventData != null) 
                SetupEventUI(currentEventData);
        }
        else
        {
            Debug.LogWarning("EventPoolManagerが見つかりません。");
        }
    }

    void GenerateShopItems(ShopConfig config)
    {
        if (config == null) return;

        // 【奇物】
        List<RelicData> poolR = new List<RelicData>(PlayerDataManager.Instance.allAvailableRelics);
        poolR.RemoveAll(r => PlayerDataManager.Instance.ownedRelics.Contains(r)); 
        
        // レアリティによる除外
        if (config.allowedRelicRarities != null && config.allowedRelicRarities.Count > 0)
        {
            poolR.RemoveAll(r => !config.allowedRelicRarities.Contains(r.rarity));
        }

        // ==========================================
        // ★新規追加：カテゴリーによる除外
        // ==========================================
        if (config.allowedRelicCategories != null && config.allowedRelicCategories.Count > 0)
        {
            poolR.RemoveAll(r => !config.allowedRelicCategories.Contains(r.relicCategory));
        }
        
        for (int i = 0; i < config.relicCount; i++)
        {
            if (poolR.Count > 0)
            {
                int idx = Random.Range(0, poolR.Count);
                shopRelics.Add(poolR[idx]);
                poolR.RemoveAt(idx);
            }
        }

        // 【カード】
        List<CardData> poolC = new List<CardData>(PlayerDataManager.Instance.allAvailableCards);
        
        // レアリティによる除外
        if (config.allowedCardRarities != null && config.allowedCardRarities.Count > 0)
        {
            poolC.RemoveAll(c => !config.allowedCardRarities.Contains(c.rarity));
        }

        for (int i = 0; i < config.cardCount; i++)
        {
            if (poolC.Count > 0)
            {
                int idx = Random.Range(0, poolC.Count);
                shopCards.Add(poolC[idx]);
                poolC.RemoveAt(idx);
            }
        }
    }

    int GetRelicPrice(Rarity rarity)
    {
        if (currentEventData != null && currentEventData.shopConfig != null && currentEventData.shopConfig.relicPrices != null)
        {
            foreach (var p in currentEventData.shopConfig.relicPrices)
            {
                if (p != null && p.rarity == rarity) return p.price;
            }
        }
        return 150;
    }

    int GetCardPrice(Rarity rarity)
    {
        if (currentEventData != null && currentEventData.shopConfig != null && currentEventData.shopConfig.cardPrices != null)
        {
            foreach (var p in currentEventData.shopConfig.cardPrices)
            {
                if (p != null && p.rarity == rarity) return p.price;
            }
        }
        return 75;
    }

    void SetupEventUI(EventData ev)
    {
        eventNameText.text = ev.eventName;
        eventDescriptionText.text = ev.eventText;
        if (ev.eventImage != null) eventImageView.sprite = ev.eventImage;

        foreach (var btn in optionButtons)
        {
            btn.gameObject.SetActive(false);
            btn.onClick.RemoveAllListeners();
        }

        for (int i = 0; i < ev.options.Length; i++)
        {
            if (i >= optionButtons.Length) break; 

            EventOption option = ev.options[i];
            Button btn = optionButtons[i];
            
            TextMeshProUGUI btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null) btnText.text = option.buttonText;

            btn.onClick.AddListener(() => OnOptionSelected(option));
            btn.gameObject.SetActive(true); 
        }
    }

    void OnOptionSelected(EventOption option)
    {
        foreach (var btn in optionButtons) btn.gameObject.SetActive(false);
        currentShopAction = option.shopAction;

        if (currentShopAction != ShopActionType.None)
        {
            StartCoroutine(ProcessShopCoroutine(option));
        }
        else
        {
            StartCoroutine(ProcessOptionCoroutine(option));
        }
    }

    // ショップ専用の処理コルーチン
    IEnumerator ProcessShopCoroutine(EventOption option)
    {
        isCanceled = false; // フラグ初期化

        if (option.shopAction == ShopActionType.Leave)
        {
            ReturnToMap();
            yield break;
        }

        if (option.shopAction == ShopActionType.BuyRelic)
        {
            if (shopRelics.Count == 0) Debug.Log("奇物は売り切れです！");
            else
            {
                yield return StartCoroutine(OpenRelicSelectionWindow("購入する奇物を選んでください", shopRelics));
                
                // ★修正：キャンセルされず、かつアイテムが正しく選ばれている場合のみ購入
                if (!isCanceled && lastSelectedRelic != null)
                {
                    int price = GetRelicPrice(lastSelectedRelic.rarity);
                    if (PlayerDataManager.Instance.gold >= price)
                    {
                        PlayerDataManager.Instance.gold -= price;
                        PlayerDataManager.Instance.AddRelic(lastSelectedRelic);
                        shopRelics.Remove(lastSelectedRelic); 
                        Debug.Log($"<color=yellow>{lastSelectedRelic.relicName} を購入しました！ 残りGold: {PlayerDataManager.Instance.gold}</color>");
                    }
                    else Debug.Log("Goldが足りません！");
                }
                else if (isCanceled)
                {
                    Debug.Log("奇物の購入をキャンセルしました。");
                }
            }
        }
        else if (option.shopAction == ShopActionType.BuyCard)
        {
            if (shopCards.Count == 0) Debug.Log("カードは売り切れです！");
            else
            {
                yield return StartCoroutine(OpenCardSelectionWindow("購入するカードを選んでください", shopCards));
                
                // ★修正：キャンセルされず、かつアイテムが正しく選ばれている場合のみ購入
                if (!isCanceled && lastSelectedCard != null)
                {
                    int price = GetCardPrice(lastSelectedCard.rarity);
                    if (PlayerDataManager.Instance.gold >= price)
                    {
                        PlayerDataManager.Instance.gold -= price;
                        PlayerDataManager.Instance.AddCard(lastSelectedCard);
                        shopCards.Remove(lastSelectedCard); 
                        Debug.Log($"<color=yellow>{lastSelectedCard.cardName} を購入しました！ 残りGold: {PlayerDataManager.Instance.gold}</color>");
                    }
                    else Debug.Log("Goldが足りません！");
                }
                else if (isCanceled)
                {
                    Debug.Log("カードの購入をキャンセルしました。");
                }
            }
        }
        else if (option.shopAction == ShopActionType.RemoveCard)
        {
            if (PlayerDataManager.Instance.deckCards.Count == 0) Debug.Log("削除するカードがありません！");
            else
            {
                yield return StartCoroutine(OpenCardSelectionWindow($"削除するカードを選んでください (費用: {option.removeCardPrice}G)", PlayerDataManager.Instance.deckCards));
                
                // ★修正：キャンセルされず、かつアイテムが正しく選ばれている場合のみ削除
                if (!isCanceled && lastSelectedCard != null)
                {
                    if (PlayerDataManager.Instance.gold >= option.removeCardPrice)
                    {
                        PlayerDataManager.Instance.gold -= option.removeCardPrice;
                        PlayerDataManager.Instance.RemoveCard(lastSelectedCard);
                        Debug.Log($"<color=yellow>{lastSelectedCard.cardName} を削除しました！ 残りGold: {PlayerDataManager.Instance.gold}</color>");
                    }
                    else Debug.Log("Goldが足りません！");
                }
                else if (isCanceled)
                {
                    Debug.Log("カードの削除をキャンセルしました。");
                }
            }
        }

        // 買い物が終わった、あるいは「すぐキャンセルした」場合もここへ来てボタンを再表示
        SetupEventUI(currentEventData);
    }

    IEnumerator ProcessOptionCoroutine(EventOption option)
    {
        if (PlayerDataManager.Instance == null) yield break;

        int oldGold = PlayerDataManager.Instance.gold;
        List<RelicData> oldRelics = new List<RelicData>(PlayerDataManager.Instance.ownedRelics);
        List<CardData> oldCards = new List<CardData>(PlayerDataManager.Instance.deckCards);

        bool isSuccess = true;

        if (option.isGambleOption)
        {
            isSuccess = (Random.value <= option.gambleSuccessChance);
            Debug.Log(isSuccess ? "<color=green>ギャンブル成功！</color>" : "<color=red>ギャンブル失敗…</color>");
        }

        if (isSuccess)
        {
            if (!option.isGambleOption)
            {
                if (option.maxHpChange != 0)
                {
                    PlayerDataManager.Instance.maxHp = Mathf.Max(1, PlayerDataManager.Instance.maxHp + option.maxHpChange);
                    if (option.maxHpChange > 0) PlayerDataManager.Instance.currentHp += option.maxHpChange;
                }
                if (option.hpChange != 0)
                {
                    PlayerDataManager.Instance.SaveHp(PlayerDataManager.Instance.currentHp + option.hpChange);
                }
            }

            if (option.rewardRelic != null) PlayerDataManager.Instance.AddRelic(option.rewardRelic);
            if (option.giveRandomRelic)
            {
                int count = Random.Range(option.minRelicCount, option.maxRelicCount + 1);
                List<Rarity> currentRarities = new List<Rarity>(option.allowedRarities);
                if (option.canUpgradeRarity && Random.value <= option.upgradeChance)
                {
                    currentRarities.Clear(); currentRarities.Add(option.upgradedRarity);
                }
                // ★修正：option.allowedCategories を引数に渡す
                PlayerDataManager.Instance.AddRandomRelicsAdvanced(count, currentRarities, option.allowedCategories, option.filterByType, option.targetRelicType, false, 0, Rarity.Common);
            }

            // 【枠2】追加のランダム奇物獲得（例：EpicのNegative奇物を1個など）
            if (option.giveSecondaryRandomRelic)
            {
                int count2 = Random.Range(option.minSecondaryRelicCount, option.maxSecondaryRelicCount + 1);
                // 追加枠用にはアップグレードやタイプフィルターは使わずシンプルに抽選
                PlayerDataManager.Instance.AddRandomRelicsAdvanced(count2, option.secondaryAllowedRarities, option.secondaryAllowedCategories, false, RelicType.None, false, 0, Rarity.Common);
            }

            // 通常イベントでの選択系処理（今回は仕様上キャンセルは想定しない、またはスキップ処理）
            if (option.loseRelicCount > 0)
            {
                if (option.loseRelicMethod == TargetSelectionMethod.Random)
                    PlayerDataManager.Instance.LoseRandomRelics(option.loseRelicCount);
                else if (option.loseRelicMethod == TargetSelectionMethod.Select)
                {
                    for (int i = 0; i < option.loseRelicCount; i++)
                    {
                        if (PlayerDataManager.Instance.ownedRelics.Count == 0) break;
                        yield return StartCoroutine(OpenRelicSelectionWindow("捨てる奇物を選んでください", PlayerDataManager.Instance.ownedRelics));
                        if(lastSelectedRelic != null) PlayerDataManager.Instance.LoseRelic(lastSelectedRelic);
                    }
                }
            }

            if (option.removeCardCount > 0)
            {
                if (option.removeCardMethod == TargetSelectionMethod.Random)
                    PlayerDataManager.Instance.RemoveRandomCards(option.removeCardCount);
                else if (option.removeCardMethod == TargetSelectionMethod.Select)
                {
                    for (int i = 0; i < option.removeCardCount; i++)
                    {
                        if (PlayerDataManager.Instance.deckCards.Count == 0) break;
                        yield return StartCoroutine(OpenCardSelectionWindow("削除するカードを選んでください", PlayerDataManager.Instance.deckCards));
                        if(lastSelectedCard != null) PlayerDataManager.Instance.RemoveCard(lastSelectedCard);
                    }
                }
            }

            if (option.transformCardCount > 0 && option.transformCardMethod == TargetSelectionMethod.Select)
            {
                for (int i = 0; i < option.transformCardCount; i++)
                {
                    if (PlayerDataManager.Instance.deckCards.Count == 0) break;
                    yield return StartCoroutine(OpenCardSelectionWindow("変化させるカードを選んでください", PlayerDataManager.Instance.deckCards));
                    if (lastSelectedCard == null) continue;
                    PlayerDataManager.Instance.RemoveCard(lastSelectedCard);

                    yield return StartCoroutine(OpenCardSelectionWindow("新しく手に入れるカードを選んでください", PlayerDataManager.Instance.allAvailableCards));
                    if (lastSelectedCard != null) PlayerDataManager.Instance.AddCard(lastSelectedCard);
                }
            }

            if (option.duplicateCardCount > 0 && option.duplicateCardMethod == TargetSelectionMethod.Select)
            {
                for (int i = 0; i < option.duplicateCardCount; i++)
                {
                    if (PlayerDataManager.Instance.deckCards.Count == 0) break;
                    yield return StartCoroutine(OpenCardSelectionWindow("コピーするカードを選んでください", PlayerDataManager.Instance.deckCards));
                    if (lastSelectedCard != null) PlayerDataManager.Instance.AddCard(lastSelectedCard);
                }
            }

            if (option.gainCardCount > 0)
            {
                if (option.gainCardMethod == TargetSelectionMethod.Random)
                    PlayerDataManager.Instance.AddRandomCardsFiltered(option.gainCardCount, option.filterCardByElement, option.targetCardElement, option.filterCardByCost, option.targetCardCost, option.filterCardByRarity, option.targetCardRarity);
                else if (option.gainCardMethod == TargetSelectionMethod.Select)
                {
                    for (int i = 0; i < option.gainCardCount; i++)
                    {
                        List<CardData> candidates = GetRandomCardCandidates(option, 3);
                        yield return StartCoroutine(OpenCardSelectionWindow("獲得するカードを選んでください", candidates));
                        if (lastSelectedCard != null) PlayerDataManager.Instance.AddCard(lastSelectedCard);
                    }
                }
            }
        }
        else
        {
            switch (option.penaltyType)
            {
                case EventPenaltyType.HpLoss: PlayerDataManager.Instance.SaveHp(PlayerDataManager.Instance.currentHp - option.penaltyValue); break;
                case EventPenaltyType.MaxHpLoss:
                    PlayerDataManager.Instance.maxHp = Mathf.Max(1, PlayerDataManager.Instance.maxHp - option.penaltyValue);
                    if (PlayerDataManager.Instance.currentHp > PlayerDataManager.Instance.maxHp) PlayerDataManager.Instance.SaveHp(PlayerDataManager.Instance.maxHp);
                    break;
                case EventPenaltyType.GoldLoss: PlayerDataManager.Instance.gold = Mathf.Max(0, PlayerDataManager.Instance.gold - option.penaltyValue); break;
            }
        }

        yield return StartCoroutine(ShowEventResultCoroutine(oldGold, oldRelics, oldCards));
    }

    IEnumerator ShowEventResultCoroutine(int oldGold, List<RelicData> oldRelics, List<CardData> oldCards)
    {
        bool hasChanges = false;
        foreach (Transform child in eventResultContentArea) Destroy(child.gameObject);

        // 1. 奇物の獲得
        var addedRelics = GetDifferences(oldRelics, PlayerDataManager.Instance.ownedRelics);
        foreach (var kvp in addedRelics)
        {
            GameObject obj = Instantiate(eventResultRelicPrefab, eventResultContentArea); // ★RelicPrefabに変更
            obj.GetComponent<EventResultItemUI>().SetupRelic(kvp.Key, kvp.Value, false);
            hasChanges = true;
        }

        // 2. 奇物の喪失
        var lostRelics = GetDifferences(PlayerDataManager.Instance.ownedRelics, oldRelics);
        foreach (var kvp in lostRelics)
        {
            GameObject obj = Instantiate(eventResultRelicPrefab, eventResultContentArea); // ★RelicPrefabに変更
            obj.GetComponent<EventResultItemUI>().SetupRelic(kvp.Key, kvp.Value, true);
            hasChanges = true;
        }

        // 3. カードの獲得
        var addedCards = GetDifferences(oldCards, PlayerDataManager.Instance.deckCards);
        foreach (var kvp in addedCards)
        {
            GameObject obj = Instantiate(eventResultCardPrefab, eventResultContentArea); // ★CardPrefabに変更
            obj.GetComponent<EventResultItemUI>().SetupCard(kvp.Key, kvp.Value, false);
            hasChanges = true;
        }

        // 4. カードの喪失(削除など)
        var lostCards = GetDifferences(PlayerDataManager.Instance.deckCards, oldCards);
        foreach (var kvp in lostCards)
        {
            GameObject obj = Instantiate(eventResultCardPrefab, eventResultContentArea); // ★CardPrefabに変更
            obj.GetComponent<EventResultItemUI>().SetupCard(kvp.Key, kvp.Value, true);
            hasChanges = true;
        }

        // 5 & 6. ゴールドの獲得と減少
        int goldDiff = PlayerDataManager.Instance.gold - oldGold;
        if (goldDiff > 0) // 獲得
        {
            GameObject obj = Instantiate(eventResultGoldPrefab, eventResultContentArea); // ★GoldPrefabに変更
            obj.GetComponent<EventResultItemUI>().SetupGold(goldDiff, goldIconSprite, goldBgSprite, false);
            hasChanges = true;
        }
        else if (goldDiff < 0) // 減少
        {
            GameObject obj = Instantiate(eventResultGoldPrefab, eventResultContentArea); // ★GoldPrefabに変更
            obj.GetComponent<EventResultItemUI>().SetupGold(Mathf.Abs(goldDiff), goldIconSprite, goldBgSprite, true);
            hasChanges = true;
        }

        if (!hasChanges)
        {
            ReturnToMap();
            yield break;
        }

        eventResultPanel.SetActive(true);
        eventResultConfirmButton.interactable = true;

        bool resultConfirmed = false;
        eventResultConfirmButton.onClick.RemoveAllListeners();
        eventResultConfirmButton.onClick.AddListener(() => { resultConfirmed = true; });

        yield return new WaitUntil(() => resultConfirmed);

        eventResultPanel.SetActive(false);
        ReturnToMap();
    }

    IEnumerator OpenCardSelectionWindow(string title, List<CardData> cards)
    {
        foreach (Transform child in cardContentArea) Destroy(child.gameObject);

        // 削除費用の取得（一回だけ計算する）
        int removePrice = 75;
        if (currentShopAction == ShopActionType.RemoveCard && currentEventData != null)
        {
            foreach (var opt in currentEventData.options)
            {
                if (opt.shopAction == ShopActionType.RemoveCard) { removePrice = opt.removeCardPrice; break; }
            }
        }

        foreach (CardData card in cards)
        {
            if (card == null) continue;
            GameObject obj = Instantiate(selectableCardPrefab, cardContentArea);
            
            // ★追加：ショップなら価格を渡す
            int price = -1;
            if (currentShopAction == ShopActionType.BuyCard && shopCards.Contains(card))
                price = GetCardPrice(card.rarity);
            else if (currentShopAction == ShopActionType.RemoveCard)
                price = removePrice;

            obj.GetComponent<EventSelectItem>().SetupCard(card, this, price); 
        }

        if (cardSelectionTitle != null) cardSelectionTitle.text = title;
        
        cardSelectionPanel.SetActive(true);
        if (cardDetailPanel != null) cardDetailPanel.SetActive(false);
        if (cardConfirmButton != null) cardConfirmButton.interactable = false;
        if (cardCancelButton != null) cardCancelButton.interactable = true;

        isSelectionWaiting = true;
        lastSelectedCard = null;

        yield return new WaitUntil(() => !isSelectionWaiting);
        
        cardSelectionPanel.SetActive(false);
    }

    IEnumerator OpenRelicSelectionWindow(string title, List<RelicData> relics)
    {
        foreach (Transform child in relicContentArea) Destroy(child.gameObject);

        foreach (RelicData relic in relics)
        {
            if (relic == null) continue;
            GameObject obj = Instantiate(selectableRelicPrefab, relicContentArea);
            
            // ★追加：ショップなら価格を渡す
            int price = -1;
            if (currentShopAction == ShopActionType.BuyRelic && shopRelics.Contains(relic))
                price = GetRelicPrice(relic.rarity);

            obj.GetComponent<EventSelectItem>().SetupRelic(relic, this, price);
        }

        if (relicSelectionTitle != null) relicSelectionTitle.text = title;

        relicSelectionPanel.SetActive(true);
        if (relicDetailPanel != null) relicDetailPanel.SetActive(false);
        if (relicConfirmButton != null) relicConfirmButton.interactable = false;
        if (relicCancelButton != null) relicCancelButton.interactable = true;

        isSelectionWaiting = true;
        lastSelectedRelic = null;

        yield return new WaitUntil(() => !isSelectionWaiting);
        
        relicSelectionPanel.SetActive(false);
    }

    public void OnCardSelected(CardData card) 
    { 
        lastSelectedCard = card; 
        lastSelectedRelic = null;

        if (cardDetailPanel != null)
        {
            cardDetailPanel.SetActive(true);
            if (cardDetailImage != null) cardDetailImage.sprite = card.cardImage;
            
            // ★修正：説明文には価格を混ぜない
            if (cardDetailText != null) cardDetailText.text = string.IsNullOrEmpty(card.cardTextS) ? card.description : card.cardTextS;

            // ★新規追加：価格テキストを分離して表示
            if (cardDetailPriceText != null)
            {
                if (currentShopAction == ShopActionType.BuyCard && shopCards.Contains(card))
                {
                    cardDetailPriceText.text = $"<color=yellow>{GetCardPrice(card.rarity)} G</color>";
                    cardDetailPriceText.gameObject.SetActive(true);
                }
                else if (currentShopAction == ShopActionType.RemoveCard)
                {
                    int removePrice = 75; 
                    if (currentEventData != null)
                    {
                        foreach (var opt in currentEventData.options)
                        {
                            if (opt.shopAction == ShopActionType.RemoveCard) { removePrice = opt.removeCardPrice; break; }
                        }
                    }
                    cardDetailPriceText.text = $"<color=yellow>{removePrice} G</color>";
                    cardDetailPriceText.gameObject.SetActive(true);
                }
                else
                {
                    cardDetailPriceText.gameObject.SetActive(false); // ショップ以外は隠す
                }
            }
        }

        if (cardConfirmButton != null) cardConfirmButton.interactable = true;
    }

    public void OnRelicSelected(RelicData relic) 
    { 
        lastSelectedRelic = relic; 
        lastSelectedCard = null;

        if (relicDetailPanel != null)
        {
            relicDetailPanel.SetActive(true);
            if (relicDetailImage != null) relicDetailImage.sprite = relic.relicIcon;
            if (relicDetailName != null) relicDetailName.text = relic.relicName;
            
            if (relicDetailBackgroundImage != null && StatusPanelManager.Instance != null)
                relicDetailBackgroundImage.sprite = StatusPanelManager.Instance.GetRelicBackground(relic);

            if (relicDetailStarImage != null && StatusPanelManager.Instance != null)
            {
                Sprite starSprite = StatusPanelManager.Instance.GetRelicStarSprite(relic);
                if (starSprite != null)
                {
                    relicDetailStarImage.sprite = starSprite;
                    relicDetailStarImage.gameObject.SetActive(true);
                }
                else relicDetailStarImage.gameObject.SetActive(false);
            }

            // ★修正：説明文には価格を混ぜない
            if (relicDetailText != null) relicDetailText.text = relic.description;

            // ★新規追加：価格テキストを分離して表示
            if (relicDetailPriceText != null)
            {
                if (currentShopAction == ShopActionType.BuyRelic && shopRelics.Contains(relic))
                {
                    relicDetailPriceText.text = $"<color=yellow>{GetRelicPrice(relic.rarity)} G</color>";
                    relicDetailPriceText.gameObject.SetActive(true);
                }
                else
                {
                    relicDetailPriceText.gameObject.SetActive(false); // ショップ以外は隠す
                }
            }
        }

        if (relicConfirmButton != null) relicConfirmButton.interactable = true;
    }

    public void OnConfirmClicked()
    {
        if (lastSelectedCard != null || lastSelectedRelic != null)
        {
            isSelectionWaiting = false; 

            if (cardDetailPanel != null) cardDetailPanel.SetActive(false);
            if (relicDetailPanel != null) relicDetailPanel.SetActive(false);
            if (cardConfirmButton != null) cardConfirmButton.interactable = false;
            if (relicConfirmButton != null) relicConfirmButton.interactable = false;
        }
    }

    public void OnCancelClicked()
    {
        // ★修正：アイテム未選択でもここに来るので、即座に終了処理に移行させる
        isCanceled = true;
        isSelectionWaiting = false; 
        lastSelectedCard = null;
        lastSelectedRelic = null;

        if (cardDetailPanel != null) cardDetailPanel.SetActive(false);
        if (relicDetailPanel != null) relicDetailPanel.SetActive(false);
        if (cardConfirmButton != null) cardConfirmButton.interactable = false;
        if (relicConfirmButton != null) relicConfirmButton.interactable = false;
    }

    List<CardData> GetRandomCardCandidates(EventOption option, int count)
    {
        List<CardData> pool = new List<CardData>(PlayerDataManager.Instance.allAvailableCards);
        if (option.filterCardByElement) pool = pool.FindAll(c => c.elementType == option.targetCardElement);
        if (option.filterCardByCost) pool = pool.FindAll(c => c.cost == option.targetCardCost);
        if (option.filterCardByRarity) pool = pool.FindAll(c => c.rarity == option.targetCardRarity);

        List<CardData> result = new List<CardData>();
        for (int i = 0; i < count; i++)
        {
            if (pool.Count > 0)
            {
                int idx = Random.Range(0, pool.Count);
                result.Add(pool[idx]);
                pool.RemoveAt(idx); 
            }
        }
        return result;
    }

    Dictionary<T, int> GetDifferences<T>(List<T> oldList, List<T> newList) where T : ScriptableObject
    {
        Dictionary<T, int> counts = new Dictionary<T, int>();
        List<T> tempOld = new List<T>(oldList);
        foreach (var item in newList)
        {
            if (tempOld.Contains(item))
            {
                tempOld.Remove(item); // 以前から持っていた分は相殺
            }
            else
            {
                if (counts.ContainsKey(item)) counts[item]++;
                else counts[item] = 1; // 新しく増えた分
            }
        }
        return counts;
    }

    void ReturnToMap()
    {
        Debug.Log("マップシーンへ帰還します。");
        SceneManager.LoadScene("Map"); 
    }
}