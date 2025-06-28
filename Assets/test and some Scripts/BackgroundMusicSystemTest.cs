using UnityEngine;
using System.Collections;

/// <summary>
/// 场景背景音乐控制系统的测试脚本
/// 用于验证系统功能是否正常工作
/// </summary>
public class BackgroundMusicSystemTest : MonoBehaviour
{
    [Header("测试设置")]
    [SerializeField] private bool runTestsOnStart = true;
    [SerializeField] private bool showDetailedLogs = true;
    
    private SceneBackgroundMusicController musicController;
    private AudioVolumeManager audioVolumeManager;
    
    void Start()
    {
        if (runTestsOnStart)
        {
            StartCoroutine(RunAllTests());
        }
    }
    
    private IEnumerator RunAllTests()
    {
        LogTest("=== 开始背景音乐系统测试 ===");
        
        // 测试1: 系统组件检测
        yield return StartCoroutine(TestSystemComponents());
        
        // 测试2: 基础播放功能
        yield return StartCoroutine(TestBasicPlayback());
        
        // 测试3: 按键响应
        yield return StartCoroutine(TestKeyResponse());
        
        // 测试4: 音量控制集成
        yield return StartCoroutine(TestVolumeIntegration());
        
        // 测试5: 状态查询
        yield return StartCoroutine(TestStatusQueries());
        
        LogTest("=== 背景音乐系统测试完成 ===");
        GenerateTestReport();
    }
    
    private IEnumerator TestSystemComponents()
    {
        LogTest("--- 测试1: 系统组件检测 ---");
        
        // 查找背景音乐控制器
        musicController = FindFirstObjectByType<SceneBackgroundMusicController>();
        if (musicController != null)
        {
            LogTest("✓ SceneBackgroundMusicController 找到");
        }
        else
        {
            LogTest("✗ SceneBackgroundMusicController 未找到");
            yield break;
        }
        
        // 查找音量管理器
        audioVolumeManager = FindFirstObjectByType<AudioVolumeManager>();
        if (audioVolumeManager != null)
        {
            LogTest("✓ AudioVolumeManager 找到");
        }
        else
        {
            LogTest("⚠ AudioVolumeManager 未找到（音量控制可能无法正常工作）");
        }
        
        yield return new WaitForSeconds(1f);
    }
    
    private IEnumerator TestBasicPlayback()
    {
        LogTest("--- 测试2: 基础播放功能 ---");
        
        if (musicController == null) yield break;
        
        // 测试立即播放音乐1
        LogTest("测试立即播放音乐1...");
        musicController.PlayMusic1Immediately();
        yield return new WaitForSeconds(2f);
        
        if (musicController.IsPlaying() && !musicController.IsPlayingMusic2())
        {
            LogTest("✓ 音乐1播放正常");
        }
        else
        {
            LogTest("✗ 音乐1播放失败");
        }
        
        // 测试立即播放音乐2
        LogTest("测试立即播放音乐2...");
        musicController.PlayMusic2Immediately();
        yield return new WaitForSeconds(2f);
        
        if (musicController.IsPlaying() && musicController.IsPlayingMusic2())
        {
            LogTest("✓ 音乐2播放正常");
        }
        else
        {
            LogTest("✗ 音乐2播放失败");
        }
        
        // 测试暂停和恢复
        LogTest("测试暂停功能...");
        musicController.PauseMusic();
        yield return new WaitForSeconds(1f);
        
        if (!musicController.IsPlaying())
        {
            LogTest("✓ 暂停功能正常");
        }
        else
        {
            LogTest("✗ 暂停功能失败");
        }
        
        LogTest("测试恢复功能...");
        musicController.ResumeMusic();
        yield return new WaitForSeconds(1f);
        
        if (musicController.IsPlaying())
        {
            LogTest("✓ 恢复功能正常");
        }
        else
        {
            LogTest("✗ 恢复功能失败");
        }
    }
    
    private IEnumerator TestKeyResponse()
    {
        LogTest("--- 测试3: 按键响应 ---");
        LogTest("请在5秒内按P键测试按键响应...");
        
        if (musicController == null) yield break;
        
        bool keyPressed = false;
        float waitTime = 5f;
        float elapsed = 0f;
        
        while (elapsed < waitTime)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                keyPressed = true;
                LogTest("✓ 检测到P键按下");
                break;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (!keyPressed)
        {
            LogTest("⚠ 未检测到P键按下（可能需要手动测试）");
        }
        
        yield return new WaitForSeconds(1f);
    }
    
    private IEnumerator TestVolumeIntegration()
    {
        LogTest("--- 测试4: 音量控制集成 ---");
        
        if (audioVolumeManager == null)
        {
            LogTest("⚠ 跳过音量测试（AudioVolumeManager不存在）");
            yield break;
        }
        
        // 保存原始音量
        float originalVolume = audioVolumeManager.GetCurrentVolume();
        
        // 测试音量改变
        LogTest("测试音量控制...");
        audioVolumeManager.SetVolume(0.5f);
        yield return new WaitForSeconds(1f);
        
        LogTest($"当前音量: {audioVolumeManager.GetCurrentVolume()}");
        
        // 恢复原始音量
        audioVolumeManager.SetVolume(originalVolume);
        yield return new WaitForSeconds(1f);
        
        LogTest("✓ 音量控制集成测试完成");
    }
    
    private IEnumerator TestStatusQueries()
    {
        LogTest("--- 测试5: 状态查询 ---");
        
        if (musicController == null) yield break;
        
        // 播放音乐1并查询状态
        musicController.PlayMusic1Immediately();
        yield return new WaitForSeconds(1f);
        
        LogTest($"是否正在播放: {musicController.IsPlaying()}");
        LogTest($"是否正在播放音乐2: {musicController.IsPlayingMusic2()}");
        LogTest($"是否正在切换: {musicController.IsSwitching()}");
        
        // 播放音乐2并查询状态
        musicController.PlayMusic2Immediately();
        yield return new WaitForSeconds(1f);
        
        LogTest($"切换后 - 是否正在播放: {musicController.IsPlaying()}");
        LogTest($"切换后 - 是否正在播放音乐2: {musicController.IsPlayingMusic2()}");
        
        LogTest("✓ 状态查询测试完成");
    }
    
    private void GenerateTestReport()
    {
        LogTest("\n=== 测试报告 ===");
        LogTest($"测试时间: {System.DateTime.Now}");
        LogTest($"SceneBackgroundMusicController: {(musicController != null ? "正常" : "缺失")}");
        LogTest($"AudioVolumeManager: {(audioVolumeManager != null ? "正常" : "缺失")}");
        
        if (musicController != null)
        {
            LogTest($"当前播放状态: {(musicController.IsPlaying() ? "播放中" : "已停止")}");
            LogTest($"当前音乐: {(musicController.IsPlayingMusic2() ? "音乐2" : "音乐1")}");
        }
        
        LogTest("建议: 如果发现问题，请检查音频片段是否已正确分配");
        LogTest("===================");
    }
    
    private void LogTest(string message)
    {
        if (showDetailedLogs)
        {
            Debug.Log($"[BackgroundMusicTest] {message}");
        }
    }
    
    // 手动测试方法
    [ContextMenu("Run Manual Test")]
    public void RunManualTest()
    {
        StartCoroutine(RunAllTests());
    }
    
    [ContextMenu("Test Key Response")]
    public void TestKeyResponseManual()
    {
        StartCoroutine(TestKeyResponse());
    }
    
    [ContextMenu("Test Basic Playback")]
    public void TestBasicPlaybackManual()
    {
        StartCoroutine(TestBasicPlayback());
    }
    
    void Update()
    {
        // 额外的实时测试快捷键
        if (Input.GetKeyDown(KeyCode.T))
        {
            LogTest("手动测试触发 - 播放音乐1");
            if (musicController != null)
            {
                musicController.PlayMusic1Immediately();
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Y))
        {
            LogTest("手动测试触发 - 播放音乐2");
            if (musicController != null)
            {
                musicController.PlayMusic2Immediately();
            }
        }
        
        if (Input.GetKeyDown(KeyCode.U))
        {
            LogTest("手动测试触发 - 停止音乐");
            if (musicController != null)
            {
                musicController.StopMusic();
            }
        }
    }
}
