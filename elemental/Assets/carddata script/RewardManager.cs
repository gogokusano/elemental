using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class RewardManager : MonoBehaviour
{
    [Header("パネルの参照")]
    public GameObject rewardListPanel;    
    public GameObject cardSelectionPanel; 

    [Header("戦利品パネルのボタン")]
    public Button goldButton;
    public TextMeshProUGUI goldText; 
    public Button addCardButton;
    public Button skipListButton; 

    // ★追加：レリック報酬用のUI要素
    [Header("戦利品パネル（奇物追加分）")]
    public Button relicButton;       // レリック獲得用のボタン
    public TextMeshProUGUI relicText;// 「奇物名」を表示するテキスト

    [Header("カード選択パネルの要素")]
    public CardUI[] cardUIs; 
    public Button skipSelectionButton; 

    private int generatedGoldAmount;
    private RelicData generatedRelicData; // 今回ランダム選出された奇物

    private void Start()
    {
        rewardListPanel.SetActive(false);
        cardSelectionPanel.SetActive(false);

        goldButton.onClick.AddListener(OnGoldClicked);
        addCardButton.onClick.AddListener(OnAddCardClicked);
        skipListButton.onClick.AddListener(EndReward);
        skipSelectionButton.onClick.AddListener(EndReward);

        // ★追加：レリックボタンのイベント登録
        if (relicButton != null)
        {
            relicButton.onClick.AddListener(OnRelicClicked);
        }
    }

    // ★修正：中ボス戦だったかどうか（isMidBoss）を受け取る
    public void ShowReward(bool isMidBoss = false)
    {
        rewardListPanel.SetActive(true);
        
        generatedGoldAmount = Random.Range(10, 20);
        if (goldText != null) goldText.text = $"{generatedGoldAmount}ゴールド";
        goldButton.interactable = true;

        // ★追加：中ボス戦の時だけ奇物を報酬に出す！
        if (isMidBoss && PlayerDataManager.Instance != null && relicButton != null && relicText != null)
        {
            relicButton.gameObject.SetActive(true);

            // 未所持の奇物をランダムに1つ抽選する
            generatedRelicData = PlayerDataManager.Instance.GetRandomRewardRelic();

            if (generatedRelicData != null)
            {
                relicText.text = $"奇物: {generatedRelicData.relicName}"; 
                relicButton.interactable = true;
            }
            else
            {
                relicText.text = "獲得可能な奇物がありません";
                relicButton.interactable = false; // 全ての奇物を取り尽くした場合
            }
        }
        else if (relicButton != null)
        {
            // 中ボス戦以外（通常戦闘）なら奇物ボタンを非表示にする
            relicButton.gameObject.SetActive(false);
        }
    }

    private void OnGoldClicked()
    {
        PlayerDataManager.Instance.AddGold(generatedGoldAmount);
        goldButton.interactable = false; 
        if (goldText != null) goldText.text = "獲得済み";
    }

    // ★追加：奇物ボタンがクリックされた時の処理
    private void OnRelicClicked()
    {
        if (PlayerDataManager.Instance != null && generatedRelicData != null)
        {
            // PlayerDataManagerの既存の関数を使って奇物を追加！
            PlayerDataManager.Instance.AddRelic(generatedRelicData);
            
            relicButton.interactable = false; 
            if (relicText != null) relicText.text = "獲得済み";
        }
    }

    private void OnAddCardClicked()
    {
        rewardListPanel.SetActive(false);
        cardSelectionPanel.SetActive(true);

        List<CardData> rewards = PlayerDataManager.Instance.GetRewardCards(3);
        
        for (int i = 0; i < cardUIs.Length; i++)
        {
            if (i < rewards.Count)
            {
                cardUIs[i].gameObject.SetActive(true);
                cardUIs[i].SetupCard(rewards[i]);

                int index = i; 
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
    }
}