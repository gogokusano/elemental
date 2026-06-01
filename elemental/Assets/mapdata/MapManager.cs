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
        foreach (var node in allNodes)
        {
            if (node != null) node.SetState(false);
        }

        string lastCleared = PlayerPrefs.GetString("LastClearedNode", "");
        string currentChallenging = PlayerPrefs.GetString("CurrentChallengingNode", "");

        int seed = PlayerPrefs.GetInt("MapSeed", 0);
        if (string.IsNullOrEmpty(lastCleared) && string.IsNullOrEmpty(currentChallenging) && seed == 0)
        {
            seed = Random.Range(1, 999999);
            PlayerPrefs.SetInt("MapSeed", seed);
        }

        RandomizeMapNodes(seed);

        // ★バトル中の不正離脱（タスクキル）があった場合のみリセット
        if (!string.IsNullOrEmpty(currentChallenging))
        {
            Debug.LogWarning($"バトル中の不正な離脱を検知しました。データをリセットします。");
            ResetProgress();
            return;
        }

        if (string.IsNullOrEmpty(lastCleared))
        {
            foreach (var st in startNodes)
            {
                if (st != null) st.SetState(true);
            }
            if (MapSlideHandler.Instance != null) MapSlideHandler.Instance.RestorePosition(0f);
        }
        else
        {
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

    // バトルマス用：挑戦中としてロックする
    public void SaveChallengingNode(MapNode node)
    {
        PlayerPrefs.SetString("CurrentChallengingNode", node.name);
        SaveCameraPosition(node);
        PlayerPrefs.Save();
    }

    // イベント・宝箱マス用：即座に次のマスを開放してセーブする
    public void OpenNextNodesOnly(MapNode clearedNode)
    {
        PlayerPrefs.SetString("LastClearedNode", clearedNode.name);
        SaveCameraPosition(clearedNode);
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

    // 本当のバトル勝利時に呼び出す関数
    public void ClearCurrentNode()
    {
        string challengingNodeName = PlayerPrefs.GetString("CurrentChallengingNode", "");
        if (!string.IsNullOrEmpty(challengingNodeName))
        {
            PlayerPrefs.SetString("LastClearedNode", challengingNodeName);
            PlayerPrefs.DeleteKey("CurrentChallengingNode");
            PlayerPrefs.Save();
            Debug.Log($"バトルクリア確定: {challengingNodeName}");
        }
    }

    private void SaveCameraPosition(MapNode node)
    {
        if (MapSlideHandler.Instance != null)
        {
            float currentX = MapSlideHandler.Instance.GetCurrentX();
            if (node.isMidBoss) currentX = MapSlideHandler.Instance.targetX;
            PlayerPrefs.SetFloat("MapSavedX", currentX);
        }
    }

    private void RandomizeMapNodes(int seed)
    {
        if (allNodes == null || allNodes.Count == 0) return;
        if (availableEvents == null || availableEvents.Count == 0) return;

        Random.InitState(seed);

        foreach (var node in allNodes)
        {
            if (node == null) continue;

            if (node.isMidBoss || node.isBoss || node.isFixedNode)
            {
                node.gameObject.name = $"Fixed_{node.transform.parent.name}_{node.transform.GetSiblingIndex()}";
                continue;
            }

            int randomIndex = Random.Range(0, availableEvents.Count);
            MapEventData selectedEvent = availableEvents[randomIndex];

            node.sceneName = selectedEvent.sceneName;

            if (node.nodeIcon == null) node.nodeIcon = node.GetComponent<UnityEngine.UI.Image>();

            if (node.nodeIcon != null)
            {
                node.nodeIcon.sprite = selectedEvent.iconSprite;
                float currentAlpha = node.nodeIcon.color.a;
                Color finalColor = selectedEvent.eventColor;
                finalColor.a = currentAlpha;
                node.nodeIcon.color = finalColor;
            }

            node.gameObject.name = $"{selectedEvent.eventName}_{node.transform.GetSiblingIndex()}";
        }
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("LastClearedNode");
        PlayerPrefs.DeleteKey("CurrentChallengingNode");
        PlayerPrefs.DeleteKey("MapSavedX");
        PlayerPrefs.DeleteKey("MapSeed");
        PlayerPrefs.Save();

        if (MapSlideHandler.Instance != null) MapSlideHandler.Instance.RestorePosition(0f);
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}