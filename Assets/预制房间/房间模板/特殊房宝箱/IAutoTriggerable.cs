// IAutoTriggerable.cs
using UnityEngine;

public interface IAutoTriggerable
{
    void OnTriggerActivated(GameObject activator); // 当被激活时调用，传入激活者（如玩家）
    // 你也可以添加一个Deactivated方法，如果物体在玩家离开时需要有反应
    // void OnTriggerDeactivated(GameObject activator);
}
