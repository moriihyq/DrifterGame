# Boss测试指南

## 快速测试设置

以下是快速测试Boss功能的步骤：

### 1. 创建测试场景

1. 在Unity中创建一个新场景 (File → New Scene)
2. 保存场景为 "BossTest" (File → Save As...)

### 2. 设置基本环境

1. 创建一个平面作为地面:
   - 右键点击Hierarchy窗口 → 3D Object → Plane
   - 调整Transform: Position = (0, 0, 0)，Rotation = (0, 0, 0)，Scale = (1, 1, 1)
   - 如果是2D游戏，则创建一个Sprite作为地面

2. 添加Boss对象:
   - 创建一个空游戏对象: 右键点击Hierarchy → Create Empty
   - 重命名为 "Boss"
   - 添加组件:
     * Sprite Renderer (如果是2D游戏)
     * Box Collider 2D
     * Rigidbody 2D
     * Animator
     * BossController (您的自定义脚本)

3. 设置玩家对象:
   - 创建一个空游戏对象: 右键点击Hierarchy → Create Empty
   - 重命名为 "Player"
   - 给它添加一个Tag: "Player"
   - 添加组件:
     * Sprite Renderer (如果是2D游戏)
     * Box Collider 2D
     * Rigidbody 2D
     * PlayerAttackSystem脚本 (或任何处理玩家生命值的脚本)

### 3. 配置Boss

1. 选中Boss对象，在Inspector中的BossController组件下设置以下内容:
   - Max Health: 500
   - Melee Attack Damage: 40
   - Ranged Attack Damage: 60
   - Player Layer: 选择包含玩家的层

2. 确保Boss有一个子对象作为近战攻击点:
   - 右键点击Boss → Create Empty
   - 重命名为 "MeleeAttackPoint"
   - 将其放置在Boss前方
   - 在BossController组件中设置Melee Attack Point为这个对象

3. 设置动画:
   - 按照 BossAnimatorSetupGuide.md 中的说明设置动画控制器
   - 将动画控制器分配给Boss的Animator组件

### 4. 运行测试

1. 点击Play开始测试
2. 按P键激活Boss
3. 观察Boss的行为:
   - 应该先向玩家移动
   - 接近后执行一次近战攻击
   - 然后根据随机模式交替执行近战和远程攻击

### 调试提示

如果遇到问题:

1. 检查控制台输出，查看彩色的调试消息
2. 确保所有动画参数名称与代码中完全匹配
3. 验证玩家对象是否正确设置了Tag
4. 在Scene视图中查看攻击范围的Gizmos是否显示正确
5. 确保所有必要的组件都已添加到游戏对象上
