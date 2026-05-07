using UnityEngine;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    [Header("全てのボタン（GameObject）をここに入れる")]
    public List<GameObject> allNodes;

    [Header("最初の階層のボタンだけここに入れる")]
    public List<GameObject> startNodes;

    void Start()
    {
        string lastCleared = PlayerPrefs.GetString("LastClearedNode", "");

        // 一旦全部無効
        foreach (var go in allNodes)
        {
            go.GetComponent<UnityEngine.UI.Button>().interactable = false;
        }

        if (string.IsNullOrEmpty(lastCleared))
        {
            // 最初のボタンを有効
            foreach (var st in startNodes)
                st.GetComponent<UnityEngine.UI.Button>().interactable = true;
        }
        else
        {
            // 前回クリアしたマスの「次」を有効にする
            GameObject lastGo = allNodes.Find(x => x.name == lastCleared);
            if (lastGo != null)
            {
                // MapNodeスクリプトから次のリストをもらって有効化
                var nexts = lastGo.GetComponent<MapNode>().nextNodes;
                foreach (var n in nexts)
                    n.GetComponent<UnityEngine.UI.Button>().interactable = true;
            }
        }
    }
}