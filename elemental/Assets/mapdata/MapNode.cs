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
        // 中ボスにチェックが入っている場合、スライドを開始
        if (isMidBoss)
        {
            MapSlideHandler.Instance.StartSlide();
        }
        // 1. このマスの名前を保存
        PlayerPrefs.SetString("LastClearedNode", this.name);
        PlayerPrefs.Save(); // 確実に保存

        // 2. シーン遷移判定
        if (!string.IsNullOrEmpty(sceneName))
        {
            Debug.Log($"{this.name} を開始: {sceneName} へ遷移します。");
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
        else
        {
            // デバッグ用：シーン名がなければ即座にマップを再読み込みして「次」を開放
            Debug.Log($"{this.name} はシーン名がないため、即時クリア扱いとしてマップを更新します。");
            // 現在のマップシーンを再読み込み（MapManagerが再判定してくれる）
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
}