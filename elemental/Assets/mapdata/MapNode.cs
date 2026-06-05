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

        // ★判定：遷移先が「バトルシーン」の時だけ、途中で落としたらリセットにする
        if (sceneName == "BattleScene")
        {
            if (MapManager.Instance != null)
            {
                MapManager.Instance.SaveChallengingNode(this);
            }
        }
        else
        {
            // ■ バトル以外のマス（宝箱、イベントなど）
            // 遷移した時点で即座に「クリア扱い（次のマスを開放）」にしてセーブする
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