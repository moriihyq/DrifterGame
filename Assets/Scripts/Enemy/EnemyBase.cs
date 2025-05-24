using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("基本属性")]
    public string enemyID; // 唯一标识符
    public string enemyType = "Normal"; // 敌人类型
    public int maxHealth = 100;
    public int currentHealth;
    
    [Header("状态")]
    public bool isActive = true;
    
    protected virtual void Awake()
    {
        // 生成唯一ID
        if (string.IsNullOrEmpty(enemyID))
        {
            enemyID = System.Guid.NewGuid().ToString();
        }
        
        currentHealth = maxHealth;
        
        // 确保有Enemy标签
        if (!gameObject.CompareTag("Enemy"))
        {
            gameObject.tag = "Enemy";
        }
    }
    
    public virtual void TakeDamage(int damage)
    {
        if (!isActive) return;
        
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }
    
    protected virtual void Die()
    {
        isActive = false;
        // 播放死亡动画、音效等
        Destroy(gameObject, 1f); // 1秒后销毁
    }
    
    // 用于存档系统
    public EnemyData GetEnemyData()
    {
        return new EnemyData
        {
            enemyID = this.enemyID,
            enemyType = this.enemyType,
            currentHealth = this.currentHealth,
            maxHealth = this.maxHealth,
            position = transform.position,
            isActive = this.isActive
        };
    }
    
    // 用于加载存档
    public void LoadEnemyData(EnemyData data)
    {
        enemyID = data.enemyID;
        enemyType = data.enemyType;
        currentHealth = data.currentHealth;
        maxHealth = data.maxHealth;
        transform.position = data.position;
        isActive = data.isActive;
        
        if (!isActive)
        {
            gameObject.SetActive(false);
        }
    }
} 