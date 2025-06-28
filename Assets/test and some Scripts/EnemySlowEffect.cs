using UnityEngine;
using System.Collections;

public class EnemySlowEffect : MonoBehaviour
{
    private bool isSlowed = false;
    private float originalMoveSpeed = 5f; // Enemy默认移动速度
    private float slowMultiplier = 1f;
    private Rigidbody2D rb;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // 获取Enemy组件中硬编码的移动速度值
        originalMoveSpeed = 5f; // 这是Enemy.cs中使用的速度值
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
        Debug.Log($"应用敌人减速效果，减速倍数: {slowMultiplier}");
        
        yield return new WaitForSeconds(duration);
        
        isSlowed = false;
        slowMultiplier = 1f;
        Debug.Log("敌人减速效果结束");
    }
    
    // 这个方法会在Enemy的UpdateMovement中被调用来获取调整后的速度
    public float GetAdjustedSpeed(float baseSpeed)
    {
        return isSlowed ? baseSpeed * slowMultiplier : baseSpeed;
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
