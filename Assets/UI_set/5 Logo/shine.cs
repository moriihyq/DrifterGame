using UnityEngine;
using UnityEngine.UI; // 如果是 UI Image
using System.Collections;

public class NeonFlicker : MonoBehaviour
{
    public float minIntensity = 0.2f; // 最小亮度/Alpha
    public float maxIntensity = 1.0f; // 最大亮度/Alpha
    public float minFlickerSpeed = 0.05f; // 最快闪烁间隔
    public float maxFlickerSpeed = 0.3f; // 最慢闪烁间隔

    private SpriteRenderer spriteRenderer;
    private Image uiImage;
    // private UnityEngine.Rendering.Universal.Light2D light2D; // 如果用 Light2D

    private Color originalColor;
    private float originalIntensity; // for Light2D

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        uiImage = GetComponent<Image>();
        // light2D = GetComponent<UnityEngine.Rendering.Universal.Light2D>();

        if (spriteRenderer != null) originalColor = spriteRenderer.color;
        if (uiImage != null) originalColor = uiImage.color;
        // if (light2D != null) originalIntensity = light2D.intensity;

        StartCoroutine(Flicker());
    }

    IEnumerator Flicker()
    {
        while (true)
        {
            // 随机决定下一个亮度
            float targetIntensity = Random.Range(minIntensity, maxIntensity);
            // 随机决定下一次闪烁的时间间隔
            float flickerTime = Random.Range(minFlickerSpeed, maxFlickerSpeed);

            // 应用亮度
            if (spriteRenderer != null)
            {
                Color newColor = originalColor;
                newColor.a = targetIntensity; // 控制 Alpha
                // 或者如果你用了带 Emission 的材质，可以控制 Emission 强度
                // spriteRenderer.material.SetColor("_EmissionColor", originalColor * targetIntensity);
                spriteRenderer.color = newColor;
            }
            if (uiImage != null)
            {
                Color newColor = originalColor;
                newColor.a = targetIntensity;
                uiImage.color = newColor;
            }
            // if (light2D != null)
            // {
            //     light2D.intensity = originalIntensity * targetIntensity;
            // }

            // 等待随机时间
            yield return new WaitForSeconds(flickerTime);
        }
    }
}
