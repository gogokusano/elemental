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
    }

    // ボタンから呼ぶ用：タイトル画面に戻る
    public void BackToTitle()
    {
        // 今はとりあえず今のシーンをリロード（やり直し）にします
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}