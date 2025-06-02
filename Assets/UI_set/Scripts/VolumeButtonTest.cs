using UnityEngine;

using UnityEngine.UI;
public class VolumeButtonTest : MonoBehaviour
{
    [Header("测试设置")]
    [SerializeField] private VolumeButton volumeButton;
    [SerializeField] private AudioVolumeManager audioManager;
    
    [Header("测试音频")]
    [SerializeField] private AudioClip testClip;
    
    private void Start()
    {
        // 自动查找组件
        if (volumeButton == null)
        {
            volumeButton = FindFirstObjectByType<VolumeButton>();
        }
        
        if (audioManager == null)
        {
            audioManager = FindFirstObjectByType<AudioVolumeManager>();
        }
        
        Debug.Log("VolumeButton测试脚本已启动");
    }
    
    private void Update()
    {
        // 按下空格键测试静音切换
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TestToggleMute();
        }
        
        // 按下P键播放测试音频
        if (Input.GetKeyDown(KeyCode.P))
        {
            PlayTestSound();
        }
        
        // 按下V键显示当前音量
        if (Input.GetKeyDown(KeyCode.V))
        {
            ShowCurrentVolume();
        }
    }
    
    private void TestToggleMute()
    {
        if (audioManager != null)
        {
            audioManager.ToggleMute();
            Debug.Log("已切换静音状态");
        }
        else
        {
            Debug.LogError("AudioVolumeManager未找到！");
        }
    }
    
    private void PlayTestSound()
    {
        if (testClip != null && audioManager != null)
        {
            audioManager.PlaySFX(testClip);
            Debug.Log("播放测试音频");
        }
        else
        {
            Debug.LogWarning("无法播放测试音频：testClip或audioManager为空");
        }
    }
    
    private void ShowCurrentVolume()
    {
        if (audioManager != null)
        {
            float volume = audioManager.GetCurrentVolume();
            bool isMuted = audioManager.IsMuted();
            Debug.Log($"当前音量: {volume * 100}% | 静音状态: {isMuted}");
        }
    }
    
    private void OnGUI()
    {
        // 在屏幕上显示提示信息
        GUI.Label(new Rect(10, 10, 300, 20), "按空格键切换静音");
        GUI.Label(new Rect(10, 30, 300, 20), "按P键播放测试音频");
        GUI.Label(new Rect(10, 50, 300, 20), "按V键显示当前音量");
        
        if (audioManager != null)
        {
            float volume = audioManager.GetCurrentVolume();
            bool isMuted = audioManager.IsMuted();
            GUI.Label(new Rect(10, 80, 300, 20), $"音量: {volume * 100:F0}%");
            GUI.Label(new Rect(10, 100, 300, 20), $"静音: {(isMuted ? "是" : "否")}");
        }
    }
} 