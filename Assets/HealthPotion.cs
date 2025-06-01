using UnityEngine;

public class HealthPotion : MonoBehaviour
{
    [Header("血瓶设置")]
    [SerializeField] private bool showPrompt = true; // 是否显示提示
    [SerializeField] private string promptText = "按 E 键使用血瓶"; // 提示文本
    [SerializeField] private AudioClip drinkSound; // 喝药音效（可选）
    
    private bool playerInRange = false;
    private GameObject player;
    private PlayerAttackSystem playerHealth;
    
    // GUI相关
    private bool showGUI = false;
    
    void Start()
    {
        // 确保血瓶有碰撞器且为Trigger
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            // 如果没有碰撞器，添加一个
            CircleCollider2D circle = gameObject.AddComponent<CircleCollider2D>();
            circle.isTrigger = true;
            circle.radius = 1.0f; // 设置触发范围
            Debug.Log("为血瓶自动添加了CircleCollider2D");
        }
        else
        {
            collider.isTrigger = true; // 确保是触发器
        }
    }
    
    void Update()
    {
        // 检查玩家是否在范围内且按下E键
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            UsePotion();
        }
    }
    
    private void UsePotion()
    {
        if (player == null || playerHealth == null) return;
        
        // 检查玩家是否已满血
        if (playerHealth.GetCurrentHealth() >= playerHealth.MaxHealth)
        {
            Debug.Log("玩家已满血，无需使用血瓶！");
            return;
        }
        
        // 回复玩家全部生命值
        playerHealth.HealToFull();
        
        // 播放喝药动画
        Animator playerAnimator = player.GetComponent<Animator>();
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("dring-potion");
            Debug.Log("播放喝药动画：dring-potion");
        }
        else
        {
            Debug.LogWarning("玩家没有Animator组件，无法播放喝药动画");
        }
        
        // 播放音效（如果有）
        if (drinkSound != null)
        {
            AudioSource.PlayClipAtPoint(drinkSound, transform.position);
        }
        
        Debug.Log($"<color=green>玩家使用了血瓶！生命值已回复至满血: {playerHealth.GetCurrentHealth()}/{playerHealth.MaxHealth}</color>");
        
        // 血瓶消失
        Destroy(gameObject);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            player = other.gameObject;
            playerHealth = player.GetComponent<PlayerAttackSystem>();
            showGUI = showPrompt;
            
            if (playerHealth == null)
            {
                Debug.LogError("玩家对象上没有PlayerAttackSystem组件！");
            }
            else
            {
                Debug.Log("玩家靠近血瓶，按E键使用");
            }
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            player = null;
            playerHealth = null;
            showGUI = false;
            Debug.Log("玩家离开血瓶范围");
        }
    }
    
    // 显示使用提示GUI
    void OnGUI()
    {
        if (showGUI && playerInRange)
        {
            // 计算屏幕位置
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            screenPos.y = Screen.height - screenPos.y; // 转换Y坐标
            
            // 创建GUI样式
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.fontSize = 16;
            style.normal.textColor = Color.white;
            style.normal.background = MakeTexture(2, 2, new Color(0, 0, 0, 0.7f));
            
            // 显示提示文本
            Vector2 textSize = style.CalcSize(new GUIContent(promptText));
            GUI.Box(new Rect(screenPos.x - textSize.x/2, screenPos.y - 60, textSize.x + 20, textSize.y + 10), promptText, style);
        }
    }
    
    // 创建纯色纹理的辅助方法
    private Texture2D MakeTexture(int width, int height, Color color)
    {
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        
        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
    
    // 在编辑器中绘制触发范围
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 1.0f);
    }
}
