using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MapNode : MonoBehaviour
{
    [Header("基本設定")]
    public string sceneName;
    public Button nodeButton;
    public Image nodeIcon;

    [Header("進行管理")]
    public List<MapNode> nextNodes;
    public bool isMidBoss;
    public bool isBoss;

    // ★追加：このマスをランダム化せず、インスペクターの設定で固定にするか
    [Header("特定のイベントに固定する")]
    public bool isFixedNode;

    private void Awake()
    {
        if (nodeButton == null) nodeButton = GetComponent<Button>();
        if (nodeIcon == null) nodeIcon = GetComponent<Image>();
    }

    public void SetState(bool canClick)
    {
        if (nodeButton != null)
        {
            nodeButton.interactable = canClick;
        }

        if (nodeIcon != null)
        {
            Color c = nodeIcon.color;
            if (canClick) c.a = 1.0f;
            else c.a = 0.25f;
            nodeIcon.color = c;
        }
    }

    public void OnClickNode()
    {
        if (isMidBoss && MapSlideHandler.Instance != null)
        {
            MapSlideHandler.Instance.StartSlide();
        }

        // ========================================================
        // ★修正：実際のシーン名「battlescene」に完全一致させました
        // ========================================================
        if (sceneName == "battlescene")
        {
            if (MapManager.Instance != null)
            {
                // バトルシーンの場合は、途中で落としたらリセットするロックをかける
                MapManager.Instance.SaveChallengingNode(this);
            }
        }
        else
        {
            // ■ バトル以外のマス（宝箱、イベントなど）
            // 遷移した時点で即座に「次のマスを開放」してセーブする
            if (MapManager.Instance != null)
            {
                MapManager.Instance.OpenNextNodesOnly(this);
            }
        }

        if (!string.IsNullOrEmpty(sceneName))
        {
            Debug.Log($"{this.name} を開始: {sceneName} へ遷移します。");
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }
}