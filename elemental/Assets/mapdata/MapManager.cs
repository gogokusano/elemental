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
        string currentChallenging = PlayerPrefs.GetString("CurrentChallengingNode", "");
        int savedSeed = PlayerPrefs.GetInt("MapSeed", 0);

        // --- ★修正：マップシード（配置）の生成・ロード判定を最適化 ---
        if (savedSeed == 0)
        {
            // 完全な新規ゲーム開始時のみシード値を新しく作る
            savedSeed = Random.Range(1, 999999);
            PlayerPrefs.SetInt("MapSeed", savedSeed);
            PlayerPrefs.Save();
        }

        // 常に保存された（または今作った）同じシード値で生成するため配置が固定化される
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
            // ■ 初期状態：最初の3マスをアクティブに
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
                // クリアしたマスの次のマスたちをアクティブにする
                foreach (var next in lastNode.nextNodes)
                {
                    if (next != null) next.SetState(true);
                }
            }
            else
            {
                // セーブされたマス名がヒエラルキーに見つからない場合は不整合防止のため最初から
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

    // ★バトルマスを押した瞬間に呼ばれる、挑戦中を記録する関数
    public void SaveChallengingNode(MapNode node)
    {
        PlayerPrefs.SetString("CurrentChallengingNode", node.name);
        SaveCameraPosition(node);
        PlayerPrefs.Save();
    }

    // ★本当のバトル勝利時に呼び出す、クリア確定関数（GameManager側の予備としても機能）
    public void ClearCurrentNode()
    {
        string challengingNodeName = PlayerPrefs.GetString("CurrentChallengingNode", "");
        if (!string.IsNullOrEmpty(challengingNodeName))
        {
            PlayerPrefs.SetString("LastClearedNode", challengingNodeName);
            PlayerPrefs.DeleteKey("CurrentChallengingNode"); // 挑戦中ロックを解除
            PlayerPrefs.Save();
            Debug.Log($"バトルクリア確定: {challengingNodeName}");
        }
    }

    // カメラ位置を保存する共通処理
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

        foreach (var node in allNodes)
        {
            if (node == null) continue;

            // 中ボス、ボス、または「固定マス」にチェックがある場合はランダム化をスキップ
            if (node.isMidBoss || node.isBoss || node.isFixedNode)
            {
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

    // 非戦闘マス（宝箱・イベントなど）用の即時開放処理
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