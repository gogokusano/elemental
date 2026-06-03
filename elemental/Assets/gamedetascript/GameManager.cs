using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("UIパネル")]
    public GameObject victoryPanel;  // 勝利時に表示するPanel
    public GameObject gameOverPanel; // 敗北時に表示するPanel

    void Start()
    {
        // 最初はパネルを隠しておく
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // ==========================================
        // ★追加：【戦闘開始時】の奇物効果（初期ブロック付与など）を一斉発動
        // ==========================================
        if (PlayerDataManager.Instance != null)
        {
            foreach (RelicData relic in PlayerDataManager.Instance.ownedRelics)
            {
                relic.OnBattleStart();
            }
        }
    }

    // ==========================================
    // ★追加：【プレイヤーのターン開始時】の奇物効果（リジェネなど）を発動する関数
    // 毎ターン、カードをドローさせる直前などにこの関数を呼び出してください
    // ==========================================
    public void PlayerTurnStart()
    {
        if (PlayerDataManager.Instance != null)
        {
            foreach (RelicData relic in PlayerDataManager.Instance.ownedRelics)
            {
                relic.OnTurnStart();
            }
        }
    }

    // ==========================================
    // ★追加：【プレイヤーのターン終了時】の奇物効果（ターン終了時ブロックなど）を発動する関数
    // 「ターン終了ボタン」を押したときの処理などにこの関数を呼び出してください
    // ==========================================
    public void PlayerTurnEnd()
    {
        if (PlayerDataManager.Instance != null)
        {
            foreach (RelicData relic in PlayerDataManager.Instance.ownedRelics)
            {
                relic.OnTurnEnd();
            }
        }
    }

    // 勝利した時に呼ばれる
    public void WinGame()
    {
        Debug.Log("勝利！");
        if (victoryPanel != null) victoryPanel.SetActive(true);
    }

    // 敗北した時に呼ばれる
    public void LoseGame()
    {
        Debug.Log("敗北...");
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        // ★追加：完全に敗北が確定したため、データをリセット（最初からやり直し）にする
        PlayerPrefs.DeleteKey("LastClearedNode");
        PlayerPrefs.DeleteKey("CurrentChallengingNode");
        PlayerPrefs.DeleteKey("MapSavedX");
        PlayerPrefs.DeleteKey("MapSeed");
        PlayerPrefs.Save();
    }

    // ボタンから呼ぶ用：タイトル画面に戻る
    public void BackToTitle()
    {
        SceneManager.LoadScene("title");
    }

    // ★重要修正：クリア後のポップアップボタンから呼ばれる関数
    public void ResultOnMap()
    {
        // ==========================================
        // ★追加：この場（GameManager内）で直接セーブデータを書き換え、クリアを確定させます
        // ==========================================
        string challenging = PlayerPrefs.GetString("CurrentChallengingNode", "");

        if (!string.IsNullOrEmpty(challenging))
        {
            // 「挑戦中」だった現在のマスを「クリア済み」に昇格させ、ロックを解除する
            PlayerPrefs.SetString("LastClearedNode", challenging);
            PlayerPrefs.DeleteKey("CurrentChallengingNode");
            PlayerPrefs.Save();

            Debug.Log($"GameManager側で直接クリアを確定しました: {challenging}");
        }
        else
        {
            Debug.LogWarning("挑戦中のマス（CurrentChallengingNode）のデータが見つかりませんでした。");
        }

        // データの書き換えが終わってから安全にマップシーンへ戻る
        SceneManager.LoadScene("Map");
    }
}