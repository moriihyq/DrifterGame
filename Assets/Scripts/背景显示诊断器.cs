using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 背景显示诊断器 - 检查背景不显示的原因并修复
/// </summary>
public class 背景显示诊断器 : MonoBehaviour
{
    /// <summary>
    /// 一键诊断背景显示问题
    /// </summary>
    [ContextMenu("🔍 诊断背景显示问题")]
    public void DiagnoseBackgroundDisplay()
    {
        Debug.Log("<color=cyan>===== 🔍 背景显示诊断开始 =====</color>");
        
        bool hasIssues = false;
        
        // 1. 检查背景对象是否存在
        GameObject backgroundParent = GameObject.Find("BackgroundParallax");
        if (backgroundParent == null)
        {
            Debug.LogError("<color=red>❌ 严重问题：找不到BackgroundParallax对象！</color>");
            Debug.LogError("<color=yellow>建议：运行自动背景设置器创建背景</color>");
            hasIssues = true;
        }
        else
        {
            Debug.Log("<color=green>✅ BackgroundParallax对象存在</color>");
            
            // 2. 检查背景层数量
            int layerCount = backgroundParent.transform.childCount;
            if (layerCount == 0)
            {
                Debug.LogError("<color=red>❌ 问题：背景没有子层！</color>");
                hasIssues = true;
            }
            else
            {
                Debug.Log($"<color=green>✅ 找到 {layerCount} 个背景层</color>");
                
                // 3. 检查每个背景层
                for (int i = 0; i < layerCount; i++)
                {
                    Transform child = backgroundParent.transform.GetChild(i);
                    DiagnoseBackgroundLayer(child, ref hasIssues);
                }
            }
        }
        
        // 4. 检查摄像机
        DiagnoseCameraSettings(ref hasIssues);
        
        // 5. 检查渲染层级
        DiagnoseRenderingLayers(ref hasIssues);
        
        // 6. 总结和建议
        Debug.Log("<color=cyan>===== 📋 诊断结果 =====</color>");
        if (!hasIssues)
        {
            Debug.Log("<color=green>🎉 没有发现明显问题！背景应该正常显示</color>");
            Debug.Log("<color=yellow>如果还是看不到，试试调整摄像机位置或背景位置</color>");
        }
        else
        {
            Debug.Log("<color=red>⚠️ 发现了一些问题，请查看上面的详细信息</color>");
            Debug.Log("<color=cyan>可以尝试一键修复功能</color>");
        }
        
        Debug.Log("<color=cyan>===== 🔍 背景显示诊断完成 =====</color>");
    }
    
    /// <summary>
    /// 诊断单个背景层
    /// </summary>
    private void DiagnoseBackgroundLayer(Transform layer, ref bool hasIssues)
    {
        Debug.Log($"<color=white>检查背景层: {layer.name}</color>");
        
        // 检查SpriteRenderer
        SpriteRenderer sr = layer.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogError($"<color=red>  ❌ {layer.name} 缺少SpriteRenderer组件！</color>");
            hasIssues = true;
            return;
        }
        
        // 检查精灵
        if (sr.sprite == null)
        {
            Debug.LogError($"<color=red>  ❌ {layer.name} 没有分配精灵图片！</color>");
            hasIssues = true;
            return;
        }
        
        // 检查激活状态
        if (!layer.gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"<color=yellow>  ⚠️ {layer.name} 对象未激活</color>");
            hasIssues = true;
        }
        
        if (!sr.enabled)
        {
            Debug.LogWarning($"<color=yellow>  ⚠️ {layer.name} 的SpriteRenderer未启用</color>");
            hasIssues = true;
        }
        
        // 检查缩放
        Vector3 scale = layer.localScale;
        if (scale.x < 0.01f || scale.y < 0.01f)
        {
            Debug.LogWarning($"<color=yellow>  ⚠️ {layer.name} 缩放过小: {scale.x:F3} x {scale.y:F3}</color>");
            hasIssues = true;
        }
        
        // 检查透明度
        if (sr.color.a < 0.1f)
        {
            Debug.LogWarning($"<color=yellow>  ⚠️ {layer.name} 透明度过低: {sr.color.a:F2}</color>");
            hasIssues = true;
        }
        
        // 显示基本信息
        Debug.Log($"<color=white>  位置: {layer.position}</color>");
        Debug.Log($"<color=white>  缩放: {scale.x:F2} x {scale.y:F2}</color>");
        Debug.Log($"<color=white>  排序层: {sr.sortingLayerName}, 顺序: {sr.sortingOrder}</color>");
        Debug.Log($"<color=white>  颜色: {sr.color}, 透明度: {sr.color.a:F2}</color>");
    }
    
    /// <summary>
    /// 诊断摄像机设置
    /// </summary>
    private void DiagnoseCameraSettings(ref bool hasIssues)
    {
        Debug.Log("<color=white>检查摄像机设置...</color>");
        
        Camera mainCamera = Camera.main ?? FindObjectOfType<Camera>();
        if (mainCamera == null)
        {
            Debug.LogError("<color=red>❌ 找不到摄像机！</color>");
            hasIssues = true;
            return;
        }
        
        Debug.Log($"<color=green>✅ 找到摄像机: {mainCamera.name}</color>");
        Debug.Log($"<color=white>  摄像机位置: {mainCamera.transform.position}</color>");
        Debug.Log($"<color=white>  摄像机旋转: {mainCamera.transform.eulerAngles}</color>");
        
        if (mainCamera.orthographic)
        {
            Debug.Log($"<color=white>  正交摄像机，大小: {mainCamera.orthographicSize}</color>");
        }
        else
        {
            Debug.Log($"<color=white>  透视摄像机，FOV: {mainCamera.fieldOfView}</color>");
        }
        
        // 检查摄像机是否指向背景
        GameObject backgroundParent = GameObject.Find("BackgroundParallax");
        if (backgroundParent != null)
        {
            float distance = Vector3.Distance(mainCamera.transform.position, backgroundParent.transform.position);
            Debug.Log($"<color=white>  摄像机到背景距离: {distance:F1}</color>");
            
            if (distance > 100f)
            {
                Debug.LogWarning("<color=yellow>⚠️ 摄像机距离背景很远，可能看不到背景</color>");
                hasIssues = true;
            }
        }
    }
    
    /// <summary>
    /// 诊断渲染层级
    /// </summary>
    private void DiagnoseRenderingLayers(ref bool hasIssues)
    {
        Debug.Log("<color=white>检查渲染层级...</color>");
        
        GameObject backgroundParent = GameObject.Find("BackgroundParallax");
        if (backgroundParent == null) return;
        
        for (int i = 0; i < backgroundParent.transform.childCount; i++)
        {
            Transform child = backgroundParent.transform.GetChild(i);
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            
            if (sr != null)
            {
                if (sr.sortingOrder > 0)
                {
                    Debug.LogWarning($"<color=yellow>⚠️ {child.name} 排序顺序过高: {sr.sortingOrder}</color>");
                    Debug.LogWarning("<color=yellow>  背景应该使用负数或0的排序顺序</color>");
                    hasIssues = true;
                }
            }
        }
    }
    
    /// <summary>
    /// 一键修复常见问题
    /// </summary>
    [ContextMenu("🛠️ 一键修复背景显示")]
    public void QuickFixBackgroundDisplay()
    {
        Debug.Log("<color=cyan>🛠️ 开始一键修复背景显示问题...</color>");
        
        GameObject backgroundParent = GameObject.Find("BackgroundParallax");
        if (backgroundParent == null)
        {
            Debug.LogError("<color=red>❌ 找不到BackgroundParallax对象！</color>");
            Debug.LogError("<color=yellow>请先运行自动背景设置器创建背景</color>");
            return;
        }
        
        Camera mainCamera = Camera.main ?? FindObjectOfType<Camera>();
        if (mainCamera == null)
        {
            Debug.LogError("<color=red>❌ 找不到摄像机！</color>");
            return;
        }
        
        int fixedCount = 0;
        
        // 修复背景层
        for (int i = 0; i < backgroundParent.transform.childCount; i++)
        {
            Transform child = backgroundParent.transform.GetChild(i);
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            
            if (sr != null)
            {
                // 激活对象
                if (!child.gameObject.activeInHierarchy)
                {
                    child.gameObject.SetActive(true);
                    Debug.Log($"<color=green>✅ 激活了 {child.name}</color>");
                    fixedCount++;
                }
                
                // 启用SpriteRenderer
                if (!sr.enabled)
                {
                    sr.enabled = true;
                    Debug.Log($"<color=green>✅ 启用了 {child.name} 的SpriteRenderer</color>");
                    fixedCount++;
                }
                
                // 修复缩放过小的问题
                if (child.localScale.x < 0.1f || child.localScale.y < 0.1f)
                {
                    child.localScale = Vector3.one * 1.2f;
                    Debug.Log($"<color=green>✅ 修复了 {child.name} 的缩放</color>");
                    fixedCount++;
                }
                
                // 修复透明度
                if (sr.color.a < 0.5f)
                {
                    Color color = sr.color;
                    color.a = 1f;
                    sr.color = color;
                    Debug.Log($"<color=green>✅ 修复了 {child.name} 的透明度</color>");
                    fixedCount++;
                }
                
                // 修复排序层级
                if (sr.sortingOrder > 0)
                {
                    sr.sortingOrder = -5 - i; // 背景用负数
                    Debug.Log($"<color=green>✅ 修复了 {child.name} 的排序层级为 {sr.sortingOrder}</color>");
                    fixedCount++;
                }
            }
        }
        
        // 调整摄像机位置（如果距离背景太远）
        float distance = Vector3.Distance(mainCamera.transform.position, backgroundParent.transform.position);
        if (distance > 50f)
        {
            Vector3 newPos = backgroundParent.transform.position;
            newPos.z = mainCamera.transform.position.z; // 保持Z距离
            mainCamera.transform.position = newPos;
            Debug.Log($"<color=green>✅ 调整了摄像机位置到 {newPos}</color>");
            fixedCount++;
        }
        
        Debug.Log($"<color=green>🎉 修复完成！共修复了 {fixedCount} 个问题</color>");
        Debug.Log("<color=yellow>现在背景应该能够正常显示了！</color>");
    }
    
    /// <summary>
    /// 重置背景位置到摄像机前方
    /// </summary>
    [ContextMenu("📍 重置背景位置")]
    public void ResetBackgroundPosition()
    {
        GameObject backgroundParent = GameObject.Find("BackgroundParallax");
        Camera mainCamera = Camera.main ?? FindObjectOfType<Camera>();
        
        if (backgroundParent == null || mainCamera == null)
        {
            Debug.LogError("<color=red>❌ 找不到背景或摄像机</color>");
            return;
        }
        
        // 将背景放到摄像机前方
        Vector3 cameraPos = mainCamera.transform.position;
        Vector3 newBackgroundPos = new Vector3(cameraPos.x, cameraPos.y, cameraPos.z + 10f);
        backgroundParent.transform.position = newBackgroundPos;
        
        Debug.Log($"<color=green>✅ 背景位置重置为: {newBackgroundPos}</color>");
        Debug.Log("<color=yellow>背景现在应该出现在摄像机前方了</color>");
    }
}

#if UNITY_EDITOR
/// <summary>
/// 编辑器菜单扩展
/// </summary>
public static class BackgroundDisplayMenu
{
    [MenuItem("Tools/背景设置/🔍 诊断背景显示问题")]
    public static void DiagnoseDisplay()
    {
        var diagnoser = FindOrCreateDiagnoser();
        diagnoser.DiagnoseBackgroundDisplay();
        CleanupDiagnoser(diagnoser);
    }
    
    [MenuItem("Tools/背景设置/🛠️ 一键修复背景显示")]
    public static void QuickFixDisplay()
    {
        var diagnoser = FindOrCreateDiagnoser();
        diagnoser.QuickFixBackgroundDisplay();
        CleanupDiagnoser(diagnoser);
    }
    
    [MenuItem("Tools/背景设置/📍 重置背景位置")]
    public static void ResetPosition()
    {
        var diagnoser = FindOrCreateDiagnoser();
        diagnoser.ResetBackgroundPosition();
        CleanupDiagnoser(diagnoser);
    }
    
    private static 背景显示诊断器 FindOrCreateDiagnoser()
    {
        var diagnoser = Object.FindObjectOfType<背景显示诊断器>();
        if (diagnoser == null)
        {
            GameObject tempObj = new GameObject("TempBackgroundDisplayDiagnoser");
            diagnoser = tempObj.AddComponent<背景显示诊断器>();
        }
        return diagnoser;
    }
    
    private static void CleanupDiagnoser(背景显示诊断器 diagnoser)
    {
        if (diagnoser.gameObject.name == "TempBackgroundDisplayDiagnoser")
        {
            Object.DestroyImmediate(diagnoser.gameObject);
        }
    }
}
#endif 