# 场景背景音乐控制系统使用指南

## 概述

`SceneBackgroundMusicController` 是一个专门为Unity游戏场景设计的背景音乐控制系统。它可以：

1. **自动播放背景音乐**：在场景启动时自动播放预设的背景音乐1
2. **按键切换音乐**：按P键停止当前音乐5秒，然后播放背景音乐2
3. **音量集成**：与现有的 `AudioVolumeManager` 系统完全集成
4. **灵活控制**：提供丰富的公共接口用于程序化控制

## 快速设置

### 1. 添加组件到场景

1. 在场景中创建一个空的GameObject（建议命名为"BackgroundMusicController"）
2. 将 `SceneBackgroundMusicController.cs` 脚本附加到该GameObject上

### 2. 配置音频片段

在Inspector面板中设置：
- **Background Music 1**: 场景默认播放的背景音乐
- **Background Music 2**: 按P键后播放的背景音乐
- **Music Source**: 可选，如果不设置会自动创建
- **Switch Key**: 切换音乐的按键（默认P键）
- **Stop Duration**: 停止音乐的持续时间（默认5秒）

### 3. 确保AudioVolumeManager存在

系统会自动查找场景中的 `AudioVolumeManager`，确保音量控制正常工作。

## 功能特性

### 自动功能
- ✅ 场景开始时自动播放背景音乐1
- ✅ 按P键触发音乐切换（停止5秒后播放音乐2）
- ✅ 自动应用音量管理器的音量设置
- ✅ 音量改变时自动更新背景音乐音量

### 手动控制API

```csharp
// 获取控制器引用
SceneBackgroundMusicController controller = FindFirstObjectByType<SceneBackgroundMusicController>();

// 立即播放音乐（无延迟）
controller.PlayMusic1Immediately();
controller.PlayMusic2Immediately();

// 切换音乐（带5秒延迟）
controller.SwitchToBackgroundMusic1();
controller.SwitchToBackgroundMusic2();

// 音乐控制
controller.StopMusic();
controller.PauseMusic();
controller.ResumeMusic();

// 状态查询
bool isPlayingMusic2 = controller.IsPlayingMusic2();
bool isSwitching = controller.IsSwitching();
bool isPlaying = controller.IsPlaying();

// 动态设置
controller.SetBackgroundMusic1(newAudioClip);
controller.SetBackgroundMusic2(newAudioClip);
controller.SetSwitchKey(KeyCode.M);
controller.SetStopDuration(3f);
```

## 键盘控制

| 按键 | 功能 | 说明 |
|------|------|------|
| P | 切换背景音乐 | 停止5秒后播放另一首音乐 |
| 1 | 立即播放音乐1 | 无延迟切换（需示例脚本） |
| 2 | 立即播放音乐2 | 无延迟切换（需示例脚本） |
| Space | 暂停/恢复 | 暂停或恢复当前音乐（需示例脚本） |
| S | 停止音乐 | 完全停止音乐播放（需示例脚本） |

> 注：除P键外，其他按键需要添加 `BackgroundMusicExample.cs` 脚本

## 与现有系统的集成

### AudioVolumeManager集成
- 自动检测并使用AudioVolumeManager的音量设置
- 音量改变时自动更新背景音乐音量
- 支持静音/取消静音功能

### 现有音频系统兼容
- 不会干扰现有的音效播放（Enemy.cs, BossController.cs等）
- 使用独立的音频源，避免冲突
- 支持与其他音频系统并存

## 使用示例

### 基础使用
```csharp
public class GameManager : MonoBehaviour
{
    private SceneBackgroundMusicController bgMusic;
    
    void Start()
    {
        bgMusic = FindFirstObjectByType<SceneBackgroundMusicController>();
    }
    
    // 在特定事件时切换音乐
    public void OnBossAppear()
    {
        bgMusic.SwitchToBackgroundMusic2();
    }
    
    public void OnBossDefeated()
    {
        bgMusic.SwitchToBackgroundMusic1();
    }
}
```

### UI按钮集成
```csharp
public class MusicControlUI : MonoBehaviour
{
    private SceneBackgroundMusicController bgMusic;
    
    void Start()
    {
        bgMusic = FindFirstObjectByType<SceneBackgroundMusicController>();
    }
    
    // 绑定到UI按钮
    public void OnMusicButton1() => bgMusic.PlayMusic1Immediately();
    public void OnMusicButton2() => bgMusic.PlayMusic2Immediately();
    public void OnStopButton() => bgMusic.StopMusic();
    public void OnPauseButton() => bgMusic.PauseMusic();
}
```

## 调试和故障排除

### 常见问题

1. **音乐不播放**
   - 检查音频片段是否已分配
   - 确认AudioSource组件正常工作
   - 查看Console是否有错误信息

2. **按P键无反应**
   - 确认 `switchKey` 设置正确
   - 检查是否有其他脚本拦截了按键输入
   - 确认游戏对象是活跃状态

3. **音量控制无效**
   - 检查场景中是否存在AudioVolumeManager
   - 确认AudioVolumeManager工作正常

### 调试信息
系统会在Console中输出详细的调试信息：
- 音乐播放状态
- 按键检测情况
- 音量更新信息
- 错误和警告信息

## 系统要求

- Unity 2019.4 或更高版本
- 现有的AudioVolumeManager系统（可选，但推荐）
- 音频片段资源（AudioClip格式）

## 文件结构

```
Assets/
├── SceneBackgroundMusicController.cs     # 主要控制脚本
├── BackgroundMusicExample.cs             # 使用示例脚本
├── UI_set/Scripts/AudioVolumeManager.cs  # 音量管理系统（已更新）
└── BackgroundMusicSystemGuide.md         # 本使用指南
```

## 版本信息

- **版本**: 1.0
- **创建日期**: 2024年
- **兼容性**: 与现有音频系统完全兼容
- **测试状态**: 已集成测试
