using UnityEngine;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    [Header("全てのマス（ヒエラルキーから全て入れる）")]
    public List<MapNode> allNodes;

    [Header("最初の3マス")]
    public List<MapNode> startNodes;

    void Awake() => Instance = this;

    void Start()
    {
        RefreshMap();
    }

    // セーブデータを読み込んでマップの状態を復元する
    public void RefreshMap()
    {
        // 1. まず全てのマスを一旦非アクティブ（ロック）にする
        foreach (var node in allNodes)
        {
            node.SetState(false);
        }

        // 2. セーブデータ（最後にクリアしたマスの名前）を取得
        string lastCleared = PlayerPrefs.GetString("LastClearedNode", "");

        if (string.IsNullOrEmpty(lastCleared))
        {
            // セーブデータがない場合：初期状態（最初の3マスを有効化）
            foreach (var st in startNodes)
            {
                st.SetState(true);
            }
        }
        else
        {
            // セーブデータがある場合：前回クリアしたマスの「次のマス」だけを有効化
            MapNode lastNode = allNodes.Find(x => x.name == lastCleared);
            if (lastNode != null)
            {
                foreach (var next in lastNode.nextNodes)
                {
                    if (next != null) next.SetState(true);
                }
            }
            else
            {
                // 万が一マスが見つからないエラー時の保険（最初に戻す）
                ResetProgress();
            }
        }
    }

    // 選んだルートの次だけを開放し、進捗をセーブする
    public void OpenNextNodesOnly(MapNode clearedNode)
    {
        // 1. 進捗をセーブデータに保存
        PlayerPrefs.SetString("LastClearedNode", clearedNode.name);
        PlayerPrefs.Save(); // 確実に即時保存

        // 2. 全てのマスを一旦ロックし、クリアしたマスの「次」だけを光らせる
        foreach (var node in allNodes)
        {
            node.SetState(false);
        }

        foreach (var next in clearedNode.nextNodes)
        {
            if (next != null) next.SetState(true);
        }
    }

    // 【便利！】最初からの動きをテストしたい時に呼び出す関数
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("LastClearedNode");
        PlayerPrefs.Save();
        // 現在のマップシーンを再読み込みして初期状態（最初の3マス）に戻す
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}