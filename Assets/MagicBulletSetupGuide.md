# 魔法子弹技能系统设置指南

## 概述
魔法子弹技能是一个强大的攻击技能，当玩家按下Q键时激活。该技能会从玩家位置发射魔法子弹，子弹会自动追踪最近的活跃敌人。

## 特性
- 按Q键激活技能
- 发射多个初始方向随机的魔法子弹
- 子弹会自动追踪最近的活跃敌人
- 子弹可以穿过墙壁和地形
- 子弹碰撞敌人时造成伤害并消失
- 技能有30秒冷却时间
- 子弹在5秒后自动销毁

## 设置步骤

### 1. 添加MagicBulletSkill脚本
1. 选择玩家角色GameObject
2. 点击Inspector面板中的"Add Component"按钮
3. 搜索并选择"MagicBulletSkill"脚本

### 2. 配置魔法子弹预制体
1. 将"magicBullet.prefab"从项目窗口拖到Inspector面板中的"Magic Bullet Prefab"槽中
2. 如果需要，自定义外观：
   - 更改Sprite为您的自定义图片
   - 调整大小以匹配您的图片
   
### 3. 添加动画效果（可选）
1. 选择"magicBullet"预制体
2. 添加Animator组件（如果还没有）
3. 创建一个新的Animator Controller并分配给预制体
4. 在Animator窗口中设置以下状态：
   - **Idle/Loop**: 子弹飞行时的循环动画
   - **Hit**: 子弹击中目标时的动画
5. 设置触发器参数：
   - **Play**: 触发开始播放动画
   - **Loop**: 布尔值，控制是否循环播放
   - **Hit**: 触发击中动画

### 4. 调整技能参数
可以根据需要调整以下参数：
- **Bullet Count**: 每次施放的子弹数量（默认为3）
- **Cooldown Time**: 技能冷却时间（默认为30秒）
- **Bullet Lifetime**: 子弹存在时间（默认为5秒）
- **Bullet Damage**: 子弹造成的伤害（默认为35）
- **Initial Speed**: 子弹初始速度（默认为8）

### 5. 设置技能UI（可选）
1. 创建一个名为"MagicBulletCooldownUI"的UI面板
2. 添加一个名为"CooldownText"的Text组件用于显示冷却时间
3. 添加一个名为"CooldownFill"的Image组件用于显示冷却进度条
4. 将这些UI元素拖到Inspector面板中相应的槽中

### 6. 添加音效（可选）
1. 导入一个魔法音效文件到项目中
2. 将音效文件拖到Inspector面板中的"Magic Bullet Sound"槽中

## 故障排除

### 子弹不追踪敌人
- 确保敌人GameObject有"Enemy"标签
- 检查敌人是否有Enemy脚本组件
- 验证敌人是否处于活跃状态（不处于死亡状态且GameObject是激活的）

### 技能没有冷却提示
- 确保已设置UI元素或创建了名为"MagicBulletCooldownUI"的GameObject
- 检查UI层级是否正确设置

### 子弹没有造成伤害
- 确认Enemy类中有TakeDamage方法
- 检查子弹的碰撞器是否正确设置为触发器
- 验证敌人的碰撞器是否激活

## 高级自定义
您可以修改MagicBullet.cs脚本来自定义子弹的行为，例如：
- 改变追踪算法
- 添加粒子效果
- 实现弹射或穿透机制
- 添加击中特效
