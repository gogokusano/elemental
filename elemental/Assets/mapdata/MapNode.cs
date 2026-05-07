using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MapNode : MonoBehaviour
{
    public string sceneName;
    public Button nodeButton;

    [Header("このマスをクリアした時に開放されるマス")]
    public List<MapNode> nextNodes;

    public void SetInteractable(bool canClick)
    {
        nodeButton.interactable = canClick;
    }

    public void OnNodeClick()
    {
        // 他の担当者へ：このID（名前）をクリア済みとして保存する処理
        PlayerPrefs.SetString("LastClearedNode", this.name);
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}