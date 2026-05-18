using UnityEngine;

public class MapSlideHandler : MonoBehaviour
{
    public static MapSlideHandler Instance;

    [Header("スライドさせる親オブジェクト（MAP 1）")]
    public RectTransform mapContainer;

    [Header("スライド後のX座標（例: -1920）")]
    public float targetX = -1920f;

    public float speed = 5f;
    private float currentTargetX;
    private bool isMoving = false;

    void Awake() => Instance = this;

    void Start()
    {
        string lastCleared = PlayerPrefs.GetString("LastClearedNode", "");

        // もし最後にクリアしたのが「中ボス」なら、最初からTargetXの位置にする
        // ※"MidBossNodeName" は、ヒエラルキー上の中ボスボタンの名前に書き換えてください
        if (lastCleared == "MidBossNodeName")
        {
            Vector2 pos = mapContainer.anchoredPosition;
            pos.x = targetX;
            mapContainer.anchoredPosition = pos;
            currentTargetX = targetX;
        }
        else
        {
            currentTargetX = mapContainer.anchoredPosition.x;
        }
    }

    void Update()
    {
        if (isMoving)
        {
            Vector2 pos = mapContainer.anchoredPosition;
            // 滑らかに移動させる
            pos.x = Mathf.Lerp(pos.x, currentTargetX, Time.deltaTime * speed);
            mapContainer.anchoredPosition = pos;

            // 十分近づいたら停止
            if (Mathf.Abs(pos.x - currentTargetX) < 0.1f) isMoving = false;
        }
    }

    // 中ボスを倒した時などにこれを呼ぶ
    public void StartSlide()
    {
        currentTargetX = targetX;
        isMoving = true;
    }
}