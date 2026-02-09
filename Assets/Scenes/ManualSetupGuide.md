# 手动设置程序化舞蹈场景

## 步骤 1：打开场景
在 Unity 中打开 `Assets/Scenes/ProceduralDanceTest.unity`

## 步骤 2：加载 VRM 模型
1. 在 Project 窗口中找到 VRM 模型（如 `AvatarSample_A.vrm`）
2. 将其拖入 Hierarchy 窗口
3. 选中该对象，在 Inspector 中将名称改为 `DigitalHuman`

## 步骤 3：添加组件
选中 `DigitalHuman` 对象，添加以下组件：

### 3.1 添加 Animator
1. 点击 `Add Component`
2. 搜索 `Animator`
3. 确保 Avatar 已设置（通常导入 VRM 时会自动设置）

### 3.2 添加 ProceduralDanceGenerator
1. 点击 `Add Component`
2. 搜索 `ProceduralDanceGenerator`
3. 添加该组件

### 3.3 添加 QuickBoneBinder（辅助工具）
1. 点击 `Add Component`
2. 搜索 `QuickBoneBinder`
3. 添加该组件

## 步骤 4：使用快速绑定工具（推荐）

### 4.1 配置 QuickBoneBinder
在 Inspector 的 `QuickBoneBinder` 组件中：

1. **VRM Root** 字段：
   - 拖入 `DigitalHuman` 对象（或其子对象中的根骨骼）

2. **Dance Generator** 字段：
   - 拖入刚才添加的 `ProceduralDanceGenerator` 组件

3. **Bind Bones** 复选框：
   - 勾选此复选框
   - 骨骼会自动绑定
   - 查看控制台输出，确认哪些骨骼已绑定

## 步骤 5：手动补充未绑定的骨骼

如果自动绑定后还有未绑定的骨骼，需要手动操作：

### 找到骨骼的方法
1. 在 Hierarchy 中展开 `DigitalHuman` 对象
2. 找到对应的骨骼名称
3. 将骨骼对象拖入 `ProceduralDanceGenerator` 的对应字段

### 标准骨骼映射
```
身体部分          VRM骨骼名称（可能略有不同）
─────────────────────────────────────────────
Hips              → Hips
Spine             → Spine
Chest             → Chest/UpperChest
Neck              → Neck
Head              → Head

左手臂            → LeftUpperArm
左上臂            → LeftUpperArm
左下臂            → LeftLowerArm
左手              → LeftHand

右肩              → RightShoulder
右上臂            → RightUpperArm
右下臂            → RightLowerArm
右手              → RightHand

左大腿            → LeftUpperLeg
左小腿            → LeftLowerLeg
左脚              → LeftFoot

右大腿            → RightUpperLeg
右小腿            → RightLowerLeg
右脚              → RightFoot
```

## 步骤 6：检查设置

### 6.1 检查关键骨骼
确保以下关键骨骼都已绑定（优先级从高到低）：
```
必须绑定的骨骼（最少）：
✓ Hips
✓ Head
✓ LeftUpperArm
✓ RightUpperArm
✓ LeftUpperLeg
✓ RightUpperLeg

推荐绑定的骨骼（完整效果）：
✓ Spine
✓ Chest
✓ Neck
✓ LeftLowerArm
✓ RightLowerArm
✓ LeftHand
✓ RightHand
✓ LeftLowerLeg
✓ RightLowerLeg
```

### 6.2 测试运行
1. 点击 Unity 的 `Play` 按钮
2. 在 Game 窗口中点击 `开始跳舞` 按钮
3. 观察模型是否开始动作

## 常见问题解决

### Q1: QuickBoneBinder 没有找到所有骨骼
**A**: 不同 VRM 模型的骨骼命名可能不同。
**解决方法**:
1. 展开 Hierarchy 中的 VRM 模型
2. 查看实际的骨骼名称
3. 手动将对应的骨骼拖入字段

### Q2: 模型不动
**检查清单**:
- [ ] Animator 组件的 Avatar 是否已设置？
- [ ] ProceduralDanceGenerator 的骨骼引用是否都已绑定？
- [ ] 是否点击了 `开始跳舞` 按钮？
- [ ] 控制台是否有错误信息？

### Q3: 动作很奇怪
**可能原因**:
- 骨骼绑定错误（如把左手绑到了右臂）
- 骨骼层级关系不正确

**解决方法**:
1. 检查每个骨骼是否正确
2. 确保 Left 对应左侧，Right 对应右侧
3. 尝试只绑定最少必需的骨骼测试

### Q4: 找不到某些骨骼
**变通方法**:
- 如果找不到 `Chest`，可以绑定到 `Spine`
- 如果找不到 `Neck`，可以绑定到 `Head`
- 如果找不到 `LowerArm`，可以绑定到 `UpperArm`

## 最小化配置示例

如果只想快速测试，可以只绑定以下 6 个骨骼：

```
Hips           → 必需（身体中心）
Head           → 必需（头部动作）
LeftUpperArm   → 必需（左手臂）
RightUpperArm  → 必需（右手臂）
LeftUpperLeg   → 必需（左腿）
RightUpperLeg  → 必需（右腿）
```

其他骨骼可以暂时留空，程序会使用默认值。

## 下一步

设置完成后：
1. 保存场景（Ctrl+S）
2. 点击 Play 测试
3. 尝试不同的舞蹈风格和参数
4. 根据需要调整骨骼绑定

## 技术提示

- 使用 QuickBoneBinder 可以快速完成大部分绑定
- 控制台会显示哪些骨骼成功绑定
- 如果骨骼名称不匹配，需要手动查找并绑定
- 绑定后记得保存场景

祝您使用愉快！
