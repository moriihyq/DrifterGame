using UnityEngine;
using System.Collections;
using System.Reflection;

public class BossSlowEffect : MonoBehaviour
{
    private bool isSlowed = false;
    private float originalMoveSpeed;
    private float slowMultiplier = 1f;
    private BossController bossController;
    
    void Start()
    {
        bossController = GetComponent<BossController>();
        
        // 使用反射来获取私有的moveSpeed字段
        if (bossController != null)
        {
            FieldInfo moveSpeedField = typeof(BossController).GetField("moveSpeed", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (moveSpeedField != null)
            {
                originalMoveSpeed = (float)moveSpeedField.GetValue(bossController);
            }
            else
            {
                // 如果反射失败，使用默认值
                originalMoveSpeed = 3f;
            }
        }
    }
    
    public void ApplySlowEffect(float slowStrength, float duration)
    {
        if (isSlowed)
        {
            // 如果已经有减速效果，停止之前的协程
            StopAllCoroutines();
        }
        
        slowMultiplier = slowStrength;
        StartCoroutine(SlowCoroutine(duration));
    }
    
    IEnumerator SlowCoroutine(float duration)
    {
        isSlowed = true;
        
        // 使用反射来修改Boss的移动速度
        if (bossController != null)
        {
            FieldInfo moveSpeedField = typeof(BossController).GetField("moveSpeed", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (moveSpeedField != null)
            {
                float newSpeed = originalMoveSpeed * slowMultiplier;
                moveSpeedField.SetValue(bossController, newSpeed);
                Debug.Log($"Boss速度从 {originalMoveSpeed} 减速到 {newSpeed}");
            }
        }
        
        yield return new WaitForSeconds(duration);
        
        // 恢复原始速度
        if (bossController != null)
        {
            FieldInfo moveSpeedField = typeof(BossController).GetField("moveSpeed", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (moveSpeedField != null)
            {
                moveSpeedField.SetValue(bossController, originalMoveSpeed);
                Debug.Log($"Boss速度恢复到 {originalMoveSpeed}");
            }
        }
        
        isSlowed = false;
        slowMultiplier = 1f;
    }
    
    public bool IsSlowed()
    {
        return isSlowed;
    }
    
    public float GetSlowMultiplier()
    {
        return slowMultiplier;
    }
}
