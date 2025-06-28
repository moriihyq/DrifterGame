using UnityEngine;

// 用于检查和设置翻滚功能所需的组件和层
public class RollSetupCheck : MonoBehaviour
{
    void Start()
    {
        CheckRollRequirements();
    }

    // 检查翻滚功能的必要设置
    void CheckRollRequirements()
    {
        // 1. 检查必要的层是否存在
        CheckRequiredLayers();
        
        // 2. 检查层碰撞设置组件是否存在
        CheckLayerCollisionSetup();
        
        // 3. 检查玩家是否有Animator和翻滚动画
        CheckPlayerAnimator();
    }

    // 检查必要的层是否存在
    void CheckRequiredLayers()
    {
        int enemyLayerIndex = LayerMask.NameToLayer("Enemy");
        int rollingPlayerLayerIndex = LayerMask.NameToLayer("RollingPlayer");

        if (enemyLayerIndex == -1)
        {
            Debug.LogError("<color=red>警告: 未找到'Enemy'层！翻滚穿透功能需要此层。</color>\n" +
                "请在Edit > Project Settings > Tags and Layers中创建该层。");
        }

        if (rollingPlayerLayerIndex == -1)
        {
            Debug.LogError("<color=red>警告: 未找到'RollingPlayer'层！翻滚穿透功能需要此层。</color>\n" +
                "请在Edit > Project Settings > Tags and Layers中创建该层，并在Layer Collision Matrix中取消它与Enemy层的碰撞。");
        }
    }

    // 检查层碰撞设置组件是否存在
    void CheckLayerCollisionSetup()
    {
        LayerCollisionSetup layerCollisionSetup = FindFirstObjectByType<LayerCollisionSetup>();

        if (layerCollisionSetup == null)
        {
            Debug.LogWarning("<color=yellow>注意: 场景中未找到LayerCollisionSetup组件！</color>\n" +
                "翻滚穿透功能需要此组件来设置层碰撞关系。\n" +
                "正在尝试自动添加...");

            GameObject layerManager = new GameObject("LayerManager");
            layerCollisionSetup = layerManager.AddComponent<LayerCollisionSetup>();
            Debug.Log("<color=green>已创建LayerManager对象并添加LayerCollisionSetup组件。</color>");
        }
    }

    // 检查玩家是否有Animator和翻滚动画
    void CheckPlayerAnimator()
    {
        PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
        
        if (playerMovement != null)
        {
            Animator animator = playerMovement.GetComponent<Animator>() ?? playerMovement.GetComponentInChildren<Animator>();
            
            if (animator == null)
            {
                Debug.LogError("<color=red>警告: 玩家没有Animator组件！</color>\n" +
                    "翻滚功能需要Animator组件来播放翻滚动画。");
                return;
            }
            
            // 尝试检测翻滚动画参数是否存在
            // 注意：运行时检测参数存在与否的方法有限制，这里只是一个简单的提示
            Debug.Log("<color=yellow>提示: 请确保在Animator Controller中添加了名为\"Roll\"的触发器参数和对应的翻滚动画状态。</color>");
        }
        else
        {
            Debug.LogWarning("<color=yellow>注意: 场景中未找到PlayerMovement组件！</color>\n" +
                "无法检查玩家的动画设置。");
        }
    }
}
