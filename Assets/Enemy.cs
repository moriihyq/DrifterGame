using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("基本属性")]
    [SerializeField] private int maxHealth = 100; // 最大生命值
    [SerializeField] private int attackDamage = 25; // 攻击伤害
    [SerializeField] private float attackCooldown = 1f; // 攻击冷却时间
    [SerializeField] private float attackRange = 1.2f; // 攻击范围
    [SerializeField] private Transform attackPoint; // 攻击判定点
    [SerializeField] private LayerMask playerLayer; // 玩家图层
    
    // 组件引用
    private Animator anim;
    private Rigidbody2D rb;
    
    // 状态变量
    private bool isMoving;
    private bool approach;
    public float distanceToPlayer;
    private int currentHealth;
    private bool isDead;
    private float originScaleX;
    private float lastSpeed;
    private float nextAttackTime = 0f; // 下次可攻击时间
    private GameObject player;
    void Start()
    {
        isMoving = false;
        approach = false;
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        lastSpeed = 0;
        currentHealth = maxHealth; // 初始化生命值
        isDead = false;
        originScaleX = transform.localScale.x;
        
        // 确保有碰撞器
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            Debug.LogWarning("Enemy没有碰撞器，添加BoxCollider2D");
            BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
            // 设置合适的大小，这需要根据你的敌人精灵大小调整
            boxCollider.size = new Vector2(1f, 1f);
            boxCollider.offset = Vector2.zero;
        }
        
        // 确保敌人在正确的层级上
        if (LayerMask.NameToLayer("Enemy") != -1)
        {
            gameObject.layer = LayerMask.NameToLayer("Enemy");
        }
    }
    
    // 敌人受到伤害
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        
        // 播放受伤动画
        if (anim != null)
        {
            anim.SetTrigger("Hurt");
        }
        
        // 检查是否死亡
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        
        // 通知战斗管理器记录伤害
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.LogDamage(GameObject.FindGameObjectWithTag("Player"), gameObject, damage);
            
            // 检查是否击杀
            if (currentHealth <= 0)
            {
                CombatManager.Instance.HandleKill(GameObject.FindGameObjectWithTag("Player"), gameObject);
            }
        }
    }
    
    // 敌人死亡
    private void Die()
    {
        isDead = true;
        
        // 播放死亡动画
        if (anim != null)
        {
            anim.SetBool("IsDead", true);
        }
        
        // 禁用组件
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
        
        // 在一段时间后销毁对象
        Destroy(gameObject, 2f);
    }

    // Update is called once per frame
    void Update()
    {
        GameObject enemy = this.gameObject;
        GameObject player = GameObject.Find("Player");
        //rb.linearVelocityX = 5;
        if (player != null)
        {
            distanceToPlayer = player.transform.position.x - enemy.transform.position.x;
        }
        else
        {
            distanceToPlayer = 0;
            Debug.Log("Player not found");
        }

        if ((distanceToPlayer < 5 && distanceToPlayer > 1) || (distanceToPlayer > -5 && distanceToPlayer < -1))
        {
            if (enemy.transform.position.x > player.transform.position.x)
            {
                rb.linearVelocityX = -5;
            }
            else
            {
                rb.linearVelocityX = 5;
            }
        }

        AnimatorController();
    }
    private void AnimatorController()
    {
        Vector3 currentScale = transform.localScale;
        currentScale.x = distanceToPlayer > 0 ? originScaleX : -originScaleX;
        transform.localScale = currentScale;
        lastSpeed = rb.linearVelocityX;
        isMoving = rb.linearVelocityX != 0;
        anim.SetBool("isMoving", isMoving);
        approach = distanceToPlayer < 1 && distanceToPlayer > -1;
        anim.SetBool("approach", approach);
    }
}
