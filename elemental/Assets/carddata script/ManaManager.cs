using UnityEngine;
using TMPro;

public class ManaManager : MonoBehaviour
{
    [Header("コスト設定")]
    public int maxMana = 5;
    public int currentMana;

    [Header("UI設定")]
    public TextMeshProUGUI manaText;

    void Start()
    {
        ResetMana();
    }

    // コストを全回復させる
    public void ResetMana()
    {
        currentMana = maxMana;

        // ==========================================
        // ★デバフ処理：金縛りにかかっていたら回復マナを減らす
        // ==========================================
        if (PlayerDebuffManager.Instance != null)
        {
            // 金縛りのペナルティ（例：-2）を取得して加算する
            int penalty = PlayerDebuffManager.Instance.GetParalysisManaPenalty();
            currentMana += penalty;

            if (currentMana < 0) currentMana = 0; // マナがマイナスにならないよう防御
        }

        UpdateManaUI();
    }

    public bool TryConsumeMana(int cost)
    {
        if (currentMana >= cost)
        {
            currentMana -= cost;
            UpdateManaUI();
            return true;
        }
        return false;
    }

    public void UpdateManaUI()
    {
        if (manaText != null)
        {
            manaText.text = currentMana + " / " + maxMana;
        }
    }
}