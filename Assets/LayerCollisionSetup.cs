using UnityEngine;

// 此脚本设置不同层之间的碰撞关系
public class LayerCollisionSetup : MonoBehaviour
{
    // 在游戏开始时执行一次
    void Start()
    {
        SetupLayerCollisions();
    }

    // 设置层碰撞矩阵
    public void SetupLayerCollisions()
    {
        Debug.Log("正在设置层碰撞矩阵...");

        // 检查必要的层是否存在
        int enemyLayerIndex = LayerMask.NameToLayer("Enemy");
        int playerLayerIndex = LayerMask.NameToLayer("Player");
        int rollingPlayerLayerIndex = LayerMask.NameToLayer("RollingPlayer");

        // 输出层索引信息以便调试
        Debug.Log($"Enemy层索引: {enemyLayerIndex}, Player层索引: {playerLayerIndex}, RollingPlayer层索引: {rollingPlayerLayerIndex}");

        // 如果RollingPlayer层不存在，提示创建
        if (rollingPlayerLayerIndex == -1)
        {
            string message = "未找到'RollingPlayer'层！请在Unity编辑器中创建此层:\n" +
                "1. 打开Edit > Project Settings > Tags and Layers\n" +
                "2. 在Layers部分，找到一个未使用的User Layer(例如Layer 9)\n" +
                "3. 将其命名为'RollingPlayer'\n" +
                "4. 在Layer Collision Matrix中，取消'RollingPlayer'与'Enemy'层之间的勾选";
            
            Debug.LogWarning(message);
            
            #if UNITY_EDITOR
            // 在编辑器中创建新层的提示 (仅在编辑器中显示)
            UnityEditor.EditorUtility.DisplayDialog("需要创建层", message, "了解");
            #endif
            
            return;
        }

        // 如果Enemy层不存在，则无法设置碰撞关系
        if (enemyLayerIndex == -1)
        {
            Debug.LogWarning("未找到Enemy层！无法设置层碰撞关系。");
            return;
        }

        // 设置RollingPlayer层和Enemy层之间不产生碰撞
        // 注意：Physics2D.IgnoreLayerCollision仅在运行时生效
        if (rollingPlayerLayerIndex != -1 && enemyLayerIndex != -1)
        {
            Physics2D.IgnoreLayerCollision(rollingPlayerLayerIndex, enemyLayerIndex, true);
            Debug.Log($"已设置RollingPlayer层({rollingPlayerLayerIndex})和Enemy层({enemyLayerIndex})之间忽略碰撞");
        }
    }
}
