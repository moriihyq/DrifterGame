using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AudioVolumeManager : MonoBehaviour
{
    [Header("音频控制")]
    [SerializeField] private Slider volumeSlider;      // 音量滑动条
    [SerializeField] private TextMeshProUGUI volumeValueText;  // 显示音量值的文本

    [Header("音频源")]
    [SerializeField] private AudioSource musicSource;  // 背景音乐
    [SerializeField] private AudioSource sfxSource;    // 音效

    [Header("设置")]
    [SerializeField] private float defaultVolume = 0.8f;  // 默认音量值
    [SerializeField] private string volumeDisplayFormat = "音量: {0}%";  // 音量显示格式
    [SerializeField] private bool saveVolumeSettings = true;  // 是否保存音量设置
    [SerializeField] private string volumePrefsKey = "GameVolume";  // 音量存储的键名

    private void Start()
    {
        // 初始化组件检查
        if (volumeSlider == null)
        {
            Debug.LogError("音量滑动条未分配！");
            return;
        }

        if (volumeValueText == null)
        {
            Debug.LogWarning("音量文本未分配！");
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

    // 当滑动条值改变时调用
    private void OnVolumeChanged(float value)
    {
        ApplyVolume(value);

        // 保存音量设置
        if (saveVolumeSettings)
        {
            SaveVolumeSettings(value);
        }
    }

    // 应用音量到所有音频源
    private void ApplyVolume(float volume)
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

    // 更新音量显示文本
    private void UpdateVolumeDisplay(float volume)
    {
        if (volumeValueText != null)
        {
            int volumePercent = Mathf.RoundToInt(volume * 100);
            volumeValueText.text = string.Format(volumeDisplayFormat, volumePercent);
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
        volumeSlider.value = volume;
        // 不需要手动调用 ApplyVolume，因为 onValueChanged 事件会触发
    }

    // 静音/取消静音功能
    public void ToggleMute()
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
} 