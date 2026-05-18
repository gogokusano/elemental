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
        // 毎回必ず初期状態（最初の3マスだけ押せる）からスタート
        ResetToStart();
    }

    public void ResetToStart()
    {
        // 1. 一旦すべてのボタンをロック
        foreach (var node in allNodes)
        {
            node.SetState(false);
        }

        // 2. スタートの3マスだけを有効化
        foreach (var st in startNodes)
        {
            st.SetState(true);
        }
    }

    // 次のマスを開放し、それ以外をすべてロックする
    public void OpenNextNodesOnly(List<MapNode> nextNodes)
    {
        // 1. まず全てのマスを一旦非アクティブ（ロック）にする
        foreach (var node in allNodes)
        {
            node.SetState(false);
        }

        // 2. 今回進んだルートの「次のマス」だけをピンポイントで有効化する
        foreach (var next in nextNodes)
        {
            if (next != null)
            {
                next.SetState(true);
            }
        }
    }
}