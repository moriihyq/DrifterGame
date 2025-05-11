using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class AudioVolumeManager : MonoBehaviour
{
    [Header("音频控制")]
    [SerializeField] private Slider volumeSlider;      // 音量滑动条
    [SerializeField] private TextMeshProUGUI volumeValueText;  // 显示音量值的文本

    [Header("音频源")]
    [SerializeField] private AudioSource musicSource;  // 背景音乐
    [SerializeField] private AudioSource sfxSource;    // 音效
    
    // 预设音频片段，防止类型转换错误
    [Header("预设音频")]
    [SerializeField] private AudioClip defaultMusicClip;  // 默认背景音乐片段
    [SerializeField] private AudioClip defaultSfxClip;    // 默认音效片段

    [Header("设置")]
    [SerializeField] private float defaultVolume = 0.8f;  // 默认音量值
    [SerializeField] private bool displayAsInteger = true;  // 是否将音量显示为整数
    [SerializeField] private bool addPercentSign = true;   // 是否添加百分号
    [SerializeField] private bool saveVolumeSettings = true;  // 是否保存音量设置
    [SerializeField] private string volumePrefsKey = "GameVolume";  // 音量存储的键名

    private void Awake()
    {
        // 检查并初始化音频源
        InitializeAudioSources();
    }

    private void Start()
    {
        // 初始化组件检查
        if (volumeSlider == null)
        {
            Debug.LogError("音量滑动条未分配！");
            return;
        }

        // 确保滑动条可交互并设置正确的导航
        ConfigureSlider();

        if (volumeValueText == null)
        {
            Debug.LogWarning("音量文本未分配！");
        }
        else
        {
            // 确保文本组件使用正确的字体设置
            ConfigureTextComponent();
        }

        // 从PlayerPrefs加载上次的音量设置，如果没有则使用默认值
        float startVolume = saveVolumeSettings ? LoadVolumeSettings() : defaultVolume;

        // 设置初始值
        volumeSlider.value = startVolume;

        // 添加监听器
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        // 初始化应用音量
        ApplyVolume(startVolume);
    }
    
    // 初始化音频源，确保它们有有效的类型
    private void InitializeAudioSources()
    {
        // 检查并初始化音乐源
        if (musicSource == null)
        {
            Debug.LogWarning("未分配背景音乐源，将自动创建一个");
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
        }
        
        // 检查并初始化音效源
        if (sfxSource == null)
        {
            Debug.LogWarning("未分配音效源，将自动创建一个");
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
        }
        
        // 设置默认音频片段（如果有）
        if (defaultMusicClip != null && musicSource.clip == null)
        {
            musicSource.clip = defaultMusicClip;
        }
        
        if (defaultSfxClip != null && sfxSource.clip == null)
        {
            sfxSource.clip = defaultSfxClip;
        }
    }

    // 安全播放背景音乐
    public void PlayMusic(AudioClip clip)
    {
        if (musicSource == null)
        {
            Debug.LogError("无法播放音乐：音乐源未初始化");
            return;
        }
        
        if (clip == null)
        {
            Debug.LogError("无法播放音乐：音频片段为空");
            return;
        }
        
        try
        {
            musicSource.clip = clip;
            musicSource.Play();
        }
        catch (Exception e)
        {
            Debug.LogError($"播放音乐时出错: {e.Message}");
        }
    }
    
    // 安全播放音效
    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null)
        {
            Debug.LogError("无法播放音效：音效源未初始化");
            return;
        }
        
        if (clip == null)
        {
            Debug.LogError("无法播放音效：音频片段为空");
            return;
        }
        
        try
        {
            sfxSource.clip = clip;
            sfxSource.Play();
        }
        catch (Exception e)
        {
            Debug.LogError($"播放音效时出错: {e.Message}");
        }
    }
    
    // 从资源加载并播放音乐
    public void PlayMusicFromResources(string resourcePath)
    {
        try
        {
            AudioClip clip = Resources.Load<AudioClip>(resourcePath);
            if (clip == null)
            {
                Debug.LogError($"无法加载音乐：资源路径 '{resourcePath}' 无效或不是AudioClip类型");
                return;
            }
            
            PlayMusic(clip);
        }
        catch (Exception e)
        {
            Debug.LogError($"从资源加载音乐时出错: {e.Message}");
        }
    }
    
    // 从资源加载并播放音效
    public void PlaySFXFromResources(string resourcePath)
    {
        try
        {
            AudioClip clip = Resources.Load<AudioClip>(resourcePath);
            if (clip == null)
            {
                Debug.LogError($"无法加载音效：资源路径 '{resourcePath}' 无效或不是AudioClip类型");
                return;
            }
            
            PlaySFX(clip);
        }
        catch (Exception e)
        {
            Debug.LogError($"从资源加载音效时出错: {e.Message}");
        }
    }

    private void OnEnable()
    {
        // 确保启用时滑动条交互性正确
        if (volumeSlider != null)
        {
            ConfigureSlider();
        }
    }

    private void Update()
    {
        // 检查并确保滑动条在运行时保持可交互性
        if (volumeSlider != null && !volumeSlider.interactable)
        {
            ConfigureSlider();
        }
    }

    // 配置滑动条，确保其可交互性
    private void ConfigureSlider()
    {
        if (volumeSlider != null)
        {
            // 确保滑动条可交互
            volumeSlider.interactable = true;
            
            // 设置滑动条导航为自动
            Navigation nav = volumeSlider.navigation;
            nav.mode = Navigation.Mode.Automatic;
            volumeSlider.navigation = nav;
            
            // 确保滑动条句柄可以正确接收射线
            Image sliderHandle = volumeSlider.transform.Find("Handle Slide Area/Handle")?.GetComponent<Image>();
            if (sliderHandle != null)
            {
                sliderHandle.raycastTarget = true;
            }
            
            // 确保滑动条填充区域不会阻挡交互
            Image fillRect = volumeSlider.fillRect?.GetComponent<Image>();
            if (fillRect != null)
            {
                fillRect.raycastTarget = false;
            }
        }
    }

    // 配置文本组件的字体设置
    private void ConfigureTextComponent()
    {
        if (volumeValueText != null)
        {
            // 确保文本使用ASCII字符集，避免特殊字符
            volumeValueText.richText = false;
            
            // 使用默认对齐方式
            volumeValueText.alignment = TextAlignmentOptions.Center;
            
            // 确保字体大小合适
            if (volumeValueText.fontSize <= 0)
            {
                volumeValueText.fontSize = 24;
            }
        }
    }

    // 当滑动条值改变时调用
    private void OnVolumeChanged(float value)
    {
        try
        {
            ApplyVolume(value);

            // 保存音量设置
            if (saveVolumeSettings)
            {
                SaveVolumeSettings(value);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"更改音量时出错: {e.Message}");
        }
    }

    // 应用音量到所有音频源
    private void ApplyVolume(float volume)
    {
        try
        {
            // 更新音频源音量
            if (musicSource != null)
            {
                musicSource.volume = volume;
            }

            if (sfxSource != null)
            {
                sfxSource.volume = volume;
            }

            // 更新UI显示
            UpdateVolumeDisplay(volume);
        }
        catch (Exception e)
        {
            Debug.LogError($"应用音量时出错: {e.Message}");
        }
    }

    // 更新音量显示文本
    private void UpdateVolumeDisplay(float volume)
    {
        if (volumeValueText != null)
        {
            string volumeText;
            
            if (displayAsInteger)
            {
                // 显示为整数
                int volumePercent = Mathf.RoundToInt(volume * 100);
                volumeText = volumePercent.ToString();
            }
            else
            {
                // 显示为小数
                volumeText = (volume * 100).ToString("F0");
            }
            
            // 添加百分号（如果需要）
            if (addPercentSign)
            {
                volumeText += "%";
            }
            
            // 设置文本
            volumeValueText.text = volumeText;
        }
    }

    // 保存音量设置
    private void SaveVolumeSettings(float volume)
    {
        PlayerPrefs.SetFloat(volumePrefsKey, volume);
        PlayerPrefs.Save();
    }

    // 加载音量设置
    private float LoadVolumeSettings()
    {
        return PlayerPrefs.HasKey(volumePrefsKey) ? 
               PlayerPrefs.GetFloat(volumePrefsKey) : 
               defaultVolume;
    }

    // 公共方法，用于外部调用设置音量
    public void SetVolume(float volume)
    {
        try
        {
            // 确保音量值在合法范围内
            volume = Mathf.Clamp01(volume);
            
            if (volumeSlider != null)
            {
                volumeSlider.value = volume;
                // 不需要手动调用 ApplyVolume，因为 onValueChanged 事件会触发
            }
            else
            {
                // 滑动条为空，直接应用音量
                ApplyVolume(volume);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"设置音量时出错: {e.Message}");
            
            // 尝试恢复
            try
            {
                if (musicSource != null)
                {
                    musicSource.volume = volume;
                }
                
                if (sfxSource != null)
                {
                    sfxSource.volume = volume;
                }
            }
            catch (Exception recoveryEx)
            {
                Debug.LogError($"尝试恢复音量时出错: {recoveryEx.Message}");
            }
        }
    }

    // 静音/取消静音功能
    public void ToggleMute()
    {
        try
        {
            if (volumeSlider.value > 0)
            {
                // 存储当前音量并设置为0
                PlayerPrefs.SetFloat(volumePrefsKey + "_last", volumeSlider.value);
                SetVolume(0);
            }
            else
            {
                // 恢复上次的音量，如果没有则使用默认值
                float lastVolume = PlayerPrefs.HasKey(volumePrefsKey + "_last") ? 
                                  PlayerPrefs.GetFloat(volumePrefsKey + "_last") : 
                                  defaultVolume;
                SetVolume(lastVolume);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"切换静音状态时出错: {e.Message}");
        }
    }

    // 用于从外部重置滑动条交互性的公共方法
    public void ResetSliderInteraction()
    {
        ConfigureSlider();
    }
} 