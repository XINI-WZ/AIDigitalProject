# 数字人表演系统实现指南

> 让数字人像真人一样跳舞、唱歌、表达情感

## 📋 目录
1. [系统概述](#系统概述)
2. [快速开始](#快速开始)
3. [获取动画资源](#获取动画资源)
4. [Animator Controller 配置](#animator-controller-配置)
5. [系统使用](#系统使用)
6. [高级功能](#高级功能)
7. [常见问题](#常见问题)

---

## 系统概述

### 已创建的组件

| 组件 | 功能 | 文件位置 |
|------|------|---------|
| **DancingManager** | 舞蹈播放、停止、切换管理 | `Scripts/DigitalHuman/Animation/DancingManager.cs` |
| **SingingCoordinator** | 唱歌时协调口型、表情、手势 | `Scripts/DigitalHuman/Animation/SingingCoordinator.cs` |
| **PerformanceController** | 统一管理所有表演行为 | `Scripts/DigitalHuman/Animation/PerformanceController.cs` |

### 系统架构

```
PerformanceController (统一控制器)
    ├── DancingManager (舞蹈管理)
    │   └── Animator (播放动画)
    ├── SingingCoordinator (唱歌协调)
    │   ├── AvatarLipSyncController (口型同步)
    │   └── AudioSource (歌曲播放)
    ├── AvatarAnimationController (表情控制)
    └── VoiceDrivenAnimationMixer (动画混合)
```

---

## 快速开始

### 步骤 1: 添加 PerformanceController

1. 在你的 VRM 模型 GameObject 上添加 `PerformanceController` 组件
2. 系统会自动查找并连接所有子组件

### 步骤 2: 配置 DancingManager

在 `PerformanceController` 检查 `DancingManager` 引用，配置：

```csharp
// DancingManager 组件需要配置：
_danceClips: 舞蹈片段列表
```

### 步骤 3: 配置 SingingCoordinator

在 `PerformanceController` 检查 `SingingCoordinator` 引用，配置：

```csharp
// SingingCoordinator 组件需要配置：
_emotionConfigs: 唱歌情感配置
```

### 步骤 4: 测试

运行场景，你会看到调试 UI（左上角），可以点击按钮测试功能：
- 开始跳舞 / 下一个舞蹈 / 停止跳舞
- 开始唱歌 / 停止唱歌
- 触发各种手势和表情

---

## 获取动画资源

### 方案 1: Mixamo（推荐）

**Mixamo** 是 Adobe 提供的免费 3D 动画库。

#### 访问 Mixamo
1. 网址：https://www.mixamo.com
2. 使用 Adobe 账号登录（免费）

#### 下载舞蹈动画
1. 在搜索框输入：**"Dance"** 或 **"Singing"**
2. 推荐动画：
   - `Hip Hop Dance` - 嘻哈舞蹈
   - `Dancing To Beat` - 节奏舞蹈
   - `Standing Hip Hop` - 站立嘻哈
   - `Singing` - 唱歌动画
   - `Singing Soft` - 温柔唱歌
   - `Idle Standing` - 待机动画

#### 下载设置
- **Character Type**: 选择与你模型类似的类型（Humanoid）
- **Format**: FBX for Unity
- **Skin**: All Skins
- **Animations**: In Place（原地）

#### 导入到 Unity
1. 下载的 `.fbx` 文件放入 `Animations/` 文件夹
2. Unity 会自动导入
3. 检查 Import Settings：
   - Rig: Humanoid
   - Animation Type: Humanoid

### 方案 2: Unity Asset Store

搜索关键词：
- "Dance Animation Pack"
- "Singing Animations"
- "Female/Male Animation Pack"

### 方案 3: 自定义动画

如果你有 3D 制作能力，可以使用：
- Blender
- Maya
- Motion Capture (动作捕捉)

---

## Animator Controller 配置

### 创建 Animator Controller

1. 在 `Animations/` 文件夹右键 → Create → Animator Controller
2. 命名为 `PerformanceController`

### 参数设置

在 Parameters 窗口添加：

| 参数名 | 类型 | 说明 |
|--------|------|------|
| `IsDancing` | Bool | 是否跳舞 |
| `IsSpeaking` | Bool | 是否在说话/唱歌 |
| `DanceType` | Int | 舞蹈类型（0=N, 1=Hiphop, 2=Slow）|

### 状态机设计

#### 基础状态

```
Idle (待机)
    ↓ IsDancing=true
Dancing (跳舞)
    ↓ IsDancing=false
Idle

Talking (说话/唱歌)
    ↓ IsSpeaking=false
Idle
```

#### 动画状态配置

**Idle 状态**:
- Motion: `X Bot@Idle` (你现有的动画)
- Loop Time: ✓

**Dancing 状态**:
- Motion: 选择一个舞蹈动画（如 `Hip Hop Dance`）
- Loop Time: ✓
- Transitions: Idle → Dancing (条件: IsDancing=true)
              Dancing → Idle (条件: IsDancing=false)

**Talking 状态**:
- Motion: `X Bot@Talking`
- Loop Time: ✓
- Transitions: Idle → Talking (条件: IsSpeaking=true)
              Talking → Idle (条件: IsSpeaking=false)

### 混合树（可选）

如果你有多个舞蹈动画，可以使用 Blend Tree：

1. 右键 → Create New Blend Tree
2. 命名为 `DanceBlendTree`
3. 双击打开，添加多个舞蹈动画
4. 使用 `DanceType` 参数控制混合

---

## 系统使用

### 在代码中控制

```csharp
using DigitalHuman.Animation;

// 获取 PerformanceController
var performance = GetComponent<PerformanceController>();

// 跳舞
performance.StartDancing("Happy");  // 开始跳舞
performance.NextDance();            // 切换下一个舞蹈
performance.StopDancing();           // 停止跳舞

// 唱歌
AudioClip songClip = Resources.Load<AudioClip>("Music/MySong");
performance.StartSinging(songClip, "Happy");
performance.StopSinging();

// 表情和手势
performance.SetEmotion("Happy");
performance.TriggerGesture("Wave");
performance.TriggerGesture("Nod");
```

### 与 ChatManager 集成

修改 `ChatManager.cs`，添加表演动作解析：

```csharp
private void ProcessAiResponseWithAudio(string audioBase64)
{
    // ... 现有代码 ...

    // 解析表演动作
    if (result.text.Contains("[Dance]"))
    {
        _performanceController.StartDancing("Happy");
    }
    else if (result.text.Contains("[StopDance]"))
    {
        _performanceController.StopDancing();
    }

    // ... 其余代码 ...
}
```

### AI 指令格式

在 AI 的 System Prompt 中添加：

```
你可以在回复中使用以下表演指令：
- [Dance]: 开始跳舞
- [StopDance]: 停止跳舞
- [Sing]: 开始唱歌（如果有音频）
- [Wave]: 挥手
- [Happy]: 开心表情
- [Sad]: 难过表情
- [Angry]: 生气表情

示例：
[Happy] [Dance] 我很高兴见到你！[Wave]
```

---

## 高级功能

### 1. 音乐节奏检测

```csharp
public class RhythmDetector : MonoBehaviour
{
    public float DetectRhythm(AudioClip clip)
    {
        // 使用音频分析检测节拍
        // 返回 BPM (每分钟节拍数)
        // TODO: 实现音频分析
        return 120f;
    }
}
```

### 2. 基于歌词的情感切换

```csharp
public class LyricEmotionAnalyzer : MonoBehaviour
{
    public string AnalyzeLyricEmotion(string lyric)
    {
        // 分析歌词情感
        if (lyric.Contains("爱") || lyric.Contains("心"))
            return "Happy";
        if (lyric.Contains("泪") || lyric.Contains("痛"))
            return "Sad";
        return "Neutral";
    }
}
```

### 3. 舞蹈同步系统

```csharp
// 根据音乐 BPM 调整舞蹈速度
animator.SetFloat("DanceSpeed", 120f / musicBPM);
```

### 4. 多人舞蹈

创建 `GroupDancingManager`，协调多个数字人同步跳舞。

---

## 常见问题

### Q1: 导入的动画不播放？

**解决方案**:
1. 检查 Animator 是否有 Animator Controller
2. 检查动画的 Rig 类型是否为 Humanoid
3. 检查动画的 Avatar 是否与模型匹配
4. 在 Animator 窗口手动触发动画状态

### Q2: 舞蹈时身体扭曲？

**解决方案**:
1. 检查模型的骨骼是否正确绑定
2. 使用 Mixamo 时选择正确的 Character Type
3. 调整动画的 Import Settings → Avatar Definition

### Q3: 口型与动画不同步？

**解决方案**:
1. 确保 `AvatarLipSyncController` 的 `ConfigureForAudioSource` 被调用
2. 检查 `uLipSync` 组件的配置
3. 调整口型延迟参数

### Q4: 表情切换不自然？

**解决方案**:
1. 调整 `AvatarAnimationController` 中的 `EXPRESSION_BLEND_SPEED`
2. 确保表情权重正确设置
3. 使用 VRM 的 Expression Preview 查看效果

### Q5: 如何添加自己的舞蹈动画？

**步骤**:
1. 下载/制作舞蹈动画 FBX
2. 导入到 `Animations/` 文件夹
3. 在 Animator Controller 中添加状态
4. 在 `DancingManager` 的 `_danceClips` 中添加配置：
   ```csharp
   new DanceClip {
       name = "My Dance",
       animationName = "MyDance",  // Animator 中的状态名
       emotion = "Happy",
       rhythm = DanceRhythm.Medium,
       speedMultiplier = 1.0f
   }
   ```

---

## 下一步

1. ✅ 获取舞蹈动画（Mixamo 或 Asset Store）
2. ✅ 配置 Animator Controller
3. ✅ 添加 `PerformanceController` 到场景
4. ✅ 配置 `_danceClips` 和 `_emotionConfigs`
5. ✅ 测试跳舞和唱歌功能
6. ✅ 集成到 ChatManager

---

## 技术支持

如有问题，请检查：
- Unity Console 的错误日志
- Animator 窗口的状态切换
- 组件的引用是否正确设置

**祝你的数字人舞起来！** 🎉
