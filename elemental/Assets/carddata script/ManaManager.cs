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