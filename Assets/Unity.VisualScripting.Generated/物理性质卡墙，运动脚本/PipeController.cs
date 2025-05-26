using UnityEngine;
using System.Collections;

public class PipeController : MonoBehaviour
{
    [Header("管子路径点")]
    [Tooltip("玩家从这里进入管子（通常与触发器位置一致或稍靠内）")]
    [SerializeField] private Transform entryPoint;
    [Tooltip("玩家将从这个位置出来（管子顶部）")]
    [SerializeField] private Transform exitPoint;

    [Header("管子设置")]
    [Tooltip("玩家在管子内移动的速度")]
    [SerializeField] private float travelSpeed = 3f; // 调整一个合适的速度
    [Tooltip("进入管子需要按下的按键")]
    [SerializeField] private KeyCode entryKey = KeyCode.W;
    [Tooltip("从管子中途下降需要按下的按键")]
    [SerializeField] private KeyCode descendKey = KeyCode.S;

    // --- 私有变量 ---
    private GameObject playerObject;
    private Rigidbody2D playerRb;
    private PlayerMovement playerMovementScript; // !! 确保这是你玩家移动脚本的正确类名 !!
    private Collider2D playerCollider;           // 玩家的碰撞体，用于临时禁用
    private float originalGravityScale;
    private bool isPlayerInTrigger = false;
    private bool isTraveling = false;          // 标记是否正在管子内传送 (上或下)
    private Coroutine currentTravelCoroutine = null; // 引用当前正在运行的传送协程

    // --- Unity 生命周期方法 ---

    void Start()
    {
        // 启动时检查路径点是否已设置
        if (entryPoint == null || exitPoint == null)
        {
            Debug.LogError("错误：管子 (" + gameObject.name + ") 没有设置 Entry Point 或 Exit Point！请在 Inspector 中拖拽 Transform。", gameObject);
            enabled = false;
        }
    }

    void Update()
    {
        // 情况1：玩家在触发器内，且 *没有* 正在传送，检测进入键'W'
        if (isPlayerInTrigger && !isTraveling && Input.GetKeyDown(entryKey))
        {
            if (playerObject != null)
            {
                // 如果有旧的协程残留（理论上不应该），先停止
                if (currentTravelCoroutine != null) StopCoroutine(currentTravelCoroutine);
                // 开始向上移动的协程
                currentTravelCoroutine = StartCoroutine(TravelPipeSequence(playerObject, true)); // true表示向上
            }
        }
        // 情况2：玩家正在传送中 (无论在不在触发器内)，检测下降键'S'
        else if (isTraveling && Input.GetKeyDown(descendKey))
        {
            if (playerObject != null)
            {
                Debug.Log("检测到下降键 S，中断当前移动，开始下降...");
                // 停止当前的协程 (无论是上还是下)
                if (currentTravelCoroutine != null) StopCoroutine(currentTravelCoroutine);
                // 开始向下移动的协程
                currentTravelCoroutine = StartCoroutine(TravelPipeSequence(playerObject, false)); // false表示向下
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isTraveling && other.CompareTag("Player")) // 只有不在传送过程中才允许记录玩家进入
        {
            Debug.Log("玩家进入管子触发区域。");
            playerObject = other.gameObject;
            isPlayerInTrigger = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == playerObject)
        {
            Debug.Log("玩家离开管子触发区域。");
            isPlayerInTrigger = false;
            // 注意：即使玩家离开触发器，如果isTraveling为true，我们仍然保留playerObject引用
            // 直到传送协程结束时才完全清除。
        }
    }

    // --- 协程：处理管子传送过程 (上或下) ---
    IEnumerator TravelPipeSequence(GameObject player, bool movingUp)
    {
        isTraveling = true; // 标记开始传送

        // --- 1. 获取/检查组件 & 禁用控制 ---
        if (playerRb == null) playerRb = player.GetComponent<Rigidbody2D>();
        if (playerMovementScript == null) playerMovementScript = player.GetComponent<PlayerMovement>(); // !! 替换成你的脚本名 !!
        if (playerCollider == null) playerCollider = player.GetComponent<Collider2D>(); // 获取玩家主碰撞体

        // 安全检查
        if (playerRb == null || playerCollider == null)
        {
            Debug.LogError("错误：玩家缺少 Rigidbody2D 或 Collider2D！无法传送。", player);
            isTraveling = false;
            currentTravelCoroutine = null;
            yield break; // 退出协程
        }
        if (playerMovementScript == null) Debug.LogWarning("警告：未找到 PlayerMovement 脚本，无法禁用/启用玩家输入。", player);

        // (可选) 临时禁用玩家碰撞，防止卡在管子边缘（传送结束后恢复）
        playerCollider.enabled = false;

        // 存储原始重力，禁用重力，停止速度
        originalGravityScale = playerRb.gravityScale;
        playerRb.gravityScale = 0f;
        playerRb.linearVelocity = Vector2.zero;
        if (playerMovementScript != null) playerMovementScript.enabled = false;

        // --- 2. 对齐并移动 ---
        Transform targetPoint = movingUp ? exitPoint : entryPoint;
        Vector3 startAlignPos = player.transform.position;
        Vector3 targetAlignPos = new Vector3(entryPoint.position.x, player.transform.position.y, player.transform.position.z); // 水平对齐到入口X坐标

        // (可选但推荐) 短暂地水平移动到入口中心线
        float alignDuration = 0.1f; // 对齐所需时间
        float alignTimer = 0f;
        while (alignTimer < alignDuration)
        {
             // 在此期间也允许按 S 中断（如果正在向上移动）
            if (movingUp && Input.GetKeyDown(descendKey)) {
                Debug.Log("在对齐过程中按S，中断并开始下降...");
                if (currentTravelCoroutine != null) StopCoroutine(currentTravelCoroutine);
                currentTravelCoroutine = StartCoroutine(TravelPipeSequence(playerObject, false)); // 开始下降
                yield break; // 退出当前(向上)的协程
            }

            alignTimer += Time.deltaTime;
            player.transform.position = Vector3.Lerp(startAlignPos, targetAlignPos, alignTimer / alignDuration);
            yield return null;
        }
        player.transform.position = targetAlignPos; // 确保精确对齐

        Debug.Log(movingUp ? "开始向上移动..." : "开始向下移动...");

        // 向目标点移动
        while (Vector2.Distance(player.transform.position, targetPoint.position) > 0.05f) // 使用稍小的阈值
        {
            // 在此循环中不再检测 'W' 键
            // 如果是向上移动，仍然检测 'S' 键以允许中途下降
            if (movingUp && Input.GetKeyDown(descendKey))
            {
                Debug.Log("在向上移动过程中按S，中断并开始下降...");
                if (currentTravelCoroutine != null) StopCoroutine(currentTravelCoroutine);
                currentTravelCoroutine = StartCoroutine(TravelPipeSequence(playerObject, false)); // 开始下降
                yield break; // 退出当前(向上)的协程
            }

            player.transform.position = Vector2.MoveTowards(player.transform.position, targetPoint.position, travelSpeed * Time.deltaTime);
            yield return null;
        }

        // --- 3. 到达目标点 & 恢复控制 ---
        player.transform.position = targetPoint.position; // 精确设置最终位置
        Debug.Log("到达目标点: " + (movingUp ? "出口" : "入口"));

        // 恢复重力
        playerRb.gravityScale = originalGravityScale;
        // 重新启用玩家移动脚本
        if (playerMovementScript != null) playerMovementScript.enabled = true;
        // 重新启用玩家碰撞体
        playerCollider.enabled = true;

        // --- 4. 清理状态 ---
        // 短暂延迟，防止因玩家仍在触发器内并按住W/S而立即重新触发
        yield return new WaitForSeconds(0.1f);

        isTraveling = false;        // 标记传送结束
        currentTravelCoroutine = null; // 清除协程引用

        // 只有当玩家已经不在入口触发器范围内时，才清除playerObject引用
        if (!isPlayerInTrigger)
        {
            playerObject = null;
        }
         Debug.Log("管子传送流程结束。isTraveling: " + isTraveling);
    }
}