using UnityEngine;

public class OptionsButtonScript : MonoBehaviour
{
    // 在Unity编辑器中分配选项菜单管理器
    public OptionsMenuManager optionsMenuManager;

    private void Start()
    {
        // 如果没有分配OptionsMenuManager，尝试自动查找
        if (optionsMenuManager == null)
        {
            optionsMenuManager = FindObjectOfType<OptionsMenuManager>();
            if (optionsMenuManager == null)
            {
                Debug.LogWarning("未找到OptionsMenuManager！请确保场景中存在该组件。");
            }
        }
    }

    // 当Options按钮被点击时调用
    public void OnOptionsButtonClick()
    {
        if (optionsMenuManager != null)
        {
            // 使用OptionsMenuManager的切换方法，确保dimmer逻辑生效
            optionsMenuManager.ToggleOptionsMenu();
        }
        else
        {
            Debug.LogWarning("OptionsMenuManager未分配或找不到！请检查引用设置。");
        }
    }

    // 关闭选项菜单
    public void CloseOptionsMenu()
    {
        if (optionsMenuManager != null)
        {
            // 使用OptionsMenuManager的关闭方法，确保dimmer逻辑生效
            optionsMenuManager.CloseOptionsMenu();
        }
    }

    // 检测OPTIONS按键按下事件
    void Update()
    {
        // 这里使用O键作为OPTIONS按键，可以根据需要修改
        if (Input.GetKeyDown(KeyCode.O))
        {
            OnOptionsButtonClick();
        }
    }
} 