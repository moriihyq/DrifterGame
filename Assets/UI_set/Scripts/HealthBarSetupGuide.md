# 血量条UI设置指南

## 概述
本指南将帮助您在5.26地图中设置角色血量条功能。系统支持两种血量条显示方式：
1. **跟随血量条** - 跟随在角色头顶的血量条
2. **固定血量条** - 固定在屏幕某个位置的血量条

## 设置步骤

### 第一步：创建固定血量条UI（屏幕左上角）

1. **创建Canvas**（如果场景中还没有Canvas）
   - 在Hierarchy中右键 → UI → Canvas
   - 设置Canvas Scaler：
     - UI Scale Mode: Scale With Screen Size
     - Reference Resolution: 1920x1080

2. **创建血量条容器**
   - 在Canvas下创建空GameObject，命名为"HealthBarFixed"
   - 添加RectTransform，设置：
     - Anchor Preset: 左上角（Top Left）
     - Pos X: 150, Pos Y: -50
     - Width: 300, Height: 40

3. **创建血量条组件**
   - 在HealthBarFixed下创建以下子对象：
   
   a) **背景图片（Background）**
      - 创建UI → Image
      - 命名为"HealthBarBackground"
      - Source Image: HealthBar1.png（或其他背景图）
      - Image Type: Sliced
      - 设置RectTransform充满父对象

   b) **填充图片（Fill）**
      - 创建UI → Image
      - 命名为"HealthBarFill"
      - Source Image: HealthBar2.png（或其他填充图）
      - Image Type: Filled
      - Fill Method: Horizontal
      - Fill Origin: Left
      - 设置RectTransform充满父对象

   c) **边框图片（Frame）**（可选）
      - 创建UI → Image
      - 命名为"HealthBarFrame"
      - Source Image: HealthBar3.png（或其他边框图）
      - 设置RectTransform充满父对象

   d) **血量文本**（可选）
      - 创建UI → Text - TextMeshPro
      - 命名为"HealthText"
      - 设置文本属性：
        - Font Size: 18
        - Alignment: Center
        - Color: White

4. **添加脚本组件**
   - 选择HealthBarFixed对象
   - 添加FixedHealthBarUI脚本
   - 拖拽分配各个组件引用

### 第二步：创建跟随血量条（可选）

1. **创建World Space Canvas**
   - 创建新的Canvas，命名为"WorldCanvas"
   - 设置Render Mode: World Space
   - 设置合适的缩放（如0.01, 0.01, 0.01）

2. **创建血量条对象**
   - 在WorldCanvas下创建空GameObject，命名为"HealthBarFollow"
   - 设置大小：Width: 100, Height: 20

3. **重复创建血量条组件**
   - 按照固定血量条的方式创建Background、Fill、Frame等组件

4. **添加脚本组件**
   - 选择HealthBarFollow对象
   - 添加HealthBarUI脚本
   - 设置Offset为(0, 2, 0)以显示在角色头顶

### 第三步：设置健康管理器

1. **创建HealthManager**
   - 在场景中创建空GameObject，命名为"HealthManager"
   - 添加HealthManager脚本

2. **配置引用**
   - 将玩家对象拖拽到Player字段
   - 将创建的血量条UI拖拽到对应字段

### 第四步：配置血量条样式

#### 使用不同的血量条图片
您有四种血量条样式可选：
- HealthBar1-4.png：不同风格的血量条
- EnergyBar1-8.png：可作为特殊效果血量条

#### 配置颜色渐变
在Inspector中设置Health Gradient：
- 0%：红色 (低血量)
- 50%：黄色 (中等血量)
- 100%：绿色 (满血)

#### 配置动画效果
- Smooth Speed：血量变化的平滑速度
- Damage Effect Duration：受伤特效持续时间
- Damage Effect Scale：受伤时的缩放效果

## 代码集成

### 与PlayerController集成
如果使用PlayerController：
```csharp
// 受到伤害时
public void TakeDamage(int damage)
{
    if (HealthManager.Instance != null)
    {
        HealthManager.Instance.TakeDamage(damage);
    }
    else
    {
        // 原有的伤害处理逻辑
        currentHealth -= damage;
    }
}
```

### 与PlayerAttackSystem集成
如果使用PlayerAttackSystem：
```csharp
// 在TakeDamage方法中
public void TakeDamage(int damage)
{
    if (HealthManager.Instance != null)
    {
        HealthManager.Instance.TakeDamage(damage);
    }
    else
    {
        // 原有逻辑
    }
}
```

## 测试血量条

1. **测试伤害**
   - 在Inspector中调用HealthManager的TakeDamage方法
   - 观察血量条是否正确减少

2. **测试恢复**
   - 调用Heal方法
   - 观察血量条是否正确增加

3. **测试特效**
   - 确认受伤时有缩放动画
   - 确认颜色渐变正常工作

## 常见问题

1. **血量条不显示**
   - 检查Canvas的Layer设置
   - 确认Image组件的Alpha值不为0
   - 检查血量条的Active状态

2. **血量条位置不对**
   - 调整Offset值
   - 检查Canvas的Render Mode设置

3. **血量不更新**
   - 确认PlayerController/PlayerAttackSystem组件存在
   - 检查脚本引用是否正确连接

## 高级功能

1. **多个血量条**
   - 可以同时使用固定和跟随血量条
   - 通过HealthManager统一管理

2. **自定义效果**
   - 修改Gradient添加更多颜色
   - 调整AnimationCurve改变动画效果

3. **事件监听**
   ```csharp
   HealthManager.OnHealthChanged += (current, max) => {
       // 自定义逻辑
   };
   ``` 