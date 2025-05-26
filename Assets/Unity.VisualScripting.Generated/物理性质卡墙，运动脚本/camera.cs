using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // 1. 目标 (玩家)
    public Transform target; // 在 Inspector 中拖拽玩家 GameObject 到这里

    // 2. 平滑速度/时间
    // 这个值越小，摄像机跟随得越快、越“硬”；值越大，跟随得越慢、越“软”
    public float smoothTime = 0.3f;

    // 3. 摄像机相对于目标的偏移量
    private Vector3 offset;

    // 4. SmoothDamp 需要的当前速度引用 (不需要手动修改)
    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        // 检查 Target 是否已设置
        if (target == null)
        {
            Debug.LogError("CameraFollow: Target not assigned! Please assign the player's Transform in the Inspector.");
            // 可以在这里尝试自动查找玩家，例如:
            // GameObject player = GameObject.FindGameObjectWithTag("Player");
            // if (player != null) target = player.transform;
            // else return; // 如果找不到就禁用脚本或停止
            enabled = false; // 禁用此脚本，避免后续出错
            return;
        }

        // 计算并存储初始偏移量
        // 这是摄像机当前位置与目标当前位置的差值
        offset = transform.position - target.position;
    }

    void LateUpdate()
    {
        // 如果目标丢失 (例如玩家被销毁了)，则停止更新
        if (target == null)
        {
            return;
        }

        // 1. 计算摄像机应该移动到的目标位置
        //    目标位置 = 玩家当前位置 + 初始偏移量
        Vector3 targetPosition = target.position + offset;

        // 确保 Z 轴偏移量保持不变 (对于 2D 游戏尤其重要，摄像机通常固定 Z 轴)
        // 如果你的游戏是纯 2D 俯视或侧视，并且不希望摄像机 Z 轴变化，可以取消下面这行的注释
        // targetPosition.z = transform.position.z;
        // 或者，更通用的方法是确保初始 offset 的 Z 值是你想要的固定 Z 差值

        // 2. 使用 SmoothDamp 平滑地移动摄像机
        //    参数：当前位置, 目标位置, ref 当前速度, 平滑时间
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

        // 可选：如果需要摄像机始终朝向玩家 (通常用于 3D 游戏)
        // transform.LookAt(target);
        // 对于你的 2D Tilemap 游戏，通常不需要 LookAt
    }
}
