using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct MapEventData
{
    public string eventName;
    public Sprite iconSprite;
    public string sceneName;
    public Color eventColor;
    public int weight;
    public int minCountPerLayer;
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

    [Header("各層に1マス限定にするイベントの設定")]
    public string uniqueEventName = "ショップ";

    [Header("最初の3マスで出現を禁止するイベント（複数指定可能）")]
    public List<string> forbiddenEventNamesForStart = new List<string> { "ショップ", "Bonus" };

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
        int savedSeed = PlayerPrefs.GetInt("MapSeed", 0);

        if (savedSeed == 0)
        {
            savedSeed = Random.Range(1, 999999);
            PlayerPrefs.SetInt("MapSeed", savedSeed);
            PlayerPrefs.Save();
        }

        RandomizeMapNodes(savedSeed);

        if (!string.IsNullOrEmpty(currentChallenging))
        {
            Debug.LogWarning($"バトル中の不正な離脱を検知しました（未クリアマス: {currentChallenging}）。データをリセットします。");
            ResetProgress();
            return;
        }

        if (string.IsNullOrEmpty(lastCleared))
        {
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

    public void SaveChallengingNode(MapNode node)
    {
        PlayerPrefs.SetString("CurrentChallengingNode", node.name);
        SaveCameraPosition(node);
        PlayerPrefs.Save();
    }

    public void ClearCurrentNode()
    {
        string challengingNodeName = PlayerPrefs.GetString("CurrentChallengingNode", "");
        if (!string.IsNullOrEmpty(challengingNodeName))
        {
            PlayerPrefs.SetString("LastClearedNode", challengingNodeName);
            PlayerPrefs.DeleteKey("CurrentChallengingNode");
            PlayerPrefs.Save();
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

        // ==========================================================
        // ★最強版：マスの「繋がり（nextNodes）」をたどって階層を計算する！
        // 座標やヒエラルキー構造に一切依存しない、絶対確実な方法です。
        // ==========================================================
        Dictionary<MapNode, int> nodeFloors = new Dictionary<MapNode, int>();
        foreach (var node in allNodes)
        {
            if (node != null) nodeFloors[node] = 1; // 初期値は1
        }

        Queue<MapNode> queue = new Queue<MapNode>();
        foreach (var st in startNodes)
        {
            if (st != null)
            {
                nodeFloors[st] = 1; // スタート地点は1層目
                queue.Enqueue(st);
            }
        }

        // 繋がりを辿って、深い階層の数字を更新していく
        while (queue.Count > 0)
        {
            MapNode current = queue.Dequeue();
            int currentFloorNum = nodeFloors[current];

            if (current.nextNodes != null)
            {
                foreach (var nextNode in current.nextNodes)
                {
                    if (nextNode != null)
                    {
                        // 次のマスには +1 した階層をセットする
                        if (nodeFloors[nextNode] < currentFloorNum + 1)
                        {
                            nodeFloors[nextNode] = currentFloorNum + 1;
                            queue.Enqueue(nextNode);
                        }
                    }
                }
            }
        }
        // ==========================================================

        List<MapEventData> normalEvents = new List<MapEventData>();
        MapEventData uniqueEvent = default;
        bool hasUniqueEvent = false;

        foreach (var ev in availableEvents)
        {
            if (ev.eventName == uniqueEventName)
            {
                uniqueEvent = ev;
                hasUniqueEvent = true;
            }
            else
            {
                normalEvents.Add(ev);
            }
        }

        if (normalEvents.Count == 0) normalEvents = availableEvents;

        List<MapEventData> startNodeAvailableEvents = new List<MapEventData>();
        int totalWeightForStart = 0;

        foreach (var ev in availableEvents)
        {
            if (!forbiddenEventNamesForStart.Contains(ev.eventName))
            {
                startNodeAvailableEvents.Add(ev);
                totalWeightForStart += Mathf.Max(1, ev.weight);
            }
        }

        if (startNodeAvailableEvents.Count == 0)
        {
            startNodeAvailableEvents = normalEvents;
            totalWeightForStart = 0;
            foreach (var ev in normalEvents) totalWeightForStart += Mathf.Max(1, ev.weight);
        }

        int totalWeight = 0;
        foreach (var ev in normalEvents)
        {
            totalWeight += Mathf.Max(1, ev.weight);
        }

        // 階層（Floor）ごとにグループ分けする
        Dictionary<int, List<MapNode>> layerGroups = new Dictionary<int, List<MapNode>>();
        foreach (var pair in nodeFloors)
        {
            MapNode node = pair.Key;
            int floorNum = pair.Value;

            if (node.isMidBoss || node.isBoss || node.isFixedNode) continue;

            if (!layerGroups.ContainsKey(floorNum))
            {
                layerGroups[floorNum] = new List<MapNode>();
            }
            layerGroups[floorNum].Add(node);
        }

        Dictionary<MapNode, MapEventData> forcedNodeEvents = new Dictionary<MapNode, MapEventData>();

        foreach (var pair in layerGroups)
        {
            List<MapNode> availableNodesInLayer = new List<MapNode>(pair.Value);
            if (availableNodesInLayer.Count == 0) continue;

            if (hasUniqueEvent)
            {
                List<MapNode> shopCandidates = availableNodesInLayer.FindAll(n => !startNodes.Contains(n));
                MapNode shopNode = null;

                if (shopCandidates.Count > 0)
                {
                    shopNode = shopCandidates[Random.Range(0, shopCandidates.Count)];
                }
                else
                {
                    shopNode = availableNodesInLayer[Random.Range(0, availableNodesInLayer.Count)];
                }

                forcedNodeEvents[shopNode] = uniqueEvent;
                availableNodesInLayer.Remove(shopNode);
            }

            foreach (var ev in availableEvents)
            {
                if (ev.eventName == uniqueEventName || ev.minCountPerLayer <= 0) continue;

                for (int i = 0; i < ev.minCountPerLayer; i++)
                {
                    if (availableNodesInLayer.Count == 0) break;

                    MapNode targetNode = null;
                    if (forbiddenEventNamesForStart.Contains(ev.eventName))
                    {
                        List<MapNode> safeCandidates = availableNodesInLayer.FindAll(n => !startNodes.Contains(n));
                        if (safeCandidates.Count > 0)
                        {
                            targetNode = safeCandidates[Random.Range(0, safeCandidates.Count)];
                        }
                    }

                    if (targetNode == null)
                    {
                        targetNode = availableNodesInLayer[Random.Range(0, availableNodesInLayer.Count)];
                    }

                    forcedNodeEvents[targetNode] = ev;
                    availableNodesInLayer.Remove(targetNode);
                }
            }
        }

        // --- ④ 実際の配置と命名処理 ---
        foreach (var node in allNodes)
        {
            if (node == null) continue;

            // ★マスの繋がりから計算した「絶対に正しい階層」を取得
            int actualFloor = nodeFloors.ContainsKey(node) ? nodeFloors[node] : 1;

            if (node.isMidBoss || node.isBoss || node.isFixedNode)
            {
                node.gameObject.name = $"Fixed_Floor{actualFloor}_{node.transform.GetSiblingIndex()}";
                continue;
            }

            MapEventData selectedEvent = default;

            if (forcedNodeEvents.ContainsKey(node))
            {
                selectedEvent = forcedNodeEvents[node];
            }
            else if (startNodes.Contains(node))
            {
                int rolledValue = Random.Range(0, totalWeightForStart);
                int currentWeightSum = 0;

                for (int i = 0; i < startNodeAvailableEvents.Count; i++)
                {
                    currentWeightSum += Mathf.Max(1, startNodeAvailableEvents[i].weight);
                    if (rolledValue < currentWeightSum)
                    {
                        selectedEvent = startNodeAvailableEvents[i];
                        break;
                    }
                }
            }
            else
            {
                int rolledValue = Random.Range(0, totalWeight);
                int currentWeightSum = 0;

                for (int i = 0; i < normalEvents.Count; i++)
                {
                    currentWeightSum += Mathf.Max(1, normalEvents[i].weight);
                    if (rolledValue < currentWeightSum)
                    {
                        selectedEvent = normalEvents[i];
                        break;
                    }
                }
            }

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

            // ★絶対に正しい階層の数字を名前に刻み込む！
            node.gameObject.name = $"{selectedEvent.eventName}_Floor{actualFloor}_{node.transform.GetSiblingIndex()}";
        }
    }

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

    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("LastClearedNode");
        PlayerPrefs.DeleteKey("CurrentChallengingNode");
        PlayerPrefs.DeleteKey("MapSavedX");
        PlayerPrefs.DeleteKey("MapSeed");
        PlayerPrefs.Save();

        if (MapSlideHandler.Instance != null)
        {
            MapSlideHandler.Instance.RestorePosition(0f);
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void GiveUpAndBackToTitle()
    {
        Debug.Log("ゲームを途中離脱します。タイトルへ戻ります。");

        // データを完全にリセット
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.ResetAllData();
        }

        // タイトルシーンへ遷移
        UnityEngine.SceneManagement.SceneManager.LoadScene("title");
    }
}