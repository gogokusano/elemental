using UnityEngine;

public class MapSlideHandler : MonoBehaviour
{
    public static MapSlideHandler Instance;

    [Header("スライドさせる親オブジェクト（MapContainer）")]
    public RectTransform mapContainer;

    [Header("ステージ2のスライド先X座標")]
    public float targetX = -2436f;

    public float speed = 5f;
    private float currentTargetX;
    private bool isMoving = false;

    void Awake() => Instance = this;

    void Start()
    {
        if (mapContainer != null)
        {
            currentTargetX = mapContainer.anchoredPosition.x;
        }
    }

    // ★修正：保存されたX座標に直接セットする関数
    public void RestorePosition(float savedX)
    {
        if (mapContainer == null) return;

        Vector2 pos = mapContainer.anchoredPosition;
        pos.x = savedX;
        mapContainer.anchoredPosition = pos;

        currentTargetX = savedX;
    }

    void Update()
    {
        if (isMoving)
        {
            Vector2 pos = mapContainer.anchoredPosition;
            pos.x = Mathf.Lerp(pos.x, currentTargetX, Time.deltaTime * speed);
            mapContainer.anchoredPosition = pos;

            if (Mathf.Abs(pos.x - currentTargetX) < 0.1f)
            {
                pos.x = currentTargetX;
                mapContainer.anchoredPosition = pos;
                isMoving = false;
            }
        }
    }

    public void StartSlide()
    {
        currentTargetX = targetX;
        isMoving = true;
    }

    // ★追加：現在のマップのX座標を外から取得するための関数
    public float GetCurrentX()
    {
        if (mapContainer == null) return 0f;
        return mapContainer.anchoredPosition.x;
    }
}