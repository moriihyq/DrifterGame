using UnityEngine;
using UnityEngine.UI; // 如果你的背景是 UI Image

public class SingleLayerMouseFollow : MonoBehaviour
{
    [Header("Target Layer")]
    [Tooltip("将你想要移动的单个背景 UI 元素的 RectTransform 拖拽到这里")]
    public RectTransform backgroundLayer; // The single UI layer to move

    [Header("Movement Settings")]
    [Tooltip("移动幅度: 值越大，移动越多。可以尝试 5 到 30 之间的值")]
    public float moveStrength = 15f; // How much the layer moves relative to mouse offset

    [Tooltip("移动平滑度: 值越小越平滑，但响应越慢")]
    public float smoothness = 5f; // How quickly the layer follows the mouse

    private Vector3 initialPosition; // Store the starting position of the layer
    private Vector2 screenCenter;

    void Start()
    {
        if (backgroundLayer == null)
        {
            Debug.LogError("Background Layer RectTransform is not assigned!", this);
            this.enabled = false; // Disable script if target is not set
            return;
        }

        // Calculate screen center
        screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        // Store the initial position (use anchoredPosition for UI elements)
        initialPosition = backgroundLayer.anchoredPosition;
    }

    void Update()
    {
        if (backgroundLayer == null) return; // Safety check

        // Get current mouse position
        Vector2 mousePos = Input.mousePosition;

        // --- Optional: Recalculate center if screen resolution can change ---
        // screenCenter.x = Screen.width / 2f;
        // screenCenter.y = Screen.height / 2f;
        // ---

        // Calculate mouse offset from the screen center
        Vector2 offset = new Vector2(mousePos.x - screenCenter.x, mousePos.y - screenCenter.y);
        // Normalize the offset roughly based on screen size
        Vector2 normalizedOffset = new Vector2(offset.x / screenCenter.x, offset.y / screenCenter.y);

        // Calculate the target position based on initial position and mouse offset
        // We only want to affect X and Y position
        Vector3 targetOffset = new Vector3(normalizedOffset.x, normalizedOffset.y, 0) * moveStrength;
        Vector3 targetPosition = initialPosition + targetOffset;

        // Smoothly move the layer towards the target position using Lerp
        backgroundLayer.anchoredPosition = Vector3.Lerp(
            backgroundLayer.anchoredPosition,
            targetPosition,
            Time.deltaTime * smoothness
        );
    }
}