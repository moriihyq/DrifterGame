using System.Collections;
using UnityEngine;

public class MagicBulletSkill : MonoBehaviour
{    [Header("魔法子弹设置")]
    [SerializeField] private GameObject magicBulletPrefab; // 魔法子弹预制体
    [SerializeField] private int bulletCount = 3; // 每次释放的子弹数量
    [SerializeField] private float cooldownTime = 30f; // 冷却时间(秒)
    [SerializeField] private float bulletLifetime = 5f; // 子弹存在时间
    [SerializeField] private int bulletDamage = 35; // 子弹伤害
    [SerializeField] private float initialSpeed = 8f; // 初始速度
    [SerializeField] private float bulletSize = 1.0f; // 子弹大小倍数
    [SerializeField] private float spreadAngle = 180f; // 子弹散布角度（默认180度，即面向方向前方半圆范围）
    
    [Header("触发器设置")]
    [SerializeField] private string triggerName = "MagicBulletTrigger"; // 触发器名称
    [SerializeField] private bool showTriggerNameOnUse = true; // 使用时是否显示触发器名称

    [Header("技能UI")]
    [SerializeField] private GameObject skillCooldownUI;
    [SerializeField] private UnityEngine.UI.Text cooldownText;
    [SerializeField] private UnityEngine.UI.Image cooldownFillImage;

    [Header("音效")]
    [SerializeField] private AudioClip magicBulletSound;
    private AudioSource audioSource;
    private AudioVolumeManager audioVolumeManager;

    // 私有变量
    private bool isOnCooldown = false;
    private float currentCooldownTime = 0f;
    private Animator playerAnimator;

    void Start()
    {
        // 获取组件
        playerAnimator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // 查找音频音量管理器
        audioVolumeManager = FindFirstObjectByType<AudioVolumeManager>();
        if (audioVolumeManager == null)
        {
            Debug.LogWarning("AudioVolumeManager not found in scene. Magic bullet audio will use default volume.");
        }
        
        // 初始化UI
        InitializeUI();
    }

    void Update()
    {
        // 检测Q键输入
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TryUseSkill();
        }
        
        // 更新冷却时间UI
        UpdateCooldownUI();
    }    void TryUseSkill()
    {
        // 检查是否在冷却中
        if (isOnCooldown)
        {
            Debug.Log($"<color=#FF0000>魔法子弹技能还在冷却中，剩余时间: {currentCooldownTime:F1}秒</color>");
            return;
        }
        
        // 激活触发器并显示名称
        ActivateTrigger();
        
        // 执行技能
        Debug.Log($"<color=#00FF00>释放魔法子弹技能！</color>");
        CastMagicBullets();
        
        // 开始冷却
        StartCooldown();
    }
    
    void ActivateTrigger()
    {
        // 显示触发器名称
        if (showTriggerNameOnUse)
        {
            Debug.Log($"<color=#00FFFF>触发器 '{triggerName}' 已激活！</color>");
            
            // 在玩家头顶显示触发器名称
            StartCoroutine(ShowFloatingText());
        }
        
        // 如果有设置动画器触发器，则触发它
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger(triggerName);
        }
    }
    
    IEnumerator ShowFloatingText()
    {
        // 创建一个临时文本对象
        GameObject textObj = new GameObject("TriggerNameText");
        textObj.transform.position = transform.position + Vector3.up * 2f;
        
        // 添加Text组件
        UnityEngine.UI.Text textComponent = textObj.AddComponent<UnityEngine.UI.Text>();
        textComponent.text = triggerName;
        textComponent.color = Color.cyan;
        textComponent.fontSize = 24;
        textComponent.alignment = TextAnchor.MiddleCenter;
        
        // 添加Canvas和CanvasRenderer组件
        Canvas canvas = textObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        textObj.AddComponent<CanvasRenderer>();
        
        // 设置Canvas大小
        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(200, 50);
        
        // 文本动画
        float duration = 2.0f;
        float timer = 0f;
        Vector3 startPosition = textObj.transform.position;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = 1.0f - (timer / duration);
            textComponent.color = new Color(textComponent.color.r, textComponent.color.g, textComponent.color.b, alpha);
            textObj.transform.position = startPosition + Vector3.up * timer * 0.5f;
            yield return null;
        }
        
        // 销毁文本对象
        Destroy(textObj);
    }    // 获取玩家面向方向（1=右，-1=左）
    private float GetPlayerFacingDirection()
    {
        // 方法1：通过SpriteRenderer.flipX判断
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            // flipX=true表示面向左，flipX=false表示面向右
            return sr.flipX ? -1f : 1f;
        }
        
        // 方法2：通过localScale.x判断（备用方法）
        return transform.localScale.x > 0 ? 1f : -1f;
    }
    
    void CastMagicBullets()
    {
        // 播放动画
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("MagicAttack");
        }
          // 播放音效
        if (audioSource != null && magicBulletSound != null)
        {
            float finalVolume = 1.0f;            if (audioVolumeManager != null)
            {
                finalVolume *= audioVolumeManager.GetCurrentVolume();
            }
            audioSource.PlayOneShot(magicBulletSound, finalVolume);
        }
          // 获取玩家面向方向
        float facingDirection = GetPlayerFacingDirection();
        
        // 确定基准角度（面向右=0度，面向左=180度）
        float baseAngle = facingDirection > 0 ? 0 : 180;
        
        // 计算散布角度的一半（用于确定随机角度的范围）
        float halfSpreadAngle = Mathf.Clamp(spreadAngle, 0f, 360f) * 0.5f;
        
        // 生成魔法子弹
        for (int i = 0; i < bulletCount; i++)
        {
            // 设置随机初始方向（在面向方向的散布角度范围内）
            float randomAngle = baseAngle + Random.Range(-halfSpreadAngle, halfSpreadAngle);
            float radians = randomAngle * Mathf.Deg2Rad;
            Vector2 randomDirection = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
            
            // 生成子弹
            GameObject bulletObj = Instantiate(magicBulletPrefab, transform.position, Quaternion.identity);
            
            // 设置子弹大小
            bulletObj.transform.localScale = new Vector3(bulletSize, bulletSize, bulletSize);
            
            // 配置魔法子弹行为
            MagicBullet bulletScript = bulletObj.AddComponent<MagicBullet>();
            if (bulletScript != null)
            {
                bulletScript.Initialize(initialSpeed, randomDirection, bulletDamage, bulletLifetime);
                
                // 调试信息
                Debug.Log($"<color=#00FFFF>生成魔法子弹 {i+1}/{bulletCount}: 角度={randomAngle}°, 方向=({randomDirection.x:F2}, {randomDirection.y:F2})</color>");
            }
            else
            {
                Debug.LogError("无法添加MagicBullet组件到子弹预制体！");
                Destroy(bulletObj);
            }
        }
    }

    void StartCooldown()
    {
        isOnCooldown = true;
        currentCooldownTime = cooldownTime;
        
        StartCoroutine(CooldownCoroutine());
    }

    IEnumerator CooldownCoroutine()
    {
        while (currentCooldownTime > 0)
        {
            yield return new WaitForSeconds(0.1f);
            currentCooldownTime -= 0.1f;
        }
        
        isOnCooldown = false;
        currentCooldownTime = 0f;
        
        Debug.Log("<color=#00FF00>魔法子弹技能冷却完成！</color>");
    }

    void InitializeUI()
    {
        // 如果没有指定UI元素，尝试自动查找
        if (skillCooldownUI == null)
        {
            skillCooldownUI = GameObject.Find("MagicBulletCooldownUI");
        }
        
        if (cooldownText == null && skillCooldownUI != null)
        {
            cooldownText = skillCooldownUI.transform.Find("CooldownText")?.GetComponent<UnityEngine.UI.Text>();
        }
        
        if (cooldownFillImage == null && skillCooldownUI != null)
        {
            cooldownFillImage = skillCooldownUI.transform.Find("CooldownFill")?.GetComponent<UnityEngine.UI.Image>();
        }
    }

    void UpdateCooldownUI()
    {
        if (skillCooldownUI == null) return;
        
        // 根据冷却状态显示/隐藏UI
        if (isOnCooldown)
        {
            skillCooldownUI.SetActive(true);
            
            // 更新冷却时间文本
            if (cooldownText != null)
            {
                cooldownText.text = $"魔法子弹: {currentCooldownTime:F1}s";
            }
            
            // 更新冷却填充图像
            if (cooldownFillImage != null)
            {
                float fillAmount = currentCooldownTime / cooldownTime;
                cooldownFillImage.fillAmount = fillAmount;
            }
        }
        else
        {
            skillCooldownUI.SetActive(false);
        }
    }      // 可视化检测范围（仅在Scene视图中显示）
    void OnDrawGizmosSelected()
    {
        // 绘制释放位置
        Gizmos.color = isOnCooldown ? Color.red : Color.magenta;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        // 显示技能信息
        #if UNITY_EDITOR
        UnityEditor.Handles.color = isOnCooldown ? Color.red : Color.white;
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f,
            $"魔法子弹技能\n状态: {(isOnCooldown ? $"冷却中 ({currentCooldownTime:F1}s)" : "就绪")}\n子弹数: {bulletCount}\n触发器: {triggerName}");
          // 绘制子弹可能的发射方向范围
        float facingDirection = GetPlayerFacingDirection();
        float baseAngle = facingDirection > 0 ? 0 : 180;
        float halfSpreadAngle = Mathf.Clamp(spreadAngle, 0f, 360f) * 0.5f;
        
        // 绘制扇形表示发射范围
        UnityEditor.Handles.color = new Color(0f, 1f, 1f, 0.2f); // 青色半透明
        Vector3 center = transform.position;
        float radius = 2f;
        
        // 绘制扇形填充
        Vector3[] points = new Vector3[37]; // 36个分段 + 中心点
        points[0] = center;
        for (int i = 0; i < 36; i++)
        {
            float angle = baseAngle + Mathf.Lerp(-halfSpreadAngle, halfSpreadAngle, i / 35f);
            float rad = angle * Mathf.Deg2Rad;
            points[i+1] = center + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * radius;
        }
        UnityEditor.Handles.DrawAAConvexPolygon(points);
        
        // 绘制扇形轮廓
        UnityEditor.Handles.color = new Color(0f, 1f, 1f, 0.8f); // 青色不透明
        UnityEditor.Handles.DrawWireArc(center, Vector3.forward, 
            new Vector3(Mathf.Cos((baseAngle-halfSpreadAngle) * Mathf.Deg2Rad), Mathf.Sin((baseAngle-halfSpreadAngle) * Mathf.Deg2Rad), 0), 
            spreadAngle, radius);
        UnityEditor.Handles.DrawLine(center, center + new Vector3(Mathf.Cos((baseAngle-halfSpreadAngle) * Mathf.Deg2Rad), Mathf.Sin((baseAngle-halfSpreadAngle) * Mathf.Deg2Rad), 0) * radius);
        UnityEditor.Handles.DrawLine(center, center + new Vector3(Mathf.Cos((baseAngle+halfSpreadAngle) * Mathf.Deg2Rad), Mathf.Sin((baseAngle+halfSpreadAngle) * Mathf.Deg2Rad), 0) * radius);
        #endif
    }
}
