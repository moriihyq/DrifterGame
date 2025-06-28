using UnityEngine;

/// <summary>
/// 构建测试 - 验证项目是否可以正常构建
/// </summary>
public class 构建测试 : MonoBehaviour
{
    void Start()
    {
        Debug.Log("🎮 游戏成功启动！项目构建测试通过！");
        Debug.Log($"📊 Unity版本: {Application.unityVersion}");
        Debug.Log($"🎯 平台: {Application.platform}");
        Debug.Log($"📱 产品名称: {Application.productName}");
    }

    void Update()
    {
        // 按ESC键退出游戏（仅在构建版本中）
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            #if !UNITY_EDITOR
            Application.Quit();
            #endif
        }
    }
} 