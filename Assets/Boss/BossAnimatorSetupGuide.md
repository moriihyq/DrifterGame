# Boss动画控制器设置指南

## 概述
本文档将指导您如何在Unity编辑器中为Boss创建和设置动画控制器。

## 步骤 1: 创建动画控制器

1. 在Unity编辑器中，右键点击Assets/Boss文件夹
2. 选择 **Create → Animator Controller**
3. 将新控制器命名为 **BossAnimatorController**

## 步骤 2: 设置动画状态

打开BossAnimatorController（双击它），然后按照以下步骤设置状态机：

1. **创建状态**
   - 在Animator窗口中右键点击，选择 **Create State → Empty**
   - 创建以下六个状态：
     * Idle (默认状态)
     * Walk
     * Attack (近战攻击)
     * Shoot (远程攻击)
     * Hurt (受伤)
     * Death (死亡)

2. **导入动画片段**
   - 确保你已经为Boss创建了相应的动画片段（.anim文件）
   - 将这些动画片段拖放到相应的状态上

## 步骤 3: 创建参数

在Animator窗口的Parameters选项卡中，添加以下参数：

| 参数名称 | 类型    | 默认值 |
|---------|---------|-------|
| isMoving | Bool   | false |
| isActive | Bool   | false |
| Attack  | Trigger | -     |
| Shoot   | Trigger | -     |
| Hurt    | Trigger | -     |
| IsDead  | Bool    | false |

## 步骤 4: 设置转换条件

创建以下状态转换并设置条件：

### 从Idle状态转换
1. **Idle → Walk**
   - 条件: `isMoving` = true

2. **Idle → Attack**
   - 条件: `Attack`触发器被激活

3. **Idle → Shoot**
   - 条件: `Shoot`触发器被激活

4. **Idle → Hurt**
   - 条件: `Hurt`触发器被激活

5. **Idle → Death**
   - 条件: `IsDead` = true

### 从Walk状态转换
1. **Walk → Idle**
   - 条件: `isMoving` = false

2. **Walk → Attack**
   - 条件: `Attack`触发器被激活

3. **Walk → Shoot**
   - 条件: `Shoot`触发器被激活

4. **Walk → Hurt**
   - 条件: `Hurt`触发器被激活

5. **Walk → Death**
   - 条件: `IsDead` = true

### 从Attack状态转换
1. **Attack → Idle**
   - 勾选"Has Exit Time"
   - 设置Exit Time为0.8（或根据动画长度调整）

2. **Attack → Death**
   - 条件: `IsDead` = true

### 从Shoot状态转换
1. **Shoot → Idle**
   - 勾选"Has Exit Time"
   - 设置Exit Time为0.9（或根据动画长度调整）

2. **Shoot → Death**
   - 条件: `IsDead` = true

### 从Hurt状态转换
1. **Hurt → Idle**
   - 勾选"Has Exit Time"
   - 设置Exit Time为0.75（或根据动画长度调整）

2. **Hurt → Death**
   - 条件: `IsDead` = true

## 步骤 5: 连接到Boss对象

1. 将BossAnimatorController拖放到Boss游戏对象上
   - 或者在Inspector中为Boss的Animator组件选择BossAnimatorController

2. 确保Boss对象上有Animator组件
   - 如果没有，添加Animator组件

## 备注

1. 所有带有触发器条件的转换应设置为"Interruption Source: Current State Then Next State"
2. 可以通过右击转换线并选择"Settings"来调整转换设置
3. Death状态不应该有任何出口转换
4. 确保所有状态都正确设置了动画片段，并且"Motion"字段不为空
