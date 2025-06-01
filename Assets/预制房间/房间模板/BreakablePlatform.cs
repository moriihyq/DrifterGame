using UnityEngine;
using System.Collections; // 需要这个来进行协程操作

public class BreakablePlatform : MonoBehaviour
{
    [Header("Settings")]
    public float timeToBreak = 2f;     // 玩家站上去后多少秒开始碎裂
    public float breakEffectDuration = 0.5f; // 碎裂动画/效果持续时间（如果只是消失则为0）
    public bool respawnable = false;    // 是否可以重生
    public float respawnDelay = 5f;     // 如果可以重生，重生延迟时间

    [Header("References (Optional)")]
    public GameObject breakEffectPrefab; // 碎裂特效预制件 (例如粒子效果)
    public AudioClip breakSound;         // 碎裂音效

    private SpriteRenderer sr;
    private Collider2D platformCollider;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;

    private bool isPlayerOnPlatform = false;
    private bool isBreaking = false;
    private float breakTimer = 0f;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        platformCollider = GetComponent<Collider2D>();
        if (platformCollider == null)
        {
            Debug.LogError("BreakablePlatform requires a Collider2D component.", this);
            enabled = false; // 如果没有碰撞体，禁用脚本
            return;
        }

        // 保存初始状态以便重生
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;
    }

    void Update()
    {
        if (isPlayerOnPlatform && !isBreaking)
        {
            breakTimer += Time.deltaTime;
            // (可选) 在这里可以添加一些视觉提示，比如砖块开始晃动或变色
            // ShakePlatform();

            if (breakTimer >= timeToBreak)
            {
                StartCoroutine(BreakSequence());
            }
        }
    }

    // 当有物体与此砖块碰撞时调用 (不是Trigger)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 检查碰撞的是否是玩家 (假设玩家对象有一个 "Player" 标签)
        // 并且只在玩家从上方接触平台时触发（避免从侧面或下方触发）
        if (!isBreaking && collision.gameObject.CompareTag("Player"))
        {
            // 检查接触点是否在平台上方
            ContactPoint2D[] contacts = new ContactPoint2D[1];
            collision.GetContacts(contacts);
            if (contacts.Length > 0 && contacts[0].normal.y < -0.5f) // normal.y < -0.5f 表示碰撞来自上方
            {
                if (!isPlayerOnPlatform) // 避免重复启动计时器
                {
                    isPlayerOnPlatform = true;
                    breakTimer = 0f; // 重置计时器
                    Debug.Log("Player landed on breakable platform.", this);
                }
            }
        }
    }

    // 当有物体离开与此砖块的碰撞时调用
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!isBreaking && collision.gameObject.CompareTag("Player"))
        {
            isPlayerOnPlatform = false;
            breakTimer = 0f; // 玩家离开，重置计时器 (除非你希望计时器不重置)
            // (可选) 停止晃动等视觉提示
            // StopShakingPlatform();
            Debug.Log("Player left breakable platform.", this);
        }
    }

    IEnumerator BreakSequence()
    {
        isBreaking = true;
        isPlayerOnPlatform = false; // 停止计时

        Debug.Log("Platform is breaking!", this);

        // 1. 播放碎裂音效 (如果设置了)
        if (breakSound != null && GetComponent<AudioSource>())
        {
            GetComponent<AudioSource>().PlayOneShot(breakSound);
        }
        else if (breakSound != null) // 如果没有AudioSource但有音效，可以创建一个临时的
        {
            AudioSource.PlayClipAtPoint(breakSound, transform.position);
        }


        // 2. 播放碎裂特效 (如果设置了)
        if (breakEffectPrefab != null)
        {
            Instantiate(breakEffectPrefab, transform.position, Quaternion.identity);
        }

        // 3. (可选) 播放砖块自身的碎裂动画或效果，比如快速闪烁、缩小等
        // 例如：快速闪烁
        float startTime = Time.time;
        while(Time.time < startTime + breakEffectDuration * 0.5f) // 效果前半段
        {
            if (sr != null) sr.enabled = !sr.enabled; // 闪烁
            yield return new WaitForSeconds(0.05f); // 闪烁频率
        }


        // 4. 隐藏砖块并禁用碰撞
        if (sr != null) sr.enabled = false;
        if (platformCollider != null) platformCollider.enabled = false;

        // (可选) 如果有碎裂动画，等待动画播放完毕
        yield return new WaitForSeconds(breakEffectDuration * 0.5f); // 效果后半段或只是等待

        // 5. 处理重生 (如果设置了)
        if (respawnable)
        {
            yield return new WaitForSeconds(respawnDelay);
            Respawn();
        }
        else
        {
            // 如果不重生，可以选择销毁GameObject
            // Destroy(gameObject);
            // 或者只是保持禁用状态
        }
    }

    void Respawn()
    {
        Debug.Log("Platform respawning!", this);
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        transform.localScale = originalScale;

        if (sr != null) sr.enabled = true;
        if (platformCollider != null) platformCollider.enabled = true;

        isBreaking = false;
        isPlayerOnPlatform = false;
        breakTimer = 0f;
    }

    // (可选) 晃动平台的简单实现
    // private Vector3 initialShakePosition;
    // private float shakeMagnitude = 0.05f;
    // private float shakeDurationRemaining;
    // void ShakePlatform() {
    //     if (shakeDurationRemaining <= 0) initialShakePosition = transform.localPosition;
    //     shakeDurationRemaining = 0.1f; // 每次晃动一小段时间
    //     transform.localPosition = initialShakePosition + Random.insideUnitSphere * shakeMagnitude;
    // }
    // void StopShakingPlatform() {
    //     if(shakeDurationRemaining > 0) transform.localPosition = initialShakePosition;
    //     shakeDurationRemaining = 0;
    // }
    // 在Update中调用 if(shakeDurationRemaining > 0) shakeDurationRemaining -= Time.deltaTime; else if(isPlayerOnPlatform) ShakePlatform();
}
