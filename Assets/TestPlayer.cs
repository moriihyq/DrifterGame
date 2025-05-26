using UnityEngine;

public class TestPlayer : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f; // 移动速度

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 获取用户输入
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // 创建移动向量
        Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput);
        
        // 应用移动
        transform.Translate(movement * moveSpeed * Time.deltaTime);
    }
}
