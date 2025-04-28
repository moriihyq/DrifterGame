using UnityEngine;
using UnityEngine.SceneManagement; // 必须引入场景管理命名空间

public class TitleScreenManager : MonoBehaviour
{
    // 在 Inspector 中指定要加载的游戏场景名称
    public string gameSceneName = "YourGameSceneName"; // !!! 修改为你实际的游戏场景名称 !!!

    // 公开方法，用于绑定到“开始游戏”按钮的 OnClick 事件
    public void StartGame()
    {
        Debug.Log("开始加载游戏场景: " + gameSceneName);
        // 异步加载场景可以防止卡顿，并可以显示加载进度条（如果需要）
        // SceneManager.LoadSceneAsync(gameSceneName);
        // 简单直接加载：
        SceneManager.LoadScene(gameSceneName);
    }

    // 公开方法，用于绑定到“选项”按钮的 OnClick 事件
    public void OpenOptions()
    {
        Debug.Log("打开选项菜单（功能待实现）");
        // 这里可以实现：
        // 1. 激活一个隐藏的选项面板 (GameObject.SetActive(true))
        // 2. 加载一个专门的选项场景 (SceneManager.LoadScene("OptionsScene"))
    }

    // 公开方法，用于绑定到“退出游戏”按钮的 OnClick 事件
    public void ExitGame()
    {
        Debug.Log("尝试退出游戏...");
        // 在编辑器模式下停止播放
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        // 在构建好的游戏中退出应用程序
        Application.Quit();
        #endif
    }

}
