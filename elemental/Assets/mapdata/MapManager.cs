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
        // 1. まず全てのマスを一旦非アクティブ（半透明化）にする
        foreach (var node in allNodes)
        {
            if (node != null) node.SetState(false);
        }

        // --- マップを全く同じルールで完全再現（名前を確定）させる ---
        int savedSeed = PlayerPrefs.GetInt("MapSeed", 0);
        if (savedSeed == 0)
        {
            savedSeed = Random.Range(1, 999999);
            PlayerPrefs.SetInt("MapSeed", savedSeed);
            PlayerPrefs.Save();
        }
        RandomizeMapNodes(savedSeed);

        // --- 名前の確定後にセーブデータを読み込む ---
        string lastCleared = PlayerPrefs.GetString("LastClearedNode", "");
        string currentChallenging = PlayerPrefs.GetString("CurrentChallengingNode", "");

        bool isBossClearedBack = false; // ボス撃破から戻ってきたフラグ

        // --- ★最重要：中ボス・ボス戦から「正常に」戻ってきたかどうかの判定 ---
        if (!string.IsNullOrEmpty(currentChallenging))
        {
            MapNode challengingNode = allNodes.Find(x => x.name == currentChallenging);
            if (challengingNode != null && (challengingNode.isMidBoss || challengingNode.isBoss))
            {
                // 中ボス・ボスは、シーン移動（またはシーン空スキップ）から戻った時点で自動でクリア扱いにする
                PlayerPrefs.SetString("LastClearedNode", currentChallenging);
                PlayerPrefs.DeleteKey("CurrentChallengingNode");
                lastCleared = currentChallenging; // 下の開放処理に流す
                currentChallenging = "";
                PlayerPrefs.Save();

                isBossClearedBack = true; // 演出の分岐用フラグを立てる
                Debug.Log($"<color=cyan>【MapManager】中ボス・ボスの撃破戻りを検知しました。次を開放します: {lastCleared}</color>");
            }
            else
            {
                // 通常戦闘でのタスクキルはペナルティ
                Debug.LogWarning($"バトル中の不正な離脱を検知しました（未クリアマス: {currentChallenging}）。データをリセットします。");
                ResetProgress();
                return;
            }
        }

        if (string.IsNullOrEmpty(lastCleared))
        {
            // ■ 初期状態
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
            // ■ シーン遷移から正常に戻ってきた時
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
                Debug.LogWarning($"前回クリアしたノードが見つかりません: {lastCleared}。進行状況をリセットします。");
                ResetProgress();
                return;
            }

            // ========================================================
            // ★修正：カメラ位置復元とスライド演出のバッティングを解消
            // ========================================================
            if (MapSlideHandler.Instance != null)
            {
                if (isBossClearedBack || lastNode.isMidBoss || lastNode.isBoss)
                {
                    // 【中ボスを倒して帰ってきた時】
                    // 1. 最初は前のカメラ位置（0f）にピタッと初期化する
                    MapSlideHandler.Instance.RestorePosition(0f);
                    // 2. 画面が開いた直後から、なめらかなスライド演出をスタートさせる！
                    MapSlideHandler.Instance.StartSlide();
                }
                else
                {
                    // 【通常マスから帰ってきた時】
                    // 保存されているスクロール位置を一瞬で復元する
                    float savedX = PlayerPrefs.GetFloat("MapSavedX", 0f);
                    MapSlideHandler.Instance.RestorePosition(savedX);
                }
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

        Dictionary<Transform, List<MapNode>> layerGroups = new Dictionary<Transform, List<MapNode>>();

        foreach (var node in allNodes)
        {
            if (node == null || node.isMidBoss || node.isBoss || node.isFixedNode) continue;

            Transform parent = node.transform.parent;
            if (parent == null) continue;

            if (!layerGroups.ContainsKey(parent))
            {
                layerGroups[parent] = new List<MapNode>();
            }
            layerGroups[parent].Add(node);
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

        foreach (var node in allNodes)
        {
            if (node == null) continue;

            if (node.isMidBoss || node.isBoss || node.isFixedNode)
            {
                node.gameObject.name = $"Fixed_{node.transform.parent.name}_{node.transform.GetSiblingIndex()}";
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

            string layerName = node.transform.parent != null ? node.transform.parent.name : "Layer";
            node.gameObject.name = $"{selectedEvent.eventName}_{layerName}_{node.transform.GetSiblingIndex()}";
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

        if (StatusPanelManager.Instance != null)
        {
            Destroy(StatusPanelManager.Instance.gameObject);
        }
        
        // タイトルシーンへ遷移
        UnityEngine.SceneManagement.SceneManager.LoadScene("title");
    }
}