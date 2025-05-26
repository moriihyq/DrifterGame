// VictoryPortal.cs
using UnityEngine;
// using UnityEngine.SceneManagement; // 如果结算界面在不同场景

public class VictoryPortal : MonoBehaviour, IInteractable
{
    public string interactionPrompt = "按 [互动键] 进入传送门";
    // public string victorySceneName = "VictoryScene"; // 如果结算界面是一个单独的场景
    public GameObject victoryUICanvas; // 直接引用场景中的结算UI Canvas

    private bool isPlayerInRange = false;
    private GameObject currentPlayer;

    void Start()
    {
        if (victoryUICanvas == null)
        {
            Debug.LogWarning("VictoryPortal: Victory UI Canvas is not set for " + gameObject.name + ". Victory screen might not show.");
        }
        else
        {
            victoryUICanvas.SetActive(false); // 确保结算界面初始是隐藏的
        }
    }

    public string GetInteractionPrompt()
    {
        return interactionPrompt;
    }

    public void Interact(GameObject interactor)
    {
        if (!isPlayerInRange || currentPlayer != interactor)
        {
            Debug.LogWarning("Cannot interact with portal. Player not in range or not the correct interactor.");
            return;
        }

        Debug.Log(interactor.name + " has reached the Victory Portal!");

        // 触发游戏胜利逻辑
        TriggerVictory();
    }

    private void TriggerVictory()
    {
        // 选项1: 直接激活场景内的UI Canvas
        if (victoryUICanvas != null)
        {
            victoryUICanvas.SetActive(true);
            // （可选）暂停游戏
            Time.timeScale = 0f; // 暂停游戏时间
            // （可选）禁用玩家控制，避免玩家在结算界面时还能移动
            PlayerInteractionController playerInteraction = currentPlayer.GetComponent<PlayerInteractionController>();
            if(playerInteraction != null)
            {
                playerInteraction.SetPlayerControl(false);
            }
            Debug.Log("Victory UI Activated!");
        }
        else
        {
            Debug.LogError("Victory UI Canvas is not assigned in VictoryPortal!");
        }

        // 选项2: 通知GameManager处理胜利状态和UI (更推荐用于复杂项目)
        // if (GameManager.Instance != null)
        // {
        //     GameManager.Instance.PlayerWon();
        // }
        // else
        // {
        //    Debug.LogError("GameManager instance not found!");
        // }

        // 选项3: 加载新的结算场景
        // SceneManager.LoadScene(victorySceneName);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            currentPlayer = other.gameObject;
            PlayerInteractionController playerInteraction = currentPlayer.GetComponent<PlayerInteractionController>();
            if (playerInteraction != null)
            {
                playerInteraction.DisplayInteractionPrompt(GetInteractionPrompt(), this);
            }
            Debug.Log("Player entered Victory Portal trigger: " + other.name);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.gameObject == currentPlayer)
        {
            isPlayerInRange = false;
            PlayerInteractionController playerInteraction = currentPlayer.GetComponent<PlayerInteractionController>();
            if (playerInteraction != null)
            {
                playerInteraction.HideInteractionPrompt(this);
            }
            currentPlayer = null;
            Debug.Log("Player exited Victory Portal trigger: " + other.name);
        }
    }
}