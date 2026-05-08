using UnityEngine;

public class MapSlideHandler : MonoBehaviour
{
    public static MapSlideHandler Instance;

    [Header("移動させる親オブジェクト（全マスが入っている箱）")]
    public RectTransform mapContainer;

    [Header("マップ2へ切り替える時の移動量")]
    [Tooltip("例: 画面横幅が1920なら -1920 を入れると右側が映ります")]
    public float slideDistance = -1920f;

    public float slideSpeed = 5f;
    private Vector2 targetPos;
    private bool isMoving = false;

    void Awake() => Instance = this;

    void Start()
    {
        targetPos = mapContainer.anchoredPosition;
    }

    void Update()
    {
        if (isMoving)
        {
            mapContainer.anchoredPosition = Vector2.Lerp(
                mapContainer.anchoredPosition,
                targetPos,
                Time.deltaTime * slideSpeed
            );

            if (Vector2.Distance(mapContainer.anchoredPosition, targetPos) < 0.1f)
                isMoving = false;
        }
    }

    // 中ボスクリア時にこれを呼ぶ
    public void SlideToMap2()
    {
        targetPos = new Vector2(slideDistance, mapContainer.anchoredPosition.y);
        isMoving = true;
    }
}