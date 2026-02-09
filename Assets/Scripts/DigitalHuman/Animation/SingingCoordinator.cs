using UnityEngine;
using DigitalHuman.Core;

namespace DigitalHuman.Animation
{
    /// <summary>
    /// 唱歌表情协调器 - 协调口型、表情、手势和舞蹈
    /// </summary>
    public class SingingCoordinator : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private AvatarAnimationController _animController;
        [SerializeField] private AvatarLipSyncController _lipSyncController;
        [SerializeField] private DancingManager _dancingManager;

        [Header("设置")]
        [SerializeField] private bool _enableAutoExpression = true;
        [SerializeField] private float _expressionChangeInterval = 3f;
        [SerializeField] private float _gestureProbability = 0.1f; // 每帧触发表情手势的概率

        [Header("唱歌情感配置")]
        [SerializeField] private SingingEmotionConfig[] _emotionConfigs;

        private AudioSource _audioSource;
        private bool _isSinging = false;
        private string _currentEmotion = "Happy";
        private float _lastExpressionChangeTime;
        private float _songProgress = 0f;

        public bool IsSinging => _isSinging;
        public string CurrentEmotion => _currentEmotion;

        void Awake()
        {
            if (_animController == null) _animController = GetComponent<AvatarAnimationController>();
            if (_lipSyncController == null) _lipSyncController = GetComponent<AvatarLipSyncController>();
            if (_dancingManager == null) _dancingManager = GetComponent<DancingManager>();
            _audioSource = GetComponent<AudioSource>();
        }

        void Update()
        {
            if (!_isSinging) return;

            // 更新歌曲进度
            if (_audioSource != null && _audioSource.isPlaying)
            {
                _songProgress = _audioSource.time / _audioSource.clip.length;
            }

            // 自动切换表情
            if (_enableAutoExpression && Time.time - _lastExpressionChangeTime > _expressionChangeInterval)
            {
                TryChangeExpression();
                _lastExpressionChangeTime = Time.time;
            }

            // 随机触发手势
            if (Random.value < _gestureProbability * Time.deltaTime)
            {
                TriggerSingingGesture();
            }
        }

        /// <summary>
        /// 开始唱歌
        /// </summary>
        public void StartSinging(AudioClip songClip, string emotion = "Happy")
        {
            if (songClip == null)
            {
                Debug.LogError("[SingingCoordinator] 歌曲片段为空！");
                return;
            }

            Debug.Log($"[SingingCoordinator] 开始唱歌: {songClip.name}, 情感: {emotion}");

            _currentEmotion = emotion;
            _isSinging = true;
            _songProgress = 0f;
            _lastExpressionChangeTime = Time.time;

            // 播放歌曲
            if (_audioSource != null)
            {
                _audioSource.clip = songClip;
                _audioSource.Play();

                // 配置口型同步
                if (_lipSyncController != null)
                {
                    _lipSyncController.ConfigureForAudioSource(_audioSource);
                }
            }

            // 设置表情
            SetEmotionForSinging(emotion);

            // 开始舞蹈
            if (_dancingManager != null)
            {
                _dancingManager.StartSinging(songClip, emotion);
            }

            // 通知动画控制器
            if (_animController != null)
            {
                _animController.OnStartSpeaking();
            }
        }

        /// <summary>
        /// 停止唱歌
        /// </summary>
        public void StopSinging()
        {
            if (!_isSinging) return;

            Debug.Log("[SingingCoordinator] 停止唱歌");

            _isSinging = false;

            // 停止音频
            if (_audioSource != null)
            {
                _audioSource.Stop();
            }

            // 停止舞蹈
            if (_dancingManager != null)
            {
                _dancingManager.StopSinging();
            }

            // 恢复中性表情
            if (_animController != null)
            {
                _animController.SetEmotion("Neutral");
                _animController.OnStopSpeaking();
            }
        }

        /// <summary>
        /// 设置唱歌时的表情
        /// </summary>
        private void SetEmotionForSinging(string emotion)
        {
            // 查找情感配置
            SingingEmotionConfig config = FindEmotionConfig(emotion);

            if (config != null && _animController != null)
            {
                _animController.SetEmotion(config.baseEmotion);

                // 随机触发一个辅助表情
                if (config.alternativeEmotions.Length > 0 && Random.value < 0.3f)
                {
                    string altEmotion = config.alternativeEmotions[Random.Range(0, config.alternativeEmotions.Length)];
                    _animController.SetEmotion(altEmotion);
                }
            }
        }

        /// <summary>
        /// 尝试切换表情（基于歌曲进度）
        /// </summary>
        private void TryChangeExpression()
        {
            SingingEmotionConfig config = FindEmotionConfig(_currentEmotion);

            if (config == null || config.alternativeEmotions.Length == 0) return;

            // 基于歌曲进度和随机性切换表情
            float progressSegment = 1f / (config.alternativeEmotions.Length + 1);
            int segment = Mathf.FloorToInt(_songProgress / progressSegment);

            if (segment >= 0 && segment < config.alternativeEmotions.Length)
            {
                string newEmotion = config.alternativeEmotions[segment];
                if (_animController != null)
                {
                    _animController.SetEmotion(newEmotion);
                    Debug.Log($"[SingingCoordinator] 切换表情: {newEmotion} (进度: {_songProgress:P0})");
                }
            }
        }

        /// <summary>
        /// 触发唱歌手势
        /// </summary>
        private void TriggerSingingGesture()
        {
            if (_animController == null) return;

            // 根据情感选择手势
            SingingEmotionConfig config = FindEmotionConfig(_currentEmotion);
            if (config == null || config.gestures.Length == 0) return;

            string gesture = config.gestures[Random.Range(0, config.gestures.Length)];
            _animController.TriggerGesture(gesture);

            Debug.Log($"[SingingCoordinator] 触发手势: {gesture}");
        }

        /// <summary>
        /// 查找情感配置
        /// </summary>
        private SingingEmotionConfig FindEmotionConfig(string emotion)
        {
            if (_emotionConfigs == null || _emotionConfigs.Length == 0) return null;

            foreach (var config in _emotionConfigs)
            {
                if (config.emotionName.ToLower() == emotion.ToLower())
                {
                    return config;
                }
            }

            // 未找到配置，返回第一个作为默认
            return _emotionConfigs[0];
        }

        /// <summary>
        /// 改变情感（用于歌曲中间的情感转换）
        /// </summary>
        public void ChangeEmotion(string newEmotion)
        {
            if (!_isSinging) return;

            _currentEmotion = newEmotion;
            SetEmotionForSinging(newEmotion);

            // 更新舞蹈风格
            if (_dancingManager != null)
            {
                _dancingManager.ChangeDanceByEmotion(newEmotion);
            }
        }

        void OnGUI()
        {
            if (!Application.isEditor) return;

            GUILayout.BeginArea(new Rect(10, 200, 250, 200));
            GUILayout.Label("=== 唱歌系统 ===");
            GUILayout.Label($"唱歌状态: {(_isSinging ? "唱歌中" : "停止")}");
            GUILayout.Label($"当前情感: {_currentEmotion}");
            GUILayout.Label($"歌曲进度: {_songProgress:P0}");

            GUILayout.Space(10);

            if (GUILayout.Button("测试：快乐歌曲"))
            {
                TestSing("Happy");
            }

            if (GUILayout.Button("测试：伤感歌曲"))
            {
                TestSing("Sad");
            }

            if (GUILayout.Button("停止"))
            {
                StopSinging();
            }

            GUILayout.EndArea();
        }

        private void TestSing(string emotion)
        {
            // TODO: 添加测试歌曲片段
            Debug.Log($"[SingingCoordinator] 测试唱歌: {emotion}");
        }
    }

    /// <summary>
    /// 唱歌情感配置
    /// </summary>
    [System.Serializable]
    public class SingingEmotionConfig
    {
        [Tooltip("情感名称")]
        public string emotionName;

        [Tooltip("基础表情")]
        public string baseEmotion;

        [Tooltip("辅助表情（基于歌曲进度切换）")]
        public string[] alternativeEmotions = new string[0];

        [Tooltip("可触发手势列表")]
        public string[] gestures = new string[] { "Nod", "Wave", "Tilt" };

        [Tooltip("手势触发概率倍率")]
        [Range(0.1f, 5f)]
        public float gestureProbabilityMultiplier = 1f;
    }
}
