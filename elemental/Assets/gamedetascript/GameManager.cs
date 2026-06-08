using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI; // ★追加：Imageコンポーネントを扱うために必要です
using System.Collections.Generic; // ★追加：List（画像リスト）を扱うために必要です

public class GameManager : MonoBehaviour
{
    [Header("UIパネル")]
    public GameObject victoryPanel;  // 勝利時に表示するPanel
    public GameObject gameOverPanel; // 敗北時に表示するPanel

    [Header("▼ ゲームオーバー画面のリボン設定")]
    public Image ribbonImage; // リボンを表示しているUIのImageコンポーネント

    [Header("ボス直前で死んだ時の悔しいリボン画像（1種類）")]
    public Sprite bossApproachSprite;

    [Header("道中で死んだ時のリボン画像（3種類登録してください）")]
    public List<Sprite> normalGameOverSprites = new List<Sprite>();

    void Start()
    {
        // 最初はパネルを隠しておく
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

        // ★追加：敗北画面が表示された瞬間にリボン画像を進行度に合わせて切り替える
        SetupGameOverRibbon();
        
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.ResetAllData();
        }

        // 完全に敗北が確定したため、データをリセット（最初からやり直し）にする
        PlayerPrefs.DeleteKey("LastClearedNode");
        PlayerPrefs.DeleteKey("CurrentChallengingNode");
        PlayerPrefs.DeleteKey("MapSavedX");
        PlayerPrefs.DeleteKey("MapSeed");
        PlayerPrefs.Save();
    }

    /// <summary>
    /// ★追加：進行度に応じてゲームオーバーのリボン画像をセットする
    /// </summary>
    private void SetupGameOverRibbon()
    {
        if (ribbonImage == null) return;

        // ボス直前かどうかのチェック（下の窓を呼び出す）
        bool isBossApproach = CheckIfBossApproach();

        if (isBossApproach)
        {
            // ボス直前なら、固定の悔しいリボンにする
            if (bossApproachSprite != null)
            {
                ribbonImage.sprite = bossApproachSprite;
            }
        }
        else
        {
            // 道中なら、登録された3種類の中からランダムに選ぶ
            if (normalGameOverSprites != null && normalGameOverSprites.Count > 0)
            {
                int randomIndex = Random.Range(0, normalGameOverSprites.Count);
                ribbonImage.sprite = normalGameOverSprites[randomIndex];
            }
        }
    }

    /// <summary>
    /// ★ここが将来用の「窓」です！
    /// 将来ボスが実装されたら、ここの条件を書き換えます。
    /// </summary>
    private bool CheckIfBossApproach()
    {
        // 【現状】ボスが未実装なので、ひとまず常に「false（道中）」にしておきます。
        // 💡テストしたい時は、ここを「return true;」に書き換えると、ボス直前のリボンに固定してテストできます！
        return false;

        // 【将来、マップやステージ進行が完成した時の合流イメージ】
        // if (StageManager.Instance.currentFloor == 10) // 例えば10階がボス部屋なら
        // {
        //     return true;
        // }
        // return false;
    }

    // ボタンから呼ぶ用：タイトル画面に戻る
    public void BackToTitle()
    {
        SceneManager.LoadScene("title");
    }

    // クリア後のポップアップボタンから呼ばれる関数
    public void ResultOnMap()
    {
        string challenging = PlayerPrefs.GetString("CurrentChallengingNode", "");

        if (!string.IsNullOrEmpty(challenging))
        {
            PlayerPrefs.SetString("LastClearedNode", challenging);
            PlayerPrefs.DeleteKey("CurrentChallengingNode");
            PlayerPrefs.Save();

            Debug.Log($"GameManager側で直接クリアを確定しました: {challenging}");
        }
        else
        {
            Debug.LogWarning("挑戦中のマス（CurrentChallengingNode）のデータが見つかりませんでした。");
        }

        SceneManager.LoadScene("Map");
    }
}