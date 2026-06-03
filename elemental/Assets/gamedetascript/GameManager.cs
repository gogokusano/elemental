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
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        if (PlayerDataManager.Instance != null)
        {
            foreach (RelicData relic in PlayerDataManager.Instance.ownedRelics)
            {
                relic.OnBattleStart();
            }
        }
    }

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

        // ★追加：敗北が確定した時点で、ローグライクなのでセーブデータを全消去する
        if (MapManager.Instance != null)
        {
            MapManager.Instance.ResetProgress();
        }
        else
        {
            PlayerPrefs.DeleteKey("LastClearedNode");
            PlayerPrefs.DeleteKey("CurrentChallengingNode");
            PlayerPrefs.DeleteKey("MapSavedX");
            PlayerPrefs.DeleteKey("MapSeed");
            PlayerPrefs.Save();
        }
    }

    // ボタンから呼ぶ用：タイトル画面に戻る（※敗北パネルのボタンなど用）
    public void BackToTitle()
    {
        // 念のためここでもデータを綺麗にしておく（シーン名がプロジェクトと合っているか確認してください）
        SceneManager.LoadScene("title");
    }

    // ★重要：勝利後に「マップに戻る」ボタンから呼び出す関数
    public void ResultOnMap()
    {
        // ★追加：マップの管理者に「無事クリアしたよ！」と伝えてセーブデータを昇格させる
        if (MapManager.Instance != null)
        {
            MapManager.Instance.ClearCurrentNode();
        }
        else
        {
            // 万が一MapManagerがいなくても、直接データを書き換えて安全を確保する
            string challenging = PlayerPrefs.GetString("CurrentChallengingNode", "");
            if (!string.IsNullOrEmpty(challenging))
            {
                PlayerPrefs.SetString("LastClearedNode", challenging);
                PlayerPrefs.DeleteKey("CurrentChallengingNode");
                PlayerPrefs.Save();
            }
        }

        // マップシーンへ戻る（※シーン名が「Map」で合っているか確認してください）
        SceneManager.LoadScene("map");
    }
}