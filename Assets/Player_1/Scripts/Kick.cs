using UnityEngine;

using UnityEngine.UI;
public class Kick : MonoBehaviour
{
    private Animator animator;
    private static readonly int KickTrigger = Animator.StringToHash("Kick");
    
    private float lastKickTime;  // 记录上次踢腿的时间
    private const float KickCooldown = 0.2f;  // 踢腿冷却时间（秒）

    void Start()
    {
        // 获取Animator组件
        animator = GetComponent<Animator>();
        lastKickTime = -KickCooldown;  // 初始化上次踢腿时间，使游戏开始时可以立即踢腿
    }

    void Update()
    {
        // 检测鼠标左键点击，并且检查是否已经过了冷却时间
        if (Input.GetMouseButtonDown(0) && Time.time - lastKickTime >= KickCooldown)
        {
            // 触发踢腿动画
            animator.SetTrigger(KickTrigger);
            // 更新上次踢腿时间
            lastKickTime = Time.time;
        }
    }
} 