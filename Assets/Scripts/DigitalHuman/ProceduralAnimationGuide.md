# 程序化动画系统 - 完整实现指南

> **不依赖预设 FBX 动画，完全由代码实时生成舞蹈动作**

## 📋 目录

1. [系统概述](#系统概述)
2. [核心组件](#核心组件)
3. [快速开始](#快速开始)
4. [骨骼配置](#骨骼配置)
5. [舞蹈风格详解](#舞蹈风格详解)
6. [AI 生成系统](#ai-生成系统)
7. [音频节奏分析](#音频节奏分析)
8. [高级功能](#高级功能)
9. [故障排除](#故障排除)

---

## 系统概述

### 什么是程序化动画？

程序化动画（Procedural Animation）是一种**不依赖预设动画片段**，而是通过**数学函数、算法、物理模拟**等方式实时生成动画的技术。

**优点：**
- ✅ 无需预设动画文件
- ✅ 动作无限可变
- ✅ 可响应音乐节奏
- ✅ 可学习和进化
- ✅ 完全实时生成

**实现原理：**
```
数学函数（正弦波、噪声）+ 节奏分析 + AI 学习
                ↓
        实时计算骨骼旋转
                ↓
        直接驱动 Transform
```

---

## 核心组件

| 组件 | 文件 | 功能 |
|------|------|------|
| **ProceduralDanceGenerator** | `ProceduralDanceGenerator.cs` | 核心舞蹈生成器，通过数学函数驱动骨骼 |
| **AudioRhythmAnalyzer** | `AudioRhythmAnalyzer.cs` | 音频节奏分析，从音乐中提取 BPM |
| **AIDanceGenerator** | `AIDanceGenerator.cs` | AI 舞蹈生成，使用算法和学习自动生成动作 |
| **ProceduralPerformanceController** | `ProceduralPerformanceController.cs` | 统一控制器，整合所有组件 |

---

## 快速开始

### 步骤 1: 添加控制器

1. 在你的 VRM 模型 GameObject 上添加 `ProceduralPerformanceController` 组件
2. 勾选 `Auto Setup Bones` 自动配置骨骼
3. 点击 Play

### 步骤 2: 测试舞蹈

运行场景后，你会看到调试 UI（左上角）：

点击以下按钮测试：
- **Hip Hop** - 嘻哈风格舞蹈
- **Pop** - 流行风格舞蹈
- **Ballet** - 芭蕾风格舞蹈
- **Robot** - 机器人风格舞蹈
- **Wave** - 波浪风格舞蹈

### 步骤 3: 调整参数

在调试 UI 中调整：
- **BPM 滑块** - 调整舞蹈速度（60-200）
- **强度滑块** - 调整舞蹈强度（0-1）

---

## 骨骼配置

### 自动配置（推荐）

添加 `ProceduralPerformanceController` 时勾选 `Auto Setup Bones`，系统会自动：
- 查找 `Animator` 组件
- 获取主要骨骼（Hips）
- 自动配置所有子骨骼

### 手动配置

如果自动配置失败，可以手动配置：

1. 在 `ProceduralDanceGenerator` 组件中找到骨骼引用
2. 逐个拖入对应的骨骼 Transform：

| 骨骼 | 说明 |
|------|------|
| `Hips` | 臀部（必需）|
| `Spine` | 脊柱 |
| `Chest` | 胸部 |
| `Neck` | 脖子 |
| `Head` | 头部 |
| `LeftShoulder` | 左肩 |
| `LeftUpperArm` | 左上臂 |
| `LeftLowerArm` | 左前臂 |
| `LeftHand` | 左手 |
| `RightShoulder` | 右肩 |
| `RightUpperArm` | 右上臂 |
| `RightLowerArm` | 右前臂 |
| `RightHand` | 右手 |
| `LeftUpperLeg` | 左大腿 |
| `LeftLowerLeg` | 左小腿 |
| `LeftFoot` | 左脚 |
| `RightUpperLeg` | 右大腿 |
| `RightLowerLeg` | 右小腿 |
| `RightFoot` | 右脚 |

**注意：** 如果某些骨骼为空，对应的身体部位将不会动，但不影响其他部位。

---

## 舞蹈风格详解

### 1. HipHop（嘻哈）

**特点：**
- 强烈的上下律动
- 躯干和胸部大幅扭动
- 手臂大幅度摆动
- 腿部有力律动

**适用场景：**
- 嘻哈音乐
- 电子舞曲
- 高能量音乐

**数学函数示例：**
```csharp
hipsPosition.y = Mathf.Sin(beat * π * 2) * 0.05f;
spineRotation = new Vector3(
    Mathf.Sin(beat * π * 2) * 5f,
    Mathf.Sin(beat * π * 4) * 3f,
    0
);
```

### 2. Pop（流行）

**特点：**
- 轻快的节奏
- 躯干挺直
- 胸部轻微扭动
- 手臂优雅摆动
- 腿部轻盈移动

**适用场景：**
- 流行音乐
- 轻音乐
- 欢快歌曲

### 3. Ballet（芭蕾）

**特点：**
- 优雅的身体移动
- 躯干挺直
- 头部优雅抬起
- 手臂优雅伸展
- 腿部优雅移动

**适用场景：**
- 古典音乐
- 抒情歌曲
- 优雅音乐

### 4. Robot（机器人）

**特点：**
- 机械式身体移动
- 躯干僵硬
- 动作分段
- 使用 `Mathf.Floor()` 实现机械感

**适用场景：**
- 电子音乐
- 科技感音乐
- 机械舞风格

**数学函数示例：**
```csharp
hipsPosition.y = Mathf.Floor(Mathf.Sin(beat * π * 2) * 2) * 0.03f;
spineRotation = new Vector3(
    Mathf.Floor(Mathf.Sin(beat * π * 2)) * 5f,
    0,
    0
);
```

### 5. Wave（波浪）

**特点：**
- 身体波浪式移动
- 躯干波浪
- 胸部波浪
- 头部波浪
- 手臂波浪

**适用场景：**
- 波浪音乐风格
- 流畅音乐
- 梦幻音乐

---

## AI 生成系统

### 工作原理

`AIDanceGenerator` 使用以下技术生成舞蹈：

1. **风格混合** - 根据权重自动切换舞蹈风格
2. **随机手势** - 随机触发表演手势
3. **学习模式** - 从音乐中学习节奏和风格
4. **噪声添加** - 在动作上添加随机噪声，增加多样性

### 风格权重系统

AI 会根据音乐能量自动调整风格权重：

```csharp
// 高能量音乐 → 增加 HipHop 权重
if (energy > average * 1.5f)
    styleWeights[HipHop] += learningRate;

// 低能量音乐 → 增加 Ballet 权重
if (energy < average * 0.7f)
    styleWeights[Ballet] += learningRate;
```

### 使用 AI 生成

```csharp
// 获取控制器
var controller = GetComponent<ProceduralPerformanceController>();

// 开始 AI 生成
controller.StartProceduralDance(DanceStyle.HipHop);

// AI 会自动：
// 1. 检测音乐节奏
// 2. 调整舞蹈风格
// 3. 添加随机动作
// 4. 学习音乐特征
```

---

## 音频节奏分析

### 功能

`AudioRhythmAnalyzer` 实时分析音乐：

1. **BPM 检测** - 检测每分钟节拍数
2. **能量分析** - 分析音乐能量强度
3. **频率分离** - 分离低音、中音、高音
4. **频谱分析** - 获取音频频谱数据

### 使用示例

```csharp
// 获取分析器
var analyzer = GetComponent<AudioRhythmAnalyzer>();

// 开始分析
analyzer.StartAnalyzing();

// 获取 BPM
float bpm = analyzer.GetBPM();

// 获取能量
float energy = analyzer.GetCurrentEnergy();

// 获取频率能量
float bass = analyzer.GetBassEnergy();
float mid = analyzer.GetMidEnergy();
float high = analyzer.GetHighEnergy();

// 获取频谱
float[] spectrum = analyzer.GetSpectrum();
```

### 事件系统

```csharp
// 订阅节拍事件
analyzer.OnBeat += (bpm) => {
    Debug.Log($"检测到节拍！BPM: {bpm}");
    // 在节拍时触发特殊效果
};

// 订阅能量变化事件
analyzer.OnEnergyChange += (energy) => {
    // 根据能量调整舞蹈强度
    danceGenerator.SetDanceIntensity(energy);
};
```

---

## 高级功能

### 1. 自定义舞蹈风格

在 `ProceduralDanceGenerator.cs` 中添加新风格：

```csharp
private void GenerateMyCustomMove(float beatProgress)
{
    // 身体上下律动
    _currentMove.hipsPosition.y = Mathf.Sin(beatProgress * Mathf.PI * 2) * 0.04f;

    // 躯干旋转
    _currentMove.spineRotation = new Vector3(
        Mathf.Sin(beatProgress * Mathf.PI * 2) * 5f,
        0,
        0
    );

    // ... 更多骨骼控制
}
```

然后在 `GenerateDanceMove()` 中添加：

```csharp
case DanceStyle.MyCustom:
    GenerateMyCustomMove(beatProgress);
    break;
```

### 2. 噪声和随机性

```csharp
// 启用噪声
danceGenerator.GetComponent<AIDanceGenerator>()._enableNoise = true;

// 调整噪声幅度
danceGenerator.GetComponent<AIDanceGenerator>()._noiseAmplitude = 0.2f;
```

### 3. 音乐跟随

```csharp
// 音乐跟随模式
rhythmAnalyzer.StartAnalyzing();

// 舞蹈会自动跟随音乐：
// - BPM 自动调整
// - 能量自动调整强度
// - 风格自动切换
```

### 4. 混合多种风格

```csharp
// 风格混合模式
aiGenerator._enableStyleMixing = true;

// AI 会根据权重自动混合多种风格
```

### 5. 学习和进化

```csharp
// 启用学习模式
aiGenerator._enableLearning = true;

// AI 会从音乐中学习：
// - 节奏模式
// - 风格偏好
// - 能量分布
```

---

## 故障排除

### Q1: 骨骼不动？

**解决方案：**
1. 检查是否正确设置了骨骼引用
2. 确保勾选了 `Auto Setup Bones`
3. 检查 Hips 骨骼是否正确
4. 查看 Console 是否有错误

### Q2: 动作很僵硬？

**解决方案：**
1. 调整 `Dance Intensity` 滑块
2. 启用噪声（`_enableNoise = true`）
3. 增加 `transitionSpeed` 让过渡更平滑
4. 尝试不同的舞蹈风格

### Q3: 节拍检测不准确？

**解决方案：**
1. 调整 `Sensitivity` 参数
2. 调整 `Min Beat Interval`
3. 确保音乐音量足够
4. 检查 `Sample Size` 和 `FFT Size`

### Q4: 身体扭曲？

**解决方案：**
1. 检查骨骼层级是否正确
2. 减小 `Dance Intensity`
3. 检查初始姿态是否正常
4. 调整特定骨骼的旋转幅度

### Q5: 性能问题？

**解决方案：**
1. 减少 `Sample Size`
2. 降低 `Generation Interval`
3. 禁用不必要的调试 UI
4. 优化骨骼引用数量

---

## 与 ChatManager 集成

```csharp
// 在 ChatManager.cs 中添加
[SerializeField] private ProceduralPerformanceController _performanceController;

void Awake()
{
    // ...
    if (_performanceController == null)
        _performanceController = GetComponent<ProceduralPerformanceController>();
}

// 解析 AI 指令
private void ProcessAiResponseWithAudio(string audioBase64)
{
    // ...
    if (result.text.Contains("[Dance]"))
    {
        _performanceController.StartProceduralDance(DanceStyle.HipHop);
    }
    else if (result.text.Contains("[StopDance]"))
    {
        _performanceController.StopProceduralDance();
    }
    // ...
}
```

---

## 总结

你现在拥有：

✅ **完全程序化的舞蹈系统**
✅ **5 种预定义舞蹈风格**
✅ **AI 动作生成**
✅ **音频节奏分析**
✅ **音乐跟随功能**
✅ **学习和进化能力**

**这是一个真正意义上的"动态捕捉"系统，但完全由代码驱动！**

开始创作吧！🎉
