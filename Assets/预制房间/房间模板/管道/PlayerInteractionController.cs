// PlayerInteractionController.cs
using UnityEngine;
using UnityEngine.UI; // 如果要显示提示文本

public class PlayerInteractionController : MonoBehaviour
{
    public KeyCode interactionKey = KeyCode.E;
    public Text interactionPromptText; // (可选) UI Text元素用于显示提示

    private IInteractable currentInteractable;
    private PlayerMovement playerMovement; // 假设你有这个脚本控制移动
    // 你可能还有其他脚本如 PlayerAttack 等

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>(); // 获取移动脚本
        if (interactionPromptText != null)
        {
            interactionPromptText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(interactionKey) && currentInteractable != null)
        {
            Debug.Log("Interaction key pressed for: " + ((MonoBehaviour)currentInteractable).name);
            currentInteractable.Interact(this.gameObject);
        }
    }

    public void DisplayInteractionPrompt(string prompt, IInteractable interactable)
    {
        currentInteractable = interactable;
        if (interactionPromptText != null)
        {
            interactionPromptText.text = prompt.Replace("[互动键]", interactionKey.ToString());
            interactionPromptText.gameObject.SetActive(true);
        }
        Debug.Log("Can interact with: " + ((MonoBehaviour)interactable).name + " - Prompt: " + prompt);
    }

    public void HideInteractionPrompt(IInteractable interactable)
    {
        // 确保我们隐藏的是当前正在交互或提示的物体
        if (currentInteractable == interactable)
        {
            currentInteractable = null;
            if (interactionPromptText != null)
            {
                interactionPromptText.gameObject.SetActive(false);
            }
            Debug.Log("No longer interacting with: " + ((MonoBehaviour)interactable).name);
        }
    }

    // 这个方法会被管道脚本调用，以禁用/启用玩家的常规控制
    public void SetPlayerControl(bool canControl)
    {
        if (playerMovement != null)
        {
            playerMovement.enabled = canControl;
        }
        // 也可以在这里禁用/启用其他玩家脚本，如攻击、跳跃等
        // GetComponent<PlayerAttack>().enabled = canControl;

        if (canControl)
        {
            Debug.Log("Player control ENABLED");
        }
        else
        {
            Debug.Log("Player control DISABLED");
        }
    }
}
