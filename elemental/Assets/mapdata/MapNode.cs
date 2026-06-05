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

        // ========================================================
        // ★修正：戦闘マスかどうかの安全な自動判定
        // 中ボス・ボスフラグがあるか、シーン名に「scene」が入っていれば戦闘とみなす
        // (インスペクターでシーン名が空っぽの中ボスでも正しく判定されます)
        // ========================================================
        string lowerScene = !string.IsNullOrEmpty(sceneName) ? sceneName.ToLower() : "";
        bool isBattleNode = isMidBoss || isBoss || lowerScene.Contains("scene") || lowerScene.Contains("battle");

        if (isBattleNode)
        {
            if (MapManager.Instance != null)
            {
                MapManager.Instance.SaveChallengingNode(this);
            }
        }
        else
        {
            // ■ バトル以外のマス（宝箱、イベント、ショップなど）
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
        else if (isBattleNode)
        {
            // 【デバッグ用】中ボスのシーン名が空の場合、即座にクリア扱いにしてマップをリフレッシュする
            Debug.LogWarning($"{this.name} のシーン名が空です。デバッグ用即時クリアを処理します。");
            if (MapManager.Instance != null)
            {
                MapManager.Instance.ClearCurrentNode();
                MapManager.Instance.RefreshMap();
            }
        }
    }
}