using UnityEngine;
using DigitalHuman.Core;
using DigitalHuman.Data;

namespace DigitalHuman.Animation
{
    /// <summary>
    /// 表演控制器 - 统一管理舞蹈、唱歌、表情和手势
    /// 这是数字人"像真人一样"表现的核心控制器
    /// </summary>
    public class PerformanceController : MonoBehaviour
    {
        [Header("子系统集成")]
        [SerializeField] private DancingManager _dancingManager;
        [SerializeField] private SingingCoordinator _singingCoordinator;
        [SerializeField] private AvatarAnimationController _animController;
        [SerializeField] private VoiceDrivenAnimationMixer _animationMixer;

        [Header("表演设置")]
        [SerializeField] private bool _enableAutoPerformance = true;
        [SerializeField] private float _idleDanceProbability = 0.01f; // 空闲时自动跳舞的概率

        private PerformanceState _currentState = PerformanceState.Idle;

        public PerformanceState CurrentState => _currentState;
        public DancingManager DancingManager => _dancingManager;
        public SingingCoordinator SingingCoordinator => _singingCoordinator;

        void Awake()
        {
            if (_dancingManager == null) _dancingManager = GetComponent<DancingManager>();
            if (_singingCoordinator == null) _singingCoordinator = GetComponent<SingingCoordinator>();
            if (_animController == null) _animController = GetComponent<AvatarAnimationController>();
            if (_animationMixer == null) _animationMixer = GetComponent<VoiceDrivenAnimationMixer>();
        }

        void Update()
        {
            // 空闲时的自动表演
            if (_enableAutoPerformance && _currentState == PerformanceState.Idle)
            {
                HandleIdleBehavior();
            }
        }

        /// <summary>
        /// 表演状态枚举
        /// </summary>
        public enum PerformanceState
        {
            Idle,           // 空闲
            Talking,        // 对话中
            Dancing,        // 跳舞
            Singing,        // 唱歌
            DancingAndSinging // 边唱边跳
        }

        // ==================== 跳舞控制 ====================

        /// <summary>
        /// 开始跳舞
        /// </summary>
        public void StartDancing(string emotion = "Happy")
        {
            if (_dancingManager == null) return;

            _currentState = PerformanceState.Dancing;

            Debug.Log($"[PerformanceController] 开始跳舞，情感: {emotion}");

            // 设置表情
            if (_animController != null)
            {
                _animController.SetEmotion(emotion);
            }

            // 开始舞蹈
            _dancingManager.StartRandomDance();

            // 更新动画混合器
            if (_animationMixer != null)
            {
                _animationMixer.SetEmotion(emotion);
            }
        }

        /// <summary>
        /// 停止跳舞
        /// </summary>
        public void StopDancing()
        {
            if (_dancingManager == null) return;

            Debug.Log("[PerformanceController] 停止跳舞");

            _dancingManager.StopDancing();

            if (_currentState == PerformanceState.Dancing || _currentState == PerformanceState.DancingAndSinging)
            {
                _currentState = PerformanceState.Idle;
            }

            // 恢复表情
            if (_animController != null)
            {
                _animController.SetEmotion("Neutral");
            }
        }

        /// <summary>
        /// 切换舞蹈
        /// </summary>
        public void NextDance()
        {
            if (_dancingManager != null)
            {
                _dancingManager.NextDance();
            }
        }

        // ==================== 唱歌控制 ====================

        /// <summary>
        /// 开始唱歌
        /// </summary>
        public void StartSinging(AudioClip songClip, string emotion = "Happy", bool withDance = true)
        {
            if (_singingCoordinator == null || songClip == null) return;

            _currentState = PerformanceState.Singing;

            Debug.Log($"[PerformanceController] 开始唱歌: {songClip.name}, 情感: {emotion}");

            // 开始唱歌（包含舞蹈）
            _singingCoordinator.StartSinging(songClip, emotion);

            // 如果不需要跳舞，手动停止舞蹈
            if (!withDance && _dancingManager != null)
            {
                _dancingManager.StopDancing();
                _currentState = PerformanceState.Singing;
            }
            else if (withDance && _dancingManager != null && _dancingManager.IsDancing)
            {
                _currentState = PerformanceState.DancingAndSinging;
            }
        }

        /// <summary>
        /// 停止唱歌
        /// </summary>
        public void StopSinging()
        {
            if (_singingCoordinator == null) return;

            Debug.Log("[PerformanceController] 停止唱歌");

            _singingCoordinator.StopSinging();

            if (_currentState == PerformanceState.Singing || _currentState == PerformanceState.DancingAndSinging)
            {
                _currentState = PerformanceState.Idle;
            }
        }

        // ==================== 对话控制 ====================

        /// <summary>
        /// 开始对话（与 ChatManager 集成）
        /// </summary>
        public void StartSpeaking()
        {
            if (_animController == null) return;

            Debug.Log("[PerformanceController] 开始对话");

            // 停止唱歌/跳舞
            StopSinging();
            StopDancing();

            _currentState = PerformanceState.Talking;

            // 通知动画控制器
            _animController.OnStartSpeaking();
        }

        /// <summary>
        /// 停止对话
        /// </summary>
        public void StopSpeaking()
        {
            if (_animController == null) return;

            Debug.Log("[PerformanceController] 停止对话");

            _animController.OnStopSpeaking();

            if (_currentState == PerformanceState.Talking)
            {
                _currentState = PerformanceState.Idle;
            }
        }

        /// <summary>
        /// 设置对话时的表情
        /// </summary>
        public void SetEmotion(string emotion)
        {
            if (_animController != null)
            {
                _animController.SetEmotion(emotion);
            }

            if (_animationMixer != null)
            {
                _animationMixer.SetEmotion(emotion);
            }
        }

        // ==================== 手势控制 ====================

        /// <summary>
        /// 触发手势
        /// </summary>
        public void TriggerGesture(string gestureName)
        {
            if (_animController != null)
            {
                _animController.TriggerGesture(gestureName);
            }

            if (_animationMixer != null)
            {
                _animationMixer.TriggerGesture(gestureName);
            }
        }

        // ==================== 自动行为 ====================

        /// <summary>
        /// 处理空闲时的自动行为
        /// </summary>
        private void HandleIdleBehavior()
        {
            // 随机开始跳舞
            if (Random.value < _idleDanceProbability * Time.deltaTime)
            {
                string randomEmotion = GetRandomEmotion();
                StartDancing(randomEmotion);

                // 5秒后自动停止
                Invoke(nameof(StopDancing), 5f);
            }
        }

        /// <summary>
        /// 获取随机情感
        /// </summary>
        private string GetRandomEmotion()
        {
            string[] emotions = { "Happy", "Surprised", "Neutral" };
            return emotions[Random.Range(0, emotions.Length)];
        }

        // ==================== 调试 UI ====================

        void OnGUI()
        {
            if (!Application.isEditor) return;

            GUILayout.BeginArea(new Rect(10, 10, 280, 400));
            GUILayout.Label("=== 表演控制器 ===");
            GUILayout.Label($"当前状态: {_currentState}");
            GUILayout.Label($"自动表演: {(_enableAutoPerformance ? "开启" : "关闭")}");

            GUILayout.Space(10);

            // 跳舞控制
            GUILayout.Label("--- 跳舞 ---");
            if (GUILayout.Button("开始跳舞"))
            {
                StartDancing("Happy");
            }
            if (GUILayout.Button("下一个舞蹈"))
            {
                NextDance();
            }
            if (GUILayout.Button("停止跳舞"))
            {
                StopDancing();
            }

            GUILayout.Space(10);

            // 唱歌控制
            GUILayout.Label("--- 唱歌 ---");
            if (GUILayout.Button("开始唱歌（测试）"))
            {
                // TODO: 添加测试歌曲
                Debug.Log("[PerformanceController] 需要添加测试歌曲");
            }
            if (GUILayout.Button("停止唱歌"))
            {
                StopSinging();
            }

            GUILayout.Space(10);

            // 手势控制
            GUILayout.Label("--- 手势 ---");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("点头")) TriggerGesture("Nod");
            if (GUILayout.Button("摇头")) TriggerGesture("Shake");
            if (GUILayout.Button("歪头")) TriggerGesture("Tilt");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("挥手")) TriggerGesture("Wave");
            if (GUILayout.Button("思考")) TriggerGesture("Think");
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // 情感控制
            GUILayout.Label("--- 情感 ---");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("开心")) SetEmotion("Happy");
            if (GUILayout.Button("生气")) SetEmotion("Angry");
            if (GUILayout.Button("难过")) SetEmotion("Sad");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("惊讶")) SetEmotion("Surprised");
            if (GUILayout.Button("中性")) SetEmotion("Neutral");
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            if (GUILayout.Button(_enableAutoPerformance ? "关闭自动表演" : "开启自动表演"))
            {
                _enableAutoPerformance = !_enableAutoPerformance;
            }

            GUILayout.EndArea();
        }

        // ==================== API 接口 ====================

        /// <summary>
        /// 执行表演动作（用于 AI 指令解析）
        /// </summary>
        public void ExecutePerformanceAction(string action, string parameter = "")
        {
            Debug.Log($"[PerformanceController] 执行动作: {action}, 参数: {parameter}");

            switch (action.ToLower())
            {
                case "dance":
                case "跳舞":
                    StartDancing(string.IsNullOrEmpty(parameter) ? "Happy" : parameter);
                    break;

                case "stopdance":
                case "stop_dance":
                case "停止跳舞":
                    StopDancing();
                    break;

                case "sing":
                case "唱歌":
                    // TODO: 需要音频资源
                    Debug.Log("[PerformanceController] 唱歌功能需要音频资源");
                    break;

                case "stopsing":
                case "stop_sing":
                case "停止唱歌":
                    StopSinging();
                    break;

                case "wave":
                case "挥手":
                    TriggerGesture("Wave");
                    break;

                case "nod":
                case "点头":
                    TriggerGesture("Nod");
                    break;

                case "shake":
                case "摇头":
                    TriggerGesture("Shake");
                    break;

                case "happy":
                case "开心":
                    SetEmotion("Happy");
                    break;

                case "sad":
                case "难过":
                    SetEmotion("Sad");
                    break;

                case "angry":
                case "生气":
                    SetEmotion("Angry");
                    break;

                default:
                    Debug.LogWarning($"[PerformanceController] 未知的动作: {action}");
                    break;
            }
        }
    }
}
