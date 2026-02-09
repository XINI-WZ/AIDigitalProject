using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DigitalHuman.Core
{
    /// <summary>
    /// 语音活动检测 (VAD) - 自动检测用户何时开始/停止说话
    /// </summary>
    public class VoiceActivityDetector : MonoBehaviour
    {
        [Header("检测设置")]
        [Tooltip("声音阈值，超过此值认为开始说话")]
        [Range(0.001f, 0.1f)]
        [SerializeField] private float _speechThreshold = 0.02f;
        
        [Tooltip("开始说话前的缓冲时间（秒）")]
        [SerializeField] private float _speechStartDelay = 0.3f;
        
        [Tooltip("停止说话后的缓冲时间（秒）")]
        [SerializeField] private float _speechEndDelay = 1.0f;
        
        [Tooltip("最大录音时长（秒）")]
        [SerializeField] private float _maxRecordingDuration = 30f;

        [Header("麦克风设置")]
        [SerializeField] private string _selectedDevice = "";
        [SerializeField] private int _sampleRate = 44100;
        
        [Header("启用控制")]
        [Tooltip("是否启用语音唤醒功能。如果禁用，则不会自动监听麦克风。")]
        [SerializeField] private bool _enableVoiceWakeUp = false;

        // 事件
        public System.Action OnSpeechStarted;      // 开始说话
        public System.Action<byte[]> OnSpeechEnded; // 停止说话，返回音频数据
        public System.Action<float> OnVolumeChanged; // 音量变化（用于UI显示）

        // 状态
        private AudioClip _recordingClip;
        private bool _isRecording = false;
        private bool _isSpeechDetected = false;
        private float _currentVolume = 0f;
        private float _silenceTimer = 0f;
        private float _speechTimer = 0f;
        private float _recordingStartTime = 0f;
        
        // 音频样本
        private float[] _samples;
        private int _lastPosition = 0;

        void Start()
        {
            // 只有在启用了语音唤醒功能时才自动开始监听
            if (_enableVoiceWakeUp)
            {
                InitializeMicrophone();
                StartContinuousListening();
                Debug.Log("[VoiceActivityDetector] 语音唤醒功能已启用，开始持续监听...");
            }
            else
            {
                Debug.Log("[VoiceActivityDetector] 语音唤醒功能已禁用。如需启用，请在 Inspector 中勾选 'Enable Voice Wake Up'。");
            }
        }

        void Update()
        {
            if (_isRecording)
            {
                AnalyzeAudio();
            }
        }

        void OnDestroy()
        {
            StopRecording();
        }

        private void InitializeMicrophone()
        {
            string[] devices = Microphone.devices;
            if (devices.Length == 0)
            {
                Debug.LogError("[VoiceActivityDetector] 没有检测到麦克风设备！");
                return;
            }

            if (string.IsNullOrEmpty(_selectedDevice))
            {
                _selectedDevice = devices[0];
            }

            _samples = new float[_sampleRate / 10]; // 0.1秒的样本
            
            Debug.Log($"[VoiceActivityDetector] 初始化麦克风: {_selectedDevice}");
        }

        private void StartContinuousListening()
        {
            if (Microphone.IsRecording(_selectedDevice)) return;

            // 开始循环录音（用于实时检测）
            _recordingClip = Microphone.Start(_selectedDevice, true, 300, _sampleRate);
            _isRecording = true;
            _lastPosition = 0;
            
            Debug.Log("[VoiceActivityDetector] 开始持续监听...");
        }

        private void AnalyzeAudio()
        {
            int currentPosition = Microphone.GetPosition(_selectedDevice);
            if (currentPosition == _lastPosition) return;

            // 获取音频数据
            _recordingClip.GetData(_samples, 0);
            
            // 计算音量（RMS）
            float sum = 0f;
            int sampleCount = 0;
            
            for (int i = 0; i < _samples.Length; i++)
            {
                if (_samples[i] != 0)
                {
                    sum += _samples[i] * _samples[i];
                    sampleCount++;
                }
            }

            if (sampleCount > 0)
            {
                _currentVolume = Mathf.Sqrt(sum / sampleCount);
                OnVolumeChanged?.Invoke(_currentVolume);
            }

            // 语音活动检测逻辑
            if (!_isSpeechDetected)
            {
                // 检测是否开始说话
                if (_currentVolume > _speechThreshold)
                {
                    _speechTimer += Time.deltaTime;
                    if (_speechTimer >= _speechStartDelay)
                    {
                        StartSpeech();
                    }
                }
                else
                {
                    _speechTimer = Mathf.Max(0, _speechTimer - Time.deltaTime * 0.5f);
                }
            }
            else
            {
                // 检测是否停止说话
                if (_currentVolume < _speechThreshold * 0.6f) // 停止阈值略低
                {
                    _silenceTimer += Time.deltaTime;
                    if (_silenceTimer >= _speechEndDelay)
                    {
                        EndSpeech();
                    }
                }
                else
                {
                    _silenceTimer = 0f;
                }

                // 检查最大录音时长
                if (Time.time - _recordingStartTime > _maxRecordingDuration)
                {
                    Debug.Log("[VoiceActivityDetector] 达到最大录音时长，自动结束");
                    EndSpeech();
                }
            }

            _lastPosition = currentPosition;
        }

        private void StartSpeech()
        {
            _isSpeechDetected = true;
            _recordingStartTime = Time.time;
            _silenceTimer = 0f;
            
            Debug.Log("[VoiceActivityDetector] 🎤 检测到语音开始！");
            OnSpeechStarted?.Invoke();
        }

        private void EndSpeech()
        {
            if (!_isSpeechDetected) return;

            _isSpeechDetected = false;
            _speechTimer = 0f;
            _silenceTimer = 0f;

            // 提取录音数据
            byte[] audioData = ExtractRecordingData();
            
            if (audioData != null && audioData.Length > 1000)
            {
                Debug.Log($"[VoiceActivityDetector] ✓ 语音结束，数据大小: {audioData.Length} bytes");
                OnSpeechEnded?.Invoke(audioData);
            }
            else
            {
                Debug.LogWarning("[VoiceActivityDetector] 录音数据太短，忽略");
            }
        }

        private byte[] ExtractRecordingData()
        {
            int currentPosition = Microphone.GetPosition(_selectedDevice);
            if (currentPosition <= 0) return null;

            float[] allSamples = new float[_recordingClip.samples * _recordingClip.channels];
            _recordingClip.GetData(allSamples, 0);

            // 计算录音时长（从 _recordingStartTime 到现在）
            float recordingDuration = Time.time - _recordingStartTime;
            int sampleCount = Mathf.Min((int)(recordingDuration * _sampleRate * _recordingClip.channels), allSamples.Length);

            if (sampleCount <= 0) return null;

            float[] samples = new float[sampleCount];
            System.Array.Copy(allSamples, samples, sampleCount);

            return ConvertToWav(samples, _recordingClip.channels, _sampleRate);
        }

        private byte[] ConvertToWav(float[] samples, int channels, int sampleRate)
        {
            using (var stream = new System.IO.MemoryStream())
            {
                using (var writer = new System.IO.BinaryWriter(stream))
                {
                    // RIFF header
                    writer.Write(new char[4] { 'R', 'I', 'F', 'F' });
                    writer.Write(36 + samples.Length * 2);
                    writer.Write(new char[4] { 'W', 'A', 'V', 'E' });
                    
                    // fmt chunk
                    writer.Write(new char[4] { 'f', 'm', 't', ' ' });
                    writer.Write(16);
                    writer.Write((short)1); // PCM
                    writer.Write((short)channels);
                    writer.Write(sampleRate);
                    writer.Write(sampleRate * channels * 2);
                    writer.Write((short)(channels * 2));
                    writer.Write((short)16);
                    
                    // data chunk
                    writer.Write(new char[4] { 'd', 'a', 't', 'a' });
                    writer.Write(samples.Length * 2);
                    
                    foreach (var sample in samples)
                    {
                        writer.Write((short)(sample * short.MaxValue));
                    }
                }
                return stream.ToArray();
            }
        }

        public void StopRecording()
        {
            if (Microphone.IsRecording(_selectedDevice))
            {
                Microphone.End(_selectedDevice);
            }
            _isRecording = false;
            _isSpeechDetected = false;
        }

        // 调试：在 Inspector 中实时显示音量
        void OnGUI()
        {
            if (Application.isEditor)
            {
                GUILayout.BeginArea(new Rect(Screen.width - 210, 10, 200, 100));
                GUILayout.Label("=== VAD 状态 ===");
                GUILayout.Label($"音量: {_currentVolume:F4}");
                GUILayout.Label($"检测状态: {(_isSpeechDetected ? "🎤 说话中" : "👂 监听中")}");
                
                // 音量条
                GUILayout.HorizontalSlider(_currentVolume, 0, 0.1f);
                GUILayout.EndArea();
            }
        }
    }
}
