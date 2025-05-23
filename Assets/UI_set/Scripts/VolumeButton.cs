using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class VolumeButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("音频管理器引用")]
    [SerializeField] private AudioVolumeManager audioVolumeManager;
    
    [Header("图标精灵")]
    [SerializeField] private Sprite volumeOnIcon;   // 有声音时的图标
    [SerializeField] private Sprite volumeOffIcon;  // 静音时的图标
    
    [Header("视觉反馈")]
    [SerializeField] private float clickScale = 0.9f;  // 点击时的缩放比例
    [SerializeField] private float scaleDuration = 0.1f;  // 缩放动画持续时间
    
    private Image iconImage;
    private bool isMuted = false;
    private Vector3 originalScale;
    
    private void Start()
    {
        // 获取Image组件
        iconImage = GetComponent<Image>();
        if (iconImage == null)
        {
            Debug.LogError("VolumeButton需要一个Image组件！");
            return;
        }
        
        // 记录原始缩放
        originalScale = transform.localScale;
        
        // 自动查找AudioVolumeManager（如果未手动分配）
        if (audioVolumeManager == null)
        {
            audioVolumeManager = FindObjectOfType<AudioVolumeManager>();
            if (audioVolumeManager == null)
            {
                Debug.LogError("场景中未找到AudioVolumeManager！");
            }
        }
        
        // 确保此对象可以接收射线检测
        if (iconImage != null)
        {
            iconImage.raycastTarget = true;
        }
        
        // 初始化图标状态
        UpdateIconState();
    }
    
    // 处理点击事件
    public void OnPointerClick(PointerEventData eventData)
    {
        if (audioVolumeManager == null)
        {
            Debug.LogError("无法切换静音：AudioVolumeManager未设置！");
            return;
        }
        
        // 切换静音状态
        audioVolumeManager.ToggleMute();
        
        // 更新静音状态
        isMuted = !isMuted;
        
        // 更新图标
        UpdateIconState();
        
        // 播放点击动画
        PlayClickAnimation();
        
        // 可选：播放点击音效
        PlayClickSound();
    }
    
    // 实时更新图标状态
    private void Update()
    {
        // 每隔一定时间检查音量状态
        if (Time.frameCount % 30 == 0) // 每30帧检查一次，约0.5秒
        {
            UpdateIconState();
        }
    }
    
    // 更新图标显示
    private void UpdateIconState()
    {
        if (iconImage == null) return;
        
        // 检查当前音量状态
        if (audioVolumeManager != null)
        {
            isMuted = audioVolumeManager.IsMuted();
        }
        
        // 根据静音状态切换图标
        if (volumeOnIcon != null && volumeOffIcon != null)
        {
            iconImage.sprite = isMuted ? volumeOffIcon : volumeOnIcon;
        }
    }
    
    // 播放点击动画
    private void PlayClickAnimation()
    {
        // 取消之前的动画
        StopAllCoroutines();
        
        // 开始新的动画
        StartCoroutine(ScaleAnimation());
    }
    
    // 缩放动画协程
    private System.Collections.IEnumerator ScaleAnimation()
    {
        float elapsedTime = 0f;
        Vector3 targetScale = originalScale * clickScale;
        
        // 缩小
        while (elapsedTime < scaleDuration / 2)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsedTime / (scaleDuration / 2));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 放大回原始大小
        elapsedTime = 0f;
        while (elapsedTime < scaleDuration / 2)
        {
            transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsedTime / (scaleDuration / 2));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 确保回到原始大小
        transform.localScale = originalScale;
    }
    
    // 播放点击音效
    private void PlayClickSound()
    {
        // 获取音效源
        AudioSource sfxSource = GetComponent<AudioSource>();
        if (sfxSource == null)
        {
            // 尝试从AudioVolumeManager获取
            if (audioVolumeManager != null)
            {
                var sources = audioVolumeManager.GetComponentsInChildren<AudioSource>();
                foreach (var source in sources)
                {
                    if (!source.loop) // SFX源通常不循环
                    {
                        sfxSource = source;
                        break;
                    }
                }
            }
        }
        
        // 播放点击音效（如果有的话）
        if (sfxSource != null && sfxSource.clip != null)
        {
            sfxSource.PlayOneShot(sfxSource.clip, 0.5f);
        }
    }
    
    // 当鼠标悬停时的视觉反馈
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (iconImage != null)
        {
            // 稍微提亮图标
            Color currentColor = iconImage.color;
            iconImage.color = new Color(currentColor.r * 1.2f, currentColor.g * 1.2f, currentColor.b * 1.2f, currentColor.a);
        }
    }
    
    // 当鼠标离开时恢复
    public void OnPointerExit(PointerEventData eventData)
    {
        if (iconImage != null)
        {
            // 恢复原始颜色
            iconImage.color = Color.white;
        }
    }
    
    // 在编辑器中更新图标（用于测试）
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            UpdateIconState();
        }
    }
} 