using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class IngameMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject optionsPanel;

    [Header("Sliders")]
    public Slider bgmSlider;
    public Slider seSlider;

    [Header("Value Texts")]
    public TextMeshProUGUI bgmValueLabel;
    public TextMeshProUGUI seValueLabel;

    [Header("Scene Names")]
    public string titleSceneName = "TitleScene";

    void Start()
    {
        // 最初はパネルを閉じておく
        if (optionsPanel != null) optionsPanel.SetActive(false);
    }

    // ★ 歯車ボタンから呼ばれる：オプションを開く
    public void OpenOptions()
    {
        optionsPanel.SetActive(true);
        
        // パズルや推理が進行しないよう時間を止める（任意）
        // Time.timeScale = 0f; 

        // 保存されている現在の値をスライダーに反映
        float savedBGM = PlayerPrefs.GetFloat("BGM_VAL", 0.75f);
        float savedSE = PlayerPrefs.GetFloat("SE_VAL", 0.75f);

        bgmSlider.value = savedBGM;
        seSlider.value = savedSE;
        UpdateBGMText(savedBGM);
        UpdateSEText(savedSE);

        // スライダー変更イベントを登録
        bgmSlider.onValueChanged.AddListener(OnBGMChanged);
        seSlider.onValueChanged.AddListener(OnSEChanged);
    }

    // ★ ＜ボタン（戻る）から呼ばれる：オプションを閉じる
    public void CloseOptions()
    {
        // Time.timeScale = 1f; // 時間を再開
        optionsPanel.SetActive(false);
        bgmSlider.onValueChanged.RemoveAllListeners();
        seSlider.onValueChanged.RemoveAllListeners();
        PlayerPrefs.Save();
    }

    // ★ タイトルに戻るボタンから呼ばれる
    public void BackToTitle()
    {
        Time.timeScale = 1f; // 必ず時間を動かしてから戻る
        SceneManager.LoadScene(titleSceneName);
    }

    // --- 音量更新ロジック（AudioManager経由） ---
    void OnBGMChanged(float value)
    {
        if (AudioManager.instance != null) AudioManager.instance.SetBGMVolume(value);
        UpdateBGMText(value);
    }

    void OnSEChanged(float value)
    {
        if (AudioManager.instance != null) AudioManager.instance.SetSEVolume(value);
        UpdateSEText(value);
    }

    void UpdateBGMText(float value)
    {
        bgmValueLabel.text = Mathf.RoundToInt(value * 100f).ToString();
    }

    void UpdateSEText(float value)
    {
        seValueLabel.text = Mathf.RoundToInt(value * 100f).ToString();
    }

    // ★追加：音量設定をデフォルトにリセットする処理
    public void ResetAudioSettings()
    {
        // タイトル画面と全く同じ仕組みでリセット
        bgmSlider.value = 0.50f;
        seSlider.value = 0.50f;

        Debug.Log("【インゲームオプション】音量をデフォルトにリセットしました");
    }
}