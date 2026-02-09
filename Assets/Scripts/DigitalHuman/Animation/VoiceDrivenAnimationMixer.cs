using UnityEngine;
using System.Collections.Generic;

namespace DigitalHuman.Animation
{
    /// <summary>
    /// 动画混合系统 - 核心控制器
    /// 
    /// 设计思路：
    /// 1. 这是一个分层混合系统，每一层控制不同的动画维度
    /// 2. 采用权重混合（Weight-based Blending），不是简单的切换
    /// 3. 支持实时修改，实现平滑过渡
    /// 
    /// 层次结构：
    /// Layer 0: 基础姿态（Base Posture）- 情绪驱动
    /// Layer 1: 手势层（Gestures）- 关键词触发
    /// Layer 2: 动态层（Dynamics）- 语音节奏微调
    /// 
    /// 使用示例：
    /// var mixer = GetComponent<VoiceDrivenAnimationMixer>();
    /// mixer.SetEmotionWeight("Happy", 0.8f);
    /// mixer.TriggerGesture("Wave");
    /// mixer.SetVoiceIntensity(0.6f);
    /// </summary>
    public class VoiceDrivenAnimationMixer : MonoBehaviour
    {
        [Header("目标对象")]
        [Tooltip("要控制的VRM模型Transform")]
        [SerializeField] private Transform _targetAvatar;
        
        [Header("混合设置")]
        [Tooltip("姿态混合速度，值越大切换越快")]
        [Range(0.5f, 10f)]
        [SerializeField] private float _blendSpeed = 3f;
        
        [Tooltip("是否启用语音节奏影响")]
        [SerializeField] private bool _enableVoiceRhythm = true;
        
        // ==================== 核心数据 ====================
        
        /// <summary>
        /// 动画层基类
        /// 每个层代表一个动画维度，有自己的权重和目标值
        /// </summary>
        private abstract class AnimationLayer
        {
            public string Name { get; protected set; }
            public float CurrentWeight { get; set; } // 当前实际权重
            public float TargetWeight { get; set; }  // 目标权重
            public bool IsActive { get; set; }       // 是否激活
            
            public AnimationLayer(string name)
            {
                Name = name;
                CurrentWeight = 0f;
                TargetWeight = 0f;
                IsActive = true;
            }
            
            /// <summary>
            /// 计算这一层对最终姿态的贡献
            /// </summary>
            public abstract AvatarPose CalculatePose();
        }
        
        /// <summary>
        /// 姿态结构 - 表示角色的完整姿态
        /// 包含身体各部分的位置和旋转
        /// </summary>
        public struct AvatarPose
        {
            public Vector3 BodyPosition;      // 身体位置偏移
            public Quaternion BodyRotation;   // 身体旋转
            public float SpineBend;           // 脊柱弯曲度 (-1到1)
            public float HeadTilt;            // 头部倾斜
            public float HeadNod;             // 头部点头
            public float LeftArmRaise;        // 左臂举起程度
            public float RightArmRaise;       // 右臂举起程度
            public float GestureIntensity;    // 手势强度 (0-1)
            
            /// <summary>
            /// 两个姿态的线性插值
            /// </summary>
            public static AvatarPose Lerp(AvatarPose a, AvatarPose b, float t)
            {
                return new AvatarPose
                {
                    BodyPosition = Vector3.Lerp(a.BodyPosition, b.BodyPosition, t),
                    BodyRotation = Quaternion.Slerp(a.BodyRotation, b.BodyRotation, t),
                    SpineBend = Mathf.Lerp(a.SpineBend, b.SpineBend, t),
                    HeadTilt = Mathf.Lerp(a.HeadTilt, b.HeadTilt, t),
                    HeadNod = Mathf.Lerp(a.HeadNod, b.HeadNod, t),
                    LeftArmRaise = Mathf.Lerp(a.LeftArmRaise, b.LeftArmRaise, t),
                    RightArmRaise = Mathf.Lerp(a.RightArmRaise, b.RightArmRaise, t),
                    GestureIntensity = Mathf.Lerp(a.GestureIntensity, b.GestureIntensity, t)
                };
            }
        }
        
        // ==================== 层级定义 ====================
        
        /// <summary>
        /// 基础姿态层 - 由情绪驱动
        /// 例如：开心时身体挺直，难过时弯腰
        /// </summary>
        private class BasePostureLayer : AnimationLayer
        {
            // 情绪对应的预设姿态
            private Dictionary<string, AvatarPose> _emotionPoses;
            private string _currentEmotion = "Neutral";
            
            public BasePostureLayer() : base("BasePosture")
            {
                // 初始化情绪姿态库
                _emotionPoses = new Dictionary<string, AvatarPose>
                {
                    ["Neutral"] = new AvatarPose
                    {
                        BodyPosition = Vector3.zero,
                        BodyRotation = Quaternion.identity,
                        SpineBend = 0f,
                        HeadTilt = 0f,
                        HeadNod = 0f,
                        LeftArmRaise = 0f,
                        RightArmRaise = 0f,
                        GestureIntensity = 0.2f
                    },
                    ["Happy"] = new AvatarPose
                    {
                        BodyPosition = new Vector3(0, 0.02f, 0), // 微微上扬
                        BodyRotation = Quaternion.identity,
                        SpineBend = 0.1f, // 挺直
                        HeadTilt = 0f,
                        HeadNod = 0.1f, // 轻微点头
                        LeftArmRaise = 0.1f,
                        RightArmRaise = 0.1f,
                        GestureIntensity = 0.7f
                    },
                    ["Sad"] = new AvatarPose
                    {
                        BodyPosition = new Vector3(0, -0.02f, 0),
                        BodyRotation = Quaternion.identity,
                        SpineBend = -0.2f, // 弯腰
                        HeadTilt = 0.1f,
                        HeadNod = -0.2f, // 低头
                        LeftArmRaise = 0f,
                        RightArmRaise = 0f,
                        GestureIntensity = 0.1f
                    },
                    ["Surprised"] = new AvatarPose
                    {
                        BodyPosition = new Vector3(0, 0.01f, -0.01f), // 后仰
                        BodyRotation = Quaternion.identity,
                        SpineBend = 0.15f,
                        HeadTilt = 0f,
                        HeadNod = -0.1f, // 抬头
                        LeftArmRaise = 0.2f,
                        RightArmRaise = 0.2f,
                        GestureIntensity = 0.8f
                    }
                };
            }
            
            public void SetEmotion(string emotion)
            {
                if (_emotionPoses.ContainsKey(emotion))
                {
                    _currentEmotion = emotion;
                    TargetWeight = 1f; // 激活这一层
                }
            }
            
            public override AvatarPose CalculatePose()
            {
                if (_emotionPoses.TryGetValue(_currentEmotion, out var pose))
                {
                    return pose;
                }
                return _emotionPoses["Neutral"];
            }
        }
        
        /// <summary>
        /// 手势层 - 由关键词触发
        /// 例如：挥手、点头、思考姿势
        /// </summary>
        private class GestureLayer : AnimationLayer
        {
            private string _currentGesture = "None";
            private float _gestureTimer = 0f;
            private float _gestureDuration = 1.5f; // 手势持续时间
            
            public GestureLayer() : base("Gesture")
            {
                TargetWeight = 0f; // 默认不激活
            }
            
            public void TriggerGesture(string gestureName)
            {
                _currentGesture = gestureName;
                _gestureTimer = 0f;
                TargetWeight = 1f; // 激活手势层
            }
            
            public void Update(float deltaTime)
            {
                if (TargetWeight > 0)
                {
                    _gestureTimer += deltaTime;
                    // 手势结束后自动淡出
                    if (_gestureTimer >= _gestureDuration)
                    {
                        TargetWeight = 0f;
                        _currentGesture = "None";
                    }
                }
            }
            
            public override AvatarPose CalculatePose()
            {
                // 根据手势名称返回对应的姿态偏移
                switch (_currentGesture)
                {
                    case "Wave":
                        return new AvatarPose
                        {
                            RightArmRaise = 1f, // 举起右手
                            GestureIntensity = 0.8f
                        };
                    case "Nod":
                        return new AvatarPose
                        {
                            HeadNod = 0.3f,
                            GestureIntensity = 0.5f
                        };
                    case "Think":
                        return new AvatarPose
                        {
                            HeadTilt = 0.3f, // 歪头
                            LeftArmRaise = 0.3f, // 模拟手托下巴
                            GestureIntensity = 0.4f
                        };
                    default:
                        return new AvatarPose();
                }
            }
        }
        
        /// <summary>
        /// 动态层 - 由语音节奏微调
        /// 根据音量大小调整动作幅度
        /// </summary>
        private class RhythmLayer : AnimationLayer
        {
            private float _voiceIntensity = 0f; // 语音强度 0-1
            private float _baseAmplitude = 0.02f; // 基础摇摆幅度
            
            public RhythmLayer() : base("Rhythm")
            {
                TargetWeight = 0.5f; // 始终有轻微影响
            }
            
            public void SetVoiceIntensity(float intensity)
            {
                _voiceIntensity = Mathf.Clamp01(intensity);
            }
            
            public override AvatarPose CalculatePose()
            {
                // 根据语音强度生成动态摇摆
                float amplitude = _baseAmplitude * (1 + _voiceIntensity * 2f);
                float time = Time.time;
                
                return new AvatarPose
                {
                    BodyPosition = new Vector3(
                        Mathf.Sin(time * 2f) * amplitude, // 左右摇摆
                        Mathf.Sin(time * 3f) * amplitude * 0.5f, // 上下起伏
                        0
                    ),
                    GestureIntensity = _voiceIntensity
                };
            }
        }
        
        // ==================== 私有成员 ====================
        
        private BasePostureLayer _baseLayer;
        private GestureLayer _gestureLayer;
        private RhythmLayer _rhythmLayer;
        private List<AnimationLayer> _allLayers;
        
        private AvatarPose _finalPose; // 计算出的最终姿态
        private AvatarPose _currentPose; // 当前平滑后的姿态
        
        // ==================== 生命周期 ====================
        
        void Awake()
        {
            // 初始化所有层
            _baseLayer = new BasePostureLayer();
            _gestureLayer = new GestureLayer();
            _rhythmLayer = new RhythmLayer();
            
            _allLayers = new List<AnimationLayer>
            {
                _baseLayer,
                _gestureLayer,
                _rhythmLayer
            };
            
            _currentPose = _baseLayer.CalculatePose(); // 初始姿态
            
            if (_targetAvatar == null)
            {
                _targetAvatar = transform;
                Debug.Log("[VoiceDrivenAnimationMixer] 未指定目标，使用自身Transform");
            }
        }
        
        void Update()
        {
            // 更新各层权重（平滑过渡）
            UpdateLayerWeights();
            
            // 更新手势计时器
            _gestureLayer.Update(Time.deltaTime);
            
            // 混合所有层的姿态
            BlendPoses();
            
            // 应用到角色
            ApplyPoseToAvatar();
        }
        
        // ==================== 核心算法 ====================
        
        /// <summary>
        /// 平滑更新各层的权重
        /// 使用线性插值实现平滑过渡
        /// </summary>
        private void UpdateLayerWeights()
        {
            float delta = Time.deltaTime * _blendSpeed;
            
            foreach (var layer in _allLayers)
            {
                if (!layer.IsActive) continue;
                
                // 向目标权重平滑过渡
                layer.CurrentWeight = Mathf.MoveTowards(
                    layer.CurrentWeight,
                    layer.TargetWeight,
                    delta
                );
            }
        }
        
        /// <summary>
        /// 混合所有层的姿态
        /// 采用逐层混合策略：Base → Gesture → Rhythm
        /// </summary>
        private void BlendPoses()
        {
            // 从基础层开始
            _finalPose = _baseLayer.CalculatePose();
            float remainingWeight = 1f - _baseLayer.CurrentWeight;
            
            // 混入手势层
            if (_gestureLayer.CurrentWeight > 0 && remainingWeight > 0)
            {
                var gesturePose = _gestureLayer.CalculatePose();
                float blendFactor = _gestureLayer.CurrentWeight * remainingWeight;
                _finalPose = AvatarPose.Lerp(_finalPose, gesturePose, blendFactor);
                remainingWeight -= blendFactor;
            }
            
            // 混入动态层（叠加效果）
            if (_rhythmLayer.CurrentWeight > 0 && _enableVoiceRhythm)
            {
                var rhythmPose = _rhythmLayer.CalculatePose();
                // 动态层是叠加模式，不是替换
                _finalPose.BodyPosition += rhythmPose.BodyPosition * _rhythmLayer.CurrentWeight;
                _finalPose.GestureIntensity = Mathf.Max(_finalPose.GestureIntensity, 
                    rhythmPose.GestureIntensity * _rhythmLayer.CurrentWeight);
            }
            
            // 平滑到当前姿态（防止突变）
            _currentPose = AvatarPose.Lerp(_currentPose, _finalPose, Time.deltaTime * _blendSpeed);
        }
        
        /// <summary>
        /// 将计算出的姿态应用到VRM角色
        /// 这里使用Transform操作，后续可以优化为骨骼动画
        /// </summary>
        private void ApplyPoseToAvatar()
        {
            if (_targetAvatar == null) return;
            
            // 应用位置偏移
            _targetAvatar.position = _targetAvatar.parent != null 
                ? _targetAvatar.parent.position + _currentPose.BodyPosition
                : _currentPose.BodyPosition;
            
            // 应用旋转（组合多个轴的旋转）
            Quaternion spineRotation = Quaternion.Euler(
                _currentPose.SpineBend * 30f, // 前后弯曲
                0,
                _currentPose.HeadTilt * 20f   // 左右倾斜
            );
            
            Quaternion headRotation = Quaternion.Euler(
                _currentPose.HeadNod * 30f,   // 点头
                0,
                0
            );
            
            _targetAvatar.rotation = _currentPose.BodyRotation * spineRotation * headRotation;
            
            // TODO: 应用手臂姿势（需要骨骼引用）
            // 这里暂时只记录值，后续实现骨骼动画时应用
        }
        
        // ==================== 公共接口 ====================
        
        /// <summary>
        /// 设置情绪权重
        /// 会自动切换到对应的基础姿态
        /// </summary>
        public void SetEmotion(string emotion)
        {
            Debug.Log($"[VoiceDrivenAnimationMixer] 设置情绪: {emotion}");
            _baseLayer.SetEmotion(emotion);
        }
        
        /// <summary>
        /// 触发自定义手势
        /// 手势会在持续一段时间后自动淡出
        /// </summary>
        public void TriggerGesture(string gestureName)
        {
            Debug.Log($"[VoiceDrivenAnimationMixer] 触发手势: {gestureName}");
            _gestureLayer.TriggerGesture(gestureName);
        }
        
        /// <summary>
        /// 设置语音强度（影响动态层）
        /// 值范围：0-1
        /// </summary>
        public void SetVoiceIntensity(float intensity)
        {
            _rhythmLayer.SetVoiceIntensity(intensity);
        }
        
        /// <summary>
        /// 获取当前姿态（供其他系统读取）
        /// </summary>
        public AvatarPose GetCurrentPose()
        {
            return _currentPose;
        }
        
        // ==================== 调试功能 ====================
        
        void OnGUI()
        {
            if (!Application.isEditor) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 150));
            GUILayout.Label("=== 动画混合调试 ===");
            
            foreach (var layer in _allLayers)
            {
                GUILayout.Label($"{layer.Name}: 权重={layer.CurrentWeight:F2}");
            }
            
            GUILayout.Label($"最终姿态强度: {_currentPose.GestureIntensity:F2}");
            GUILayout.EndArea();
        }
    }
}
