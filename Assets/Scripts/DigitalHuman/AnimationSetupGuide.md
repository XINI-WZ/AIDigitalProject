# VRM 数字人动画设置指南

## 1. 创建 Animator Controller

在 Unity 中：
1. 在 Project 窗口右键 → Create → Animator Controller
2. 命名为 `AvatarAnimatorController`
3. 双击打开 Animator 窗口

## 2. 创建动画参数

在 Animator 窗口的 Parameters 面板添加：

```
Bool:
- IsSpeaking (是否正在说话)

Float:
- IdleSwayX (待机摇摆 X 轴)
- IdleSwayY (待机摇摆 Y 轴)
- IdleTime (待机时间，用于循环动画)

Trigger:
- StartTalking (开始说话)
- Wave (挥手)
```

## 3. 创建动画状态

### 3.1 Idle (待机状态)
- 创建新的 State，命名为 "Idle"
- 不需要 Motion（由代码控制身体摇摆）
- 添加一个 Blend Tree 来控制身体摇摆

### 3.2 Talking (说话状态)
- 创建新的 State，命名为 "Talking"
- 可以添加一个简单的 Loop 动画（如手部轻微动作）
- 或者保持空状态，由代码控制

### 3.3 过渡设置
- Idle → Talking: 条件为 `IsSpeaking = true`
- Talking → Idle: 条件为 `IsSpeaking = false`

## 4. 为 VRM 模型添加组件

1. 选中场景中的 VRM 模型
2. 添加组件：
   - `AvatarAnimationController` (新创建的)
   - `Animator` (如果还没有)

3. 配置引用：
   - Vrm Instance: 拖入 Vrm10Instance 组件
   - Animator: 拖入 Animator 组件，并设置 AvatarAnimatorController

## 5. 可选：添加手势动画

如果你想添加更复杂的手势，可以：

1. 创建 Animation Clip：
   - 在 Project 窗口右键 → Create → Animation
   - 命名为 `WaveAnimation`
   - 录制挥手动作（移动手臂骨骼）

2. 在 Animator Controller 中添加：
   - 新的 State：Wave
   - Motion：拖入 WaveAnimation
   - Transition：从 Idle 到 Wave，条件为 Wave Trigger

## 6. 快速测试

运行场景后：
1. 说话观察是否有身体摇摆（Idle 动画）
2. AI 回复时观察是否有说话动画
3. 不同情绪时观察是否有相应的手势

## 7. 进阶：使用 Mixamo 动画

如果你想让角色更生动：

1. 访问 https://www.mixamo.com/
2. 下载免费的待机动画（如 Idle、Talking）
3. 导入 Unity，设置为 Humanoid 类型
4. 在 Animator Controller 中使用这些动画

## 8. 表情调试

AvatarAnimationController 已包含以下自动功能：
- ✅ 自动眨眼
- ✅ 表情平滑过渡
- ✅ 根据情绪组合多个表情（如 Happy + Fun）

你无需额外配置，直接运行即可看到效果！
