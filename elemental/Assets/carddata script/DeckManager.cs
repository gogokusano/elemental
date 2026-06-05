using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckManager : MonoBehaviour
{
    [Header("カードデータ")]
    public List<CardData> drawPile = new List<CardData>();
    public List<CardData> discardPile = new List<CardData>();
    private List<CardMovement> handCards = new List<CardMovement>(); 

    [Header("UI設定")]
    public GameObject cardPrefab;
    public Transform handArea;
    public Button endTurnButton;    

    [Header("デッキUI設定")]
    public TextMeshProUGUI drawPileText;
    public TextMeshProUGUI discardPileText;

    [Header("▼ カード一覧確認UI設定（★追加）")]
    public GameObject cardListPanel;          
    public Transform cardListContent;        
    public TMPro.TextMeshProUGUI cardListTitleText; 

    [Header("ゲームルール設定")]
    public int maxHandSize = 10;    
    public int drawAmount = 5;      

    [Header("状態管理")]
    public bool isEnemyTurn = false; 
    private int nextTurnDrawBonus = 0;

    void Start() 
    { 
        if (PlayerDataManager.Instance != null)
        {
            drawPile = new List<CardData>(PlayerDataManager.Instance.deckCards);
            
            foreach (RelicData relic in PlayerDataManager.Instance.ownedRelics)
            {
                if (relic is RelicCore core)
                {
                    drawAmount += core.drawAmountBonus;
                }
            }
        }

        Shuffle(drawPile); 
        StartFirstTurn(); 
        UpdateDeckUI();
    }

    void StartFirstTurn()
    {
        StartPlayerActionPhase();
    }

    public void EndTurn()
    {
        if (isEnemyTurn) return; 

        GameManager gm = Object.FindFirstObjectByType<GameManager>();
        if (gm != null) gm.PlayerTurnEnd();

        if(endTurnButton != null) endTurnButton.interactable = false;

        List<CardMovement> keptCards = new List<CardMovement>();

        foreach (var card in handCards) {
            if (card != null) {
                if (card.GetComponent<CardDisplay>().cardData.isUnusable) {
                    keptCards.Add(card);
                } else {
                    discardPile.Add(card.GetComponent<CardDisplay>().cardData);
                    Destroy(card.gameObject);
                }
            }
        }

        handCards = keptCards;
        UpdateHandLayout();
        UpdateDeckUI(); 

        StartCoroutine(EnemyTurnRoutine());
    }

    // ========================================================
    // ★修正箇所：画面にいるすべての敵が順番に行動するように変更！
    // ========================================================
    IEnumerator EnemyTurnRoutine()
    {
        isEnemyTurn = true;
        yield return new WaitForSeconds(0.5f);

        // 画面にいる全ての EnemyManager を取得
        EnemyManager[] allEnemies = Object.FindObjectsByType<EnemyManager>(FindObjectsSortMode.None);
        
        foreach (EnemyManager enemy in allEnemies)
        {
            // 敵が生きていて表示されている場合のみ行動
            if (enemy != null && enemy.gameObject.activeSelf) 
            {
                enemy.ResetBlock(); 
                enemy.ExecuteAction();
                
                // 次の敵が動くまでに少し待つ（順番に攻撃してくるようにする）
                yield return new WaitForSeconds(0.5f); 
            }
        }

        yield return new WaitForSeconds(0.5f);
        StartPlayerActionPhase();
    }
    // ========================================================

    void StartPlayerActionPhase()
    {
        isEnemyTurn = false;

        ManaManager mm = Object.FindFirstObjectByType<ManaManager>();
        if (mm != null) mm.ResetMana();
        PlayerManager pm = Object.FindFirstObjectByType<PlayerManager>();
        if (pm != null) pm.ResetBlock();
        GameManager gm = Object.FindFirstObjectByType<GameManager>();
        if (gm != null) gm.PlayerTurnStart();

        int totalDraw = drawAmount + nextTurnDrawBonus;
        nextTurnDrawBonus = 0;


        for (int i = 0; i < totalDraw; i++)
        {
            DrawCard();
        }

        if(endTurnButton != null) endTurnButton.interactable = true;
        UpdateHandLayout();
        UpdateDeckUI();
    }

    public void ApplyImmediateDraw(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            DrawCard();
        }
        UpdateHandLayout();
    }

    public void AddNextTurnDrawBonus(int amount)
    {
        nextTurnDrawBonus += amount;
    }

    public void DrawCard()
    {
        if (handCards.Count >= maxHandSize) return; 

        if (drawPile.Count == 0) {
            if (discardPile.Count == 0) return; 
            
            drawPile.AddRange(discardPile); 
            discardPile.Clear(); 
            Shuffle(drawPile);
            UpdateDeckUI();
        }

        CardData drawnCard = drawPile[0];
        drawPile.RemoveAt(0);
        GenerateCardToHand(drawnCard);
        
        UpdateDeckUI();
    }

    public void GenerateCardToHand(CardData data)
    {
        GameObject newCard = Instantiate(cardPrefab, handArea);
        newCard.transform.localPosition = new Vector3(0, 500f, 0f); 
        CardDisplay display = newCard.GetComponent<CardDisplay>();
        display.Setup(data);
        handCards.Add(newCard.GetComponent<CardMovement>());
    }

    public void SendToDiscard(CardData usedCard)
    {
        discardPile.Add(usedCard);
        UpdateDeckUI();
    }

    public void AddCardToDrawPile(CardData newCard)
    {
        drawPile.Add(newCard); 
        Shuffle(drawPile);     
        UpdateDeckUI();        
    }

    void Shuffle(List<CardData> list) { 
        for (int i = list.Count - 1; i > 0; i--) { 
            int j = Random.Range(0, i + 1); 
            var temp = list[i]; list[i] = list[j]; list[j] = temp; 
        } 
    }

    public void UpdateHandLayout()
    {
        handCards.RemoveAll(card => card == null || !card.gameObject.activeSelf);
        float xStep = 120f; float angleStep = 3f; float yCurve = 5f;
        for (int i = 0; i < handCards.Count; i++) {
            float normalizedIndex = i - (handCards.Count - 1) * 0.5f;
            handCards[i].targetPosition = new Vector3(normalizedIndex * xStep, -Mathf.Pow(normalizedIndex, 2) * yCurve, 0);
            handCards[i].targetRotation = Quaternion.Euler(0, 0, -normalizedIndex * angleStep);
        }
    }

    public void UpdateDeckUI()
    {
        if (drawPileText != null) drawPileText.text = drawPile.Count.ToString();
        if (discardPileText != null) discardPileText.text = discardPile.Count.ToString();
    }

    public void ShowDrawPile()
    {
        OpenCardList("山札一覧", drawPile);
    }

    public void ShowDiscardPile()
    {
        OpenCardList("捨て札一覧", discardPile);
    }

    private void OpenCardList(string title, List<CardData> cards)
    {
        if (cardListPanel == null || cardListContent == null) return;

        foreach (Transform child in cardListContent)
        {
            Destroy(child.gameObject);
        }

        if (cardListTitleText != null) cardListTitleText.text = title;

        foreach (CardData data in cards)
        {
            GameObject newCard = Instantiate(cardPrefab, cardListContent);

            CardDisplay display = newCard.GetComponent<CardDisplay>();
            if (display != null)
            {
                display.Setup(data);
            }

            CardMovement movement = newCard.GetComponent<CardMovement>();
            if (movement != null)
            {
                Destroy(movement);
            }
        }

        cardListPanel.SetActive(true);
    }

    public void CloseCardList()
    {
        if (cardListPanel != null)
        {
            cardListPanel.SetActive(false);
        }
    }
}