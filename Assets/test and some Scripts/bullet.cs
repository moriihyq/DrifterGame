using UnityEngine;

public class bullet : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float speed; // �ӵ��ٶȣ��ɷ��������ã�
    public Vector2 direction; // �ӵ������ɷ��������ã�
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction * speed; // ���ó�ʼ�ٶ�

        // ��ѡ��2����Զ������ӵ�
        Destroy(gameObject, 2f);
    }

    // ��ײ��⣨��ѡ��
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) // ��������Լ�
            Destroy(gameObject);
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
