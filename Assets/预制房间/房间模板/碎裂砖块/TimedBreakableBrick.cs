using UnityEngine;
using System.Collections; // 需要使用协程 (Coroutines)

public class TimedBreakableBrick : MonoBehaviour, ITimedBreakable
{
    public float timeToBreak = 2.0f;         // 踩上去后多少秒破碎
    public GameObject shatteredBrickPrefab;  // 破碎后的砖块碎片预制体 (包含Rigidbody2D等)
    public AudioClip crackingSound;          // 开始裂开的声音 (可选)
    public AudioClip breakSound;             // 最终破碎的声音

    private Coroutine breakCoroutine;
    private bool isPlayerOn = false;
    private bool isBreaking = false;
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer; // 用于播放裂纹动画或改变外观
    public Sprite[] crackSprites; // 0: 初始, 1: 轻微裂纹, 2: 严重裂纹 (可选)


    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void OnPlayerStepOn()
    {
        if (isBreaking) return; // 如果已经在破碎过程中，则不重复触发

        isPlayerOn = true;
        Debug.Log(gameObject.name + " - Player stepped on.");

        if (breakCoroutine == null)
        {
            breakCoroutine = StartCoroutine(BreakTimerCoroutine());
        }
        // 可选：播放开始裂开的音效或轻微晃动动画
        if (crackingSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(crackingSound);
        }
        // 可选：切换到裂纹 Sprite
        if (spriteRenderer != null && crackSprites != null && crackSprites.Length > 1)
        {
            spriteRenderer.sprite = crackSprites[1];
        }
    }

    public void OnPlayerStepOff()
    {
        isPlayerOn = false;
        Debug.Log(gameObject.name + " - Player stepped off.");
        // 如果你希望玩家离开就停止计时并重置，可以在这里处理
        // if (breakCoroutine != null && !isBreaking) // 确保不是在最终破碎阶段
        // {
        //     StopCoroutine(breakCoroutine);
        //     breakCoroutine = null;
        //     Debug.Log(gameObject.name + " - Break sequence reset.");
        //     // 可选：恢复初始外观
        //     if (spriteRenderer != null && crackSprites != null && crackSprites.Length > 0)
        //     {
        //         spriteRenderer.sprite = crackSprites[0];
        //     }
        // }
    }

    public void StartBreakingSequence() // 这个方法由协程内部调用或外部强制触发
    {
        if (isBreaking) return;
        isBreaking = true; // 标记为正在破碎，防止重复
        // 这里可以播放更剧烈的晃动动画或者切换到更严重的裂纹 Sprite
        Debug.Log(gameObject.name + " - Starting breaking sequence NOW.");
        if (spriteRenderer != null && crackSprites != null && crackSprites.Length > 2)
        {
            spriteRenderer.sprite = crackSprites[2];
        }
        Break();
    }

    private IEnumerator BreakTimerCoroutine()
    {
        float timer = 0f;
        float interval = timeToBreak / (crackSprites != null ? crackSprites.Length -1 : 3f); // 时间间隔用于切换裂纹图
        int crackSpriteIndex = 1;

        while (timer < timeToBreak)
        {
            if (!isPlayerOn) // 如果玩家在中途离开了 (根据你的设计决定是否重置)
            {
                // 如果设计为玩家离开就重置计时
                // Debug.Log("Player left, resetting timer for " + gameObject.name);
                // if (spriteRenderer != null && crackSprites != null && crackSprites.Length > 0) spriteRenderer.sprite = crackSprites[0];
                // audioSource.Stop(); // 如果有持续的裂纹声
                // breakCoroutine = null;
                // yield break; // 退出协程

                // 如果设计为玩家离开后计时暂停，再次踩上继续 (更复杂，暂不实现)
                // yield return null; // 暂停一帧，等待玩家回来
                // continue;

                // 当前设计：即使玩家离开，计时器依然继续，直到破碎或玩家明确调用StepOff重置
            }

            timer += Time.deltaTime;

            // 可选：根据时间更新裂纹Sprite
            if (crackSprites != null && crackSprites.Length > 1 && timer > interval * crackSpriteIndex && crackSpriteIndex < crackSprites.Length -1)
            {
                spriteRenderer.sprite = crackSprites[crackSpriteIndex];
                crackSpriteIndex++;
                if (crackingSound != null && audioSource != null) audioSource.PlayOneShot(crackingSound); // 每次裂纹加深都响一下
            }

            yield return null; // 等待下一帧
        }

        // 时间到，开始破碎
        StartBreakingSequence();
        breakCoroutine = null;
    }

    public void Break()
    {
        Debug.Log(gameObject.name + " - Breaking!");

        // 1. 播放破碎音效
        if (breakSound != null && audioSource != null)
        {
            // 使用 PlayClipAtPoint 确保音效在物体销毁后仍能播放完毕
            AudioSource.PlayClipAtPoint(breakSound, transform.position, audioSource.volume);
        }

        // 2. 实例化破碎后的碎片
        if (shatteredBrickPrefab != null)
        {
            GameObject shatteredInstance = Instantiate(shatteredBrickPrefab, transform.position, transform.rotation);
            // (可选) 给碎片一个初始的力，让它们散开
            Rigidbody2D[] rbs = shatteredInstance.GetComponentsInChildren<Rigidbody2D>();
            foreach (Rigidbody2D rb in rbs)
            {
                Vector2 force = new Vector2(Random.Range(-1f, 1f), Random.Range(0.5f, 1.5f)) * Random.Range(2f, 5f);
                rb.AddForce(force, ForceMode2D.Impulse);
                rb.AddTorque(Random.Range(-30f, 30f), ForceMode2D.Impulse);
            }
        }

        // 3. 销毁原始砖块
        Destroy(gameObject);
    }

    // 碰撞检测来触发 OnPlayerStepOn 和 OnPlayerStepOff
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) // 确保你的玩家对象有 "Player" 标签
        {
            // 检查碰撞方向，确保是玩家踩在上面
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // 检查法线是否大致向上 (意味着碰撞发生在砖块的顶面)
                // Vector2.up 是 (0,1)。点积接近-1表示碰撞来自下方，接近1表示碰撞来自上方（玩家在砖块下），接近0表示侧面
                // 我们需要的是玩家的脚接触砖块的顶面，所以砖块的碰撞法线应该是向上的。
                if (Vector2.Dot(contact.normal, Vector2.down) < -0.7f) // 碰撞法线方向与世界下方 (-Y) 的点积小于 -0.7 (即法线接近世界正上方 +Y)
                {
                    OnPlayerStepOn();
                    break; // 找到一个符合条件的接触点就够了
                }
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            OnPlayerStepOff();
        }
    }
}