// PlayerStats.cs
using UnityEngine;
// using UnityEngine.UI; // 如果需要在UI上显示等级

public class PlayerStats : MonoBehaviour
{
    public int currentLevel = 1;
    public int maxLevel = 99;
    // public Text levelTextUI; // (可选) 用于显示等级的UI Text

    public delegate void OnLevelChanged(int newLevel); // 定义委托
    public event OnLevelChanged onLevelChanged;      // 定义事件，当等级改变时触发

    void Start()
    {
        UpdateLevelUI();
        // 可以在这里触发一次事件，让其他监听者获取初始等级
        onLevelChanged?.Invoke(currentLevel);
    }

    public void LevelUp(int levelsToGain)
    {
        if (currentLevel >= maxLevel)
        {
            Debug.Log("Player is already at max level (" + maxLevel + ").");
            return;
        }

        currentLevel += levelsToGain;
        if (currentLevel > maxLevel)
        {
            currentLevel = maxLevel;
        }

        Debug.Log(gameObject.name + " leveled up! Current level: " + currentLevel);
        // 在这里可以添加升级时的效果，比如播放音效、粒子效果等
        // PlayLevelUpEffect();

        UpdateLevelUI();

        // 触发等级改变事件
        onLevelChanged?.Invoke(currentLevel);
        // 其他模块（如UI、技能系统）可以订阅这个事件来响应等级变化
    }

    void UpdateLevelUI()
    {
        // if (levelTextUI != null)
        // {
        //     levelTextUI.text = "Level: " + currentLevel;
        // }
        // 如果有更复杂的UI更新，可以由专门的UI管理器监听onLevelChanged事件
    }

    // 也可以有其他属性，如经验值、生命值、攻击力等
    // public int experience;
    // public int experienceToNextLevel;
    // public void GainExperience(int amount) { /* ... */ }
}