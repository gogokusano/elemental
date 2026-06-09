using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TitleReturner : MonoBehaviour
{
    [Header("フェードさせる真っ黒な画面")]
    public CanvasGroup blackFadePanel; 
    public float fadeTime = 1.0f;

    private bool isFading = false;

    // ボタンにセットする関数
    public void GoToTitle()
    {
        if (isFading) return; // 連打防止
        isFading = true;
        StartCoroutine(FadeAndLoad());
    }

    private IEnumerator FadeAndLoad()
    {
        if (blackFadePanel != null) 
        {
            // ⚠️【今回の修正ポイント】
            // オブジェクト自体が非アクティブ（グレーアウト）になっていたら、ここで強制的にONにする
            blackFadePanel.gameObject.SetActive(true);
            blackFadePanel.blocksRaycasts = true; // 他のUIを触らせないようにブロック
        }

        // 画面を0（透明）から1（真っ黒）へフェード
        float timer = 0f;
        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            if (blackFadePanel != null) blackFadePanel.alpha = timer / fadeTime;
            yield return null;
        }

        if (blackFadePanel != null) blackFadePanel.alpha = 1f;

        // タイトルへ戻る前のデータリセット（安全のためエラー回避付き）
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
            Debug.LogWarning("データリセットをスキップしてシーン遷移します: " + e.Message);
        }

        // シーン移動
        SceneManager.LoadScene("title");
    }
}