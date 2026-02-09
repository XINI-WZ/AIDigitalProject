using DigitalHuman.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace DigitalHuman.UI
{
    /// <summary>
    /// 语音唤醒状态指示器 - 使用 UI Toolkit
    /// </summary>
    public class VoiceWakeUpIndicator : MonoBehaviour
    {
        [SerializeField] private VoiceActivityDetector _voiceActivityDetector;
        
        void Awake()
        {
            if (_voiceActivityDetector == null)
            {
                _voiceActivityDetector = FindObjectOfType<VoiceActivityDetector>();
            }
        }
        
        private VisualElement _root;
        private VisualElement _listeningIndicator;
        private Label _statusLabel;

        void OnEnable()
        {
            var uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                Debug.LogError("[VoiceWakeUpIndicator] 缺少 UIDocument 组件！");
                return;
            }
            _root = uiDocument.rootVisualElement;

            // 获取 UI 元素 (定义在 DigitalHumanChat.uxml 中)
            _listeningIndicator = _root.Q<VisualElement>("ListeningIndicator");
            _statusLabel = _root.Q<Label>("WakeUpStatusLabel");

            if (_listeningIndicator == null || _statusLabel == null)
            {
                Debug.LogWarning("[VoiceWakeUpIndicator] 未找到 ListeningIndicator 或 WakeUpStatusLabel 元素，请检查 UXML。");
                return;
            }

            // 订阅事件
            if (_voiceActivityDetector != null)
            {
                _voiceActivityDetector.OnSpeechStarted += OnSpeechStarted;
                _voiceActivityDetector.OnSpeechEnded += OnSpeechEnded;
            }
            
            // 初始化状态
            SetListeningState();
        }

        void OnDisable()
        {
            if (_voiceActivityDetector != null)
            {
                _voiceActivityDetector.OnSpeechStarted -= OnSpeechStarted;
                _voiceActivityDetector.OnSpeechEnded -= OnSpeechEnded;
            }
        }

        private void OnSpeechStarted()
        {
            if (_listeningIndicator == null) return;
            
            _listeningIndicator.RemoveFromClassList("processing");
            _listeningIndicator.AddToClassList("active"); // Red
            _statusLabel.text = "🎤 正在听您说话...";
        }

        private void OnSpeechEnded(byte[] audioData)
        {
            if (_listeningIndicator == null) return;
            
            _listeningIndicator.RemoveFromClassList("active");
            _listeningIndicator.AddToClassList("processing"); // Yellow
            _statusLabel.text = "🤔 思考中...";
        }

        void Update()
        {
            // 可以在这里添加动画效果，比如指示器脉冲
        }
        
        /// <summary>
        /// 设置回监听状态（当 AI 回复完成后调用）
        /// </summary>
        public void SetListeningState()
        {
            if (_listeningIndicator == null) return;
            
            _listeningIndicator.RemoveFromClassList("active");
            _listeningIndicator.RemoveFromClassList("processing");
            // 默认状态是绿色 (base class .listening-indicator)
            
            _statusLabel.text = "👂 正在监听...";
        }
    }
}
