using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class RewardManager : MonoBehaviour
{
    [Header("パネルの参照")]
    public GameObject rewardListPanel;    // 戦利品パネル（画像2枚目）
    public GameObject cardSelectionPanel; // カード選択パネル（画像1枚目）

    [Header("戦利品パネルのボタン")]
    public Button goldButton;
    public TextMeshProUGUI goldText; // 「○ゴールド」と表示するテキスト
    public Button addCardButton;
    public Button skipListButton; // 右下のスキップボタン

    [Header("カード選択パネルの要素")]
    public CardUI[] cardUIs; // 3つの白いカードオブジェクト
    public Button skipSelectionButton; // 下のスキップボタン

    private int generatedGoldAmount;

    private void Start()
    {
        // 初期状態は両方非表示
        rewardListPanel.SetActive(false);
        cardSelectionPanel.SetActive(false);

        // ボタンに処理を登録
        goldButton.onClick.AddListener(OnGoldClicked);
        addCardButton.onClick.AddListener(OnAddCardClicked);
        skipListButton.onClick.AddListener(EndReward);
        skipSelectionButton.onClick.AddListener(EndReward);
    }

    // 戦闘終了時に他のスクリプトから呼ばれるメソッド
    public void ShowReward()
    {
        rewardListPanel.SetActive(true);
        
        // ゴールドのランダム生成
        generatedGoldAmount = Random.Range(10, 20);
        if (goldText != null) goldText.text = $"{generatedGoldAmount}ゴールド";
        goldButton.interactable = true;
    }

    private void OnGoldClicked()
    {
        PlayerDataManager.Instance.AddGold(generatedGoldAmount);
        goldButton.interactable = false; // 連続で押せないようにする
        if (goldText != null) goldText.text = "獲得済み";
    }

    private void OnAddCardClicked()
    {
        rewardListPanel.SetActive(false);
        cardSelectionPanel.SetActive(true);

        // 3枚のカードデータを取得してUIにセット
        List<CardData> rewards = PlayerDataManager.Instance.GetRewardCards(3);
        
        for (int i = 0; i < cardUIs.Length; i++)
        {
            if (i < rewards.Count)
            {
                cardUIs[i].gameObject.SetActive(true);
                cardUIs[i].SetupCard(rewards[i]);

                // ボタンのクリックイベントを登録
                int index = i; // クロージャ対策
                cardUIs[index].selectButton.onClick.RemoveAllListeners();
                cardUIs[index].selectButton.onClick.AddListener(() => OnCardSelected(rewards[index]));
            }
            else
            {
                cardUIs[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnCardSelected(CardData selectedCard)
    {
        PlayerDataManager.Instance.AddCard(selectedCard);
        EndReward();
    }

    private void EndReward()
    {
        rewardListPanel.SetActive(false);
        cardSelectionPanel.SetActive(false);
        Debug.Log("報酬画面終了。次のシーンへ遷移します。");
        // TODO: ここにマップへ戻る処理などを記述
    }
}