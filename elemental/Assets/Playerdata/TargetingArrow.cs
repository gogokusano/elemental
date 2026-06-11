using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TargetingArrow : MonoBehaviour
{
    public static TargetingArrow Instance; // どこからでも呼べるようにする
    private LineRenderer mainLineRenderer;
    private LineRenderer outlineLineRenderer; // ◀ 追加：アウトライン用のレンダラー

    [Header("線の滑らかさ")]
    public int segmentCount = 20;

    [Header("▼ アウトラインの設定")]
    [Tooltip("アウトラインの色")]
    public Color outlineColor = Color.black;
    [Tooltip("メインの線よりどれくらい太くするか（乗算）")]
    public float outlineWidthMultiplier = 1.3f;
    [Tooltip("アウトラインの描画順マテリアル（少しだけ手前に出す Zぶれ防止）")]
    public Vector3 outlineOffset = new Vector3(0, 0, 0.01f);

    void Awake()
    {
        Instance = this;
        mainLineRenderer = GetComponent<LineRenderer>();
        mainLineRenderer.positionCount = segmentCount;

        // --- アウトライン用のLineRendererを自動生成 ---
        SetupOutlineRenderer();

        // 最初は線を隠しておく
        HideArrow();
    }

    /// <summary>
    /// 背面に重ねるアウトライン用のLineRendererを初期設定する
    /// </summary>
    private void SetupOutlineRenderer()
    {
        GameObject outlineObj = new GameObject("Outline_Line");
        outlineObj.transform.SetParent(this.transform);
        outlineObj.transform.localPosition = outlineOffset; // ほんの少しだけ背面にずらす

        outlineLineRenderer = outlineObj.AddComponent<LineRenderer>();
        outlineLineRenderer.positionCount = segmentCount;

        // メインのLineRendererと同じ設定をコピー（マテリアル等）
        outlineLineRenderer.sharedMaterial = mainLineRenderer.sharedMaterial;
        outlineLineRenderer.useWorldSpace = mainLineRenderer.useWorldSpace;
        outlineLineRenderer.alignment = mainLineRenderer.alignment;
        outlineLineRenderer.textureMode = mainLineRenderer.textureMode;

        // 色と太さをアウトライン用に設定
        outlineLineRenderer.startColor = outlineColor;
        outlineLineRenderer.endColor = outlineColor;
        outlineLineRenderer.widthCurve = mainLineRenderer.widthCurve;
        outlineLineRenderer.widthMultiplier = mainLineRenderer.widthMultiplier * outlineWidthMultiplier;

        // 初期状態は非表示
        outlineLineRenderer.enabled = false;
    }

    // 矢印を表示して曲線を引く
    public void UpdateArrow(Vector3 startPos, Vector3 endPos)
    {
        mainLineRenderer.enabled = true;
        outlineLineRenderer.enabled = true; // ◀ アウトラインも表示

        // 動的に太さを同期（インスペクターでメインの太さを変えても追従するように）
        outlineLineRenderer.widthMultiplier = mainLineRenderer.widthMultiplier * outlineWidthMultiplier;

        // 始点と終点の真ん中より少し上を「引っ張るポイント」にして曲線を作る
        Vector3 controlPoint = (startPos + endPos) / 2f;
        controlPoint.y += 2.0f; // ぐわっと上にカーブさせる

        for (int i = 0; i < segmentCount; i++)
        {
            float t = i / (float)(segmentCount - 1);
            Vector3 point = CalculateBezierPoint(t, startPos, controlPoint, endPos);

            // メインの線を描画
            mainLineRenderer.SetPosition(i, point);

            // アウトラインの線を描画（offset分だけずらして重ねる）
            outlineLineRenderer.SetPosition(i, point + outlineOffset);
        }
    }

    // 矢印を消す
    public void HideArrow()
    {
        if (mainLineRenderer != null) mainLineRenderer.enabled = false;
        if (outlineLineRenderer != null) outlineLineRenderer.enabled = false; // ◀ アウトラインも隠す
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