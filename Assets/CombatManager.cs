using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [Header("设置")]
    [SerializeField] private bool showDebugInfo = true; // 是否显示调试信息
    
    // 单例实例
    public static CombatManager Instance { get; private set; }
    
    private void Awake()
    {
        // 单例模式实现
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // 记录伤害事件
    public void LogDamage(GameObject attacker, GameObject target, int damageAmount)
    {
        if (!showDebugInfo) return;
        
        Debug.Log($"{attacker.name} 对 {target.name} 造成了 {damageAmount} 点伤害");
    }
    
    // 检查是否击杀
    public bool CheckIfKilled(int remainingHealth)
    {
        return remainingHealth <= 0;
    }
    
    // 处理击杀事件
    public void HandleKill(GameObject killer, GameObject victim)
    {
        if (!showDebugInfo) return;
        
        Debug.Log($"{killer.name} 击杀了 {victim.name}");
        
        // 这里可以添加额外的击杀奖励逻辑
        // 例如：给予经验值、掉落物品等
    }
    
    // 根据距离判断是否在攻击范围内
    public bool IsInAttackRange(Vector3 attackerPosition, Vector3 targetPosition, float attackRange)
    {
        float distance = Vector3.Distance(attackerPosition, targetPosition);
        return distance <= attackRange;
    }
}
