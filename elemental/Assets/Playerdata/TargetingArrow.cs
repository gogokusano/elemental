using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TargetingArrow : MonoBehaviour
{
    public static TargetingArrow Instance; // どこからでも呼べるようにする
    private LineRenderer lineRenderer;
    
    [Header("線の滑らかさ")]
    public int segmentCount = 20; 

    void Awake()
    {
        Instance = this;
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = segmentCount;
        
        // 最初は線を隠しておく
        lineRenderer.enabled = false; 
    }

    // 矢印を表示して曲線を引く
    public void UpdateArrow(Vector3 startPos, Vector3 endPos)
    {
        lineRenderer.enabled = true;

        // 始点と終点の真ん中より少し上を「引っ張るポイント」にして曲線を作る
        Vector3 controlPoint = (startPos + endPos) / 2f;
        controlPoint.y += 2.0f; // ぐわっと上にカーブさせる

        for (int i = 0; i < segmentCount; i++)
        {
            float t = i / (float)(segmentCount - 1);
            Vector3 point = CalculateBezierPoint(t, startPos, controlPoint, endPos);
            lineRenderer.SetPosition(i, point);
        }
    }

    // 矢印を消す
    public void HideArrow()
    {
        lineRenderer.enabled = false;
    }

    // 滑らかな曲線の計算式（2次ベジェ曲線）
    Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        Vector3 p = uu * p0;
        p += 2 * u * t * p1;
        p += tt * p2;
        return p;
    }
}