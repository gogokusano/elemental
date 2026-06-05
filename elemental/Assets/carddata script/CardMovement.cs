using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

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

        isDragging = true; 
        isHovering = false;
        originalPosition = targetPosition; 
        originalRotation = targetRotation;
        transform.SetAsLastSibling(); 
        transform.localRotation = Quaternion.identity;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            (RectTransform)transform.parent, 
            eventData.position, 
            eventData.pressEventCamera, 
            out Vector3 globalMousePos);

        CardDisplay display = GetComponent<CardDisplay>();
        
        // ==========================================
        // ★修正：単体攻撃カード(isAoEがfalse)の時だけ矢印を出す
        // ==========================================
        if (display != null && display.cardData.cardType == CardType.Attack && !display.cardData.isAoE)
        {
            if (TargetingArrow.Instance != null)
            {
                Vector3 startPos = transform.position;
                startPos.z -= 1f; 

                Vector3 endPos = globalMousePos;
                endPos.z -= 1f;

                TargetingArrow.Instance.UpdateArrow(startPos, endPos);
            }
        }
        // ==========================================
        // ★全体攻撃やその他のカードはマウスについてくる（上へ投げる）
        // ==========================================
        else
        {
            transform.position = globalMousePos;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        isDragging = false;

        if (TargetingArrow.Instance != null)
        {
            TargetingArrow.Instance.HideArrow();
        }

        CardDisplay display = GetComponent<CardDisplay>();
        ManaManager manaManager = Object.FindFirstObjectByType<ManaManager>();
        
        if (display == null || manaManager == null) 
        {
            ResetPosition();
            return;
        }

        bool canUseCard = false;
        EnemyManager targetEnemy = null;

        // ★修正：単体攻撃カードの判定（マウスを離した場所に敵がいるか）
        if (display.cardData.cardType == CardType.Attack && !display.cardData.isAoE)
        {
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (RaycastResult result in results)
            {
                targetEnemy = result.gameObject.GetComponentInParent<EnemyManager>();
                if (targetEnemy != null) break;
            }

            if (targetEnemy != null) canUseCard = true;
        }
        // ★全体攻撃・スキルなどの判定（一定の高さまで投げたか）
        else
        {
            if (transform.localPosition.y > 150f) canUseCard = true;
        }

        if (canUseCard && manaManager.TryConsumeMana(display.cardData.cost))
        {
            StartCoroutine(PlayCardAnimation(display, targetEnemy));
        }
        else
        {
            ResetPosition();
        }
    }

    IEnumerator PlayCardAnimation(CardDisplay display, EnemyManager targetEnemy)
    {
        isUsing = true;
        
        Vector3 startPos = transform.position;
        Vector3 endPos;
        
        // 単体攻撃は敵へ、全体攻撃・スキルは上空へ飛ぶアニメーション
        if (display.cardData.cardType == CardType.Attack && !display.cardData.isAoE && targetEnemy != null) {
            endPos = targetEnemy.transform.position;
        } else {
            endPos = startPos + Vector3.up * 500f;
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

        // ==========================================
        // ★修正：カード効果の適用（属性コンボ復活 ＆ 全体攻撃対応！）
        // ==========================================
        if (display.cardData.cardType == CardType.Attack) 
        {
            if (display.cardData.isAoE)
            {
                // 全体攻撃：画面にいる生きている敵全員にProcessAttack（属性計算）を発動！
                EnemyManager[] allEnemies = Object.FindObjectsByType<EnemyManager>(FindObjectsSortMode.None);
                foreach (EnemyManager e in allEnemies)
                {
                    if (e != null && e.gameObject.activeSelf)
                    {
                        e.ProcessAttack(display.cardData);
                    }
                }
            }
            else if (targetEnemy != null) 
            {
                // 単体攻撃：狙った敵だけにProcessAttack（属性計算）を発動！
                targetEnemy.ProcessAttack(display.cardData);
            }
        } 
        else if (display.cardData.cardType == CardType.Skill) 
        {
            PlayerManager player = Object.FindFirstObjectByType<PlayerManager>();
            if (player != null) player.AddBlock(display.cardData.block);
        }

        // --- ドロー効果＆捨て札 ---
        DeckManager dm = Object.FindFirstObjectByType<DeckManager>();
        if (dm != null)
        {
            if (display.cardData.immediateDraw > 0) dm.ApplyImmediateDraw(display.cardData.immediateDraw);
            if (display.cardData.nextTurnDraw > 0) dm.AddNextTurnDrawBonus(display.cardData.nextTurnDraw);
            dm.SendToDiscard(display.cardData);
        }
        
        Destroy(gameObject);
    }

    void ResetPosition() {
        targetPosition = originalPosition;
        targetRotation = originalRotation;
    }
}