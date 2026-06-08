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

    // ★修正：引数に customColor を追加して外から色を指定できるようにする
    public void Setup(int damageAmount, Color customColor)
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
        
        textMesh.text = damageAmount.ToString();
        
        // ★もらった色をテキストに適用する
        textMesh.color = customColor;
        textColor = customColor;

        // 出現位置を左右に少しだけランダムにズラす
        rectTransform.anchoredPosition += new Vector2(Random.Range(-40f, 40f), Random.Range(-10f, 10f));
        
        // 指定した秒数後に消滅
        Destroy(gameObject, destroyTime);
    }

    void Update()
    {
        // 座標（anchoredPosition）を毎フレーム上に移動させる
        rectTransform.anchoredPosition += Vector2.up * floatSpeed * Time.deltaTime;

        // 毎フレーム少しずつ透明にする
        textColor.a -= fadeSpeed * Time.deltaTime;
        textMesh.color = textColor;
    }
}