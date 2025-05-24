using UnityEngine;
using System.Collections; // 需要使用协程 IEnumerator

public class StandingBreakableBlock : MonoBehaviour
{
    public float timeToBreak = 2f;          // 玩家需要站立的时间
    public GameObject breakEffectPrefab;    // 碎裂特效的 Prefab (可选)
    public float breakEffectDelay = 0.1f;   // 播放特效后的销毁延迟 (可选)
    // public AudioClip standingSound;      // 玩家站上去的音效 (可选)
    // public AudioClip breakingSound;      // 砖块碎裂的音效 (可选)
    // private AudioSource audioSource;     // 用于播放音效

    private bool playerIsOnBlock = false;
    private float standTimer = 0f;
    private bool canBreak = false;          // 标记房间小怪是否已清空
    private bool isBreaking = false;        // 标记是否已在碎裂过程中，防止重复触发

    private SpriteRenderer spriteRenderer;
    private Collider2D mainCollider;

    // 引用房间管理器 - 这是与外部系统的接口
    private RoomManager roomManager;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCollider = GetComponent<BoxCollider2D>(); // 获取主碰撞体
        // audioSource = GetComponent<AudioSource>(); // 如果需要音效，确保添加了 AudioSource 组件

        // 尝试找到场景中的 RoomManager
        // 你可能需要根据你的 RoomManager 实现方式来获取它
        // 例如，如果 RoomManager 是一个单例：
        // roomManager = RoomManager.Instance;
        // 或者通过 FindObjectOfType (如果场景中只有一个):
        roomManager = FindObjectOfType<RoomManager>();

        if (roomManager == null)
        {
            Debug.LogError("StandingBreakableBlock: RoomManager not found in the scene!");
            // 可以禁用此砖块的功能，或者让其默认不可破坏
            enabled = false; // 禁用脚本的 Update 等
            return;
        }

        // (可选) 订阅 RoomManager 的事件，当所有敌人被击败时更新 canBreak 状态
        // 假设 RoomManager 有一个这样的静态事件:
        // RoomManager.OnAllEnemiesDefeatedInRoom += HandleAllEnemiesDefeated;
        // 如果没有事件，你需要在 Update 中轮询 roomManager.AreAllEnemiesDefeatedInCurrentRoom()
    }

    // 如果你使用了 RoomManager 的事件
    // void OnDestroy()
    // {
    //     if (roomManager != null)
    //     {
    //         RoomManager.OnAllEnemiesDefeatedInRoom -= HandleAllEnemiesDefeated;
    //     }
    // }

    // private void HandleAllEnemiesDefeated()
    // {
    //     canBreak = true;
    //     // (可选) 可以在这里改变砖块的外观，提示玩家现在可以站上去了
    //     // spriteRenderer.color = Color.green; // 示例：变绿色
    //     Debug.Log("StandingBreakableBlock: All enemies defeated. Block is now potentially breakable.");
    // }


    void Update()
    {
        if (isBreaking) return; // 如果正在碎裂，则不执行任何操作

        // 如果没有使用事件，则在这里轮询检查敌人是否清空
        // 这一步应该在敌人清空后只执行一次，或者通过事件来触发canBreak=true
        if (!canBreak && roomManager != null && roomManager.AreAllEnemiesDefeatedInCurrentRoom()) {
            canBreak = true;
            // (可选) 可以在这里改变砖块的外观，提示玩家现在可以站上去了
            // spriteRenderer.color = Color.green; // 示例：变绿色
            Debug.Log("StandingBreakableBlock: All enemies defeated. Block is now potentially breakable.");
        }


        if (canBreak && playerIsOnBlock)
        {
            standTimer += Time.deltaTime;
            if (standTimer >= timeToBreak)
            {
                StartCoroutine(BreakSequence());
            }
        }
        else
        {
            standTimer = 0f; // 如果玩家离开或者条件不满足，重置计时器
        }
    }

    // 这个方法由附加在砖块上方的 Trigger Collider 调用
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isBreaking) return;

        if (other.CompareTag("Player"))
        {
            playerIsOnBlock = true;
            // Debug.Log("Player entered breakable block trigger.");
            // if (audioSource != null && standingSound != null && canBreak)
            // {
            //    audioSource.PlayOneShot(standingSound);
            // }
        }
    }

    // 这个方法由附加在砖块上方的 Trigger Collider 调用
    void OnTriggerExit2D(Collider2D other)
    {
        if (isBreaking) return;

        if (other.CompareTag("Player"))
        {
            playerIsOnBlock = false;
            standTimer = 0f; // 玩家离开，重置计时器
            // Debug.Log("Player exited breakable block trigger.");
        }
    }

    private IEnumerator BreakSequence()
    {
        isBreaking = true; // 设置为正在碎裂状态

        // Debug.Log("Block is breaking!");

        // 1. (可选) 播放碎裂音效
        // if (audioSource != null && breakingSound != null)
        // {
        //    audioSource.PlayOneShot(breakingSound);
        // }

        // 2. (可选) 生成碎裂特效
        if (breakEffectPrefab != null)
        {
            Instantiate(breakEffectPrefab, transform.position, Quaternion.identity);
        }

        // 3. 禁用 Sprite Renderer 和主碰撞体，使其不可见且不可交互
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (mainCollider != null) mainCollider.enabled = false;

        // 4. 禁用所有子对象上的碰撞体 (例如那个小的 Trigger Collider，如果它是子对象)
        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
        {
            col.enabled = false;
        }


        // 5. 延迟后销毁 GameObject
        yield return new WaitForSeconds(breakEffectDelay);
        Destroy(gameObject);
    }
}