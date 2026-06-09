using UnityEngine;
using TMPro; // TextMeshProを使用するための宣言

public class AlwaysVisibleGoldUI : MonoBehaviour
{
    [Header("常に表示するゴールドのテキスト")]
    public TextMeshProUGUI goldText;

    // 前回のゴールド数を記憶しておく変数（無駄な更新処理を省くため）
    private int lastGold = -1;

    void Update()
    {
        // PlayerDataManagerが存在するかチェック
        if (PlayerDataManager.Instance != null)
        {
            // 現在の所持ゴールドを取得
            int currentGold = PlayerDataManager.Instance.gold;

            // 前回表示した値から変化があった場合のみテキストを更新する
            if (currentGold != lastGold)
            {
                // テキストの更新（例: "150"）
                if (goldText != null)
                {
                    goldText.text = currentGold.ToString();
                }

                // 最新の値を記録
                lastGold = currentGold;
            }
        }
    }
}