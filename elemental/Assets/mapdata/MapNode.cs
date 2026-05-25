using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MapNode : MonoBehaviour
{
    // マスの種類を定義
    public enum NodeType
    {
        Battle,    // 通常戦闘（剣のマーク）
        Event,     // イベント（はてなマーク）
        Treasure,  // 宝箱（宝箱のマーク）
        Safety,    // 休憩所（焚き火のマーク）
        MidBoss,   // 中ボス（ピンクのドクロ）
        Boss       // ラスボス
    }

    [Header("現在のマスの種類")]
    public NodeType myType;

    [Header("基本設定")]
    public string sceneName;
    public Button nodeButton;
    public Image nodeIcon;

    [Header("進行管理")]
    public List<MapNode> nextNodes;
    public bool isMidBoss;         // インスペクターで手動設定してもいいし、myTypeから自動判定でもOK
    public bool isBoss;

    [Header("色の設定")]
    public Color availableColor = Color.white;
    public Color lockedColor = Color.gray;

    // ★追加：このマスのタイプ（中身）を外から変更する関数
    public void SetupNode(NodeType type, Sprite iconSprite, string targetScene)
    {
        myType = type;
        if (nodeIcon != null) nodeIcon.sprite = iconSprite;
        sceneName = targetScene;

        // ボス系のフラグもタイプに合わせて自動同期
        isMidBoss = (type == NodeType.MidBoss);
        isBoss = (type == NodeType.Boss);
    }

    public void SetState(bool canClick)
    {
        if (nodeButton != null) nodeButton.interactable = canClick;
        if (nodeIcon != null) nodeIcon.color = canClick ? availableColor : lockedColor;
    }

    public void OnClickNode()
    {
        if (isMidBoss)
        {
            if (MapSlideHandler.Instance != null) MapSlideHandler.Instance.StartSlide();
        }

        if (MapManager.Instance != null) MapManager.Instance.OpenNextNodesOnly(this);

        if (!string.IsNullOrEmpty(sceneName))
        {
            Debug.Log($"{this.name} ({myType}) を開始: {sceneName} へ遷移します。");
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.Log($"{this.name} はシーン名がないため、デバッグ用としてその場で進めました。");
        }
    }
}