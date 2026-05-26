using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshProを使うために必要

public class OptionsManager : MonoBehaviour
{
    [Header("Sliders")]
    public Slider bgmSlider;
    public Slider seSlider;

    [Header("Value Texts")]
    public TextMeshProUGUI bgmValueLabel;
    public TextMeshProUGUI seValueLabel;

    void OnEnable()
    {
        // 保存されている値をスライダーに反映
        float savedBGM = PlayerPrefs.GetFloat("BGM_VAL", 0.75f);
        float savedSE = PlayerPrefs.GetFloat("SE_VAL", 0.75f);

        bgmSlider.value = savedBGM;
        seSlider.value = savedSE;

        // 初期表示の更新
        UpdateBGMText(savedBGM);
        UpdateSEText(savedSE);

        // スライダー変更時のイベント登録
        bgmSlider.onValueChanged.AddListener(OnBGMChanged);
        seSlider.onValueChanged.AddListener(OnSEChanged);
    }

    // BGMスライダーが動いたとき
    void OnBGMChanged(float value)
    {
        AudioManager.instance.SetBGMVolume(value);
        UpdateBGMText(value);
    }

    // SEスライダーが動いたとき
    void OnSEChanged(float value)
    {
        AudioManager.instance.SetSEVolume(value);
        UpdateSEText(value);
    }

    // テキストを 0-100 に変換して表示
    void UpdateBGMText(float value)
    {
        // 0.0〜1.0 を 0〜100 にして四捨五入（intにキャスト）
        int percentage = Mathf.RoundToInt(value * 100f);
        bgmValueLabel.text = percentage.ToString();
    }

    void UpdateSEText(float value)
    {
        int percentage = Mathf.RoundToInt(value * 100f);
        seValueLabel.text = percentage.ToString();
    }

    void OnDisable()
    {
        bgmSlider.onValueChanged.RemoveAllListeners();
        seSlider.onValueChanged.RemoveAllListeners();
        PlayerPrefs.Save();
    }

    public void ResetAudioSettings()
    {
        // スライダーの値を0.50(50%)に戻す。
        // これにより自動でOnBGMChangedとOnSEChangedが呼ばれ、
        // 実際の音量、テキスト表示、保存データも連動して更新されます。
        bgmSlider.value = 0.50f;
        seSlider.value = 0.50f;
        
        Debug.Log("【オプション】音量をデフォルトにリセットしました");
    }
}