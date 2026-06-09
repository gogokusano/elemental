using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    private TextMeshProUGUI textMesh; // UI用のテキストコンポーネント
    private RectTransform rectTransform;
    
    [Header("アニメーション設定")]
    public float floatSpeed = 120f; // UIのサイズに合わせてスピードを調整
    public float fadeSpeed = 2f;    // 消えるスピード
    public float destroyTime = 1f;  // 何秒後に消滅するか

    private Color textColor;

    // 従来の数字用のSetup
    public void Setup(int damageAmount, Color customColor)
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
        
        textMesh.text = damageAmount.ToString();
        
        textMesh.color = customColor;
        textColor = customColor;

        // 出現位置を左右に少しだけランダムにズラす
        rectTransform.anchoredPosition += new Vector2(Random.Range(-40f, 40f), Random.Range(-10f, 10f));
        
        Destroy(gameObject, destroyTime);
    }

    // ★新設：ターン表示などの「文字（テキスト）」を直接出すためのSetup
    public void SetupText(string textMessage, Color customColor)
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
        
        textMesh.text = textMessage; // もらった文字をそのままセット！
        
        textMesh.color = customColor;
        textColor = customColor;

        // ターン文言は画面の真ん中から綺麗に出したいので、ランダムのズレは無しにします
        
        Destroy(gameObject, destroyTime);
    }

    void Update()
    {
        rectTransform.anchoredPosition += Vector2.up * floatSpeed * Time.deltaTime;
        textColor.a -= fadeSpeed * Time.deltaTime;
        textMesh.color = textColor;
    }
}