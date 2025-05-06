using UnityEngine;

public class OptionsButtonScript : MonoBehaviour
{
    // 在Unity编辑器中分配选项菜单面板
    public GameObject optionsPanel;

    // 当Options按钮被点击时调用
    public void OnOptionsButtonClick()
    {
        if (optionsPanel != null)
        {
            // 如果面板已激活，则隐藏；如果未激活，则显示
            bool isActive = optionsPanel.activeSelf;
            optionsPanel.SetActive(!isActive);
        }
        else
        {
            Debug.LogWarning("选项面板未分配！请在Inspector中设置引用。");
        }
    }

    // 关闭选项菜单
    public void CloseOptionsMenu()
    {
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
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