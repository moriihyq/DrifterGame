using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SaveMessageUI : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI messageText;
    public Image backgroundImage;
    public CanvasGroup canvasGroup;
    
    [Header("Color Settings")]
    public Color successColor = new Color(0.2f, 0.8f, 0.2f, 0.9f);
    public Color errorColor = new Color(0.8f, 0.2f, 0.2f, 0.9f);
    
    [Header("Animation Settings")]
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.5f;
    
    private TMP_FontAsset customFont;
    
    private void Start()
    {
        // Load custom font
        customFont = Resources.Load<TMP_FontAsset>("UI_set/10 Font/CyberpunkCraftpixPixel SDF");
        if (customFont != null && messageText != null)
        {
            messageText.font = customFont;
        }
        
        // Hide on initialization
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        
        // Check if attached to SaveManager, if so, don't close GameObject
        if (GetComponent<SaveManager>() != null)
        {
            Debug.LogWarning("SaveMessageUI should not be directly attached to SaveManager GameObject! Please put it on a separate UI GameObject.");
            // Don't close SaveManager
            return;
        }
        
        gameObject.SetActive(false);
    }
    
    public void ShowMessage(string message, bool isError = false)
    {
        if (messageText != null)
        {
            messageText.text = message;
            
            // Ensure custom font is set
            if (customFont != null && messageText.font != customFont)
            {
                messageText.font = customFont;
            }
        }
        
        if (backgroundImage != null)
        {
            backgroundImage.color = isError ? errorColor : successColor;
        }
        
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(AnimateMessage());
    }
    
    private System.Collections.IEnumerator AnimateMessage()
    {
        // Fade in
        if (canvasGroup != null)
        {
            float elapsedTime = 0f;
            while (elapsedTime < fadeInDuration)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }
        
        // Wait for display time (from SaveManager)
        if (SaveManager.Instance != null)
        {
            yield return new WaitForSeconds(SaveManager.Instance.messageDisplayTime);
        }
        else
        {
            yield return new WaitForSeconds(2f);
        }
        
        // Fade out
        if (canvasGroup != null)
        {
            float elapsedTime = 0f;
            while (elapsedTime < fadeOutDuration)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            canvasGroup.alpha = 0f;
        }
        
        gameObject.SetActive(false);
    }
} 