using UnityEngine;
using UnityEngine.InputSystem; // 🍏 新しいInput Systemを使うためにこれを追加！

public class ComboHelpController : MonoBehaviour
{
    private bool canClose = false;

    // パネルが表示（SetActive(true)）された瞬間に動く関数
    private void OnEnable()
    {
        // 開いた瞬間のクリックで即座に閉じてしまうのを防ぐ
        canClose = false;
        
        // 0.1秒後に閉じられるようにする
        Invoke(nameof(EnableClose), 0.1f);
    }

    private void EnableClose()
    {
        canClose = true;
    }

    void Update()
    {
        if (!canClose) return;

        bool isPressed = false;

        // 🍏 新しいInput Systemでのマウス左クリック判定
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            isPressed = true;
        }
        // 🍏 スマホなどの画面タップ判定
        else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            isPressed = true;
        }

        // どこかが押されていたらパネルを閉じる
        if (isPressed)
        {
            gameObject.SetActive(false);
            Debug.Log("[ComboHelp] 新方式の入力で画面クリックを検知し、説明を閉じました。");
        }
    }
}