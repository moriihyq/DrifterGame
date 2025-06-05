using UnityEngine;

/// <summary>
/// 场景背景音乐控制器的使用示例
/// 这个脚本演示了如何设置和使用SceneBackgroundMusicController
/// </summary>
public class BackgroundMusicExample : MonoBehaviour
{
    [Header("示例设置")]
    [SerializeField] private SceneBackgroundMusicController musicController;
    [SerializeField] private AudioClip exampleMusic1;  // 示例音乐1
    [SerializeField] private AudioClip exampleMusic2;  // 示例音乐2
    
    void Start()
    {
        // 如果没有手动分配音乐控制器，尝试查找
        if (musicController == null)
        {
            musicController = FindFirstObjectByType<SceneBackgroundMusicController>();
            
            if (musicController == null)
            {
                Debug.LogWarning("BackgroundMusicExample: 未找到SceneBackgroundMusicController");
                return;
            }
        }
        
        // 设置示例音乐（如果有的话）
        if (exampleMusic1 != null)
        {
            musicController.SetBackgroundMusic1(exampleMusic1);
        }
        
        if (exampleMusic2 != null)
        {
            musicController.SetBackgroundMusic2(exampleMusic2);
        }
        
        Debug.Log("BackgroundMusicExample: 背景音乐系统已初始化");
        Debug.Log($"使用说明: 按 {KeyCode.P} 键来切换背景音乐");
    }
    
    void Update()
    {
        // 额外的快捷键演示
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            musicController.PlayMusic1Immediately();
            Debug.Log("立即播放音乐1");
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            musicController.PlayMusic2Immediately();
            Debug.Log("立即播放音乐2");
        }
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (musicController.IsPlaying())
            {
                musicController.PauseMusic();
                Debug.Log("暂停音乐");
            }
            else
            {
                musicController.ResumeMusic();
                Debug.Log("恢复音乐");
            }
        }
        
        if (Input.GetKeyDown(KeyCode.S))
        {
            musicController.StopMusic();
            Debug.Log("停止音乐");
        }
    }
    
    // UI按钮调用的方法示例
    public void OnPlayMusic1ButtonClick()
    {
        musicController.PlayMusic1Immediately();
    }
    
    public void OnPlayMusic2ButtonClick()
    {
        musicController.PlayMusic2Immediately();
    }
    
    public void OnStopMusicButtonClick()
    {
        musicController.StopMusic();
    }
    
    public void OnPauseMusicButtonClick()
    {
        musicController.PauseMusic();
    }
    
    public void OnResumeMusicButtonClick()
    {
        musicController.ResumeMusic();
    }
}
