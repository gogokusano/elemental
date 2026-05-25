using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct MapEventData
{
    public string eventName;
    public Sprite iconSprite;
    public string sceneName;
    public Color eventColor;
}

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    [Header("全てのマス（ヒエラルキーから全て入れる）")]
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

    public void RefreshMap()
    {
        // 1. まず全てのマスを一旦非アクティブ（半透明化）にする
        foreach (var node in allNodes)
        {
            if (node != null) node.SetState(false);
        }

        string lastCleared = PlayerPrefs.GetString("LastClearedNode", "");

        if (string.IsNullOrEmpty(lastCleared))
        {
            // ■ 完全な新規ゲーム開始時

            // 新しいシード値（ランダムな数字）を生成して保存
            int newSeed = Random.Range(1, 999999);
            PlayerPrefs.SetInt("MapSeed", newSeed);
            PlayerPrefs.Save();

            // ★そのシード値を使ってマップを生成
            RandomizeMapNodes(newSeed);

            // 最初の3マスをアクティブに
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
            // ■ シーン遷移から戻ってきた、または続きからロード時

            // 保存されているシード値を読み出す（なければとりあえず0）
            int savedSeed = PlayerPrefs.GetInt("MapSeed", 0);

            // ★【重要】戻ってきた時も「同じシード値」で生成するため、必ず前回と同じ配置になる！
            RandomizeMapNodes(savedSeed);

            MapNode lastNode = allNodes.Find(x => x.name == lastCleared);
            if (lastNode != null)
            {
                // クリアしたマスの次のマスたちをアクティブ（不透明）にする
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

    // ★修正：引数に「seed」を受け取り、それに基づいてランダムを固定化する
    private void RandomizeMapNodes(int seed)
    {
        if (allNodes == null || allNodes.Count == 0) return;
        if (availableEvents == null || availableEvents.Count == 0) return;

        Random.InitState(seed);

        foreach (var node in allNodes)
        {
            if (node == null) continue;

            // ★修正：中ボス、ボス、または「固定マス」にチェックがある場合はランダム化をスキップ！
            if (node.isMidBoss || node.isBoss || node.isFixedNode)
            {
                // 固定マスの場合は、セーブデータが迷子にならないように名前だけ一意の固定名にする
                node.gameObject.name = $"Fixed_{node.transform.parent.name}_{node.transform.GetSiblingIndex()}";
                continue;
            }

            int randomIndex = Random.Range(0, availableEvents.Count);
            MapEventData selectedEvent = availableEvents[randomIndex];

            node.sceneName = selectedEvent.sceneName;

            if (node.nodeIcon == null)
            {
                node.nodeIcon = node.GetComponent<UnityEngine.UI.Image>();
            }

            if (node.nodeIcon != null)
            {
                node.nodeIcon.sprite = selectedEvent.iconSprite;

                float currentAlpha = node.nodeIcon.color.a;
                Color finalColor = selectedEvent.eventColor;
                finalColor.a = currentAlpha;
                node.nodeIcon.color = finalColor;
            }

            node.gameObject.name = $"{selectedEvent.eventName}_{node.transform.GetSiblingIndex()}_{randomIndex}";
        }
    }

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

    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("LastClearedNode");
        PlayerPrefs.DeleteKey("MapSavedX");
        PlayerPrefs.DeleteKey("MapSeed"); // ★追加：シード値もリセット
        PlayerPrefs.Save();

        if (MapSlideHandler.Instance != null)
        {
            MapSlideHandler.Instance.RestorePosition(0f);
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}