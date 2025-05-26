// PipeEntrance.cs
using UnityEngine;
using System.Collections;

public class PipeEntrance : MonoBehaviour, IInteractable
{
    public Transform exitPoint;             // 管道的出口Transform
    public float travelSpeed = 5f;          // 玩家在管道中移动的速度
    public AnimationCurve travelCurve = AnimationCurve.Linear(0,0,1,1); // 移动的缓动曲线 (可选)
    public Vector3 entryOffset = new Vector2(0, -0.5f); // 玩家进入管道时相对于入口的位置偏移
    public float entryScale = 0.5f;         // 玩家进入管道时的缩放比例 (可选)
    public AudioClip entrySound;
    public AudioClip exitSound;

    private bool isPlayerInRange = false;
    private GameObject currentPlayer;
    private AudioSource audioSource;

    // (可选) 存储玩家进入管道前的状态
    private Vector3 originalPlayerScale;
    private bool originalPlayerGravityState;
    private MonoBehaviour[] playerMovementScripts; // 假设玩家有多个控制脚本

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        if (exitPoint == null)
        {
            Debug.LogError("PipeEntrance: Exit Point is not set for " + gameObject.name);
            enabled = false; // 禁用脚本如果出口未设置
        }
    }

    public string GetInteractionPrompt()
    {
        return "按 [互动键] 进入管道"; // 你可以在玩家脚本里替换[互动键]为实际按键
    }

    public void Interact(GameObject interactor)
    {
        if (!isPlayerInRange || currentPlayer != interactor || exitPoint == null)
        {
            Debug.LogWarning("Cannot interact with pipe. Player not in range or exit not set.");
            return;
        }

        Debug.Log(interactor.name + " is entering the pipe: " + gameObject.name);
        StartCoroutine(TravelThroughPipe(interactor));
    }

    private IEnumerator TravelThroughPipe(GameObject player)
    {
        // 1. 禁用玩家控制和物理
        PlayerInteractionController playerInteraction = player.GetComponent<PlayerInteractionController>();
        if (playerInteraction != null) playerInteraction.SetPlayerControl(false); // 通知玩家脚本禁用控制

        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        Collider2D playerCollider = player.GetComponent<Collider2D>();

        originalPlayerScale = player.transform.localScale;
        if (playerRb != null)
        {
            originalPlayerGravityState = playerRb.simulated; // 或者 playerRb.isKinematic
            playerRb.linearVelocity = Vector2.zero;
            playerRb.simulated = false; // 或者 playerRb.isKinematic = true;
        }
        if (playerCollider != null) playerCollider.enabled = false;


        // 2. (可选) 播放进入动画/音效
        if (entrySound && audioSource) audioSource.PlayOneShot(entrySound);
        // 玩家移动到入口中心并缩小
        Vector3 entryTargetPos = transform.position + entryOffset;
        float animDuration = 0.5f; // 进入动画时长
        float timer = 0f;
        Vector3 startPos = player.transform.position;
        Vector3 startScale = player.transform.localScale;

        while(timer < animDuration)
        {
            player.transform.position = Vector3.Lerp(startPos, entryTargetPos, timer/animDuration);
            player.transform.localScale = Vector3.Lerp(startScale, Vector3.one * entryScale, timer/animDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        player.transform.position = entryTargetPos;
        player.transform.localScale = Vector3.one * entryScale;
        
        // 使玩家在管道内不可见 (如果管道不是透明的)
        SpriteRenderer playerSprite = player.GetComponent<SpriteRenderer>();
        if (playerSprite != null) playerSprite.enabled = false;


        // 3. 沿路径（简单情况：直线到出口）移动玩家
        Vector3 pipeStartPos = player.transform.position; // 已经是entryTargetPos
        Vector3 pipeEndPos = exitPoint.position + entryOffset; // 出口也应用相同偏移，方向相反
        float journeyLength = Vector3.Distance(pipeStartPos, pipeEndPos);
        float journeyTimer = 0f;

        while (journeyTimer < journeyLength / travelSpeed)
        {
            float fractionOfJourney = (journeyTimer * travelSpeed) / journeyLength;
            float curveValue = travelCurve.Evaluate(fractionOfJourney); // 使用缓动曲线
            player.transform.position = Vector3.Lerp(pipeStartPos, pipeEndPos, curveValue);
            journeyTimer += Time.deltaTime;
            yield return null;
        }
        player.transform.position = pipeEndPos;


        // 4. (可选) 播放退出动画/音效
        if (exitSound && audioSource) AudioSource.PlayClipAtPoint(exitSound, exitPoint.position); // Use PlayClipAtPoint for world space sound

        // 玩家从出口处放大出现
        if (playerSprite != null) playerSprite.enabled = true;
        timer = 0f;
        animDuration = 0.5f; // 退出动画时长
        startPos = player.transform.position;
        startScale = player.transform.localScale; // 当前是缩小状态
        // 玩家实际出现位置应该是出口点，而不是带偏移的
        Vector3 finalExitPos = exitPoint.position;


        while(timer < animDuration)
        {
            player.transform.position = Vector3.Lerp(startPos, finalExitPos, timer/animDuration);
            player.transform.localScale = Vector3.Lerp(startScale, originalPlayerScale, timer/animDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        player.transform.position = finalExitPos;
        player.transform.localScale = originalPlayerScale;


        // 5. 恢复玩家控制和物理
        if (playerRb != null) playerRb.simulated = originalPlayerGravityState; // 或者 playerRb.isKinematic = false;
        if (playerCollider != null) playerCollider.enabled = true;
        if (playerInteraction != null) playerInteraction.SetPlayerControl(true); // 通知玩家脚本恢复控制

        currentPlayer = null; // 允许其他玩家或再次交互
        isPlayerInRange = false; // 强制玩家离开范围，避免立即再次进入 (除非出口就在入口旁)
    }


    // 使用触发器检测玩家是否在管道入口附近
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered pipe trigger: " + other.name);
            isPlayerInRange = true;
            currentPlayer = other.gameObject;
            // (可选) 显示交互提示UI
            PlayerInteractionController playerInteraction = currentPlayer.GetComponent<PlayerInteractionController>();
            if (playerInteraction != null)
            {
                playerInteraction.DisplayInteractionPrompt(GetInteractionPrompt(), this);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.gameObject == currentPlayer)
        {
            Debug.Log("Player exited pipe trigger: " + other.name);
            isPlayerInRange = false;
            // (可选) 隐藏交互提示UI
            PlayerInteractionController playerInteraction = currentPlayer.GetComponent<PlayerInteractionController>();
            if (playerInteraction != null)
            {
                playerInteraction.HideInteractionPrompt(this);
            }
            currentPlayer = null;
        }
    }
}