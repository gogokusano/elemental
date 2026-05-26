using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public AudioMixer audioMixer;

    void Awake()
    {
        // シーンをまたいでもこのオブジェクトを消さない（シングルトン）
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 保存されている設定を読み込んで反映
        LoadSettings();
    }

    public void SetBGMVolume(float volume)
    {
        // スライダーの0~1をデシベル（-80~0）に変換
        float dB = volume <= 0 ? -80f : Mathf.Log10(volume) * 20f;
        audioMixer.SetFloat("BGMVolume", dB);
        PlayerPrefs.SetFloat("BGM_VAL", volume); // 保存
    }

    public void SetSEVolume(float volume)
    {
        float dB = volume <= 0 ? -80f : Mathf.Log10(volume) * 20f;
        audioMixer.SetFloat("SEVolume", dB);
        PlayerPrefs.SetFloat("SE_VAL", volume); // 保存
    }

    private void LoadSettings()
    {
        // 保存データがなければデフォルトは 0.5 (50%)
        float bgm = PlayerPrefs.GetFloat("BGM_VAL", 0.50f);
        float se = PlayerPrefs.GetFloat("SE_VAL", 0.50f);

        // 読み込み時に即座に反映
        SetBGMVolume(bgm);
        SetSEVolume(se);
    }
}