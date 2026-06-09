using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class BattleResultManager : MonoBehaviour
{
    [Header("UIパネル設定")]
    public GameObject resultPanel;
    public CanvasGroup resultCanvasGroup; 

    [Header("★タイトル戻り用の黒画面設定")]
    // インスペクターで、一番手前に作った黒画面（CanvasGroup）をここにセットしてください
    public CanvasGroup absoluteFadeCanvasGroup; 

    [Header("テキスト設定")]
    public TextMeshProUGUI clearTimeText;

    [Header("一覧表示用コンテナ")]
    public Transform relicContainer;
    public Transform cardContainer;

    [Header("表示用プレハブ")]
    public GameObject relicIconPrefab; 
    public GameObject cardPrefab;      

    [Header("演出設定")]
    public float fadeDuration = 1.0f;  // フェード（暗転）にかける時間（秒）

    private bool isGameCleared = false;
    private bool isExiting = false;    // ボタン連打バグ防止用

    void Start()
    {
        // 起動時はリザルトを完全に消しておく
        if (resultPanel != null) resultPanel.SetActive(false);
        if (resultCanvasGroup != null) resultCanvasGroup.alpha = 0f;
        if (clearTimeText != null) clearTimeText.text = "";

        // タイトル戻り用の黒画面は最初「完全に透明＆クリックの邪魔をしない」状態に
        if (absoluteFadeCanvasGroup != null)
        {
            absoluteFadeCanvasGroup.alpha = 0f;
            absoluteFadeCanvasGroup.blocksRaycasts = false;
        }
    }

    /// <summary>
    /// 【第1段階】ボス撃破時に画面を暗転させてリザルトを出す
    /// </summary>
    public void PlayBossClearResult()
    {
        if (isGameCleared) return;
        isGameCleared = true;
        StartCoroutine(ResultPresentationRoutine());
    }

    private IEnumerator ResultPresentationRoutine()
    {
        yield return new WaitForSeconds(0.8f);

        // 古いカードなどの残骸を掃除
        if (relicContainer != null) foreach (Transform child in relicContainer) Destroy(child.gameObject);
        if (cardContainer != null) foreach (Transform child in cardContainer) Destroy(child.gameObject);

        if (resultCanvasGroup != null) resultCanvasGroup.alpha = 0f;
        if (resultPanel != null) resultPanel.SetActive(true);
        
        // クリア暗転フェードイン
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            if (resultCanvasGroup != null)
            {
                resultCanvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            }
            yield return null;
        }

        if (resultCanvasGroup != null) resultCanvasGroup.alpha = 1f;

        // クリアタイムの安全な計算と表示
        if (clearTimeText != null)
        {
            float totalTime = Time.time;
            if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.gameStartTime > 0)
            {
                totalTime = Time.time - PlayerDataManager.Instance.gameStartTime;
            }
            int minutes = Mathf.FloorToInt(totalTime / 60f);
            int seconds = Mathf.FloorToInt(totalTime % 60f);
            clearTimeText.text = $"クリアタイム: {minutes:00}:{seconds:00}";
        }

        // 奇物とカードの生成（これまでの正常だった仕様を完全維持）
        PopulateRelics();
        PopulateCards();
    }

    /// <summary>
    /// 【第2段階】メインメニューボタンを押した時、もう一度フェードしてタイトルへ戻る
    /// </summary>
    public void OnMainMenuButtonClicked()
    {
        if (isExiting) return;
        isExiting = true; // 連打対策オン

        // 最手前の黒画面をフェードさせるコルーチンを起動
        StartCoroutine(ExecuteAbsoluteBlackoutRoutine());
    }

    private IEnumerator ExecuteAbsoluteBlackoutRoutine()
    {
        // フェード中は背後のUIを絶対に触らせない
        if (absoluteFadeCanvasGroup != null)
        {
            absoluteFadeCanvasGroup.blocksRaycasts = true;
        }

        // 手動で一番手前に置いた黒いパネルを、透明（0）から真っ黒（1）にフェードアウトさせる
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            if (absoluteFadeCanvasGroup != null)
            {
                absoluteFadeCanvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            }
            yield return null;
        }

        if (absoluteFadeCanvasGroup != null) absoluteFadeCanvasGroup.alpha = 1f;

        // データ初期化でエラーが起きてもフリーズしないように安全網（try-catch）を敷く
        try
        {
            if (PlayerDataManager.Instance != null) PlayerDataManager.Instance.ResetAllData();
            PlayerPrefs.DeleteKey("LastClearedNode");
            PlayerPrefs.DeleteKey("CurrentChallengingNode");
            PlayerPrefs.DeleteKey("MapSavedX");
            PlayerPrefs.DeleteKey("MapSeed");
            PlayerPrefs.Save();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("リセット処理をスキップして進みます: " + e.Message);
        }

        // シーン遷移
        SceneManager.LoadScene("title");
    }

    private void PopulateRelics()
    {
        if (relicContainer == null || relicIconPrefab == null || PlayerDataManager.Instance == null) return;

        foreach (RelicData relic in PlayerDataManager.Instance.ownedRelics)
        {
            if (relic == null) continue;

            GameObject iconObj = Instantiate(relicIconPrefab, relicContainer);
            iconObj.transform.localPosition = Vector3.zero;
            iconObj.transform.localScale = Vector3.one;

            Image parentImg = iconObj.GetComponent<Image>();
            if (parentImg != null) parentImg.enabled = false; 

            Image[] allImages = iconObj.GetComponentsInChildren<Image>(true);
            Image targetImg = null;

            foreach (Image img in allImages)
            {
                if (img.gameObject == iconObj) continue;
                targetImg = img;
                break; 
            }

            if (targetImg != null && relic.relicIcon != null)
            {
                targetImg.enabled = true; 
                targetImg.sprite = relic.relicIcon; 
            }
        }
    }

    private void PopulateCards()
    {
        if (cardContainer == null || cardPrefab == null || PlayerDataManager.Instance == null) return;

        for (int i = 0; i < PlayerDataManager.Instance.deckCards.Count; i++)
        {
            CardData card = PlayerDataManager.Instance.deckCards[i];
            if (card == null) continue;

            GameObject cardObj = Instantiate(cardPrefab, cardContainer);
            cardObj.transform.localPosition = Vector3.zero;
            cardObj.transform.localScale = Vector3.one;

            CardDisplay display = cardObj.GetComponent<CardDisplay>();
            if (display != null) display.Setup(card); 
            else
            {
                CardDisplay childDisplay = cardObj.GetComponentInChildren<CardDisplay>();
                if (childDisplay != null) childDisplay.Setup(card);
            }

            CardMovement movement = cardObj.GetComponent<CardMovement>();
            if (movement != null) Destroy(movement);
        }
    }
}