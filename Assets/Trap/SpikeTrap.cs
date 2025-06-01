using UnityEngine;
using System.Collections; // 引用 System.Collections 以使用协程

public class SpikeTrap : MonoBehaviour
{
    // === 公开变量 (在Inspector中设置) ===
    [Header("伤害设置")]
    public int damagePerTick = 10; // 每次地刺触发时造成的伤害量，固定为10点
    public float damageTickInterval = 0.5f; // 每次掉血的间隔时间（秒）

    // === 内部状态变量 ===
    private bool _isSpikeActive = false; // 地刺是否处于激活（伸出）状态
    private Coroutine _damageCoroutine = null; // 用于控制持续掉血的协程
    private bool _isPlayerInContact = false; // 标志位：玩家是否在地刺的Trigger范围内
    private HealthManager _healthManager; // 存储对HealthManager的引用

    private void Start()
    {
        // 获取HealthManager的引用
        _healthManager = HealthManager.Instance;
        if (_healthManager == null)
        {
            Debug.LogError("无法找到HealthManager实例！请确保场景中存在HealthManager。");
        }
    }

    // === 动画事件函数 ===
    // 这个函数会通过动画事件在开始帧调用
    [SerializeField]
    public void ActivateSpike()
    {
        _isSpikeActive = true;
        Debug.Log("地刺激活！开始造成伤害。");

        // 如果在激活时玩家已经在接触范围内，则立即开始掉血
        if (_isPlayerInContact)
        {
            StartDealingDamage();
        }
    }

    // 这个函数会通过动画事件在结束帧调用
    [SerializeField]
    public void DeactivateSpike()
    {
        _isSpikeActive = false;
        Debug.Log("地刺收回！停止造成伤害。");

        // 停止任何正在进行的掉血协程
        StopDealingDamage();
    }

    // === 触发器检测 (2D 游戏专用) ===
    // 当其他 2D Collider 进入地刺的 Trigger 范围时调用
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检查进入的对象是否是玩家 (通过标签判断)
        // 确保你的玩家 GameObject 的 Tag 设置为 "Player"
        if (other.CompareTag("Player"))
        {
            _isPlayerInContact = true; // 玩家进入范围
            Debug.Log(other.name + " 进入地刺范围。");

            // 如果地刺处于激活状态，则开始对玩家造成持续伤害
            if (_isSpikeActive)
            {
                StartDealingDamage();
            }
        }
    }

    // 当其他 2D Collider 离开地刺的 Trigger 范围时调用
    private void OnTriggerExit2D(Collider2D other)
    {
        // 检查离开的对象是否是玩家
        if (other.CompareTag("Player"))
        {
            Debug.Log(other.name + " 离开地刺范围。");
            StopDealingDamage(); // 玩家离开，停止掉血
            _isPlayerInContact = false; // 玩家不再接触
        }
    }

    // === 伤害处理协程控制 ===
    private void StartDealingDamage()
    {
        // 在启动新的协程之前，确保停止任何之前可能正在运行的掉血协程
        // 这可以防止多次启动协程导致的问题
        if (_damageCoroutine != null)
        {
            StopCoroutine(_damageCoroutine);
        }
        // 启动持续掉血的协程
        _damageCoroutine = StartCoroutine(DealDamageOverTime());
    }

    private void StopDealingDamage()
    {
        // 如果有掉血协程正在运行，则停止它
        if (_damageCoroutine != null)
        {
            StopCoroutine(_damageCoroutine);
            _damageCoroutine = null; // 将引用置空
            Debug.Log("停止对玩家造成伤害。");
        }
    }

    // 持续掉血的协程
    private IEnumerator DealDamageOverTime()
    {
        // 循环条件：
        // 1. 地刺仍然激活 (_isSpikeActive)
        // 2. 玩家仍然在地刺的触发器范围内 (_isPlayerInContact)
        // 3. HealthManager存在且玩家还活着
        while (_isSpikeActive && _isPlayerInContact && _healthManager != null && _healthManager.GetCurrentHealth() > 0)
        {
            // 通过HealthManager对玩家造成伤害
            _healthManager.TakeDamage(damagePerTick);
            yield return new WaitForSeconds(damageTickInterval); // 等待指定的时间间隔
        }

        // 当循环结束（例如，地刺收回、玩家离开、玩家死亡）时，确保停止协程
        StopDealingDamage();
    }
}