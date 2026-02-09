# 🚀 快速开始 - 让数字人跳舞和唱歌

> 这是最简化的配置步骤，5 分钟内让数字人动起来！

---

## 📦 第一步：获取动画资源

### 推荐方法：使用 Mixamo（免费）

1. **访问网站**
   - 网址：https://www.mixamo.com
   - 使用 Adobe 账号登录（免费）

2. **下载 3 个关键动画**
   
   搜索并下载以下动画（选择 **FBX for Unity** 格式）：

   | 动画名称 | 用途 | 搜索关键词 |
   |---------|------|-----------|
   | `Idle` | 待机状态 | "Idle Standing" |
   | `Talking` | 说话/唱歌 | "Talking" 或 "Singing" |
   | `Dance` | 跳舞 | "Dance" 或 "Hip Hop" |

3. **导入到 Unity**
   - 将下载的 `.fbx` 文件放入 `Assets/Animations/` 文件夹
   - Unity 会自动导入

4. **检查导入设置**
   - 选中导入的 FBX 文件
   - 在 Inspector 中检查：
     - **Rig**: Humanoid ✓
     - **Animation Type**: Humanoid

---

## 🎛️ 第二步：配置 Animator Controller

### 创建 Animator Controller

1. 在 `Assets/Animations/` 文件夹右键
2. 选择 `Create → Animator Controller`
3. 命名为 `PerformanceController`

### 配置参数

打开 Animator Controller 窗口，点击 `Parameters` 标签，添加：

| 参数名 | 类型 | 说明 |
|--------|------|------|
| `IsDancing` | Bool | 是否在跳舞 |
| `IsSpeaking` | Bool | 是否在说话/唱歌 |

### 配置状态机

1. **添加 Idle 状态**
   - 右键 → `Create State → Empty`
   - 命名为 `Idle`
   - 设置 Motion: 选择你的 `Idle.fbx` 动画
   - 勾选 `Loop Time`

2. **添加 Dancing 状态**
   - 右键 → `Create State → Empty`
   - 命名为 `Dancing`
   - 设置 Motion: 选择你的舞蹈 FBX 动画
   - 勾选 `Loop Time`

3. **添加 Talking 状态**
   - 右键 → `Create State → Empty`
   - 命名为 `Talking`
   - 设置 Motion: 选择你的 `Talking.fbx` 动画
   - 勾选 `Loop Time`

4. **设置转换**

   **Idle → Dancing**:
   - 右键 `Idle` → `Make Transition` → 连接到 `Dancing`
   - 选中箭头，在 Inspector 设置：
     - 条件: `IsDancing == true`
     - Has Exit Time: 取消勾选
     - Transition Duration: 0.3

   **Dancing → Idle**:
   - 右键 `Dancing` → `Make Transition` → 连接到 `Idle`
   - 选中箭头，在 Inspector 设置：
     - 条件: `IsDancing == false`
     - Has Exit Time: 取消勾选
     - Transition Duration: 0.3

   **Idle → Talking**:
   - 右键 `Idle` → `Make Transition` → 连接到 `Talking`
   - 选中箭头，在 Inspector 设置：
     - 条件: `IsSpeaking == true`
     - Has Exit Time: 取消勾选

   **Talking → Idle**:
   - 右键 `Talking` → `Make Transition` → 连接到 `Idle`
   - 选中箭头，在 Inspector 设置：
     - 条件: `IsSpeaking == false`
     - Has Exit Time: 取消勾选

---

## 🎭 第三步：添加 PerformanceController

### 添加组件

1. 在 Hierarchy 中选中你的 VRM 模型 GameObject
2. 在 Inspector 中点击 `Add Component`
3. 搜索并添加 `PerformanceController`

### 系统会自动连接以下组件：
- DancingManager
- SingingCoordinator
- AvatarAnimationController
- VoiceDrivenAnimationMixer

---

## ⚙️ 第四步：创建配置文件

### 创建舞蹈配置

1. 在 Project 窗口右键
2. 选择 `Create → Digital Human → Dance Clips Config`
3. 命名为 `DanceClipsConfig`
4. 在 Inspector 中配置：

```csharp
// 添加一个舞蹈片段：
Dance Clips (Size = 1)
  Element 0:
    Name: "Hip Hop Dance"
    Animation State Name: "Dancing"  // 必须与 Animator 中的状态名一致
    Emotion: "Happy"
    Rhythm: Medium
    Speed Multiplier: 1.0
    Is Loop: true
```

### 创建唱歌情感配置

1. 在 Project 窗口右键
2. 选择 `Create → Digital Human → Singing Emotions Config`
3. 命名为 `SingingEmotionsConfig`
4. 在 Inspector 中配置：

```csharp
// 添加一个情感配置：
Emotions (Size = 1)
  Element 0:
    Emotion Name: "Happy"
    Base Emotion: "Happy"
    Alternative Emotions: ["Surprised", "Neutral"]
    Gestures: ["Nod", "Wave"]
```

---

## 🎮 第五步：测试

### 运行场景

1. 点击 Unity 顶部的 `Play` 按钮
2. 你会看到左上角出现调试 UI

### 测试跳舞

在调试 UI 中点击：
- ✅ `开始跳舞` - 数字人应该开始跳舞
- ✅ `下一个舞蹈` - 切换舞蹈
- ✅ `停止跳舞` - 停止跳舞，回到待机状态

### 测试表情

在调试 UI 中点击：
- ✅ `开心` - 数字人应该露出开心的表情
- ✅ `惊讶` - 露出惊讶的表情
- ✅ `摇头` - 摇头动作
- ✅ `挥手` - 挥手动作

---

## 🎵 第六步：测试唱歌（可选）

### 准备歌曲文件

1. 获取一个 MP3 或 WAV 格式的歌曲文件
2. 放入 `Assets/Resources/Music/` 文件夹

### 测试唱歌

修改代码或使用 Inspector 配置：

```csharp
// 在代码中：
var songClip = Resources.Load<AudioClip>("Music/MySong");
_performanceController.StartSinging(songClip, "Happy");
```

---

## 🤖 第七步：与 AI 集成

### 修改 AI System Prompt

在 `ChatManager.cs` 中修改系统提示词：

```csharp
_llmService.SetSystemPrompt(@"你是一个友好的数字人助手。

你可以在回复中使用以下表演指令：
- [Dance]: 开始跳舞
- [StopDance]: 停止跳舞
- [Wave]: 挥手
- [Nod]: 点头
- [Happy]: 开心表情
- [Sad]: 难过表情
- [Angry]: 生气表情

示例：
[Happy] [Dance] 我很高兴见到你！[Wave]
[Sad] 我感到有点难过...
[Nod] 我明白了。
");
```

### 解析表演指令

在 `ChatManager.cs` 的 `ProcessAiResponseWithAudio` 方法中添加：

```csharp
// 解析表演指令
if (result.text.Contains("[Dance]"))
{
    var performance = GetComponent<DigitalHuman.Animation.PerformanceController>();
    performance.StartDancing("Happy");
}
else if (result.text.Contains("[StopDance]"))
{
    var performance = GetComponent<DigitalHuman.Animation.PerformanceController>();
    performance.StopDancing();
}

if (result.text.Contains("[Happy]"))
{
    _lipSyncController.SetExpression("Happy");
}
// ... 其他指令解析
```

---

## ✅ 检查清单

运行这个检查清单，确保一切正常：

- [ ] 已下载并导入 3 个动画（Idle, Dancing, Talking）
- [ ] 已创建 Animator Controller 并配置参数和状态
- [ ] 已添加 PerformanceController 组件
- [ ] 已创建 DanceClipsConfig 配置文件
- [ ] 已创建 SingingEmotionsConfig 配置文件
- [ ] 在 Inspector 中将配置文件赋值给对应的组件
- [ ] 运行场景，点击"开始跳舞"，数字人开始跳舞
- [ ] 点击"停止跳舞"，数字人停止跳舞
- [ ] 点击表情按钮，表情正常切换
- [ ] 测试 AI 指令，数字人能够响应

---

## 🎯 下一步

完成快速开始后，你可以：

1. **添加更多舞蹈**
   - 下载更多 Mixamo 动画
   - 在 DanceClipsConfig 中添加更多舞蹈片段

2. **优化表情**
   - 调整 AvatarAnimationController 的表情权重
   - 使用 VRM 的 Expression Editor

3. **添加歌曲**
   - 准备多首歌曲
   - 根据歌曲情感自动切换表情

4. **高级功能**
   - 音乐节奏检测
   - 歌词情感分析
   - 多人舞蹈同步

---

## 📞 常见问题

### Q: 点击"开始跳舞"后没有反应？

**检查**:
1. Animator Controller 是否正确赋值？
2. 动画状态名称是否与配置一致？
3. 检查 Console 是否有错误日志

### Q: 舞蹈时身体扭曲？

**检查**:
1. 动画的 Rig 类型是否为 Humanoid？
2. Avatar 是否正确配置？
3. 尝试在 Mixamo 下载时选择相似的 Character Type

### Q: 表情不切换？

**检查**:
1. AvatarLipSyncController 是否正确配置？
2. uLipSync Profile 是否正确设置？
3. 检查 VRM 模型的 Expression 设置

---

## 🎉 恭喜！

你现在有一个会跳舞、会唱歌、会表达情感的数字人了！

继续探索更多功能，让它更加生动！
