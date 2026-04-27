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

    // ボタンから呼ぶ用：タイトル画面に戻る（後でシーン名を入れる）
    public void BackToTitle()
    {
        // タイトルシーンができたら、ここの名前を変えるだけ
        // SceneManager.LoadScene("TitleScene");
        
        // 今はとりあえず今のシーンをリロード（やり直し）にします
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}