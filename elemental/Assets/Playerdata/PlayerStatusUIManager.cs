using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStatusUIManager : MonoBehaviour
{
    // どこからでもアクセスできるシングルトン
    public static PlayerStatusUIManager Instance { get; private set; }

    [Header("UIパネルの参照")]
    public GameObject statusPanel;      // 開閉するステータス画面全体
    public TextMeshProUGUI hpText;      // HP表示用テキスト
    public TextMeshProUGUI goldText;    // (おまけ)所持金表示用テキスト

    [Header("カードリスト表示用")]
    public Transform cardListContent;   // カードを並べる親オブジェクト (Content)
    public GameObject cardIconPrefab;   // カード表示用のPrefab（テキストや画像）

    [Header("奇物リスト表示用")]
    public Transform relicListContent;  // 奇物を並べる親オブジェクト (Content)
    public GameObject relicIconPrefab;  // 奇物表示用のPrefab（アイコン画像など）

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // このスクリプトがついているオブジェクト（UIキャンバス）をシーン遷移で壊さない
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 最初はパネルを閉じておく
        if (statusPanel != null)
        {
            statusPanel.SetActive(false);
        }
    }

    // ★画面上の「ステータス確認ボタン」から呼び出すメソッド
    public void ToggleStatusPanel()
    {
        if (statusPanel == null) return;

        bool isActive = statusPanel.activeSelf;
        
        // パネルを開く瞬間に、最新のデータを読み込んでUIを更新する
        if (!isActive) 
        {
            UpdateStatusUI();
        }
        
        statusPanel.SetActive(!isActive);
    }

    // プレイヤーデータを読み込んで画面に反映させる
    private void UpdateStatusUI()
    {
        if (PlayerDataManager.Instance == null) return;

        // 1. 基本ステータスの更新
        if (hpText != null) 
            hpText.text = $"HP: {PlayerDataManager.Instance.currentHp} / {PlayerDataManager.Instance.maxHp}";
        if (goldText != null) 
            goldText.text = $"Gold: {PlayerDataManager.Instance.gold}";

        // 2. 所持カードリストの更新
        ClearChildren(cardListContent);
        foreach (var card in PlayerDataManager.Instance.deckCards)
        {
            GameObject icon = Instantiate(cardIconPrefab, cardListContent);
            
            // 例: Prefabの子オブジェクトにあるTextMeshProUGUIにカード名を入れる
            TextMeshProUGUI text = icon.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null) text.text = card.cardName;
        }

        // 3. 所持奇物リストの更新
        ClearChildren(relicListContent);
        foreach (var relic in PlayerDataManager.Instance.ownedRelics)
        {
            GameObject icon = Instantiate(relicIconPrefab, relicListContent);
            
            // 例: Prefab自体についているImageコンポーネントにアイコンを入れる
            Image img = icon.GetComponent<Image>();
            if (img != null && relic.relicIcon != null) img.sprite = relic.relicIcon;
        }
    }

    // リストの中身を一度リセットするための便利関数
    private void ClearChildren(Transform parent)
    {
        if (parent == null) return;
        
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
    }
}