using UnityEngine;

namespace DigitalHuman.Animation
{
    /// <summary>
    /// 音频节奏分析器 - 实时检测音乐节拍和节奏
    /// 使用频谱分析和能量检测来提取节拍
    /// </summary>
    public class AudioRhythmAnalyzer : MonoBehaviour
    {
        [Header("音频源")]
        [SerializeField] private AudioSource _audioSource;

        [Header("分析设置")]
        [SerializeField] private int _sampleSize = 1024;      // FFT 采样大小
        [SerializeField] private int _fftSize = 1024;        // FFT 大小
        [SerializeField] private float _sensitivity = 0.5f;     // 检测敏感度
        [SerializeField] private float _minBeatInterval = 0.3f; // 最小节拍间隔（秒）

        [Header("频率范围")]
        // [SerializeField] private int _minFrequency = 100;      // 最小频率（低音）
        // [SerializeField] private int _maxFrequency = 15000;    // 最大频率（高音）

        [Header("输出")]
        [SerializeField] private bool _showDebug = true;
        [SerializeField] private int _historySize = 100;      // 历史记录大小

        // 私有变量
        private float[] _samples;        // 音频采样
        private float[] _spectrum;      // 频谱
        private float[] _energyHistory;  // 能量历史
        private float[] _beatHistory;   // 节拍历史

        private float _currentEnergy;   // 当前能量
        private float _averageEnergy;   // 平均能量
        private float _energyThreshold;  // 能量阈值
        private float _lastBeatTime;   // 上次检测到节拍的时间

        private float _bpm;            // 当前 BPM
        private int _beatCount;        // 节拍计数
        // private float _totalBeatTime;   // 总节拍时间

        private bool _isAnalyzing = false;

        // 事件
        public System.Action<float> OnBeat;       // 节拍事件（BPM）
        public System.Action<float> OnEnergyChange; // 能量变化事件

        void Awake()
        {
            InitializeArrays();
        }

        void Start()
        {
            if (_audioSource == null)
                _audioSource = GetComponent<AudioSource>();
        }

        void Update()
        {
            if (!_isAnalyzing) return;
            if (_audioSource == null || !_audioSource.isPlaying) return;

            AnalyzeAudio();
            DetectBeat();
            CalculateBPM();
        }

        /// <summary>
        /// 初始化数组
        /// </summary>
        private void InitializeArrays()
        {
            _samples = new float[_sampleSize];
            _spectrum = new float[_fftSize / 2];
            _energyHistory = new float[_historySize];
            _beatHistory = new float[_historySize];
        }

        /// <summary>
        /// 开始分析
        /// </summary>
        public void StartAnalyzing()
        {
            if (_audioSource == null)
            {
                Debug.LogError("[AudioRhythmAnalyzer] 没有 AudioSource！");
                return;
            }

            _isAnalyzing = true;
            _beatCount = 0;
            // _totalBeatTime = 0f;
            _lastBeatTime = Time.time;

            Debug.Log("[AudioRhythmAnalyzer] 开始分析音频节奏");
        }

        /// <summary>
        /// 停止分析
        /// </summary>
        public void StopAnalyzing()
        {
            _isAnalyzing = false;
            Debug.Log("[AudioRhythmAnalyzer] 停止分析");
        }

        /// <summary>
        /// 分析音频
        /// </summary>
        private void AnalyzeAudio()
        {
            // 获取音频采样
            _audioSource.GetOutputData(_samples, 0);

            // 计算当前能量（RMS）
            float sum = 0f;
            for (int i = 0; i < _sampleSize; i++)
            {
                sum += _samples[i] * _samples[i];
            }
            _currentEnergy = Mathf.Sqrt(sum / _sampleSize);

            // 获取频谱
            _audioSource.GetSpectrumData(_spectrum, 0, FFTWindow.BlackmanHarris);

            // 更新能量历史
            UpdateEnergyHistory();

            // 触发能量变化事件
            OnEnergyChange?.Invoke(_currentEnergy);
        }

        /// <summary>
        /// 更新能量历史
        /// </summary>
        private void UpdateEnergyHistory()
        {
            // 移动数组
            for (int i = _energyHistory.Length - 1; i > 0; i--)
            {
                _energyHistory[i] = _energyHistory[i - 1];
            }
            _energyHistory[0] = _currentEnergy;

            // 计算平均能量
            float sum = 0f;
            foreach (float energy in _energyHistory)
            {
                sum += energy;
            }
            _averageEnergy = sum / _energyHistory.Length;

            // 计算能量阈值
            _energyThreshold = _averageEnergy * (1f + _sensitivity);
        }

        /// <summary>
        /// 检测节拍
        /// </summary>
        private void DetectBeat()
        {
            float currentTime = Time.time;

            // 检查是否有足够的能量增量
            bool isBeat = _currentEnergy > _energyThreshold;

            // 检查最小间隔
            if (isBeat && currentTime - _lastBeatTime >= _minBeatInterval)
            {
                // 检测到节拍
                _lastBeatTime = currentTime;
                _beatCount++;

                // 计算 BPM
                float beatInterval = currentTime - _lastBeatTime;
                if (beatInterval > 0)
                {
                    float instantBPM = 60f / beatInterval;
                    UpdateBPMHistory(instantBPM);
                }

                // 触发节拍事件
                OnBeat?.Invoke(_bpm);

                Debug.Log($"[AudioRhythmAnalyzer] 检测到节拍！BPM: {_bpm:F1}");
            }
        }

        /// <summary>
        /// 更新 BPM 历史
        /// </summary>
        private void UpdateBPMHistory(float instantBPM)
        {
            // 移动数组
            for (int i = _beatHistory.Length - 1; i > 0; i--)
            {
                _beatHistory[i] = _beatHistory[i - 1];
            }
            _beatHistory[0] = instantBPM;

            // 计算平均 BPM
            float sum = 0f;
            int count = 0;
            foreach (float bpm in _beatHistory)
            {
                if (bpm > 0)
                {
                    sum += bpm;
                    count++;
                }
            }

            if (count > 0)
            {
                _bpm = sum / count;
            }
        }

        /// <summary>
        /// 计算 BPM（平滑）
        /// </summary>
        private void CalculateBPM()
        {
            // BPM 已经在 DetectBeat 中计算
        }

        /// <summary>
        /// 获取当前 BPM
        /// </summary>
        public float GetBPM()
        {
            return _bpm;
        }

        /// <summary>
        /// 获取当前能量
        /// </summary>
        public float GetCurrentEnergy()
        {
            return _currentEnergy;
        }

        /// <summary>
        /// 获取平均能量
        /// </summary>
        public float GetAverageEnergy()
        {
            return _averageEnergy;
        }

        /// <summary>
        /// 获取频率能量（用于低音检测）
        /// </summary>
        public float GetFrequencyEnergy(int frequency)
        {
            if (_spectrum == null) return 0f;

            // 计算频率对应的频谱索引
            int index = Mathf.FloorToInt((float)frequency / AudioSettings.outputSampleRate * _fftSize);

            if (index >= 0 && index < _spectrum.Length)
            {
                return _spectrum[index];
            }

            return 0f;
        }

        /// <summary>
        /// 获取低音能量（Bass）
        /// </summary>
        public float GetBassEnergy()
        {
            // 60-250 Hz 是低音范围
            return GetFrequencyEnergy(150);
        }

        /// <summary>
        /// 获取中音能量（Mids）
        /// </summary>
        public float GetMidEnergy()
        {
            // 250-4000 Hz 是中音范围
            return GetFrequencyEnergy(1000);
        }

        /// <summary>
        /// 获取高音能量（Highs）
        /// </summary>
        public float GetHighEnergy()
        {
            // 4000-20000 Hz 是高音范围
            return GetFrequencyEnergy(8000);
        }

        /// <summary>
        /// 获取频谱数据
        /// </summary>
        public float[] GetSpectrum()
        {
            return _spectrum;
        }

        /// <summary>
        /// 获取音频采样
        /// </summary>
        public float[] GetSamples()
        {
            return _samples;
        }

        // ==================== 调试 ====================

        void OnGUI()
        {
            if (!_showDebug || !Application.isEditor) return;

            GUILayout.BeginArea(new Rect(10, 350, 300, 250));
            GUILayout.Label("=== 音频节奏分析器 ===");
            GUILayout.Label($"分析状态: {(_isAnalyzing ? "分析中" : "停止")}");
            GUILayout.Label($"当前 BPM: {_bpm:F1}");
            GUILayout.Label($"节拍计数: {_beatCount}");
            GUILayout.Space(10);
            GUILayout.Label($"当前能量: {_currentEnergy:F4}");
            GUILayout.Label($"平均能量: {_averageEnergy:F4}");
            GUILayout.Label($"能量阈值: {_energyThreshold:F4}");

            GUILayout.Space(10);

            if (GUILayout.Button(_isAnalyzing ? "停止分析" : "开始分析"))
            {
                if (_isAnalyzing)
                    StopAnalyzing();
                else
                    StartAnalyzing();
            }

            GUILayout.Space(10);

            // 频率能量
            GUILayout.Label("频率能量:");
            GUILayout.Label($"低音 (Bass): {GetBassEnergy():F4}");
            GUILayout.Label($"中音 (Mid): {GetMidEnergy():F4}");
            GUILayout.Label($"高音 (High): {GetHighEnergy():F4}");

            GUILayout.EndArea();
        }
    }
}
