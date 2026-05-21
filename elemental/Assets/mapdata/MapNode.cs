using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MapNode : MonoBehaviour
{
    [Header("基本設定")]
    public string sceneName;       // 遷移先のシーン名（空ならデバッグクリア）
    public Button nodeButton;
    public Image nodeIcon;

    [Header("進行管理")]
    public List<MapNode> nextNodes;
    public bool isMidBoss;
    public bool isBoss;

    [Header("色の設定")]
    public Color availableColor = Color.white;
    public Color lockedColor = Color.gray;

    public void SetState(bool canClick)
    {
        if (nodeButton != null) nodeButton.interactable = canClick;
        if (nodeIcon != null) nodeIcon.color = canClick ? availableColor : lockedColor;
    }

    public void OnClickNode()
    {
        // 1. 中ボスならスライドを開始
        if (isMidBoss)
        {
            if (MapSlideHandler.Instance != null)
            {
                MapSlideHandler.Instance.StartSlide();
            }
        }

        // 2. 管理者にセーブと次のマスの開放を命令
        if (MapManager.Instance != null)
        {
            MapManager.Instance.OpenNextNodesOnly(this);
        }

        // 3. シーン遷移判定
        if (!string.IsNullOrEmpty(sceneName))
        {
            Debug.Log($"{this.name} を開始: {sceneName} へ遷移します。");
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.Log($"{this.name} はシーン名がないため、その場で進捗を保存して次のルートのみを開放しました。");
        }
    }
}