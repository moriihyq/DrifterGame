// AutoOpenTreasureChest.cs
using UnityEngine;

public class AutoOpenTreasureChest : MonoBehaviour, IAutoTriggerable
{
    public AudioClip openSound;         // 打开宝箱的音效
    public GameObject openEffectPrefab; // 打开时的粒子效果 (可选)
    public Sprite openedSprite;         // 宝箱打开后的Sprite (如果使用Sprite切换)
    // 或者 public AnimationClip openAnimation; // 如果使用Animation组件播放打开动画

    private bool hasBeenOpened = false;
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer; // 如果使用Sprite切换
    private Animator animator; // 如果使用Animator播放动画

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>(); // 获取Animator组件

        // 确保宝箱的碰撞器是Trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning("Collider on " + gameObject.name + " is not set to IsTrigger. AutoOpenTreasureChest might not work correctly. Forcing IsTrigger to true.");
            col.isTrigger = true; // 强制设为Trigger
        }
        else if (col == null)
        {
            Debug.LogError("No Collider2D found on " + gameObject.name + ". AutoOpenTreasureChest requires a Collider2D set to IsTrigger.");
            enabled = false;
        }
    }

    public void OnTriggerActivated(GameObject activator)
    {
        if (hasBeenOpened) // 如果已经打开过了，不再执行
        {
            return;
        }

        if (activator.CompareTag("Player")) // 确保是玩家触发的
        {
            hasBeenOpened = true;
            Debug.Log("Treasure Chest opened by " + activator.name);

            // 1. 播放打开动画/切换Sprite
            if (animator != null) // 优先使用Animator
            {
                animator.SetTrigger("Open"); // 假设Animator中有一个名为 "Open" 的Trigger参数
            }
            else if (spriteRenderer != null && openedSprite != null)
            {
                spriteRenderer.sprite = openedSprite;
            }

            // 2. 播放音效
            if (openSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(openSound);
            }

            // 3. 播放粒子效果 (可选)
            if (openEffectPrefab != null)
            {
                Instantiate(openEffectPrefab, transform.position, Quaternion.identity);
            }

            // 4. 通知玩家升级
            PlayerStats playerStats = activator.GetComponent<PlayerStats>(); // 假设玩家身上有PlayerStats脚本
            if (playerStats != null)
            {
                playerStats.LevelUp(1); // 调用玩家脚本的LevelUp方法，升1级
            }
            else
            {
                Debug.LogWarning("Player does not have a PlayerStats component. Cannot grant level up.");
            }

            // 5. （可选）禁用宝箱的触发器，防止重复触发或显示交互提示（如果玩家脚本也检测这个）
            // GetComponent<Collider2D>().enabled = false;
            // 或者，如果玩家的交互检测脚本会尝试获取IInteractable，而这个宝箱不应该再显示提示，
            // 可以在打开后移除IAutoTriggerable组件，或通过hasBeenOpened标志让它不再响应。
        }
    }

    // OnTriggerEnter2D负责调用接口方法
    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasBeenOpened) return; // 如果已打开，不重复触发

        // 尝试从进入触发器的对象获取IAutoTriggerable接口的“调用者”
        // 在这个场景下，我们直接判断是否是玩家，然后调用自身的接口方法
        if (other.CompareTag("Player"))
        {
            OnTriggerActivated(other.gameObject);
        }
    }
}
