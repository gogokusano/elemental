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

    [Header("ゲームルール設定")]
    public int maxHandSize = 10;    
    public int drawAmount = 5;      

    [Header("状態管理")]
    public bool isEnemyTurn = false; 

    void Start() 
    { 
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

    IEnumerator EnemyTurnRoutine()
    {
        isEnemyTurn = true;
        yield return new WaitForSeconds(0.5f);

        EnemyManager enemy = Object.FindFirstObjectByType<EnemyManager>();
        if (enemy != null && enemy.gameObject.activeSelf) {
            enemy.ResetBlock(); 
            enemy.ExecuteAction();
        }

        yield return new WaitForSeconds(1.0f);
        
        StartPlayerActionPhase();
    }

    void StartPlayerActionPhase()
    {
        isEnemyTurn = false;

        ManaManager mm = Object.FindFirstObjectByType<ManaManager>();
        if (mm != null) mm.ResetMana();
        PlayerManager pm = Object.FindFirstObjectByType<PlayerManager>();
        if (pm != null) pm.ResetBlock();

        for (int i = 0; i < drawAmount; i++)
        {
            DrawCard();
        }

        if(endTurnButton != null) endTurnButton.interactable = true;
        UpdateHandLayout();
        UpdateDeckUI();
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

    // 敵からの状態異常カードなどを山札に混ぜる機能
    public void AddCardToDrawPile(CardData newCard)
    {
        drawPile.Add(newCard); // 山札に追加
        Shuffle(drawPile);     // 混ざるようにシャッフル
        UpdateDeckUI();        // 山札の数字（UI）を更新
        
        Debug.Log("<color=red>敵の妨害！山札に " + newCard.cardName + " が混ざった！</color>");
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
}