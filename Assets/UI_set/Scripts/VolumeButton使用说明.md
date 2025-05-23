# Volume Button 使用说明

## 功能介绍
VolumeButton 脚本可以将任何带有 Image 组件的 UI 对象转换为一个可点击的音量按钮，点击后可以切换静音/取消静音状态。

## 使用步骤

### 1. 添加脚本到 Volume Icon
1. 在 Unity 编辑器中，找到场景中的 `Volume Icon` 对象
2. 选中该对象，在 Inspector 面板中点击 `Add Component`
3. 搜索并添加 `VolumeButton` 脚本

### 2. 配置组件
在 Inspector 面板中，你会看到以下配置选项：

#### 音频管理器引用
- **Audio Volume Manager**: 拖拽场景中的 AudioVolumeManager 对象到这里，或点击"自动查找并设置AudioVolumeManager"按钮

#### 图标精灵
- **Volume On Icon**: 有声音时显示的图标
- **Volume Off Icon**: 静音时显示的图标

#### 视觉反馈
- **Click Scale**: 点击时的缩放比例（默认0.9）
- **Scale Duration**: 缩放动画持续时间（默认0.1秒）

### 3. 快速设置
在 Inspector 面板底部，有两个快捷按钮：
- **自动查找并设置AudioVolumeManager**: 自动在场景中查找并关联音频管理器
- **加载默认音量图标**: 从项目资源中搜索并加载音量相关的图标

## 功能特性

1. **点击切换静音**: 点击按钮可以在静音和有声音之间切换
2. **图标自动更新**: 根据当前音量状态自动切换显示的图标
3. **视觉反馈**: 
   - 点击时有缩放动画
   - 鼠标悬停时图标变亮
   - 鼠标离开时恢复正常
4. **自动保存状态**: 通过 AudioVolumeManager 自动保存音量设置

## 注意事项

1. 确保 Volume Icon 对象有 Image 组件
2. 确保场景中有 AudioVolumeManager 组件
3. 如果没有设置静音/非静音图标，按钮仍然可以工作，只是不会切换图标
4. 确保 Image 组件的 Raycast Target 选项已勾选（脚本会自动设置）

## 故障排除

### 点击没有反应
- 检查是否有 Canvas 和 EventSystem 在场景中
- 确保 Image 组件的 Raycast Target 已启用
- 检查是否有其他 UI 元素遮挡了按钮

### 找不到 AudioVolumeManager
- 确保场景中有 AudioVolumeManager 组件
- 尝试手动拖拽赋值或使用"自动查找"按钮

### 图标不切换
- 确保已经设置了 Volume On Icon 和 Volume Off Icon
- 检查图标资源是否正确加载 