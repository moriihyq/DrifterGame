using UnityEngine;

public class Enemy2 : MonoBehaviour
{
    [Header("�ӵ�����")]
    public GameObject bulletPrefab; // �ӵ�Ԥ����
    public Transform firePoint;    // ����㣨��������Ϊ�����壩

    [Header("�������")]
    public float bulletSpeed = 10f; // �ӵ����ٶ�
    [Range(0, 360)] public float angle = 0f; // ����Ƕȣ�0=�ң�90=�ϣ�    [Header("基本属性")]
    [SerializeField] private int maxHealth = 100; // 最大生命值
    [SerializeField] private int attackDamage = 25; // 攻击伤害
    [SerializeField] private float attackCooldown = 1f; // 攻击冷却时间
    [SerializeField] private float attackRange = 1.2f; // 攻击范围
    [SerializeField] private Transform attackPoint; // 攻击判定点
    [SerializeField] private LayerMask playerLayer; // 玩家图层
    [SerializeField] private float attackDelay = 0.3f; // 攻击延迟
    
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 5f; // 移动速度
    [SerializeField] private float detectionRange = 5f; // 检测玩家的范围

    // �������
    private Animator anim;
    private Rigidbody2D rb;

    // ״̬����
    private bool isMoving;
    private bool approach;
    public float distanceToPlayer;
    private int currentHealth;
    private bool isDead;
    private float originScaleX;
    private float lastSpeed;
    private float nextAttackTime = 0f; // �´οɹ���ʱ��
    private GameObject player;
    private bool isAttacking = false; // ��ǰ�Ƿ����ڹ���
    private bool isActive = false; // �����Ƿ��ڼ���״̬
    void Start()
    {
        isMoving = false;
        approach = false;
        isAttacking = false;
        isActive = false;

        // ��ȡ���
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // ��ʼ������
        lastSpeed = 0;
        currentHealth = maxHealth;
        isDead = false;
        originScaleX = transform.localScale.x;

        // �������
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            player = GameObject.Find("Player");
            if (player == null)
            {
                Debug.LogWarning("�޷��ҵ���Ҷ�����ȷ����Ҷ���������'Player'��ǩ����Ϊ'Player'");
            }
        }

        // ȷ���й�����
        if (attackPoint == null)
        {
            GameObject attackPointObj = new GameObject("EnemyAttackPoint");
            attackPointObj.transform.SetParent(transform);
            attackPointObj.transform.localPosition = new Vector3(1f, 0f, 0f); // �����ڵ���ǰ��
            attackPoint = attackPointObj.transform;

            Debug.Log("Ϊ���˴�����Ĭ�Ϲ�����");
        }

        // ȷ������ײ��
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            Debug.LogWarning("Enemyû����ײ��������BoxCollider2D");
            BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
            boxCollider.size = new Vector2(1f, 1f);
            boxCollider.offset = Vector2.zero;
        }

        // ȷ����������ȷ�Ĳ㼶��
        if (LayerMask.NameToLayer("Enemy") != -1)
        {
            gameObject.layer = LayerMask.NameToLayer("Enemy");
        }
    }
    // �����ܵ��˺�
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        int prevHealth = currentHealth;
        currentHealth -= damage;

        // �ڿ���̨�������Ѫ���仯��Ϣ
        Debug.Log($"<color=#FFA500>���� {gameObject.name} �ܵ� {damage} ���˺���Ѫ���仯��{prevHealth} -> {currentHealth}</color>");

        // �������˶���
        if (anim != null)
        {
            anim.SetTrigger("Hurt");
        }

        // ����Ƿ�����
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log($"<color=#FF0000>���� {gameObject.name} ������</color>");
            Die();
        }

        // ֪ͨս����������¼�˺�
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.LogDamage(GameObject.FindGameObjectWithTag("Player"), gameObject, damage);

            // ����Ƿ��ɱ
            if (currentHealth <= 0)
            {
                CombatManager.Instance.HandleKill(GameObject.FindGameObjectWithTag("Player"), gameObject);
            }
        }
    }

    // ��������
    private void Die()
    {
        isDead = true;

        // ������������
        if (anim != null)
        {
            anim.SetBool("IsDead", true);
        }

        // �������
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;

        // ��һ��ʱ������ٶ���
        Destroy(gameObject, 2f);
    }    // Update is called once per frame
    void Update()
    {
        // �����������������ִ���κ��߼�
        if (isDead)
            return;

        UpdatePlayerDistance();
        UpdateMovement();
        UpdateAttackState();
        UpdateAnimator();
    }
    // ��������ҵľ���
    private void UpdatePlayerDistance()
    {
        // ���player���ö�ʧ���������²���
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                distanceToPlayer = 0;
                isActive = false;
                return;
            }
        }

        // ����X����루���������ƶ��ͳ����жϣ�
        distanceToPlayer = player.transform.position.x - transform.position.x;

        // ����ʵ��2D���루���ڹ����ж���
        Vector2 playerPosition = player.transform.position;
        Vector2 enemyPosition = transform.position;
        float actualDistance = Vector2.Distance(playerPosition, enemyPosition);

        // �������һ����Χ��ʱ������ˣ�ʹ��ʵ�ʾ����жϣ�
        isActive = actualDistance < detectionRange;
    }

    // ���µ����ƶ�
    private void UpdateMovement()
    {
        // ������ڹ�����δ����������ƶ�
        if (isAttacking || !isActive)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        // �ڹ�����Χ��ֹͣ��׼������
        if (Mathf.Abs(distanceToPlayer) <= attackRange)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }        // 否则向玩家移动
        else if (Mathf.Abs(distanceToPlayer) < detectionRange)
        {
            float moveDirection = distanceToPlayer > 0 ? 1 : -1;
            
            // 检查是否有减速效果
            EnemySlowEffect slowEffect = GetComponent<EnemySlowEffect>();
            float currentSpeed = moveSpeed;
            if (slowEffect != null)
            {
                currentSpeed = slowEffect.GetAdjustedSpeed(moveSpeed);
            }
            
            rb.linearVelocity = new Vector2(moveDirection * currentSpeed, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }
    // ���¹���״̬
    private void UpdateAttackState()
    {
        // ֻ���ڼ���״̬�Ҳ��ڹ���������ʱ�ſ��ǹ���
        if (!isActive || isAttacking)
            return;

        // ��������ҵ�ʵ��2D����ʹ�ֱ����
        Vector2 playerPosition = player.transform.position;
        Vector2 enemyPosition = transform.position;
        float actualDistance = Vector2.Distance(playerPosition, enemyPosition);
        float verticalDistance = Mathf.Abs(playerPosition.y - enemyPosition.y);

        // ���ÿɽ��ܵĴ�ֱ�����ݲ��ֱ�����ϵ���󹥻����룩
        float verticalAttackTolerance = 0.8f;

        // �������ڹ�����Χ�ڣ�ˮƽ�ʹ�ֱ�����㣩����ȴʱ���ѹ�
        if (Mathf.Abs(distanceToPlayer) <= attackRange &&
            verticalDistance <= verticalAttackTolerance &&
            Time.time >= nextAttackTime)
        {
            // ��ʼ����
            PerformAttack();

            // �����´ι���ʱ��
            nextAttackTime = Time.time + attackCooldown;

            // ������Ϣ
            Debug.Log($"[Enemy����] ��������! X�����: {distanceToPlayer}, ��ֱ����: {verticalDistance}");
        }
    }

    // ִ�й���
    private void PerformAttack()
    {
        isAttacking = true;

        // ������������
        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }

        // �ӳٽ��й����ж�
        Invoke("ApplyAttackDamage", attackDelay);

        // �ӳٽ�������״̬
        Invoke("EndAttackState", 0.5f); // ���蹥����������0.5������
    }
    // Ӧ�ù����˺�
    private void ApplyAttackDamage()
    {
        if (isDead) return;

        // �����Ҳ����ڣ�������˺�
        if (player == null)
            return;

        // �ٴμ������Ƿ�����Ч�Ĺ�����Χ�ڣ�������ֱ���룩
        Vector2 playerPosition = player.transform.position;
        Vector2 enemyPosition = transform.position;
        float verticalDistance = Mathf.Abs(playerPosition.y - enemyPosition.y);
        float verticalAttackTolerance = 0.8f;

        // ���������뿪������Χ��ˮƽ��ֱ���򣩣�������˺�
        if (Mathf.Abs(distanceToPlayer) > attackRange * 1.2f || verticalDistance > verticalAttackTolerance)
        {
            Debug.Log($"<color=#888888>����ʧ��! ������뿪������Χ��X�����: {distanceToPlayer}, ��ֱ����: {verticalDistance}</color>");
            return;
        }        // 检查玩家是否有生命值组件，先直接获取，如果获取不到，尝试在子对象中查找
        PlayerAttackSystem playerHealth = player.GetComponent<PlayerAttackSystem>();
        if (playerHealth == null)
        {
            // 尝试从子对象获取
            playerHealth = player.GetComponentInChildren<PlayerAttackSystem>();
            
            // 如果还是找不到，尝试从父对象获取
            if (playerHealth == null)
            {
                playerHealth = player.GetComponentInParent<PlayerAttackSystem>();
                
                if (playerHealth == null)
                {
                    Debug.LogError($"<color=red>Enemy2无法找到玩家的PlayerAttackSystem组件! 无法造成伤害!</color>");
                }
            }
        }
        
        // 如果找到了PlayerAttackSystem组件，对玩家造成伤害
        if (playerHealth != null)
        {
            // 对玩家造成伤害
            playerHealth.TakeDamage(attackDamage);
            
            // 记录到战斗管理器
            if (CombatManager.Instance != null)
            {
                CombatManager.Instance.LogDamage(gameObject, player, attackDamage);
            }

            Debug.Log($"<color=#FF4500>敌人 {gameObject.name} 对玩家造成 {attackDamage} 点伤害！X距离: {distanceToPlayer}, 垂直距离: {verticalDistance}</color>");
        }
    }

    // ��������״̬
    private void EndAttackState()
    {
        isAttacking = false;
    }

    // ���¶�����
    private void UpdateAnimator()
    {
        if (anim == null) return;

        // ���ó��򣨸������λ�ã�
        Vector3 currentScale = transform.localScale;
        currentScale.x = distanceToPlayer > 0 ? originScaleX : -originScaleX;
        transform.localScale = currentScale;

        // �����ƶ�����
        lastSpeed = rb.linearVelocity.x;
        isMoving = Mathf.Abs(rb.linearVelocity.x) > 0.1f;
        anim.SetBool("isMoving", isMoving);

        // ���½ӽ�״̬
        approach = Mathf.Abs(distanceToPlayer) <= attackRange;
        anim.SetBool("approach", approach);
        // ���ü���״̬����
        anim.SetBool("isActive", isActive);
    }

    // �ڱ༭���л��ƹ�����Χ�Ŀ��ӻ�ָʾ��
    private void OnDrawGizmos()
    {
        // ����ˮƽ������Χ
        Gizmos.color = Color.red;

        // ����2D������Χ
        float verticalAttackTolerance = 0.8f;  // ������б���һ��
        Vector2 center = transform.position;

        // ����ʵ�ʹ�����Χ����Բ�Σ�
        DrawEllipseGizmo(center, attackRange, verticalAttackTolerance, 20);
    }

    // ����������������Բ
    private void DrawEllipseGizmo(Vector2 center, float width, float height, int segments)
    {
        float angle = 0f;
        float angleStep = 2 * Mathf.PI / segments;

        Vector2 prevPoint = center + new Vector2(Mathf.Cos(0) * width, Mathf.Sin(0) * height);

        for (int i = 0; i <= segments; i++)
        {
            angle += angleStep;
            Vector2 newPoint = center + new Vector2(Mathf.Cos(angle) * width, Mathf.Sin(angle) * height);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
    public void FireBullet()
    {
        // ʵ�����ӵ�
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        // �����ӵ�����
        bullet bulletScript = bullet.GetComponent<bullet>();
        bulletScript.speed = bulletSpeed;
        bulletScript.direction = AngleToDirection(angle); // �Ƕ�ת��������
    }

    // �Ƕ�ת����������0��=�ң�90��=�ϣ�
    private Vector2 AngleToDirection(float angleDegrees)
    {
        float angleRadians = angleDegrees * Mathf.Deg2Rad;
        return new Vector2(
            Mathf.Cos(angleRadians),
            Mathf.Sin(angleRadians)
        ).normalized;
    }
}

