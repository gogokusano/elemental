using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI; 
using System.Collections.Generic; 

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

    // ==========================================
    // ★ターン切り替えのテキスト演出設定
    // ==========================================
    [Header("ターン開始エフェクト設定")]
    public DamageText damageTextPrefab; // 画面に流したいDamageTextのプレハブ
    public Transform effectSpawnTarget; // テキストを出したい親Canvas、またはバトル画面の中心オブジェクト

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
        // ★修正：プレイヤーのターン開始演出は「黄色（Color.yellow）」で出現
        SpawnTurnNotification("プレイヤーのターン", Color.yellow);

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

    // 敵のターンが始まる時に呼び出す関数
    public void EnemyTurnStart()
    {
        // ★修正：敵のターン開始演出は「赤色（Color.red）」で出現
        SpawnTurnNotification("敵のターン", Color.red);
    }

    /// <summary>
    /// 画面中央に文字アニメーションテキストを生成する共通関数
    /// </summary>
    private void SpawnTurnNotification(string msg, Color textColor)
    {
        if (damageTextPrefab != null && effectSpawnTarget != null)
        {
            DamageText textObj = Instantiate(damageTextPrefab, effectSpawnTarget);
            textObj.transform.SetAsLastSibling(); // UIの最前面に表示する
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            if (textRect != null) 
            {
                // 画面の中央（0, 0）に配置
                textRect.anchoredPosition = Vector2.zero; 
            }
            
            // 文字列モードのSetupを呼び出す
            textObj.SetupText(msg, textColor);
        }
    }

    public void WinGame()
    {
        Debug.Log("勝利！");
        if (victoryPanel != null) victoryPanel.SetActive(true);
    }

    public void LoseGame()
    {
        Debug.Log("敗北...");
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        SetupGameOverRibbon();
        
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.ResetAllData();
        }

        PlayerPrefs.DeleteKey("LastClearedNode");
        PlayerPrefs.DeleteKey("CurrentChallengingNode");
        PlayerPrefs.DeleteKey("MapSavedX");
        PlayerPrefs.DeleteKey("MapSeed");
        PlayerPrefs.Save();
    }

    private void SetupGameOverRibbon()
    {
        if (ribbonImage == null) return;

        bool isBossApproach = CheckIfBossApproach();

        if (isBossApproach)
        {
            if (bossApproachSprite != null)
            {
                ribbonImage.sprite = bossApproachSprite;
            }
        }
        else
        {
            if (normalGameOverSprites != null && normalGameOverSprites.Count > 0)
            {
                int randomIndex = Random.Range(0, normalGameOverSprites.Count);
                ribbonImage.sprite = normalGameOverSprites[randomIndex];
            }
        }
    }

    private bool CheckIfBossApproach()
    {
        return false;
    }

    public void BackToTitle()
    {
        SceneManager.LoadScene("title");
    }

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