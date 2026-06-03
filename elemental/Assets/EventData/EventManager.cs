using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public enum EventCategory { Normal, Bonus, Camp, Anomaly }

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

    // 内部管理用変数
    private bool isSelectionWaiting = false;
    private CardData lastSelectedCard = null;
    private RelicData lastSelectedRelic = null;

    void Start()
    {
        // 最初はすべての選択パネル・詳細パネルを非表示にする
        if (cardSelectionPanel != null) cardSelectionPanel.SetActive(false);
        if (relicSelectionPanel != null) relicSelectionPanel.SetActive(false);
        if (cardDetailPanel != null) cardDetailPanel.SetActive(false);
        if (relicDetailPanel != null) relicDetailPanel.SetActive(false);

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

        if (EventPoolManager.Instance != null)
        {
            EventData currentEvent = null;

            switch (eventCategory)
            {
                case EventCategory.Normal:
                    currentEvent = EventPoolManager.Instance.GetRandomEvent();
                    break;
                case EventCategory.Bonus:
                    currentEvent = EventPoolManager.Instance.GetRandomBonus();
                    break;
                case EventCategory.Camp:
                    currentEvent = EventPoolManager.Instance.GetRandomCamp();
                    break;
                case EventCategory.Anomaly:
                    currentEvent = EventPoolManager.Instance.GetRandomAnomaly();
                    break;
            }

            if (currentEvent != null) 
                SetupEventUI(currentEvent);
        }
        else
        {
            Debug.LogWarning("EventPoolManagerが見つかりません。");
        }
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
        // 他のボタンを押せないように全て非表示にする
        foreach (var btn in optionButtons) btn.gameObject.SetActive(false);

        // ★非同期（コルーチン）でイベント処理を開始
        StartCoroutine(ProcessOptionCoroutine(option));
    }

    IEnumerator ProcessOptionCoroutine(EventOption option)
    {
        if (PlayerDataManager.Instance == null) yield break;

        bool isSuccess = true;

        // 1. ギャンブル判定
        if (option.isGambleOption)
        {
            isSuccess = (Random.value <= option.gambleSuccessChance);
            Debug.Log(isSuccess ? "<color=green>ギャンブル成功！</color>" : "<color=red>ギャンブル失敗…</color>");
        }

        if (isSuccess)
        {
            // 2. ステータス変動
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

            // 3. 固定奇物獲得＆ランダム奇物獲得
            if (option.rewardRelic != null) PlayerDataManager.Instance.AddRelic(option.rewardRelic);
            if (option.giveRandomRelic)
            {
                int count = Random.Range(option.minRelicCount, option.maxRelicCount + 1);
                List<Rarity> currentRarities = new List<Rarity>(option.allowedRarities);
                if (option.canUpgradeRarity && Random.value <= option.upgradeChance)
                {
                    currentRarities.Clear(); currentRarities.Add(option.upgradedRarity);
                }
                PlayerDataManager.Instance.AddRandomRelicsAdvanced(count, currentRarities, option.filterByType, option.targetRelicType, false, 0, Rarity.Common);
            }

            // ==========================================
            // ★UIを伴う処理群（1つずつ順番にUIを開いて待機する）
            // ==========================================

            // [奇物の喪失]
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
                        PlayerDataManager.Instance.LoseRelic(lastSelectedRelic);
                    }
                }
            }

            // [カードの削除]
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
                        PlayerDataManager.Instance.RemoveCard(lastSelectedCard);
                    }
                }
            }

            // [カードの変化 (Transformation)]
            // 選んだカードを消して、全カードの中から好きなカードを選ぶ
            if (option.transformCardCount > 0 && option.transformCardMethod == TargetSelectionMethod.Select)
            {
                for (int i = 0; i < option.transformCardCount; i++)
                {
                    if (PlayerDataManager.Instance.deckCards.Count == 0) break;
                    yield return StartCoroutine(OpenCardSelectionWindow("変化させるカードを選んでください", PlayerDataManager.Instance.deckCards));
                    PlayerDataManager.Instance.RemoveCard(lastSelectedCard);

                    yield return StartCoroutine(OpenCardSelectionWindow("新しく手に入れるカードを選んでください", PlayerDataManager.Instance.allAvailableCards));
                    PlayerDataManager.Instance.AddCard(lastSelectedCard);
                }
            }

            // [カードのコピー (Duplicate)]
            if (option.duplicateCardCount > 0 && option.duplicateCardMethod == TargetSelectionMethod.Select)
            {
                for (int i = 0; i < option.duplicateCardCount; i++)
                {
                    if (PlayerDataManager.Instance.deckCards.Count == 0) break;
                    yield return StartCoroutine(OpenCardSelectionWindow("コピーするカードを選んでください", PlayerDataManager.Instance.deckCards));
                    PlayerDataManager.Instance.AddCard(lastSelectedCard);
                }
            }

            // [カードの獲得]
            if (option.gainCardCount > 0)
            {
                if (option.gainCardMethod == TargetSelectionMethod.Random)
                    PlayerDataManager.Instance.AddRandomCardsFiltered(option.gainCardCount, option.filterCardByElement, option.targetCardElement, option.filterCardByCost, option.targetCardCost, option.filterCardByRarity, option.targetCardRarity);
                else if (option.gainCardMethod == TargetSelectionMethod.Select)
                {
                    for (int i = 0; i < option.gainCardCount; i++)
                    {
                        // 獲得時はデッキではなく「候補」から選ばせる (今回は全カードからランダムに3枚抽出して提示)
                        List<CardData> candidates = GetRandomCardCandidates(option, 3);
                        yield return StartCoroutine(OpenCardSelectionWindow("獲得するカードを選んでください", candidates));
                        PlayerDataManager.Instance.AddCard(lastSelectedCard);
                    }
                }
            }
        }
        else
        {
            // デメリット処理
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

        // すべての処理（UI選択含む）が完了したらマップへ戻る
        ReturnToMap();
    }

    // ==========================================
    // UI待機用のコルーチン群
    // ==========================================
    IEnumerator OpenCardSelectionWindow(string title, List<CardData> cards)
    {
        foreach (Transform child in cardContentArea) Destroy(child.gameObject);

        foreach (CardData card in cards)
        {
            if (card == null) continue;
            GameObject obj = Instantiate(selectableCardPrefab, cardContentArea);
            // ※EventSelectItem側に、このスクリプト(this)を渡してセットアップするよう調整してください
            obj.GetComponent<EventSelectItem>().SetupCard(card, this); 
        }

        if (cardSelectionTitle != null) cardSelectionTitle.text = title;
        
        // パネル初期状態リセット
        cardSelectionPanel.SetActive(true);
        if (cardDetailPanel != null) cardDetailPanel.SetActive(false);
        if (cardConfirmButton != null) cardConfirmButton.interactable = false;

        isSelectionWaiting = true;
        lastSelectedCard = null;

        // OKボタンが押されるまでここで待機
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
            obj.GetComponent<EventSelectItem>().SetupRelic(relic, this);
        }

        if (relicSelectionTitle != null) relicSelectionTitle.text = title;

        // パネル初期状態リセット
        relicSelectionPanel.SetActive(true);
        if (relicDetailPanel != null) relicDetailPanel.SetActive(false);
        if (relicConfirmButton != null) relicConfirmButton.interactable = false;

        isSelectionWaiting = true;
        lastSelectedRelic = null;

        // OKボタンが押されるまでここで待機
        yield return new WaitUntil(() => !isSelectionWaiting);
        
        relicSelectionPanel.SetActive(false);
    }

    // アイテム側のボタンから呼ばれるコールバック
    public void OnCardSelected(CardData card) 
    { 
        lastSelectedCard = card; 
        lastSelectedRelic = null; // 奇物の選択はクリア

        // 右側のカード詳細パネルを表示・更新
        if (cardDetailPanel != null)
        {
            cardDetailPanel.SetActive(true);
            if (cardDetailImage != null) cardDetailImage.sprite = card.cardImage;
            
            // カードは画像内に名前があるので、効果テキスト(cardTextS)のみを設定
            if (cardDetailText != null)
            {
                cardDetailText.text = string.IsNullOrEmpty(card.cardTextS) ? card.description : card.cardTextS;
            }
        }

        // 選択されたのでカード用のOKボタンを押せるようにする
        if (cardConfirmButton != null) cardConfirmButton.interactable = true;
    }

    public void OnRelicSelected(RelicData relic) 
    { 
        lastSelectedRelic = relic; 
        lastSelectedCard = null; // カードの選択はクリア

        // 右側の奇物詳細パネルを表示・更新
        if (relicDetailPanel != null)
        {
            relicDetailPanel.SetActive(true);
            if (relicDetailImage != null) relicDetailImage.sprite = relic.relicIcon;
            if (relicDetailName != null) relicDetailName.text = relic.relicName;   // 名前を表示
            if (relicDetailText != null) relicDetailText.text = relic.description; // 効果を表示
        }

        // 選択されたので奇物用のOKボタンを押せるようにする
        if (relicConfirmButton != null) relicConfirmButton.interactable = true;
    }

    public void OnConfirmClicked()
    {
        if (lastSelectedCard != null || lastSelectedRelic != null)
        {
            // 待機フラグを折って、コルーチン(WaitUntil)を進める
            isSelectionWaiting = false; 

            // 詳細パネルを閉じてボタンを初期化
            if (cardDetailPanel != null) cardDetailPanel.SetActive(false);
            if (relicDetailPanel != null) relicDetailPanel.SetActive(false);
            if (cardConfirmButton != null) cardConfirmButton.interactable = false;
            if (relicConfirmButton != null) relicConfirmButton.interactable = false;
        }
    }

    // 報酬として提示するカードをランダム生成
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
                pool.RemoveAt(idx); // 被りなしにする
            }
        }
        return result;
    }

    void ReturnToMap()
    {
        Debug.Log("マップシーンへ帰還します。");
        SceneManager.LoadScene("Map"); 
    }
}