using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections; 

public class CardMovement : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isDragging = false;
    private bool isHovering = false;
    private bool isUsing = false; 

    public Vector3 targetPosition;
    public Quaternion targetRotation = Quaternion.identity; 
    

    void Update()
    {
        if (!isDragging && !isUsing) {
            Vector3 targetP = targetPosition; 
            Quaternion targetR = targetRotation; 
            Vector3 targetS = Vector3.one;

            if (isHovering) { 
                targetP += new Vector3(0, 30f, 0); 
                targetR = Quaternion.identity; 
                targetS = new Vector3(1.2f, 1.2f, 1.2f); 
            }
            

            transform.localPosition = Vector3.Lerp(transform.localPosition, targetP, Time.deltaTime * 15f);
            
            if (Quaternion.Dot(transform.localRotation, targetR) < 1.0f)
            {
                transform.localRotation = Quaternion.Lerp(transform.localRotation, targetR, Time.deltaTime * 15f);
            }
            
            transform.localScale = Vector3.Lerp(transform.localScale, targetS, Time.deltaTime * 15f);
        }
    }


    public void OnPointerEnter(PointerEventData eventData) { if(!isDragging && !isUsing) { isHovering = true; transform.SetAsLastSibling(); } }
    public void OnPointerExit(PointerEventData eventData) { isHovering = false; }

    public void OnBeginDrag(PointerEventData eventData)
    {
        DeckManager dm = Object.FindFirstObjectByType<DeckManager>();
        CardDisplay cd = GetComponent<CardDisplay>();
        
        if (dm == null || dm.isEnemyTurn || cd.cardData.isUnusable || isUsing) return;

        isDragging = true; isHovering = false;
        originalPosition = targetPosition; originalRotation = targetRotation;
        transform.SetAsLastSibling(); 
        transform.localRotation = Quaternion.identity;
    }

    public void OnDrag(PointerEventData eventData) { if (isDragging) transform.position = eventData.position; }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        isDragging = false;

        if (transform.localPosition.y > 150f) {
            CardDisplay display = GetComponent<CardDisplay>();
            ManaManager manaManager = Object.FindFirstObjectByType<ManaManager>();
            
            if (display != null && manaManager != null && manaManager.TryConsumeMana(display.cardData.cost)) {
                StartCoroutine(PlayCardAnimation(display));
            } else { ResetPosition(); }
        } else { ResetPosition(); }
    }

    IEnumerator PlayCardAnimation(CardDisplay display)
    {
        isUsing = true;
        
        Vector3 startPos = transform.position;
        Vector3 endPos;
        
        if (display.cardData.cardType == CardType.Attack) {
            EnemyManager enemy = Object.FindFirstObjectByType<EnemyManager>();
            endPos = enemy != null ? enemy.transform.position : startPos + Vector3.up * 500f;
        } else {
            PlayerManager player = Object.FindFirstObjectByType<PlayerManager>();
            endPos = player != null ? player.transform.position : startPos + Vector3.up * 500f;
        }

        float duration = 0.2f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.5f, t); 
            yield return null;
        }

        // ★ここが唯一の変更点です
        if (display.cardData.cardType == CardType.Attack) {
            EnemyManager enemy = Object.FindFirstObjectByType<EnemyManager>();
            // 単なるTakeDamageではなく、属性も渡すProcessAttackを呼び出します
            if (enemy != null) enemy.ProcessAttack(display.cardData);
        } else if (display.cardData.cardType == CardType.Skill) {
            PlayerManager player = Object.FindFirstObjectByType<PlayerManager>();
            if (player != null) player.AddBlock(display.cardData.block);
        }

        // ★後処理：DeckManagerの SendToDiscard を使って捨て札へ送る
        DeckManager dm = Object.FindFirstObjectByType<DeckManager>();
        if (dm != null)
        {
            dm.SendToDiscard(display.cardData);
            dm.UpdateHandLayout();
        }
        
        Destroy(gameObject);
    }

    void ResetPosition() {
        targetPosition = originalPosition;
        targetRotation = originalRotation;
    }
}