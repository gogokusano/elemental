using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MapNode : MonoBehaviour
{
    [Header("基本設定")]
    public string sceneName;
    public Button nodeButton;

    // ★インスペクターで空っぽのままでOKになります！
    public Image nodeIcon;

    [Header("進行管理")]
    public List<MapNode> nextNodes;
    public bool isMidBoss;
    public bool isBoss;

    // ★修正：色の設定は不要になったので削除（またはコメントアウト）
    // public Color availableColor = Color.white;
    // public Color lockedColor = Color.gray;

    private void Awake()
    {
        // 自動取得
        if (nodeButton == null) nodeButton = GetComponent<Button>();
        if (nodeIcon == null) nodeIcon = GetComponent<Image>();
    }

    // ★修正：色を一切触らず、Interactableだけを切り替える
    public void SetState(bool canClick)
    {
        if (nodeButton != null) nodeButton.interactable = canClick;
        // ↓↓↓ 色を触る処理をすべて削除 ↓↓↓
        // if (nodeIcon != null) nodeIcon.color = canClick ? Color.white : Color.gray;
    }

    public void OnClickNode()
    {
        if (isMidBoss && MapSlideHandler.Instance != null)
        {
            MapSlideHandler.Instance.StartSlide();
        }

        if (MapManager.Instance != null)
        {
            MapManager.Instance.OpenNextNodesOnly(this);
        }

        if (!string.IsNullOrEmpty(sceneName))
        {
            Debug.Log($"{this.name} を開始: {sceneName} へ遷移します。");
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }
}