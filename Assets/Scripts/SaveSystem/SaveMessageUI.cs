using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SaveMessageUI : MonoBehaviour
{
    [Header("UI组件")]
    public TextMeshProUGUI messageText;
    public Image backgroundImage;
    public CanvasGroup canvasGroup;
    
    [Header("颜色设置")]
    public Color successColor = new Color(0.2f, 0.8f, 0.2f, 0.9f);
    public Color errorColor = new Color(0.8f, 0.2f, 0.2f, 0.9f);
    
    [Header("动画设置")]
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.5f;
    
    private void Start()
    {
        // 初始化时隐藏
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        gameObject.SetActive(false);
    }
    
    public void ShowMessage(string message, bool isError = false)
    {
        if (messageText != null)
        {
            messageText.text = message;
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
        // 淡入
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
        
        // 等待显示时间（从SaveManager获取）
        if (SaveManager.Instance != null)
        {
            yield return new WaitForSeconds(SaveManager.Instance.messageDisplayTime);
        }
        else
        {
            yield return new WaitForSeconds(2f);
        }
        
        // 淡出
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