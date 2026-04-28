using UnityEngine;
using UnityEngine.SceneManagement;

public class MockMapManager : MonoBehaviour
{
    [Header("遷移先のイベントシーン名")]
    public string eventSceneName = "Event"; // 実際のイベントシーン名に合わせてください

    // UIボタンから呼び出すためのメソッド
    public void GoToEvent()
    {
        Debug.Log("【テスト用】イベントシーンへ遷移します！");
        SceneManager.LoadScene(eventSceneName);
    }
}