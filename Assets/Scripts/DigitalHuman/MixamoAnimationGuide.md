# Mixamo 动画下载与应用完整教程

## 📌 第一步：访问 Mixamo

1. 打开浏览器访问：https://www.mixamo.com/
2. 点击右上角的 **"Sign In"** 登录（需要 Adobe 账号，免费注册）
3. 登录后进入动画库

---

## 📌 第二步：搜索并下载动画

### **推荐下载的动画类型：**

#### **1. 待机动画 (Idle)**
- 搜索关键词：`idle`
- 推荐选择：
  - **"Idle"** - 基础待机
  - **"Idle (1)"** 或 **"Idle (2)"** - 带轻微动作的待机
  - **"Breathing Idle"** - 呼吸待机（很自然）

#### **2. 说话动画 (Talking)**
- 搜索关键词：`talk` 或 `speaking`
- 推荐选择：
  - **"Talking (1)"** - 基础说话手势
  - **"Talking (2)"** - 更活泼的说话
  - **"Excited"** - 兴奋说话（适合开心情绪）

#### **3. 手势动画 (Gestures)**
- 搜索关键词：`gesture` 或具体动作名
- 推荐选择：
  - **"Waving"** - 挥手
  - **"Nodding"** - 点头
  - **"Thinking"** - 思考
  - **"Happy"** - 开心庆祝
  - **"Angry"** - 生气
  - **"Surprised"** - 惊讶

#### **4. 情绪动画 (Emotions)**
- **"Happy"** - 开心
- **"Sad"** - 难过  
- **"Angry"** - 生气
- **"Surprised"** - 惊讶

---

## 📌 第三步：下载动画设置

选中动画后，在右侧设置面板：

```
📥 Download Settings:
├─ Format: FBX for Unity
├─ Skin: Without Skin（不带模型，只下载动画）
├─ Frames per Second: 30
└─ Keyframe Reduction: Uniform（建议开启以优化性能）
```

**点击 Download 按钮下载！**

---

## 📌 第四步：导入 Unity

1. 将下载的 `.fbx` 文件拖入 Unity 的 `Assets/Animations/` 文件夹
2. 选中导入的 FBX 文件
3. 在 Inspector 中设置：
   - **Animation Type**: `Humanoid`
   - **Avatar Definition**: `Create From This Model`
   - 点击 **Apply**

---

## 📌 第五步：创建 Animator Controller

### **5.1 创建 Animator Controller 文件**
1. 在 Project 窗口右键 → Create → **Animator Controller**
2. 命名为 `AvatarAnimatorController`
3. 双击打开 Animator 窗口

### **5.2 创建动画状态 (States)**

在 Animator 窗口 Base Layer 中：

#### **创建 Idle 状态**
1. 右键空白处 → **Create State → Empty**
2. 命名为 "Idle"
3. 在 Motion 字段拖入你下载的 Idle 动画
4. 勾选 **Loop Time**（循环播放）

#### **创建 Talking 状态**
1. 右键 → **Create State → Empty**
2. 命名为 "Talking"
3. 在 Motion 字段拖入 Talking 动画
4. 勾选 **Loop Time**

#### **创建其他手势状态**
- Wave（挥手）
- Nod（点头）
- 等等...

### **5.3 创建过渡 (Transitions)**

**Idle ↔ Talking:**
1. 右键 Idle → **Make Transition** → 指向 Talking
2. 选中这条线（Transition），在 Inspector 设置：
   - **Conditions**: 点击 `+` 添加
   - **IsSpeaking = true**
   - **Has Exit Time**: 取消勾选
   - **Transition Duration**: 0.25 (过渡时间)

3. 反向创建 Talking → Idle 的过渡：
   - **Conditions**: `IsSpeaking = false`

### **5.4 添加参数**

在 Animator 窗口左侧 **Parameters** 面板点击 `+`：

```
Bool 参数:
- IsSpeaking (是否正在说话)
- IsHappy
- IsSad
- IsAngry
- IsSurprised

Float 参数:
- IdleSwayX (待机摇摆 X)
- IdleSwayY (待机摇摆 Y)

Trigger 参数:
- Wave (挥手)
- Nod (点头)
- Shake (摇头)
```

---

## 📌 第六步：应用到 VRM 模型

### **6.1 添加组件**
1. 选中场景中的 VRM 模型
2. 添加 **Animator** 组件（如果还没有）
3. 在 **Controller** 字段拖入 `AvatarAnimatorController`
4. 在 **Avatar** 字段点击 **Create Avatar**（从模型生成）

### **6.2 配置 AvatarAnimationController**
1. 确保 VRM 模型上已有 `AvatarAnimationController` 组件
2. 在 Inspector 中：
   - **Vrm Instance**: 拖入 Vrm10Instance
   - **Animator**: 拖入刚添加的 Animator

---

## 📌 第七步：测试动画

### **方法 1：使用 AnimationTest 脚本**
运行场景，按以下键位：
- `1` - 开心表情 + 可能的 Happy 动画
- `2` - 难过表情
- `3` - 生气表情
- `4` - 惊讶表情
- `Space` - 说话动画

### **方法 2：手动测试**
1. 选中 VRM 模型
2. 在 Inspector 找到 Animator 组件
3. 点击 **IsSpeaking** 复选框（应该切换到 Talking 状态）
4. 取消勾选（回到 Idle 状态）

---

## 🎨 推荐动画组合

### **基础套装（最少动画）**
```
✅ Idle (待机动画) - 必须
✅ Talking (说话动画) - 必须
```

### **完整套装（推荐）**
```
✅ Idle (待机)
✅ Breathing Idle (呼吸待机)
✅ Talking (1) (基础说话)
✅ Talking (2) (活泼说话)
✅ Waving (挥手)
✅ Happy (开心庆祝)
✅ Sad (难过)
✅ Angry (生气)
✅ Surprised (惊讶)
```

---

## ⚠️ 常见问题

### **Q1: 动画导入后不显示？**
**A:** 确保 FBX 的 Animation Type 设置为 **Humanoid**

### **Q2: 动画播放时身体扭曲？**
**A:** 可能是骨骼映射问题。尝试：
1. 选中 FBX → Inspector → Rig → Configure
2. 检查骨骼映射是否正确
3. 或者尝试下载其他类似的动画

### **Q3: VRM 模型没有反应？**
**A:** 检查：
1. Animator Controller 是否正确赋值
2. Avatar 是否已创建
3. 动画状态名称是否匹配代码中的触发名称

### **Q4: 动画太快/太慢？**
**A:** 选中动画文件 → Inspector → Animation 标签：
- 调整 **Sample Rate**（采样率）
- 或者勾选 **Loop Time** 和 **Loop Pose**

---

## 🚀 下一步

下载好动画后，告诉我你下载了哪些，我可以帮你：
1. 优化 Animator Controller 的状态机
2. 添加更复杂的动画混合（如行走+说话）
3. 根据情绪自动切换不同的 Idle 动画

**现在去 Mixamo 下载动画吧！推荐先下 Idle 和 Talking 这两个最基础的。** 🎬
