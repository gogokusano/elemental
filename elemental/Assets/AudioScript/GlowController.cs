using UnityEngine;
using UnityEngine.UI;

public class GlowController : MonoBehaviour
{
    [Header("発光設定")]
    [Tooltip("チェックを入れると発光（Bloom対象）になります")]
    [SerializeField] private bool isGlowing = false;

    [Tooltip("発光時の強さ（Global VolumeのThresholdを超える値にしてください）")]
    [SerializeField] private float glowIntensity = 3.0f;

    [Tooltip("通常時の強さ（光らせない時の輝度）")]
    [SerializeField] private float normalIntensity = 1.0f;

    private Graphic uiGraphic;
    private Renderer worldRenderer;
    private Color originalColor;
    private bool lastGlowState;

    void Start()
    {
        // UI用 (ImageやText) か 3D/Particle用かを自動判別
        uiGraphic = GetComponent<Graphic>();
        worldRenderer = GetComponent<Renderer>();

        if (uiGraphic != null) originalColor = uiGraphic.color;
        else if (worldRenderer != null) originalColor = worldRenderer.material.color;

        lastGlowState = isGlowing;
        ApplyGlow(isGlowing);
    }

    void Update()
    {
        // Inspectorでの変更をリアルタイムに検知
        if (isGlowing != lastGlowState)
        {
            ApplyGlow(isGlowing);
            lastGlowState = isGlowing;
        }
    }

    private void ApplyGlow(bool glow)
    {
        float intensity = glow ? glowIntensity : normalIntensity;
        Color targetColor = originalColor * intensity;

        if (uiGraphic != null)
        {
            uiGraphic.color = targetColor;
        }
        else if (worldRenderer != null)
        {
            // URPの標準的なカラープロパティに対応
            if (worldRenderer.material.HasProperty("_BaseColor"))
                worldRenderer.material.SetColor("_BaseColor", targetColor);
            else
                worldRenderer.material.color = targetColor;
                
            // エミッション（自発光）がある場合
            if (worldRenderer.material.HasProperty("_EmissionColor"))
                worldRenderer.material.SetColor("_EmissionColor", targetColor);
        }
    }

    // エディタ上での値変更にも対応
    private void OnValidate()
    {
        if (Application.isPlaying && (uiGraphic != null || worldRenderer != null))
        {
            ApplyGlow(isGlowing);
        }
    }
}