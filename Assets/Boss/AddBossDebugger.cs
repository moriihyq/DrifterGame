using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[InitializeOnLoad]
public class AddBossDebugger
{
    static AddBossDebugger()
    {
        Debug.Log("Boss调试工具已加载");
    }

    // 在菜单中添加一个选项，用于为选中的Boss对象添加调试组件
    [MenuItem("Tools/Boss工具/添加调试显示组件")]
    static void AddDebugDisplayToBoss()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            Debug.LogWarning("请先选择Boss游戏对象！");
            return;
        }

        BossController bossController = selected.GetComponent<BossController>();
        if (bossController == null)
        {
            Debug.LogWarning("所选对象不是Boss！请选择带有BossController组件的对象。");
            return;
        }

        // 检查是否已有调试组件
        BossDebugDisplay existingDisplay = selected.GetComponent<BossDebugDisplay>();
        if (existingDisplay != null)
        {
            Debug.Log("所选Boss已有调试显示组件！");
            return;
        }

        // 添加调试组件
        selected.AddComponent<BossDebugDisplay>();
        Debug.Log("已成功为Boss添加调试显示组件！游戏运行时按Tab键可切换显示。");
    }

    // 在菜单中添加一个选项，用于查找场景中的所有Boss
    [MenuItem("Tools/Boss工具/查找所有Boss")]
    static void FindAllBosses()
    {
        BossController[] allBosses = Object.FindObjectsOfType<BossController>();
        if (allBosses.Length == 0)
        {
            Debug.LogWarning("场景中未找到任何Boss（带有BossController组件的对象）!");
            return;
        }

        Debug.Log($"场景中共找到 {allBosses.Length} 个Boss:");
        for (int i = 0; i < allBosses.Length; i++)
        {
            Debug.Log($"  {i+1}. {allBosses[i].name} - 位置: {allBosses[i].transform.position}");
        }

        // 选中第一个Boss
        Selection.activeGameObject = allBosses[0].gameObject;
    }
}
#endif

// 非编辑器运行时使用的帮助器类
public class BossDebugHelper : MonoBehaviour
{
    void Start()
    {
        BossController boss = GetComponent<BossController>();
        if (boss != null && GetComponent<BossDebugDisplay>() == null)
        {
            gameObject.AddComponent<BossDebugDisplay>();
            Debug.Log("运行时自动添加了Boss调试显示组件。按Tab键切换显示。");
        }
    }
}
