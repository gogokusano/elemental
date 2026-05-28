using UnityEngine;
using UnityEngine.SceneManagement; // シーン遷移に必要
using UnityEngine.UI;

public class TitleMenuManager : MonoBehaviour
{
    [Header("Scene Transition")]
    public string gameSceneName = "GameScene"; // 遷移先のゲーム本編シーン名

    [Header("Panels")]
    public GameObject optionsPanel;  // オプション画面
    public GameObject creditsPanel;  // クレジット画面

    [Header("Background Settings")]
    public Image titleBackgroundImage; // タイトルの背景Image
    public Sprite normalSprite;        // 通常時（未クリア）の画像
    public Sprite clearedSprite;       // クリア後の画像

    void Start()
    {
        // 最初はパネルをすべて閉じておく
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        UpdateTitleVisual();
    }

    public void UpdateTitleVisual()
    {
        // "IsCase2Cleared" という名前のフラグを確認する（0なら未クリア、1ならクリア済み）
        int isCleared = PlayerPrefs.GetInt("IsCase2Cleared", 0);

        if (isCleared == 1)
        {
            // クリア済みなら画像を切り替える
            titleBackgroundImage.sprite = clearedSprite;
            Debug.Log("クリア済み背景を表示します");
        }
        else
        {
            // 未クリアなら通常画像
            titleBackgroundImage.sprite = normalSprite;
        }
    }

    // --- ゲームを始める ---
    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    // --- オプションの開閉 ---
    public void OpenOptions()
    {
        optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
    }

    // --- クレジットの開閉 ---
    public void OpenCredits()
    {
        creditsPanel.SetActive(true);
    }

    public void CloseCredits()
    {
        creditsPanel.SetActive(false);
    }

    // --- ゲーム終了 ---
    public void QuitGame()
    {
        Debug.Log("Game Quit!"); // エディタ確認用
        Application.Quit();     // 実際のゲーム終了処理
    }

    //デバッグ用

    public void ResetClearFlag()
    {
        // 1. クリアフラグを未クリア(0)に戻す
        PlayerPrefs.SetInt("IsCase2Cleared", 0);

        // 2. 音量設定をデフォルト(0.50f = 50%)に戻す
        PlayerPrefs.SetFloat("BGM_VAL", 0.50f);
        PlayerPrefs.SetFloat("SE_VAL", 0.50f);

        // 3. 変更したデータを確実に保存する
        PlayerPrefs.Save();

        // 4. タイトルの背景画像を未クリア状態の画像に更新する
        UpdateTitleVisual();

        // 5. AudioManagerが存在していれば、実際の音量も即座にデフォルトに戻す
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetBGMVolume(0.50f);
            AudioManager.instance.SetSEVolume(0.50f);
        }

        Debug.Log("【デバッグ】クリア状況と設定をすべて初期化しました");
    }
}