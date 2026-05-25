using UnityEngine;
using UnityEngine.UI; // ★追加
using System.Collections.Generic;

// ランダムに割り当てるイベントのデータ構造（修正なし）
[System.Serializable]
public struct MapEventData
{
    public string eventName;    // 識別用
    public Sprite iconSprite;   // インスペクターで設定する画像
    public string sceneName;    // 移動先のシーン名
}

public class MapManager : MonoBehaviour
{
    // --- 略（Instance, allNodes, startNodes, availableEvents はそのまま） ---
    public static MapManager Instance;
    [Header("全てのマス")]
    public List<MapNode> allNodes;
    [Header("最初の3マス")]
    public List<MapNode> startNodes;
    [Header("ランダム用イベントの種類")]
    public List<MapEventData> availableEvents;

    void Awake() => Instance = this;

    void Start()
    {
        RefreshMap();
    }

    // --- RefreshMap 関数 ---
    public void RefreshMap()
    {
        foreach (var node in allNodes)
        {
            if (node != null) node.SetState(false);
        }

        string lastCleared = PlayerPrefs.GetString("LastClearedNode", "");

        if (string.IsNullOrEmpty(lastCleared))
        {
            // ★新規開始時のみランダム化する（ロード時はすでにランダム化されていると仮定するが、今回は簡易的にロード時もランダム化させる）
            RandomizeMapNodes();

            foreach (var st in startNodes)
            {
                if (st != null) st.SetState(true);
            }

            if (MapSlideHandler.Instance != null)
            {
                MapSlideHandler.Instance.RestorePosition(0f);
            }
        }
        else
        {
            // ロード時も見た目を反映させる
            RandomizeMapNodes();

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

            if (MapSlideHandler.Instance != null)
            {
                float savedX = PlayerPrefs.GetFloat("MapSavedX", 0f);
                MapSlideHandler.Instance.RestorePosition(savedX);
            }
        }
    }

    // ★修正：通常マスの中身をランダムに入れ替える関数
    private void RandomizeMapNodes()
    {
        if (allNodes == null || allNodes.Count == 0) return;
        if (availableEvents == null || availableEvents.Count == 0) return;

        foreach (var node in allNodes)
        {
            if (node == null) continue;
            // 中ボスとボスは除外
            if (node.isMidBoss || node.isBoss) continue;

            // ランダムに1つ選ぶ
            int randomIndex = Random.Range(0, availableEvents.Count);
            MapEventData selectedEvent = availableEvents[randomIndex];

            // マスの中身を書き換える（修正あり）
            node.sceneName = selectedEvent.sceneName;

            // ★修正点：アイコン画像の差し替え（ImageコンポーネントのSpriteをセットする）
            if (node.nodeIcon != null)
            {
                // imageコンポーネントに直接spriteを渡すことで、表示が更新される
                node.nodeIcon.sprite = selectedEvent.iconSprite;
            }
            else
            {
                Debug.LogError($"{node.name} に Image (nodeIcon) が設定されていません！");
            }

            // マスの名前も変更
            node.gameObject.name = $"{selectedEvent.eventName}_{System.Guid.NewGuid().ToString().Substring(0, 4)}";
        }
    }

    // --- OpenNextNodesOnly 関数 ---
    public void OpenNextNodesOnly(MapNode clearedNode)
    {
        PlayerPrefs.SetString("LastClearedNode", clearedNode.name);

        if (MapSlideHandler.Instance != null)
        {
            float currentX = MapSlideHandler.Instance.GetCurrentX();
            if (clearedNode.isMidBoss) currentX = MapSlideHandler.Instance.targetX;
            PlayerPrefs.SetFloat("MapSavedX", currentX);
        }

        PlayerPrefs.Save();

        foreach (var node in allNodes)
        {
            if (node != null) node.SetState(false);
        }

        foreach (var next in clearedNode.nextNodes)
        {
            if (next != null) next.SetState(true);
        }
    }

    // --- ResetProgress 関数 ---
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("LastClearedNode");
        PlayerPrefs.DeleteKey("MapSavedX");
        PlayerPrefs.Save();

        if (MapSlideHandler.Instance != null)
        {
            MapSlideHandler.Instance.RestorePosition(0f);
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}