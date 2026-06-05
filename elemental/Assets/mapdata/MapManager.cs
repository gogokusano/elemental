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
    // ★追加：このイベントを各層に「最低でも何マス出現させたいか」の設定
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

        string lastCleared = PlayerPrefs.GetString("LastClearedNode", "");
        string currentChallenging = PlayerPrefs.GetString("CurrentChallengingNode", "");
        int savedSeed = PlayerPrefs.GetInt("MapSeed", 0);

        // --- マップシード（配置）の生成・ロード判定を最適化 ---
        if (savedSeed == 0)
        {
            savedSeed = Random.Range(1, 999999);
            PlayerPrefs.SetInt("MapSeed", savedSeed);
            PlayerPrefs.Save();
        }

        // 常に保存された同じシード値で生成
        RandomizeMapNodes(savedSeed);

        // --- ★最重要：バトル中の不正な離脱（タスクキル）があった場合はデータを即リセット！ ---
        if (!string.IsNullOrEmpty(currentChallenging))
        {
            Debug.LogWarning($"バトル中の不正な離脱を検知しました（未クリアマス: {currentChallenging}）。データをリセットします。");
            ResetProgress();
            return;
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

    // 引数に「seed」を受け取り、それに基づいてランダムを固定化する
    private void RandomizeMapNodes(int seed)
    {
        if (allNodes == null || allNodes.Count == 0) return;
        if (availableEvents == null || availableEvents.Count == 0) return;

        Random.InitState(seed);

        // --- ① ショップ（限定イベント）を通常プールから分別 ---
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

        // --- 最初の3マス専用のプールを作る ---
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

        // --- 通常マス用の合計Weight（重み）を計算しておく ---
        int totalWeight = 0;
        foreach (var ev in normalEvents)
        {
            totalWeight += Mathf.Max(1, ev.weight);
        }

        // --- ② マスを「親オブジェクト（層）」ごとにグループ分けする ---
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

        // --- ③ 各マスの役割を確定させる事前抽選フェーズ ---
        // 各マスをキーとして、どのイベントを割り当てるかを記録する辞書
        Dictionary<MapNode, MapEventData> forcedNodeEvents = new Dictionary<MapNode, MapEventData>();

        foreach (var pair in layerGroups)
        {
            List<MapNode> availableNodesInLayer = new List<MapNode>(pair.Value);
            if (availableNodesInLayer.Count == 0) continue;

            // A. 各層に1マスのショップ（UniqueEvent）を最優先で割り当て
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

            // B. ★修正：インスペクターで指定された「最低保証数（minCountPerLayer）」を満たすように割り当て
            foreach (var ev in availableEvents)
            {
                // ショップ（Unique）は上で処理済みなのと、最低保証数が0以下のものはスルー
                if (ev.eventName == uniqueEventName || ev.minCountPerLayer <= 0) continue;

                for (int i = 0; i < ev.minCountPerLayer; i++)
                {
                    if (availableNodesInLayer.Count == 0) break; // 空きマスがなくなったら終了

                    // 最初の3マスの制限リストに入っているイベントの場合、startNodesを避けて選ぶ
                    MapNode targetNode = null;
                    if (forbiddenEventNamesForStart.Contains(ev.eventName))
                    {
                        List<MapNode> safeCandidates = availableNodesInLayer.FindAll(n => !startNodes.Contains(n));
                        if (safeCandidates.Count > 0)
                        {
                            targetNode = safeCandidates[Random.Range(0, safeCandidates.Count)];
                        }
                    }

                    // 適切な退避先がない、または制限のないイベントなら残りからランダム選出
                    if (targetNode == null)
                    {
                        targetNode = availableNodesInLayer[Random.Range(0, availableNodesInLayer.Count)];
                    }

                    forcedNodeEvents[targetNode] = ev;
                    availableNodesInLayer.Remove(targetNode); // 確定したので候補から消す
                }
            }
        }

        // --- ④ 実際の配置と命名処理 ---
        foreach (var node in allNodes)
        {
            if (node == null) continue;

            // 中ボス、ボス、または「固定マス」にチェックがある場合はランダム化をスキップ
            if (node.isMidBoss || node.isBoss || node.isFixedNode)
            {
                node.gameObject.name = $"Fixed_{node.transform.parent.name}_{node.transform.GetSiblingIndex()}";
                continue;
            }

            MapEventData selectedEvent = default;

            // 事前抽選（ショップや最低保証枠）で確定しているマスか判定
            if (forcedNodeEvents.ContainsKey(node))
            {
                selectedEvent = forcedNodeEvents[node];
            }
            else if (startNodes.Contains(node))
            {
                // 最初の3マスのうち、最低保証で埋まらなかった残りの通常マスを抽選
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
                // 最低保証枠から外れた、残りの完全フリーな通常マスをWeight確率で抽選
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
}