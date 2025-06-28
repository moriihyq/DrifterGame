using UnityEngine;

public class Attack_shattering_block : MonoBehaviour
{
    public AudioClip breakSound;  // 碎裂音效

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 当碰撞时，如果碰撞强度超过阈值，触发破碎
        if (collision.relativeVelocity.magnitude > 1f) // 可以调整碰撞的力度阈值
        {
            BreakBlock();  // 触发破碎
        }
    }

    private void BreakBlock()
    {
        // 播放碎裂音效
        if (breakSound != null)
        {
            AudioSource.PlayClipAtPoint(breakSound, transform.position);
        }

        // 销毁原块
        Destroy(gameObject);  // 直接销毁块
    }
}
