using UnityEngine;

public class bullet : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float speed; // 子弹速度（由发射者设置）
    public Vector2 direction; // 子弹方向（由发射者设置）
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction * speed; // 设置初始速度

        // 可选：2秒后自动销毁子弹
        Destroy(gameObject, 2f);
    }

    // 碰撞检测（可选）
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) // 避免击中自己
            Destroy(gameObject);
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
