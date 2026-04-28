using UnityEngine;
using UnityEngine.SceneManagement; // シーン遷移に必須

public class MapSceneManager : MonoBehaviour
{
    // イベントマスのボタン（またはオブジェクト）から呼ばれるメソッド
    public void GoToEventScene()
    {
        Debug.Log("イベントマスを踏みました！イベントシーンへ遷移します。");
        // "Event" の部分は、実際のEventシーンの名前に合わせて変更してください
        SceneManager.LoadScene("Event"); 
    }
}