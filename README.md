# 数字人程序化舞蹈系统

<p align="center">
  <img src="https://img.shields.io/badge/Unity-2022.3.62f3c1-blue?style=for-the-badge&logo=unity" alt="Unity Version">
  <img src="https://img.shields.io/badge/C%23-9.0-green?style=for-the-badge&logo=c-sharp" alt="C# Version">
  <img src="https://img.shields.io/badge/License-MIT-yellow?style=for-the-badge" alt="License">
</p>

<p align="center">
  <b>基于 Unity 的数字人实时舞蹈生成系统</b><br>
  无需预设动画，通过数学函数实时驱动骨骼生成舞蹈动作
</p>

---

## ✨ 特性

### 🎭 程序化舞蹈生成
- ✅ **完全代码驱动** - 不依赖预设动画文件，实时生成舞蹈动作
- ✅ **数学函数控制** - 使用正弦波、余弦波、噪声等算法驱动骨骼
- ✅ **智能节拍同步** - 支持 BPM 节奏同步，可与音乐完美配合
- ✅ **平滑过渡** - 动作流畅自然，无突兀跳变

### 🎵 多风格舞蹈
| 舞蹈风格 | 描述 |
|---------|------|
| **Hip Hop** | 节奏感强，身体律动明显，适合快节奏音乐 |
| **Pop** | 轻快优雅，动作柔和，适合流行音乐 |
| **Ballet** | 优雅伸展，动作流畅，适合古典音乐 |
| **Robot** | 机械式动作，顿挫感明显，适合电子音乐 |
| **Wave** | 波浪式律动，连续流动，适合放松音乐 |

### 🔧 技术特点
- 🎯 **实时调节** - 可在运行时调整舞蹈速度、强度、BPM
- 🎨 **可视化调试** - 内置 GUI 控制面板，实时监控舞蹈状态
- 🦴 **智能骨骼绑定** - 自动识别和绑定多种命名格式的骨骼
- 📱 **跨平台支持** - 支持 Windows、Mac、WebGL 等平台

---

## 📁 项目结构

```
Assets/
├── Scripts/
│   └── DigitalHuman/
│       ├── Animation/          # 动画系统核心代码
│       │   ├── ProceduralDanceGenerator.cs    # 程序化舞蹈生成器
│       │   ├── AudioRhythmAnalyzer.cs         # 音频节奏分析器
│       │   ├── SmartBoneFinder.cs            # 智能骨骼查找器
│       │   └── ...                           # 其他组件
│       ├── Audio/              # 音频处理模块
│       ├── Network/            # 网络通信模块（LLM、TTS、ASR）
│       ├── UI/                 # UI 控制器
│       └── Data/               # 数据模型
├── Scenes/
│   └── ProceduralDanceTest.unity  # 程序化舞蹈测试场景
├── Avatar/                     # VRM 模型资源
└── uLipSync/                   # 口型同步插件
```

---

## 🚀 快速开始

### 环境要求
- Unity 2022.3.62f3c1 或更高版本
- Windows 10/11 或 macOS 10.15+
- Git

### 安装步骤

1. **克隆仓库**
   ```bash
   git clone https://github.com/XINI-WZ/AIDigitalProject.git
   cd AIDigitalProject
   ```

2. **在 Unity 中打开项目**
   - 启动 Unity Hub
   - 点击 "Open"
   - 选择项目文件夹
   - 等待 Unity 导入资源

3. **打开测试场景**
   - 导航到 `Assets/Scenes/`
   - 打开 `ProceduralDanceTest.unity`

4. **加载 VRM 模型**
   - 从 `Assets/Avatar/` 拖入 VRM 文件到场景
   - 将模型重命名为 "DigitalHuman"

5. **配置骨骼绑定**
   - 选中 DigitalHuman 对象
   - 添加 `SmartBoneFinder` 组件
   - 勾选 "Auto Find And Bind" 自动绑定骨骼

6. **运行测试**
   - 点击 Play 按钮
   - 在 Game 窗口中点击舞蹈风格按钮
   - 享受实时生成的舞蹈！

---

## 🎮 使用指南

### 运行时控制面板

当场景运行时，Game 窗口左上角会显示控制面板：

```
=== 程序化舞蹈系统 ===
舞蹈状态: 跳舞中
舞蹈风格: HipHop
BPM: 120
舞蹈强度: 1.00

[舞蹈风格]
[Hip Hop] [Pop] [Ballet] [Robot] [Wave]

[控制]
[开始跳舞] / [停止跳舞]

BPM: ████████░░ 120
强度: ██████████ 1.0
```

### 脚本 API

```csharp
// 获取程序化舞蹈生成器
var danceGenerator = GetComponent<ProceduralDanceGenerator>();

// 开始跳舞
danceGenerator.StartDancing(DanceStyle.HipHop);

// 切换舞蹈风格
danceGenerator.SetDanceStyle(DanceStyle.Pop);

// 设置 BPM
danceGenerator.SetBPM(140f);

// 设置舞蹈强度 (0-1)
danceGenerator.SetDanceIntensity(0.8f);

// 停止跳舞
danceGenerator.StopDancing();
```

---

## 🛠️ 核心组件

### ProceduralDanceGenerator
主要舞蹈生成组件，包含以下功能：
- 实时骨骼控制
- 5 种舞蹈风格算法
- 节拍检测与同步
- 动作缓存与重置

### SmartBoneFinder
智能骨骼查找工具：
- 自动识别多种骨骼命名格式
- 支持标准 Humanoid 骨骼
- 模糊匹配算法
- 一键绑定所有骨骼

### AudioRhythmAnalyzer
音频节奏分析器：
- 实时频谱分析
- BPM 检测
- 节拍触发事件
- 能量层级分析

---

## 📚 文档

- [快速开始指南](Assets/Scripts/DigitalHuman/QuickStartGuide.md)
- [动画设置指南](Assets/Scripts/DigitalHuman/AnimationSetupGuide.md)
- [程序化动画说明](Assets/Scripts/DigitalHuman/PROCEDURAL_ANIMATION_README.md)
- [场景设置指南](Assets/Scenes/ManualSetupGuide.md)

---

## 🤝 贡献

欢迎提交 Pull Request！如果您有改进建议或发现了 bug，请通过以下方式联系我们：

1. Fork 本仓库
2. 创建您的 Feature Branch (`git checkout -b feature/AmazingFeature`)
3. 提交您的更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到 Branch (`git push origin feature/AmazingFeature`)
5. 打开 Pull Request

---

## 📝 开源协议

本项目基于 [MIT License](LICENSE) 开源。

---

## 🙏 致谢

- [uLipSync](https://github.com/hecomi/uLipSync) - 口型同步插件
- [UniVRM](https://github.com/vrm-c/UniVRM) - VRM 模型支持
- Unity Technologies - Unity 引擎

---

<p align="center">
  Made with ❤️ by DigitalHuman Team
</p>
