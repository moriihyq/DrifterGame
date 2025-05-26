// IInteractable.cs
using UnityEngine;

public interface IInteractable
{
    void Interact(GameObject interactor); // 传入发起交互的对象 (比如玩家)
    string GetInteractionPrompt(); // (可选) 获取交互提示文本，如 "按E进入管道"
}
