using UnityEngine;
using System.Collections;

public class SceneBackgroundMusicController : MonoBehaviour
{
    [Header("背景音乐设置")]
    [SerializeField] private AudioClip backgroundMusic1;    // 场景默认背景音乐
    [SerializeField] private AudioClip backgroundMusic2;    // 按P键后播放的背景音乐
    
    [Header("音频源")]
    [SerializeField] private AudioSource musicSource;      // 专用的背景音乐音频源
    
    [Header("控制设置")]
    [SerializeField] private KeyCode switchKey = KeyCode.P; // 切换音乐的按键
    [SerializeField] private float stopDuration = 5f;      // 停止音乐的持续时间
    
    private AudioVolumeManager audioVolumeManager;         // 音量管理器引用
    private bool isMusic2Playing = false;                  // 是否正在播放音乐2
    private bool isSwitching = false;                      // 是否正在切换过程中
    
    void Start()
    {
        // 查找音量管理器
        audioVolumeManager = FindFirstObjectByType<AudioVolumeManager>();
        if (audioVolumeManager == null)
        {
            Debug.LogWarning("SceneBackgroundMusicController: 未找到AudioVolumeManager，音量控制可能无法正常工作");
        }
        
        // 初始化音频源
        InitializeMusicSource();
        
        // 开始播放背景音乐1
        PlayBackgroundMusic1();
    }
    
    void Update()
    {
        // 检测按键输入
        if (Input.GetKeyDown(switchKey) && !isSwitching)
        {
            if (!isMusic2Playing)
            {
                StartCoroutine(SwitchToMusic2());
            }
            else
            {
                // 如果正在播放音乐2，可以选择切换回音乐1或者什么都不做
                // 这里选择切换回音乐1
                StartCoroutine(SwitchToMusic1());
            }
        }
    }
    
    private void InitializeMusicSource()
    {
        // 如果没有指定音频源，创建一个新的
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("SceneMusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            
            Debug.Log("SceneBackgroundMusicController: 自动创建了音频源");
        }
        
        // 设置音频源属性
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        
        // 应用当前音量设置
        UpdateVolume();
    }
    
    private void UpdateVolume()
    {
        if (audioVolumeManager != null && musicSource != null)
        {
            // 使用AudioVolumeManager的音量设置
            musicSource.volume = audioVolumeManager.GetCurrentVolume();
        }
        else if (musicSource != null)
        {
            // 如果没有音量管理器，使用默认音量
            musicSource.volume = 0.8f;
        }
    }
    
    private void PlayBackgroundMusic1()
    {
        if (backgroundMusic1 == null)
        {
            Debug.LogWarning("SceneBackgroundMusicController: 背景音乐1未设置");
            return;
        }
        
        UpdateVolume();
        musicSource.clip = backgroundMusic1;
        musicSource.Play();
        
        isMusic2Playing = false;
        
        Debug.Log("SceneBackgroundMusicController: 开始播放背景音乐1");
    }
    
    private void PlayBackgroundMusic2()
    {
        if (backgroundMusic2 == null)
        {
            Debug.LogWarning("SceneBackgroundMusicController: 背景音乐2未设置");
            return;
        }
        
        UpdateVolume();
        musicSource.clip = backgroundMusic2;
        musicSource.Play();
        
        isMusic2Playing = true;
        
        Debug.Log("SceneBackgroundMusicController: 开始播放背景音乐2");
    }
    
    private IEnumerator SwitchToMusic2()
    {
        isSwitching = true;
        
        Debug.Log($"SceneBackgroundMusicController: 按下{switchKey}键，停止音乐{stopDuration}秒后播放背景音乐2");
        
        // 停止当前音乐
        musicSource.Stop();
        
        // 等待指定时间
        yield return new WaitForSeconds(stopDuration);
        
        // 播放背景音乐2
        PlayBackgroundMusic2();
        
        isSwitching = false;
    }
    
    private IEnumerator SwitchToMusic1()
    {
        isSwitching = true;
        
        Debug.Log($"SceneBackgroundMusicController: 按下{switchKey}键，停止音乐{stopDuration}秒后播放背景音乐1");
        
        // 停止当前音乐
        musicSource.Stop();
        
        // 等待指定时间
        yield return new WaitForSeconds(stopDuration);
        
        // 播放背景音乐1
        PlayBackgroundMusic1();
        
        isSwitching = false;
    }
    
    // 公共方法：手动切换到音乐1
    public void SwitchToBackgroundMusic1()
    {
        if (!isSwitching)
        {
            StartCoroutine(SwitchToMusic1());
        }
    }
    
    // 公共方法：手动切换到音乐2
    public void SwitchToBackgroundMusic2()
    {
        if (!isSwitching)
        {
            StartCoroutine(SwitchToMusic2());
        }
    }
    
    // 公共方法：立即播放音乐1（无延迟）
    public void PlayMusic1Immediately()
    {
        if (!isSwitching)
        {
            PlayBackgroundMusic1();
        }
    }
    
    // 公共方法：立即播放音乐2（无延迟）
    public void PlayMusic2Immediately()
    {
        if (!isSwitching)
        {
            PlayBackgroundMusic2();
        }
    }
    
    // 公共方法：停止音乐
    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }
    
    // 公共方法：暂停音乐
    public void PauseMusic()
    {
        if (musicSource != null)
        {
            musicSource.Pause();
        }
    }
    
    // 公共方法：恢复音乐
    public void ResumeMusic()
    {
        if (musicSource != null)
        {
            musicSource.UnPause();
        }
    }
    
    // 获取当前播放状态
    public bool IsPlayingMusic2()
    {
        return isMusic2Playing;
    }
    
    public bool IsSwitching()
    {
        return isSwitching;
    }
    
    public bool IsPlaying()
    {
        return musicSource != null && musicSource.isPlaying;
    }
    
    // 在音量改变时调用此方法
    public void OnVolumeChanged()
    {
        UpdateVolume();
    }
    
    // 设置新的音乐片段
    public void SetBackgroundMusic1(AudioClip newClip)
    {
        backgroundMusic1 = newClip;
    }
    
    public void SetBackgroundMusic2(AudioClip newClip)
    {
        backgroundMusic2 = newClip;
    }
    
    // 设置切换按键
    public void SetSwitchKey(KeyCode newKey)
    {
        switchKey = newKey;
    }
    
    // 设置停止持续时间
    public void SetStopDuration(float newDuration)
    {
        stopDuration = newDuration;
    }
    
    void OnDestroy()
    {
        // 清理协程
        StopAllCoroutines();
    }
}
