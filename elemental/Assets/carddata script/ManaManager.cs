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

    // コストを回復させる
    public void ResetMana()
    {
        currentMana = maxMana;

        // ==========================================
        // ★正しい復旧：金縛り（Paralysis）のデバフ処理
        // ==========================================
        if (PlayerDebuffManager.Instance != null)
        {
            // PlayerDebuffManagerから金縛りのマイナス値を取得する（例：1設定なら -1 が返ってくる）
            int penalty = PlayerDebuffManager.Instance.GetParalysisManaPenalty();
            
            if (penalty < 0) // ペナルティが発生している場合
            {
                currentMana += penalty;
                if (currentMana < 0) currentMana = 0; // マナがマイナスにならないよう防御
                Debug.Log($"<color=purple>金縛りにより、マナが最大まで回復しなかった！</color>");
            }
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