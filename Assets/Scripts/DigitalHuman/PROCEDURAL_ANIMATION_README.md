# 🎭 程序化动画系统 - 实时生成舞蹈

> **完全由代码驱动，不依赖预设动画文件**

## ✨ 特性

- ✅ **5 种舞蹈风格** - HipHop、Pop、Ballet、Robot、Wave
- ✅ **实时生成** - 使用数学函数实时计算骨骼动作
- ✅ **音乐跟随** - 自动检测 BPM，跟随音乐节奏
- ✅ **AI 生成** - 使用算法和学习自动生成新动作
- ✅ **噪声系统** - 添加随机性，动作永不重复
- ✅ **风格混合** - AI 自动切换和混合多种风格
- ✅ **学习模式** - 从音乐中学习节奏和风格偏好

## 🚀 快速开始（3 步）

### 1. 添加组件

在你的 VRM 模型上添加 `ProceduralPerformanceController` 组件：

```
VRM Model GameObject
└── ProceduralPerformanceController (新组件)
```

勾选 `Auto Setup Bones` 自动配置骨骼。

### 2. 运行测试

点击 Unity 的 Play 按钮。

你会看到调试 UI（左上角），点击按钮测试：

- **Hip Hop** - 嘻哈风格
- **Pop** - 流行风格
- **Ballet** - 芭蕾风格
- **Robot** - 机器人风格
- **Wave** - 波浪风格

### 3. 调整参数

在调试 UI 中调整：
- **BPM 滑块** - 调整舞蹈速度
- **强度滑块** - 调整舞蹈强度

就这么简单！数字人开始跳舞了！

---

## 📚 核心组件

| 组件 | 功能 | 文件 |
|------|------|------|
| **ProceduralDanceGenerator** | 核心舞蹈生成器 | `ProceduralDanceGenerator.cs` |
| **AudioRhythmAnalyzer** | 音频节奏分析 | `AudioRhythmAnalyzer.cs` |
| **AIDanceGenerator** | AI 动作生成 | `AIDanceGenerator.cs` |
| **ProceduralPerformanceController** | 统一控制器 | `ProceduralPerformanceController.cs` |

---

## 💡 使用示例

### 基础使用

```csharp
// 获取控制器
var controller = GetComponent<ProceduralPerformanceController>();

// 开始跳舞（HipHop 风格）
controller.StartProceduralDance(DanceStyle.HipHop);

// 停止跳舞
controller.StopProceduralDance();

// 切换风格
controller.ChangeDanceStyle(DanceStyle.Pop);

// 设置表情
controller.SetEmotion("Happy");

// 触发手势
controller.TriggerGesture("Wave");
```

### 音乐跟随

```csharp
// 播放音乐，舞蹈会自动跟随节奏
AudioClip song = Resources.Load<AudioClip>("Music/MySong");
controller.StartProceduralSinging(song, DanceStyle.HipHop);
```

### AI 生成

```csharp
// 启用 AI 自动生成
var aiGenerator = GetComponent<AIDanceGenerator>();
aiGenerator.StartAIGeneration(DanceStyle.HipHop);

// AI 会自动：
// - 检测音乐节奏
// - 切换舞蹈风格
// - 添加随机动作
// - 学习音乐特征
```

---

## 🎨 舞蹈风格

### HipHop（嘻哈）
- 强烈的上下律动
- 躯干和胸部大幅扭动
- 手臂大幅度摆动
- 腿部有力律动

### Pop（流行）
- 轻快的节奏
- 躯干挺直
- 胸部轻微扭动
- 手臂优雅摆动

### Ballet（芭蕾）
- 优雅的身体移动
- 躯干挺直
- 头部优雅抬起
- 手臂优雅伸展

### Robot（机器人）
- 机械式身体移动
- 躯干僵硬
- 动作分段
- 使用 `Mathf.Floor()` 实现机械感

### Wave（波浪）
- 身体波浪式移动
- 躯干波浪
- 胸部波浪
- 头部波浪

---

## 🎵 音频节奏分析

系统会实时分析音乐：

- **BPM 检测** - 检测每分钟节拍数
- **能量分析** - 分析音乐能量强度
- **频率分离** - 分离低音、中音、高音
- **频谱分析** - 获取音频频谱数据

舞蹈会自动跟随：
- 节拍速度
- 能量强度
- 风格偏好

---

## 🤖 AI 生成系统

### 功能

- **风格混合** - 根据权重自动切换舞蹈风格
- **随机手势** - 随机触发表演手势
- **学习模式** - 从音乐中学习节奏和风格
- **噪声添加** - 在动作上添加随机噪声

### 学习模式

AI 会根据音乐能量自动调整风格权重：

```csharp
// 高能量音乐 → 增加 HipHop 权重
// 低能量音乐 → 增加 Ballet 权重
```

---

## 📖 详细文档

查看完整指南：**ProceduralAnimationGuide.md**

内容包括：
- 骨骼配置详解
- 舞蹈风格数学函数
- AI 生成原理
- 音频分析算法
- 自定义风格教程
- 故障排除

---

## 🎯 高级功能

### 自定义舞蹈风格

在 `ProceduralDanceGenerator.cs` 中添加你的风格：

```csharp
private void GenerateMyCustomMove(float beatProgress)
{
    // 使用数学函数生成动作
    _currentMove.hipsPosition.y = Mathf.Sin(beatProgress * Mathf.PI * 2) * 0.04f;
    _currentMove.spineRotation = new Vector3(
        Mathf.Sin(beatProgress * Mathf.PI * 2) * 5f,
        0,
        0
    );
    // ... 更多骨骼控制}
```

### 噪声和随机性

```csharp
// 启用噪声，增加动作多样性
aiGenerator._enableNoise = true;
aiGenerator._noiseAmplitude = 0.2f;
```

### 风格混合

```csharp
// 启用风格混合
aiGenerator._enableStyleMixing = true;

// AI 会根据音乐自动混合多种风格
```

---

## 🤔 程序化动画 vs 预设动画

| 特性 | 程序化动画 | 预设动画 |
|------|-----------|---------|
| **动作多样性** | 无限 | 有限 |
| **文件大小** | 极小（代码）| 较大（FBX） |
| **音乐跟随** | 自动 | 手动 |
| **学习进化** | 支持 | 不支持 |
| **制作难度** | 需编程 | 需要3D制作 |
| **真实感** | 取决于算法 | 取决于质量 |

---

## 🔧 配置说明

### 必需骨骼

至少需要设置：
- `Hips` (臀部）

可选骨骼：
- `Spine`, `Chest`, `Neck`, `Head` - 身体和头部
- `LeftShoulder`, `LeftUpperArm`, `LeftLowerArm`, `LeftHand` - 左臂
- `RightShoulder`, `RightUpperArm`, `RightLowerArm`, `RightHand` - 右臂
- `LeftUpperLeg`, `LeftLowerLeg`, `LeftFoot` - 左腿
- `RightUpperLeg`, `RightLowerLeg`, `RightFoot` - 右腿

**注意：** 如果某些骨骼为空，对应部位不会动，但不影响其他部分。

---

## 🎉 总结

你现在拥有一个：

✅ **完全程序化的舞蹈系统**
✅ **不依赖预设动画文件**
✅ **实时生成动作**
✅ **自动跟随音乐节奏**
✅ **AI 学习和进化**

**这是一个真正意义上的"动态捕捉"系统，但完全由代码驱动！**

立即开始创作你的独特舞蹈吧！🎭
