using UnityEngine;

namespace DigitalHuman.Animation
{
    /// <summary>
    /// AI 舞蹈生成器 - 使用算法和规则自动生成舞蹈动作
    /// 不依赖预设动画，基于数学函数、噪声、节奏等生成
    /// </summary>
    public class AIDanceGenerator : MonoBehaviour
    {
        [Header("组件引用")]
        [SerializeField] private ProceduralDanceGenerator _proceduralDance;
        [SerializeField] private AudioRhythmAnalyzer _rhythmAnalyzer;

        [Header("AI 生成设置")]
        [SerializeField] private bool _enableAIGeneration = true;
        [SerializeField] private float _generationInterval = 0.5f;     // 动作生成间隔
        [SerializeField] private float _transitionSpeed = 0.2f;        // 动作过渡速度

        [Header("多样性设置")]
        [SerializeField] private bool _enableStyleMixing = true;     // 启用风格混合
        [SerializeField] private bool _enableRandomGestures = true;   // 启用随机手势
        [SerializeField] private float _gestureProbability = 0.1f;   // 手势概率

        [Header("学习模式")]
        [SerializeField] private bool _enableLearning = true;         // 启用学习模式
        [SerializeField] private float _learningRate = 0.01f;          // 学习率
        [SerializeField] private int _maxMemorySize = 100;            // 最大记忆大小

        [Header("噪声设置")]
        [SerializeField] private bool _enableNoise = true;            // 启用噪声
        [SerializeField] private float _noiseAmplitude = 0.1f;         // 噪声幅度

        [Header("调试")]
        [SerializeField] private bool _showDebug = true;

        // 当前状态
        private DanceStyle _currentStyle;
        private float _currentIntensity;
        private float _currentSpeed;

        // AI 记忆
        private DanceMove[] _moveMemory;
        private int _memoryIndex;
        private float[] _styleWeights;

        // 生成计时器
        private float _lastGenerationTime;

        void Awake()
        {
            InitializeMemory();
            InitializeStyleWeights();
        }

        void Start()
        {
            if (_proceduralDance == null)
                _proceduralDance = GetComponent<ProceduralDanceGenerator>();
            if (_rhythmAnalyzer == null)
                _rhythmAnalyzer = GetComponent<AudioRhythmAnalyzer>();

            // 订阅事件
            if (_rhythmAnalyzer != null)
            {
                _rhythmAnalyzer.OnBeat += OnBeat;
                _rhythmAnalyzer.OnEnergyChange += OnEnergyChange;
            }
        }

        void Update()
        {
            if (!_enableAIGeneration) return;

            // 定期生成新动作
            if (Time.time - _lastGenerationTime >= _generationInterval)
            {
                GenerateNextMove();
                _lastGenerationTime = Time.time;
            }

            // 应用噪声
            if (_enableNoise)
            {
                ApplyNoise();
            }

            // 随机手势
            if (_enableRandomGestures)
            {
                HandleRandomGestures();
            }
        }

        /// <summary>
        /// 初始化记忆
        /// </summary>
        private void InitializeMemory()
        {
            _moveMemory = new DanceMove[_maxMemorySize];
            _memoryIndex = 0;
        }

        /// <summary>
        /// 初始化风格权重
        /// </summary>
        private void InitializeStyleWeights()
        {
            _styleWeights = new float[System.Enum.GetValues(typeof(DanceStyle)).Length];

            // 初始权重均匀
            for (int i = 0; i < _styleWeights.Length; i++)
            {
                _styleWeights[i] = 1f;
            }
        }

        /// <summary>
        /// 节拍事件
        /// </summary>
        private void OnBeat(float bpm)
        {
            // 在节拍时调整舞蹈参数
            AdjustDanceParameters(bpm);

            // 记录节拍用于学习
            if (_enableLearning)
            {
                LearnFromBeat(bpm);
            }
        }

        /// <summary>
        /// 能量变化事件
        /// </summary>
        private void OnEnergyChange(float energy)
        {
            // 根据能量调整舞蹈强度
            _currentIntensity = Mathf.Lerp(_currentIntensity, energy, _learningRate);

            if (_proceduralDance != null)
            {
                _proceduralDance.SetDanceIntensity(_currentIntensity);
            }
        }

        /// <summary>
        /// 调整舞蹈参数
        /// </summary>
        private void AdjustDanceParameters(float bpm)
        {
            if (_proceduralDance == null) return;

            // 根据 BPM 调整舞蹈速度
            float targetSpeed = bpm / 120f; // 120 BPM 为基准
            _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, _transitionSpeed);

            _proceduralDance.SetBPM(bpm);
        }

        /// <summary>
        /// 生成下一个动作
        /// </summary>
        private void GenerateNextMove()
        {
            // 决定是否切换风格
            if (_enableStyleMixing)
            {
                MaybeChangeStyle();
            }

            // 生成新动作
            DanceMove newMove = GenerateMove();

            // 记录到记忆
            RecordMove(newMove);

            // 应用到程序化舞蹈
            // 注意：这里是示例，实际需要通过 ProceduralDanceGenerator 的接口应用
        }

        /// <summary>
        /// 决定是否切换风格
        /// </summary>
        private void MaybeChangeStyle()
        {
            // 根据风格权重随机选择新风格
            float random = Random.value;
            float cumulative = 0f;

            int newStyleIndex = 0;
            for (int i = 0; i < _styleWeights.Length; i++)
            {
                cumulative += _styleWeights[i] / _styleWeights.Length;
                if (random < cumulative)
                {
                    newStyleIndex = i;
                    break;
                }
            }

            DanceStyle newStyle = (DanceStyle)newStyleIndex;

            // 如果风格不同，切换
            if (newStyle != _currentStyle)
            {
                _currentStyle = newStyle;

                if (_proceduralDance != null)
                {
                    _proceduralDance.SetDanceStyle(newStyle);
                }

                Debug.Log($"[AIDanceGenerator] 切换到风格: {newStyle}");
            }
        }

        /// <summary>
        /// 生成动作
        /// </summary>
        private DanceMove GenerateMove()
        {
            DanceMove move = new DanceMove();

            // 使用噪声和算法生成新动作
            move.hipsPosition = new Vector3(
                (Random.value - 0.5f) * 0.05f * _noiseAmplitude,
                (Random.value - 0.5f) * 0.05f * _noiseAmplitude,
                (Random.value - 0.5f) * 0.05f * _noiseAmplitude
            );

            move.hipsRotation = new Vector3(
                (Random.value - 0.5f) * 10f * _noiseAmplitude,
                (Random.value - 0.5f) * 10f * _noiseAmplitude,
                (Random.value - 0.5f) * 10f * _noiseAmplitude
            );

            // 添加更多动作参数...

            return move;
        }

        /// <summary>
        /// 记录动作到记忆
        /// </summary>
        private void RecordMove(DanceMove move)
        {
            _moveMemory[_memoryIndex] = move;
            _memoryIndex = (_memoryIndex + 1) % _maxMemorySize;
        }

        /// <summary>
        /// 从节拍学习
        /// </summary>
        private void LearnFromBeat(float bpm)
        {
            // 根据节拍强度调整风格权重
            float rhythmIntensity = _rhythmAnalyzer.GetCurrentEnergy();

            if (rhythmIntensity > _rhythmAnalyzer.GetAverageEnergy() * 1.5f)
            {
                // 高能量，增加 HipHop 权重
                _styleWeights[(int)DanceStyle.HipHop] += _learningRate;
            }
            else if (rhythmIntensity < _rhythmAnalyzer.GetAverageEnergy() * 0.7f)
            {
                // 低能量，增加 Ballet 权重
                _styleWeights[(int)DanceStyle.Ballet] += _learningRate;
            }

            // 归一化权重
            NormalizeStyleWeights();
        }

        /// <summary>
        /// 归一化风格权重
        /// </summary>
        private void NormalizeStyleWeights()
        {
            float sum = 0f;
            foreach (float weight in _styleWeights)
            {
                sum += weight;
            }

            if (sum > 0)
            {
                for (int i = 0; i < _styleWeights.Length; i++)
                {
                    _styleWeights[i] /= sum;
                }
            }
        }

        /// <summary>
        /// 应用噪声
        /// </summary>
        private void ApplyNoise()
        {
            // 在现有动作上添加噪声
            // 这里可以通过修改 ProceduralDanceGenerator 的参数来实现
        }

        /// <summary>
        /// 处理随机手势
        /// </summary>
        private void HandleRandomGestures()
        {
            if (Random.value < _gestureProbability * Time.deltaTime)
            {
                // 触发随机手势
                // 这里可以通过 AvatarAnimationController 来触发
            }
        }

        /// <summary>
        /// 开始 AI 生成
        /// </summary>
        public void StartAIGeneration(DanceStyle initialStyle = DanceStyle.HipHop)
        {
            _currentStyle = initialStyle;
            _enableAIGeneration = true;

            if (_proceduralDance != null)
            {
                _proceduralDance.StartDancing(initialStyle);
            }

            if (_rhythmAnalyzer != null)
            {
                _rhythmAnalyzer.StartAnalyzing();
            }

            Debug.Log($"[AIDanceGenerator] 开始 AI 生成，初始风格: {initialStyle}");
        }

        /// <summary>
        /// 停止 AI 生成
        /// </summary>
        public void StopAIGeneration()
        {
            _enableAIGeneration = false;

            if (_proceduralDance != null)
            {
                _proceduralDance.StopDancing();
            }

            if (_rhythmAnalyzer != null)
            {
                _rhythmAnalyzer.StopAnalyzing();
            }

            Debug.Log("[AIDanceGenerator] 停止 AI 生成");
        }

        /// <summary>
        /// 设置舞蹈风格
        /// </summary>
        public void SetDanceStyle(DanceStyle style)
        {
            _currentStyle = style;
            _proceduralDance?.SetDanceStyle(style);
        }

        /// <summary>
        /// 获取当前风格
        /// </summary>
        public DanceStyle GetCurrentStyle()
        {
            return _currentStyle;
        }

        /// <summary>
        /// 获取风格权重
        /// </summary>
        public float[] GetStyleWeights()
        {
            return _styleWeights;
        }

        // ==================== 调试 ====================

        void OnGUI()
        {
            if (!_showDebug || !Application.isEditor) return;

            GUILayout.BeginArea(new Rect(10, 600, 300, 250));
            GUILayout.Label("=== AI 舞蹈生成器 ===");
            GUILayout.Label($"生成状态: {(_enableAIGeneration ? "生成中" : "停止")}");
            GUILayout.Label($"当前风格: {_currentStyle}");
            GUILayout.Label($"舞蹈强度: {_currentIntensity:F2}");
            GUILayout.Label($"舞蹈速度: {_currentSpeed:F2}");

            GUILayout.Space(10);

            if (GUILayout.Button(_enableAIGeneration ? "停止生成" : "开始生成"))
            {
                if (_enableAIGeneration)
                    StopAIGeneration();
                else
                    StartAIGeneration();
            }

            GUILayout.Space(10);

            // 风格权重
            GUILayout.Label("风格权重:");
            foreach (DanceStyle style in System.Enum.GetValues(typeof(DanceStyle)))
            {
                int index = (int)style;
                GUILayout.Label($"{style}: {_styleWeights[index]:F3}");
            }

            GUILayout.EndArea();
        }
    }
}
