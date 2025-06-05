# 技能冷却UI手动设置指南

## 📋 概述

本指南将帮助您手动创建三个技能（Q、X、R键）的冷却UI，包括：
- 始终显示的技能图标
- **冷却时显示深灰色圆形遮盖，从满圆慢慢旋转变小**
- 冷却时显示数字倒计时，就绪时显示按键提示
- 使用您准备的三张圆形背景图

## 🎯 技能信息

- **Q键 - 魔法子弹技能**：冷却时间30秒
- **X键 - 空中攻击技能**：冷却时间20秒
- **R键 - 风力技能**：冷却时间15秒

## ✨ UI显示逻辑

- **就绪状态**：显示背景图标 + 浅绿色按键字母（Q/X/R）
- **冷却状态**：显示背景图标 + **深灰色圆形遮盖（从满圆慢慢旋转变小）** + 白色倒计时数字
- **UI始终可见**：无论技能状态如何，UI都会保持显示

---

## 🚀 第一步：准备工作

### 1.1 确保场景中有Canvas
1. 如果场景中没有Canvas，创建一个：
   - 右键Hierarchy → UI → Canvas
   - 设置Canvas的Render Mode为"Screen Space - Overlay"

### 1.2 准备背景图片
确保您的三张圆形背景图已经导入到项目中：
- `Q键技能.png`
- `X键技能.png` 
- `R键技能.png`

---

## 🎨 第二步：创建Q键技能UI

### 2.1 创建主容器
1. 右键Canvas → UI → Empty，命名为`QSkillUI`
2. 在Inspector中设置RectTransform：
   ```
   - Anchor: Top Right (右上角)
   - Position X: -100
   - Position Y: -100
   - Width: 80
   - Height: 80
   ```

### 2.2 创建技能图标背景
1. 右键QSkillUI → UI → Image，命名为`SkillIcon`
2. 设置Image组件：
   ```
   - Source Image: 拖拽Q键技能.png
   - Image Type: Simple
   - Preserve Aspect: 勾选
   ```
3. 设置RectTransform：
   ```
   - Anchors: Stretch (占满父对象)
   - Left, Right, Top, Bottom: 都设为0
   ```

### 2.3 创建冷却遮盖 ⭐ **重点**
1. 右键QSkillUI → UI → Image，命名为`CooldownFill`
2. 设置Image组件：
   ```
   - Color: 深灰色 (R:50, G:50, B:50, A:200) - 更明显的遮盖效果
   - Image Type: Filled ⭐ 必须设置
   - Fill Method: Radial 360 ⭐ 必须设置
   - Fill Origin: Top ⭐ 从顶部开始旋转
   - Clockwise: 勾选 ⭐ 顺时针方向
   ```
3. 设置RectTransform：
   ```
   - Anchors: Stretch (占满父对象)
   - Left, Right, Top, Bottom: 都设为0
   ```
4. **层级顺序很重要**：
   ```
   QSkillUI
   ├── SkillIcon (底层 - 背景图)
   ├── CooldownFill (中层 - 遮盖层)
   └── CooldownText (顶层 - 文字)
   ```

### 2.4 创建状态文本
1. 右键QSkillUI → UI → Text，命名为`CooldownText`
2. 设置Text组件：
   ```
   - Text: "Q"
   - Font: LegacyRuntime (或其他可用字体)
   - Font Size: 18
   - Color: 浅绿色 (R:200, G:255, B:200, A:255)
   - Alignment: Center Middle
   ```
3. 设置RectTransform：
   ```
   - Anchors: Center Middle
   - Position X: 0, Position Y: 0
   - Width: 40, Height: 25
   ```

---

## 🎨 第三步：创建X键技能UI

### 3.1 复制Q键UI结构
1. 在Hierarchy中右键QSkillUI → Duplicate
2. 重命名为`XSkillUI`
3. 修改位置：Position X: -200（向左移动）

### 3.2 修改背景图片
1. 选择XSkillUI下的SkillIcon
2. 将Source Image改为`X键技能.png`

### 3.3 修改默认文本
1. 选择XSkillUI下的CooldownText
2. 将Text改为"X"

---

## 🎨 第四步：创建R键技能UI

### 4.1 复制UI结构
1. 在Hierarchy中右键XSkillUI → Duplicate
2. 重命名为`RSkillUI`
3. 修改位置：Position X: -300（继续向左移动）

### 4.2 修改背景图片
1. 选择RSkillUI下的SkillIcon
2. 将Source Image改为`R键技能.png`

### 4.3 修改默认文本
1. 选择RSkillUI下的CooldownText
2. 将Text改为"R"

---

## 🔧 第五步：添加SkillCooldownUIManager组件

### 5.1 创建管理器对象
1. 在Hierarchy中创建Empty GameObject，命名为`SkillUIManager`
2. 添加`SkillCooldownUIManager`脚本组件

### 5.2 连接UI元素
在SkillCooldownUIManager组件中，按照以下方式拖拽连接：

#### Q键技能 - 魔法子弹：
- `Q Skill UI`: 拖拽QSkillUI对象
- `Q Skill Cooldown Fill`: 拖拽QSkillUI/CooldownFill ⭐ **重要**
- `Q Skill Cooldown Text`: 拖拽QSkillUI/CooldownText
- `Q Skill Icon`: 拖拽QSkillUI/SkillIcon

#### X键技能 - 空中攻击：
- `X Skill UI`: 拖拽XSkillUI对象
- `X Skill Cooldown Fill`: 拖拽XSkillUI/CooldownFill ⭐ **重要**
- `X Skill Cooldown Text`: 拖拽XSkillUI/CooldownText
- `X Skill Icon`: 拖拽XSkillUI/SkillIcon

#### R键技能 - 风力：
- `R Skill UI`: 拖拽RSkillUI对象
- `R Skill Cooldown Fill`: 拖拽RSkillUI/CooldownFill ⭐ **重要**
- `R Skill Cooldown Text`: 拖拽RSkillUI/CooldownText
- `R Skill Icon`: 拖拽RSkillUI/SkillIcon

#### 技能背景图片：
- `Q Skill Background Sprite`: 拖拽Q键技能.png
- `X Skill Background Sprite`: 拖拽X键技能.png
- `R Skill Background Sprite`: 拖拽R键技能.png

#### 显示设置（可选）：
- `Show Ready Text`: 勾选（显示就绪时的按键提示）
- `Ready Text`: "Q"（Q键就绪时显示的文字）
- `X Ready Text`: "X"（X键就绪时显示的文字）
- `R Ready Text`: "R"（R键就绪时显示的文字）

#### 🎨 冷却遮盖效果设置（新增）：
- `Cooldown Mask Color`: 深灰色 (R:50, G:50, B:50, A:200)
- `Smooth Rotation`: 勾选（平滑旋转效果）

---

## 🎮 第六步：测试冷却UI

### 6.1 运行游戏测试
1. 点击Play按钮运行游戏
2. 观察UI初始状态：应该显示三个技能图标和浅绿色的Q、X、R字母
3. 分别按Q、X、R键使用技能
4. 观察冷却效果：
   - **深灰色圆形遮盖从满圆开始，顺时针慢慢旋转变小**
   - 白色数字倒计时同步显示
   - 冷却结束后遮盖完全消失，恢复为浅绿色按键字母
   - **UI始终保持显示，不会隐藏**

### 6.2 调试检查
如果UI不工作，检查：
1. 玩家对象是否有对应的技能组件
2. SkillCooldownUIManager是否正确连接了所有UI元素
3. CooldownFill的Image Type是否设为Filled
4. Fill Method是否设为Radial 360
5. 控制台是否有错误信息

---

## 🎨 自定义冷却遮盖效果

### 调整遮盖颜色和透明度：
在SkillCooldownUIManager组件中：
```
推荐设置：
- 深灰色不透明：(50, 50, 50, 255) - 最明显
- 深灰色半透明：(50, 50, 50, 200) - 平衡效果  
- 黑色半透明：(0, 0, 0, 150) - 经典效果
```

### 调整旋转效果：
- `Smooth Rotation`: 勾选 = 平滑过渡，视觉更流畅
- `Smooth Rotation`: 取消 = 直接跳转，更精确

### 手动重设遮盖（故障排除）：
1. 选择SkillUIManager对象
2. 在SkillCooldownUIManager组件上右键
3. 选择"重新设置冷却遮盖"

---

## 🌟 UI状态说明

### 就绪状态：
- **背景**：显示技能图标
- **遮盖**：完全消失（fillAmount = 0）
- **文字**：显示浅绿色按键字母（Q/X/R）

### 冷却状态：
- **背景**：显示技能图标
- **遮盖**：深灰色圆形，从满圆（fillAmount = 1）慢慢旋转变小到空（fillAmount = 0）
- **文字**：显示白色倒计时数字

---

## 🐛 常见问题解决

### 问题1：遮盖不显示或不旋转
**解决方案：**
- 确保CooldownFill的Image Type设为"Filled"
- 确保Fill Method设为"Radial 360"
- 确保Fill Origin设为"Top"
- 确保Clockwise已勾选

### 问题2：遮盖颜色太淡看不清
**解决方案：**
- 调整Cooldown Mask Color为更深的颜色
- 增加Alpha值（透明度）到200-255
- 推荐使用：(30, 30, 30, 230)

### 问题3：旋转方向错误
**解决方案：**
- 检查Fill Origin设置（推荐Top）
- 检查Clockwise设置（推荐勾选）
- 如需逆时针，取消Clockwise勾选

### 问题4：遮盖层级错误
**解决方案：**
- 确保层级顺序：SkillIcon → CooldownFill → CooldownText
- CooldownFill必须在SkillIcon之上，CooldownText之下

---

## 📝 最终检查清单

- [ ] 三个技能UI对象已创建（QSkillUI、XSkillUI、RSkillUI）
- [ ] 每个UI包含：SkillIcon、CooldownFill、CooldownText
- [ ] **CooldownFill设置为Filled + Radial 360填充方式**
- [ ] **遮盖层级正确：背景图 → 遮盖层 → 文字**
- [ ] SkillCooldownUIManager组件已添加并正确连接
- [ ] 背景图片已正确分配
- [ ] 文字组件使用Text而非TextMesh-Pro
- [ ] 文字字体设置为LegacyRuntime或其他可用字体
- [ ] **冷却遮盖效果设置已配置**
- [ ] 游戏运行时UI始终显示
- [ ] 就绪状态显示浅绿色按键字母
- [ ] **冷却状态显示深灰色圆形遮盖从满圆旋转变小**
- [ ] 冷却状态显示白色倒计时数字

完成以上步骤后，您就拥有了带有炫酷圆形旋转遮盖效果的技能冷却UI系统！ 