using UnityEngine;
using DigitalHuman.Core;

namespace DigitalHuman.Animation
{
    /// <summary>
    /// 程序化表演控制器 - 统一管理所有程序化动画组件
    /// 这是完全由代码驱动、不依赖预设动画的表演系统
    /// </summary>
    public class ProceduralPerformanceController : MonoBehaviour
    {
        [Header("核心组件")]
        [SerializeField] private ProceduralDanceGenerator _danceGenerator;
        [SerializeField] private AudioRhythmAnalyzer _rhythmAnalyzer;
        [SerializeField] private AIDanceGenerator _aiGenerator;
        [SerializeField] private AvatarAnimationController _animController;
        [SerializeField] private AvatarLipSyncController _lipSyncController;

        [Header("骨骼设置")]
        [SerializeField] private Transform _hips;
        [SerializeField] private Animator _animator;
        [SerializeField] private bool _autoSetupBones = true;

        [Header("表演模式")]
        [SerializeField] private PerformanceMode _currentMode = PerformanceMode.Idle;
        [SerializeField] private bool _enableAIBehavior = true;

        [Header("调试")]
        [SerializeField] private bool _showDebugUI = true;

        private AudioSource _audioSource;
        private bool _isInitialized = false;

        public PerformanceMode CurrentMode => _currentMode;
        public ProceduralDanceGenerator DanceGenerator => _danceGenerator;
        public AudioRhythmAnalyzer RhythmAnalyzer => _rhythmAnalyzer;

        void Awake()
        {
            if (_autoSetupBones)
            {
                AutoSetupBones();
            }
        }

        void Start()
        {
            InitializeComponents();
        }

        void Update()
        {
            if (!_isInitialized) return;

            // 更新 AI 行为
            if (_enableAIBehavior)
            {
                UpdateAIBehavior();
            }
        }

        /// <summary>
        /// 自动设置骨骼
        /// </summary>
        private void AutoSetupBones()
        {
            if (_animator == null)
                _animator = GetComponent<Animator>();

            if (_animator != null)
            {
                // 自动获取主要骨骼
                _hips = _animator.GetBoneTransform(HumanBodyBones.Hips);

                if (_hips != null)
                {
                    Debug.Log("[ProceduralPerformanceController] 自动设置骨骼成功");
                }
            }
        }

        /// <summary>
        /// 初始化组件
        /// </summary>
        private void InitializeComponents()
        {
            // 获取或创建组件
            if (_danceGenerator == null)
                _danceGenerator = GetComponent<ProceduralDanceGenerator>();
            if (_rhythmAnalyzer == null)
                _rhythmAnalyzer = GetComponent<AudioRhythmAnalyzer>();
            if (_aiGenerator == null)
                _aiGenerator = GetComponent<AIDanceGenerator>();
            if (_animController == null)
                _animController = GetComponent<AvatarAnimationController>();
            if (_lipSyncController == null)
                _lipSyncController = GetComponent<AvatarLipSyncController>();

            // 获取 AudioSource
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
                _audioSource = gameObject.AddComponent<AudioSource>();

            // 配置组件
            if (_rhythmAnalyzer != null)
            {
                // 通过反射设置 AudioSource
                var audioField = typeof(AudioRhythmAnalyzer).GetField("_audioSource",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (audioField != null)
                {
                    audioField.SetValue(_rhythmAnalyzer, _audioSource);
                }
            }

            _isInitialized = true;
            Debug.Log("[ProceduralPerformanceController] 组件初始化完成");
        }

        /// <summary>
        /// 开始程序化舞蹈
        /// </summary>
        public void StartProceduralDance(DanceStyle style = DanceStyle.HipHop)
        {
            _currentMode = PerformanceMode.Dancing;

            // 开始 AI 生成
            if (_aiGenerator != null)
            {
                _aiGenerator.StartAIGeneration(style);
            }

            // 设置表情
            if (_animController != null)
            {
                _animController.SetEmotion(GetEmotionForStyle(style));
            }

            Debug.Log($"[ProceduralPerformanceController] 开始程序化舞蹈: {style}");
        }

        /// <summary>
        /// 停止程序化舞蹈
        /// </summary>
        public void StopProceduralDance()
        {
            _currentMode = PerformanceMode.Idle;

            if (_aiGenerator != null)
            {
                _aiGenerator.StopAIGeneration();
            }

            if (_animController != null)
            {
                _animController.SetEmotion("Neutral");
            }

            Debug.Log("[ProceduralPerformanceController] 停止程序化舞蹈");
        }

        /// <summary>
        /// 开始唱歌（程序化）
        /// </summary>
        public void StartProceduralSinging(AudioClip songClip, DanceStyle style = DanceStyle.Pop)
        {
            _currentMode = PerformanceMode.Singing;

            if (_audioSource != null)
            {
                _audioSource.clip = songClip;
                _audioSource.Play();
            }

            // 配置口型同步
            if (_lipSyncController != null)
            {
                _lipSyncController.ConfigureForAudioSource(_audioSource);
            }

            // 开始舞蹈
            StartProceduralDance(style);

            Debug.Log($"[ProceduralPerformanceController] 开始程序化唱歌: {songClip.name}");
        }

        /// <summary>
        /// 停止唱歌
        /// </summary>
        public void StopProceduralSinging()
        {
            if (_audioSource != null)
            {
                _audioSource.Stop();
            }

            StopProceduralDance();

            Debug.Log("[ProceduralPerformanceController] 停止程序化唱歌");
        }

        /// <summary>
        /// 切换舞蹈风格
        /// </summary>
        public void ChangeDanceStyle(DanceStyle newStyle)
        {
            if (_aiGenerator != null)
            {
                _aiGenerator.SetDanceStyle(newStyle);
            }

            if (_animController != null)
            {
                _animController.SetEmotion(GetEmotionForStyle(newStyle));
            }
        }

        /// <summary>
        /// 根据风格获取对应情感
        /// </summary>
        private string GetEmotionForStyle(DanceStyle style)
        {
            switch (style)
            {
                case DanceStyle.HipHop:
                    return "Happy";
                case DanceStyle.Pop:
                    return "Happy";
                case DanceStyle.Ballet:
                    return "Neutral";
                case DanceStyle.Robot:
                    return "Surprised";
                case DanceStyle.Wave:
                    return "Happy";
                default:
                    return "Neutral";
            }
        }

        /// <summary>
        /// 更新 AI 行为
        /// </summary>
        private void UpdateAIBehavior()
        {
            // 这里可以添加高级 AI 行为逻辑
            // 例如：根据音乐情感自动切换风格、手势等
        }

        /// <summary>
        /// 触发手势
        /// </summary>
        public void TriggerGesture(string gestureName)
        {
            if (_animController != null)
            {
                _animController.TriggerGesture(gestureName);
            }
        }

        /// <summary>
        /// 设置表情
        /// </summary>
        public void SetEmotion(string emotion)
        {
            if (_animController != null)
            {
                _animController.SetEmotion(emotion);
            }
        }

        // ==================== 调试 UI ====================

        void OnGUI()
        {
            if (!_showDebugUI || !Application.isEditor) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 500));
            GUILayout.Label("=== 程序化表演控制器 ===");
            GUILayout.Label($"当前模式: {_currentMode}");
            GUILayout.Label($"初始化: {(_isInitialized ? "完成" : "未完成")}");

            GUILayout.Space(10);

            // 舞蹈控制
            GUILayout.Label("--- 程序化舞蹈 ---");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Hip Hop"))
                StartProceduralDance(DanceStyle.HipHop);
            if (GUILayout.Button("Pop"))
                StartProceduralDance(DanceStyle.Pop);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Ballet"))
                StartProceduralDance(DanceStyle.Ballet);
            if (GUILayout.Button("Robot"))
                StartProceduralDance(DanceStyle.Robot);
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Wave"))
                StartProceduralDance(DanceStyle.Wave);

            if (GUILayout.Button("停止舞蹈"))
                StopProceduralDance();

            GUILayout.Space(10);

            // BPM 控制
            if (_danceGenerator != null)
            {
                GUILayout.Label($"当前 BPM: {_danceGenerator.GetBPM():F0}");
            }

            // 能量信息
            if (_rhythmAnalyzer != null && _rhythmAnalyzer.GetBPM() > 0)
            {
                GUILayout.Label($"检测 BPM: {_rhythmAnalyzer.GetBPM():F1}");
                GUILayout.Label($"当前能量: {_rhythmAnalyzer.GetCurrentEnergy():F4}");
            }

            GUILayout.Space(10);

            // 表情控制
            GUILayout.Label("--- 表情 ---");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("开心")) SetEmotion("Happy");
            if (GUILayout.Button("难过")) SetEmotion("Sad");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("生气")) SetEmotion("Angry");
            if (GUILayout.Button("惊讶")) SetEmotion("Surprised");
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // 手势控制
            GUILayout.Label("--- 手势 ---");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("点头")) TriggerGesture("Nod");
            if (GUILayout.Button("摇头")) TriggerGesture("Shake");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("挥手")) TriggerGesture("Wave");
            if (GUILayout.Button("思考")) TriggerGesture("Think");
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }
    }

    /// <summary>
    /// 表演模式
    /// </summary>
    public enum PerformanceMode
    {
        Idle,       // 空闲
        Dancing,    // 跳舞
        Singing     // 唱歌
    }
}
