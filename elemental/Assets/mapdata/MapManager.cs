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
            if (node != null) node.SetState(false);
        }

        // 2. セーブデータ（最後にクリアしたマスの名前）を取得
        string lastCleared = PlayerPrefs.GetString("LastClearedNode", "");

        if (string.IsNullOrEmpty(lastCleared))
        {
            // セーブデータがない場合：初期状態（最初の3マスを有効化）
            foreach (var st in startNodes)
            {
                if (st != null) st.SetState(true);
            }

            // 初期位置（0）に戻す
            if (MapSlideHandler.Instance != null)
            {
                MapSlideHandler.Instance.RestorePosition(0f);
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
                ResetProgress();
                return;
            }

            // ★修正：保存されていたマップのX座標を読み込んで完全に復元する
            if (MapSlideHandler.Instance != null)
            {
                float savedX = PlayerPrefs.GetFloat("MapSavedX", 0f);
                MapSlideHandler.Instance.RestorePosition(savedX);
            }
        }
    }

    // 選んだルートの次だけを開放し、進捗をセーブする
    public void OpenNextNodesOnly(MapNode clearedNode)
    {
        // 1. 進捗（マスの名前）を保存
        PlayerPrefs.SetString("LastClearedNode", clearedNode.name);

        // ★追加：移動後の「現在のマップのX座標」を取得して保存する
        if (MapSlideHandler.Instance != null)
        {
            float currentX = MapSlideHandler.Instance.GetCurrentX();

            // もし「中ボスを押した瞬間」なら、これからスライドする先(targetX)の座標を先読みして保存する
            if (clearedNode.isMidBoss)
            {
                currentX = MapSlideHandler.Instance.targetX;
            }

            PlayerPrefs.SetFloat("MapSavedX", currentX);
        }

        PlayerPrefs.Save(); // 確実に即時保存

        // 2. 全てのマスを一旦ロックし、クリアしたマスの「次」だけを光らせる
        foreach (var node in allNodes)
        {
            if (node != null) node.SetState(false);
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
        PlayerPrefs.DeleteKey("MapSavedX"); // 座標データも削除
        PlayerPrefs.Save();

        // 位置を初期位置に戻す
        if (MapSlideHandler.Instance != null)
        {
            MapSlideHandler.Instance.RestorePosition(0f);
        }

        // 現在のマップシーンを再読み込みして初期状態（最初の3マス）に戻す
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}