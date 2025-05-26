// ITimedBreakable.cs
using UnityEngine;

public interface ITimedBreakable
{
    void OnPlayerStepOn();    // 当玩家踩在上面时调用
    void OnPlayerStepOff();   // 当玩家离开时调用 (可选，用于重置或暂停计时)
    void StartBreakingSequence(); // 开始破碎序列 (内部可能包含计时和动画)
    void Break();             // 最终执行破碎的方法
}