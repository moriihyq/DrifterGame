using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MagicBullet : MonoBehaviour
{
    // 公共属性
    public float speed = 8f;
    public float lifetime = 5f;
    public int damage = 35;    // 私有变量
    private Vector2 direction;
    private Rigidbody2D rb;
    private float timer = 0f;
    private bool hasTarget = false;
    private Transform currentTarget;
    private float trackingUpdateInterval = 0.3f; // 目标追踪更新间隔
    private float lastUpdateTime = 0f;
    private float rotationSpeed = 3f; // 转向速度
    
    // 动画相关
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // 初始化方法
    public void Initialize(float initialSpeed, Vector2 initialDirection, int bulletDamage, float bulletLifetime)
    {
        speed = initialSpeed;
        direction = initialDirection.normalized;
        damage = bulletDamage;
        lifetime = bulletLifetime;
    }    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // 获取或添加组件
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        
        // 如果没有动画器但有动画资源，添加动画器组件
        if (animator == null)
        {
            animator = gameObject.AddComponent<Animator>();
            // 注意：您需要在Unity编辑器中为此动画器分配动画控制器
        }
        
        // 添加必要组件
        if (GetComponent<CircleCollider2D>() == null)
        {
            CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.3f;
        }
        
        // 设置为触发器
        Collider2D collider2d = GetComponent<Collider2D>();
        if (collider2d != null)
        {
            collider2d.isTrigger = true;
        }
        
        // 初始速度
        rb.linearVelocity = direction * speed;
        
        // 旋转子弹使其朝向移动方向
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        
        // 如果有动画器，播放动画
        PlayAnimation();
    }
      // 播放子弹动画
    private void PlayAnimation()
    {
        if (animator != null)
        {
            if (animator.runtimeAnimatorController != null)
            {
                try
                {
                    // 查找并播放默认动画（通常名为"Play"或类似名称）
                    animator.SetTrigger("Play");
                    
                    // 如果想连续循环播放动画，设置循环参数
                    animator.SetBool("Loop", true);
                    
                    Debug.Log("MagicBullet: 成功触发Play动画");
                }
                catch (System.Exception e)
                {
                    Debug.LogError("触发Play动画时出错: " + e.Message);
                }
            }
            else
            {
                Debug.LogWarning("子弹的Animator组件没有设置RuntimeAnimatorController。请在编辑器中设置动画控制器。");
            }
        }
        else
        {
            Debug.LogWarning("子弹没有Animator组件。无法播放动画。");
        }
    }

    void Update()
    {
        // 增加计时器，到达生命周期时销毁
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        // 定期更新目标
        if (Time.time - lastUpdateTime >= trackingUpdateInterval)
        {
            FindNearestActiveEnemy();
            lastUpdateTime = Time.time;
        }
        
        // 如果有目标，调整方向朝向目标
        if (hasTarget && currentTarget != null)
        {
            TrackTarget();
        }
    }    void FindNearestActiveEnemy()
    {
        // 创建目标列表
        List<Transform> potentialTargets = new List<Transform>();
        
        // 查找所有普通敌人
        Enemy[] allEnemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy enemy in allEnemies)
        {
            // 检查敌人是否活跃 - 必须同时满足：
            // 1. 敌人对象必须激活
            // 2. 敌人不能处于死亡状态
            // 3. 敌人必须处于激活状态（有攻击意图）
            
            // 检查是否处于死亡状态
            bool isEnemyDead = false;
            System.Reflection.FieldInfo deadField = typeof(Enemy).GetField("isDead", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (deadField != null)
            {
                isEnemyDead = (bool)deadField.GetValue(enemy);
            }
            
            // 检查敌人是否有攻击意图（isActive）
            bool isEnemyActive = false;
            System.Reflection.FieldInfo activeField = typeof(Enemy).GetField("isActive", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (activeField != null)
            {
                isEnemyActive = (bool)activeField.GetValue(enemy);
            }
            
            if (enemy.gameObject.activeInHierarchy && !isEnemyDead && isEnemyActive)
            {
                potentialTargets.Add(enemy.transform);
            }
        }
        
        // 查找所有Boss
        BossController[] allBosses = Object.FindObjectsByType<BossController>(FindObjectsSortMode.None);
        foreach (BossController boss in allBosses)
        {
            // 检查Boss是否活跃
            // 1. Boss对象必须激活
            // 2. Boss不能处于死亡状态
            
            // 检查Boss是否处于死亡状态
            bool isBossDead = false;
            System.Reflection.FieldInfo deadField = typeof(BossController).GetField("isDead", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (deadField != null)
            {
                isBossDead = (bool)deadField.GetValue(boss);
            }
            
            // 检查Boss是否活跃
            bool isBossActive = false;
            System.Reflection.FieldInfo activeField = typeof(BossController).GetField("isActive", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (activeField != null)
            {
                isBossActive = (bool)activeField.GetValue(boss);
            }
            
            if (boss.gameObject.activeInHierarchy && !isBossDead && isBossActive)
            {
                potentialTargets.Add(boss.transform);
            }
        }
        
        // 如果没有活跃目标，重置状态
        if (potentialTargets.Count == 0)
        {
            hasTarget = false;
            currentTarget = null;
            return;
        }
        
        // 寻找最近的活跃目标
        Transform nearestTarget = potentialTargets
            .OrderBy(t => Vector3.Distance(transform.position, t.position))
            .FirstOrDefault();
            
        // 设置为当前目标
        if (nearestTarget != null)
        {
            currentTarget = nearestTarget;
            hasTarget = true;
        }
        else
        {
            hasTarget = false;
        }
    }
    
    void TrackTarget()
    {
        // 计算子弹到目标的方向向量
        Vector2 targetDirection = (currentTarget.position - transform.position).normalized;
        
        // 逐渐调整子弹方向，创建平滑追踪效果
        direction = Vector2.Lerp(direction.normalized, targetDirection, rotationSpeed * Time.deltaTime).normalized;
        
        // 更新速度向量
        rb.linearVelocity = direction * speed;
        
        // 更新子弹旋转，使其朝向移动方向
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }    void OnTriggerEnter2D(Collider2D other)
    {
        bool didHit = false;
        
        // 检查是否碰到普通敌人
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                didHit = true;
            }
        }
        
        // 检查是否碰到Boss - 无论标签如何
        BossController boss = other.GetComponent<BossController>();
        if (boss != null)
        {
            boss.TakeDamage(damage);
            didHit = true;
            
            // 确保击中后立即调用销毁逻辑
            Debug.Log("魔法子弹击中Boss: " + boss.name);
        }
        
        // 检查其他碰撞情况
        else if (!other.CompareTag("Player") && !other.CompareTag("Bullet"))
        {
            // 如果碰到除了玩家、子弹以外的物体，检查是否为墙壁或地形
            if (other.gameObject.layer == LayerMask.NameToLayer("Ground") || 
                other.gameObject.layer == LayerMask.NameToLayer("Wall"))
            {
                // 对于墙壁和地形，子弹可以穿透（不触发销毁）
            }
            else
            {
                // 碰到其他物体时播放击中动画
                didHit = true;
            }
        }
        
        // 如果击中了目标，播放击中动画
        if (didHit)
        {
            PlayHitAnimation();
        }
    }
      // 播放击中动画并销毁
    private void PlayHitAnimation()
    {
        // 确保子弹停止移动
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.linearVelocity = Vector2.zero;
        }
        
        if (animator != null)
        {
            if (animator.runtimeAnimatorController != null)
            {
                try
                {
                    // 停止当前动画
                    animator.SetBool("Loop", false);
                    
                    // 播放击中动画
                    animator.SetTrigger("Hit");
                    
                    // 在动画完成后销毁对象
                    Destroy(gameObject, 0.5f); // 假设击中动画持续0.5秒
                    Debug.Log("MagicBullet: 播放击中动画，将在0.5秒后销毁");
                }
                catch (System.Exception e)
                {
                    Debug.LogError("播放击中动画时出错: " + e.Message + "\n" + e.StackTrace);
                    // 出错时直接销毁
                    Destroy(gameObject);
                }
            }
            else
            {
                Debug.LogWarning("子弹的Animator组件没有设置RuntimeAnimatorController，无法播放击中动画");
                Destroy(gameObject);
            }
        }
        else
        {
            // 如果没有动画控制器，直接销毁
            Debug.Log("MagicBullet: 无Animator组件，立即销毁");
            Destroy(gameObject);
        }
    }
}
