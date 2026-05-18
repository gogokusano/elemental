using UnityEngine;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    [Header("全てのマス（ヒエラルキーから全て入れる）")]
    public List<MapNode> allNodes;

    [Header("最初の3マス")]
    public List<MapNode> startNodes;

    void Start()
    {
        RefreshMap();
    }

    public void RefreshMap()
    {
        // 最後にクリアしたマスの名前を取得
        string lastCleared = PlayerPrefs.GetString("LastClearedNode", "");

        // 1. 一旦すべてのボタンをロック
        foreach (var node in allNodes)
        {
            node.SetState(false);
        }

        if (string.IsNullOrEmpty(lastCleared))
        {
            // 初回：スタートの3マスを有効化
            foreach (var st in startNodes) st.SetState(true);
        }
        else
        {
            // 前回クリアしたマスの「次」を有効化
            MapNode lastNode = allNodes.Find(x => x.name == lastCleared);
            if (lastNode != null)
            {
                foreach (var next in lastNode.nextNodes)
                {
                    next.SetState(true);
                }
            }
            else
            {
                // 万が一見つからない（セーブデータが古い等）場合はリセット
                foreach (var st in startNodes) st.SetState(true);
            }
        }
    }

    // デバッグ用：進捗リセットボタンから呼ぶ用
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("LastClearedNode");
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}