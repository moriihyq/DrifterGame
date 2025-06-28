using UnityEngine;
using System.Collections; // 需要这个来使用协程 (Coroutines)

public class BreakableBlock : MonoBehaviour
{
    public float delayBeforeBreak = 0.2f; // 踩上去后多久碎裂
    public float disableDelay = 0.2f;     // 播放完碎裂动画/音效后多久真正消失 (可选)
    public GameObject breakEffectPrefab;  // 碎裂特效预制件 (可选)
    public AudioClip breakSound;          // 碎裂音效 (可选)

    private bool isBreaking = false;
    private AudioSource audioSource;

    void Start()
    {
        // 如果有音效，获取或添加AudioSource组件
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && breakSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // 当有其他Collider进入这个物体的Collider时调用 (非Trigger)
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 检查碰撞到的是否是玩家 (假设玩家的Tag是 "Player")
        if (collision.gameObject.CompareTag("Player") && !isBreaking)
        {
            // 确保是玩家从上方踩到砖块 (可选，但更真实)
            // 检查接触点的法线方向，如果接近向上 (Vector2.up)，说明是踩在上面
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (Vector2.Dot(contact.normal, Vector2.down) > 0.7f) // 玩家的脚底碰撞砖块的顶部
                {
                    StartCoroutine(BreakSequence());
                    break; // 找到一个符合条件的接触点就足够了
                }
            }
        }
    }

    IEnumerator BreakSequence()
    {
        isBreaking = true;

        // 1. 等待一小段时间
        yield return new WaitForSeconds(delayBeforeBreak);

        // 2. (可选) 播放碎裂特效和音效
        if (breakEffectPrefab != null)
        {
            Instantiate(breakEffectPrefab, transform.position, Quaternion.identity);
        }
        if (audioSource != null && breakSound != null)
        {
            audioSource.PlayOneShot(breakSound);
        }

        // 3. (可选) 如果有音效/动画，再等待一小段时间让它们播放完
        // 同时，为了防止玩家还能站在一个即将消失的砖块上，可以先禁用碰撞体
        GetComponent<Collider2D>().enabled = false;
        // 也可以让Sprite Renderer消失，让砖块看起来已经碎了
        // GetComponent<SpriteRenderer>().enabled = false;
        yield return new WaitForSeconds(disableDelay);


        // 4. 禁用或销毁砖块
        gameObject.SetActive(false); // 禁用，可以被对象池回收
        // Destroy(gameObject); // 或者直接销毁
    }
}
