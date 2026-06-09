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
    public CanvasGroup resultCanvasGroup; // ゆっくり暗転・フェードインさせるため

    [Header("テキスト設定")]
    public TextMeshProUGUI clearTimeText;

    [Header("一覧表示用コンテナ（Scroll ViewのContentなど）")]
    public Transform relicContainer;
    public Transform cardContainer;

    [Header("表示用プレハブ")]
    public GameObject relicIconPrefab; // 奇物の簡易アイコンUI
    public GameObject cardPrefab;      // DeckManagerで使っているcardPrefabと同じ

    [Header("演出設定")]
    public float fadeDuration = 2.0f;  // 暗転にかける時間（秒）

    private float battleStartTime;
    private bool isGameCleared = false;

    void Start()
    {
        // バトル開始時刻を記録（クリアタイム計算用）
        battleStartTime = Time.time;

        if (resultPanel != null) resultPanel.SetActive(false);
        if (resultCanvasGroup != null) resultCanvasGroup.alpha = 0f;
    }

    /// <summary>
    /// ボスを倒した瞬間にEnemyManager等から呼び出す関数
    /// </summary>
    public void PlayBossClearResult()
    {
        if (isGameCleared) return;
        isGameCleared = true;

        // リザルト表示処理を開始
        StartCoroutine(ResultPresentationRoutine());
    }

    private IEnumerator ResultPresentationRoutine()
    {
        // 1. ボスが消滅した余韻のための短いウェイト
        yield return new WaitForSeconds(0.8f);

        if (resultPanel != null) resultPanel.SetActive(true);

        // 2. ゆっくり画面を暗転・フェードイン（CanvasGroupのAlphaを0から1へ）
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

        // 3. クリアタイムの計算と表示
        float totalTime = Time.time - battleStartTime;
        int minutes = Mathf.FloorToInt(totalTime / 60f);
        int seconds = Mathf.FloorToInt(totalTime % 60f);
        if (clearTimeText != null)
        {
            clearTimeText.text = $"クリアタイム: {minutes:00}:{seconds:00}";
        }

        // 4. 獲得した奇物の一覧を表示
        PopulateRelics();

        // 5. デッキ（カード）の一覧を表示
        PopulateCards();
    }

    private void PopulateRelics()
    {
        if (relicContainer == null || relicIconPrefab == null || PlayerDataManager.Instance == null) return;

        // 古い表示をクリア
        foreach (Transform child in relicContainer) Destroy(child.gameObject);

        // 所持している奇物を一覧表示
        foreach (RelicData relic in PlayerDataManager.Instance.ownedRelics)
        {
            if (relic == null) continue;

            // 生成先を正しくコンテナに指定し、位置とスケールを初期化
            GameObject iconObj = Instantiate(relicIconPrefab, relicContainer);
            iconObj.transform.localPosition = Vector3.zero;
            iconObj.transform.localScale = Vector3.one;

            Image img = iconObj.GetComponent<Image>();
            if (img != null)
            {
                // RelicDataで定義されている「relicIcon」を正しく適用
                img.sprite = relic.relicIcon; 
            }
        }
    }

    private void PopulateCards()
    {
        if (cardContainer == null || cardPrefab == null || PlayerDataManager.Instance == null) return;

        // 古い表示をクリア
        foreach (Transform child in cardContainer) Destroy(child.gameObject);

        // デッキ内のカードを一覧表示
        foreach (CardData card in PlayerDataManager.Instance.deckCards)
        {
            if (card == null) continue;

            // 生成先を正しくコンテナに指定し、位置とスケールを初期化（重なりバグ修正）
            GameObject cardObj = Instantiate(cardPrefab, cardContainer);
            cardObj.transform.localPosition = Vector3.zero;
            cardObj.transform.localScale = Vector3.one;

            CardDisplay display = cardObj.GetComponent<CardDisplay>();
            if (display != null)
            {
                display.Setup(card); // 1枚ずつ別のカードになるようデータをセット
            }

            // リザルト画面でカードが動いたりドラッグできたりしないよう、不要なスクリプトを削除
            CardMovement movement = cardObj.GetComponent<CardMovement>();
            if (movement != null) Destroy(movement);
        }
    }

    /// <summary>
    /// メインメニューボタンに割り当てる関数
    /// </summary>
    public void OnMainMenuButtonClicked()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.ResetAllData();
        }

        PlayerPrefs.DeleteKey("LastClearedNode");
        PlayerPrefs.DeleteKey("CurrentChallengingNode");
        PlayerPrefs.DeleteKey("MapSavedX");
        PlayerPrefs.DeleteKey("MapSeed");
        PlayerPrefs.Save();

        // タイトルシーン（title）へ遷移
        SceneManager.LoadScene("title");
    }
}