using UnityEngine;
using UnityEngine.UI; // 需要 UI 命名空间
using System.Collections; // 需要 Coroutine 命名空间

public class FlickerEffectVisibility : MonoBehaviour
{
    // 将顶层的、亮的 Image 组件拖拽到这里
    public Image imageOnTop;

    [Header("Flicker Timing (Random Range)")]
    public float minVisibleTime = 0.05f;  // 亮状态最短持续时间
    public float maxVisibleTime = 0.2f;   // 亮状态最长持续时间
    public float minInvisibleTime = 0.1f; // 暗状态最短持续时间
    public float maxInvisibleTime = 1.5f; // 暗状态最长持续时间

    private Color colorVisible;     // 存储 Alpha 为 1 的颜色
    private Color colorInvisible;   // 存储 Alpha 为 0 的颜色
    private Coroutine flickerCoroutine;

    void Start()
    {
        if (imageOnTop == null)
        {
            Debug.LogError("FlickerEffectVisibility: 请在 Inspector 中指定顶层的 Image 组件 (Image On Top)!");
            enabled = false; // 禁用脚本如果设置不完整
            return;
        }

        // 存储颜色状态，确保初始颜色不受影响
        colorVisible = imageOnTop.color;
        colorVisible.a = 1f; // 完全可见
        colorInvisible = imageOnTop.color;
        colorInvisible.a = 0f; // 完全不可见

        // 设置初始状态为不可见 (透明)
        imageOnTop.color = colorInvisible;

        // 启动闪烁循环
        flickerCoroutine = StartCoroutine(FlickerLoop());
    }

    IEnumerator FlickerLoop()
    {
        while (true) // 无限循环
        {
            // --- 使顶层图片可见 ---
            imageOnTop.color = colorVisible;
            // 计算可见持续时间
            float visibleDuration = Random.Range(minVisibleTime, maxVisibleTime);
            yield return new WaitForSeconds(visibleDuration);

            // --- 使顶层图片不可见 ---
            imageOnTop.color = colorInvisible;
            // 计算不可见持续时间
            float invisibleDuration = Random.Range(minInvisibleTime, maxInvisibleTime);
            yield return new WaitForSeconds(invisibleDuration);
        }
    }

    void OnDisable()
    {
        // 停止协程，避免在对象禁用或销毁后继续运行
        if (flickerCoroutine != null)
        {
            StopCoroutine(flickerCoroutine);
        }

        // (可选) 恢复到初始不可见状态
        if (imageOnTop != null)
        {
            imageOnTop.color = colorInvisible;
        }
    }

    // --- 备选方案：切换 Enabled 状态 ---
    // 如果你不想控制 Alpha，也可以直接开关 Image 组件：
    /*
    IEnumerator FlickerLoopEnabledToggle()
    {
        while (true)
        {
            imageOnTop.enabled = true; // 显示顶层
            float visibleDuration = Random.Range(minVisibleTime, maxVisibleTime);
            yield return new WaitForSeconds(visibleDuration);

            imageOnTop.enabled = false; // 隐藏顶层
            float invisibleDuration = Random.Range(minInvisibleTime, maxInvisibleTime);
            yield return new WaitForSeconds(invisibleDuration);
        }
    }
    // 如果使用这个方法，Start() 中就不需要处理 Color，直接设置 imageOnTop.enabled = false;
    // 并在 OnDisable() 中也设置 imageOnTop.enabled = false;
    */
}
