# 程序化舞蹈测试场景使用说明

## 场景位置
`Assets/Scenes/ProceduralDanceTest.unity`

## 快速设置步骤

### 方法一：使用编辑器工具（推荐）
1. 打开 Unity 编辑器
2. 在菜单栏选择 `DigitalHuman/Setup Procedural Dance Scene`
3. 在弹出的窗口中：
   - 将 VRM 模型拖入场景
   - 将模型重命名为 `DigitalHuman`
   - 点击 `自动绑定骨骼` 按钮

### 方法二：手动设置
1. **打开场景**
   - 在 Unity 中打开 `Assets/Scenes/ProceduralDanceTest.unity`

2. **导入 VRM 模型**
   - 从 `Assets/Avatar/` 文件夹中选择一个 VRM 文件
   - 拖入 Hierarchy 面板

3. **设置对象名称**
   - 选中 VRM 模型的根对象
   - 在 Inspector 中将名称改为 `DigitalHuman`

4. **添加 ProceduralDanceGenerator 组件**
   - 选中 `DigitalHuman` 对象
   - 在 Inspector 面板点击 `Add Component`
   - 搜索并添加 `ProceduralDanceGenerator`

5. **绑定骨骼引用**
   - 在 `ProceduralDanceGenerator` 组件的 `骨骼引用` 部分
   - 手动将对应的骨骼对象拖入字段：
     ```
     Hips          → 对象的 Hips 骨骼
     Spine         → 对象的 Spine 骨骼
     Chest         → 对象的 Chest 骨骼
     Neck          → 对象的 Neck 骨骼
     Head          → 对象的 Head 骨骼
     LeftShoulder   → 对象的左肩骨骼
     LeftUpperArm   → 对象的左上臂骨骼
     LeftLowerArm   → 对象的左下臂骨骼
     LeftHand      → 对象的左手骨骼
     RightShoulder  → 对象的右肩骨骼
     RightUpperArm  → 对象的右上臂骨骼
     RightLowerArm  → 对象的右下臂骨骼
     RightHand     → 对象的右手骨骼
     LeftUpperLeg   → 对象的左大腿骨骼
     LeftLowerLeg   → 对象的左小腿骨骼
     LeftFoot      → 对象的左脚骨骼
     RightUpperLeg  → 对象的右大腿骨骼
     RightLowerLeg  → 对象的右小腿骨骼
     RightFoot     → 对象的右脚骨骼
     ```

6. **设置 Animator**
   - 确保 `DigitalHuman` 对象上有 `Animator` 组件
   - Avatar 设置为 VRM 模型的 Avatar

## 使用程序化舞蹈

### 在运行时控制
1. 点击 Unity 编辑器顶部的 `Play` 按钮
2. 在 Game 窗口的左上角会显示 GUI 控制面板

### 可用控制
- **舞蹈风格选择**：
  - Hip Hop（嘻哈）
  - Pop（流行）
  - Ballet（芭蕾）
  - Robot（机器人）
  - Wave（波浪）

- **播放控制**：
  - 点击 `开始跳舞` / `停止跳舞` 按钮

- **参数调整**：
  - BPM 滑块：调整舞蹈速度（60-200）
  - 强度滑块：调整舞蹈幅度（0-1）

## 功能说明

### 舞蹈风格
1. **Hip Hop**
   - 节奏感强，身体律动明显
   - 手臂摆动幅度大
   - 适合快节奏音乐

2. **Pop**
   - 轻快优雅
   - 动作相对柔和
   - 适合流行音乐

3. **Ballet**
   - 优雅伸展
   - 动作流畅
   - 适合古典音乐

4. **Robot**
   - 机械式动作
   - 顿挫感明显
   - 适合电子音乐

5. **Wave**
   - 波浪式律动
   - 连续流动的动作
   - 适合放松音乐

### 技术特性
- ✅ 完全由代码驱动，不依赖预设动画
- ✅ 实时生成舞蹈动作
- ✅ 支持 BPM 同步
- ✅ 可调节舞蹈强度
- ✅ 多种舞蹈风格
- ✅ 平滑的骨骼动画

## 常见问题

### Q: 为什么模型没有动起来？
A: 请确保：
1. 所有关键骨骼都已正确绑定
2. Animator 组件已正确设置 Avatar
3. 点击了 `开始跳舞` 按钮

### Q: 动作看起来很僵硬？
A: 可以尝试：
1. 调整 `舞蹈强度` 滑块
2. 尝试不同的舞蹈风格
3. 调整 BPM 参数

### Q: 骨骼找不到怎么办？
A: 不同 VRM 模型的骨骼命名可能不同，需要手动查找对应名称的骨骼并绑定。

## 下一步
- 尝试添加音频节奏分析器实现音乐同步
- 尝试使用反向运动学（IK）改善手脚接触地面的效果
- 尝试添加更多自定义舞蹈风格

## 技术支持
如有问题，请检查：
1. Unity Console 是否有错误信息
2. 骨骼绑定是否完整
3. Animator 是否已启用
