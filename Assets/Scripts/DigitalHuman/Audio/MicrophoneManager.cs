using UnityEngine;
using System.IO;
using System.Linq;

namespace DigitalHuman.Core
{
    public class MicrophoneManager : MonoBehaviour
    {
        [Tooltip("选择要使用的录音设备。留空则使用系统默认。")]
        [SerializeField] private string _selectedDevice = "";
        
        private AudioClip _recordingClip;

        void Awake()
        {
            UpdateDeviceList();
        }

        public void UpdateDeviceList()
        {
            string[] devices = Microphone.devices;
            if (devices.Length == 0)
            {
                Debug.LogError("[MicrophoneManager] 没有检测到任何麦克风设备！请检查系统设置。");
                return;
            }

            Debug.Log($"[MicrophoneManager] 检测到 {devices.Length} 个录音设备:");
            for (int i = 0; i < devices.Length; i++)
            {
                Debug.Log($" [{i}] {devices[i]}");
            }

            if (string.IsNullOrEmpty(_selectedDevice) || !devices.Contains(_selectedDevice))
            {
                _selectedDevice = devices[0];
                Debug.Log($"[MicrophoneManager] 默认选择设备: {_selectedDevice}");
            }
        }

        private int _recordingStartPosition = 0;

        public void StartRecording()
        {
            if (string.IsNullOrEmpty(_selectedDevice))
            {
                if (Microphone.devices.Length > 0) _selectedDevice = Microphone.devices[0];
                else
                {
                    Debug.LogError("[MicrophoneManager] 没有可用设备，无法开始录音");
                    return;
                }
            }

            if (Microphone.IsRecording(_selectedDevice)) return;

            _recordingClip = Microphone.Start(_selectedDevice, true, 30, 44100);
            
            if (_recordingClip == null)
            {
                Debug.LogError($"[MicrophoneManager] 录音启动失败! 设备: {_selectedDevice}");
                return;
            }

            // 等待 0.5 秒让麦克风稳定，避免初始化电流噪音
            _recordingStartPosition = Microphone.GetPosition(_selectedDevice);
            Debug.Log($"[MicrophoneManager] 录音开始... 设备: {_selectedDevice}, 采样率: {_recordingClip.frequency}, 初始位置: {_recordingStartPosition}");
        }

        public byte[] StopRecording()
        {
            if (!Microphone.IsRecording(_selectedDevice)) 
            {
                Debug.LogWarning("[MicrophoneManager] StopRecording called but was not recording.");
                return null;
            }

            int lastPos = Microphone.GetPosition(_selectedDevice);
            Microphone.End(_selectedDevice);
            
            // 跳过前 0.3 秒的噪音数据（约 13230 个样本 @ 44100Hz）
            int skipSamples = (int)(0.3f * 44100);
            int effectiveStartPos = _recordingStartPosition + skipSamples;
            
            Debug.Log($"[MicrophoneManager] 录音已停止, 录制位置: {lastPos}, 跳过前 {skipSamples} 个样本");

            if (lastPos <= effectiveStartPos)
            {
                Debug.LogWarning("[MicrophoneManager] 录音时间太短，可能被过滤掉了。");
                return null;
            }

            // 跳过开头的噪音数据
            return ProcessRecordingData(lastPos, effectiveStartPos);
        }

        public string StopRecordingAsBase64()
        {
            byte[] wavData = StopRecording();
            if (wavData == null) return null;
            return System.Convert.ToBase64String(wavData);
        }

        private byte[] ProcessRecordingData(int lastPos, int startPos = 0)
        {
            if (_recordingClip == null) return null;

            float[] allSamples = new float[_recordingClip.samples * _recordingClip.channels];
            _recordingClip.GetData(allSamples, 0);

            // 只截取从 startPos 到 lastPos 的部分（跳过开头的噪音）
            int actualSampleCount = (lastPos - startPos) * _recordingClip.channels;
            if (actualSampleCount <= 0)
            {
                Debug.LogWarning("[MicrophoneManager] 有效录音数据太短，可能被过滤掉了。");
                return null;
            }
            
            float[] samples = new float[actualSampleCount];
            System.Array.Copy(allSamples, startPos * _recordingClip.channels, samples, 0, actualSampleCount);

            // 打印最大音量，用于调试
            float maxVolume = 0;
            foreach (var s in samples)
            {
                if (Mathf.Abs(s) > maxVolume) maxVolume = Mathf.Abs(s);
            }
            Debug.Log($"[MicrophoneManager] 录音最大振幅: {maxVolume}, 实际样本数: {actualSampleCount}, 跳过了 {startPos} 个样本");

            return AudioToWav(samples, _recordingClip.channels, _recordingClip.frequency);
        }



        private int TrimSilenceEnd(float[] samples, float threshold)
        {
            int endIndex = samples.Length - 1;
            for (int i = samples.Length - 1; i >= 0; i--)
            {
                if (Mathf.Abs(samples[i]) > threshold)
                {
                    endIndex = i;
                    break;
                }
            }
            return endIndex + 1;
        }

        private byte[] AudioToWav(float[] samples, int channels, int sampleRate)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(new char[4] { 'R', 'I', 'F', 'F' });
                    writer.Write(36 + samples.Length * 2);
                    writer.Write(new char[4] { 'W', 'A', 'V', 'E' });
                    writer.Write(new char[4] { 'f', 'm', 't', ' ' });
                    writer.Write(16);
                    writer.Write((short)1);
                    writer.Write((short)channels);
                    writer.Write(sampleRate);
                    writer.Write(sampleRate * channels * 2);
                    writer.Write((short)(channels * 2));
                    writer.Write((short)16);
                    writer.Write(new char[4] { 'd', 'a', 't', 'a' });
                    writer.Write(samples.Length * 2);
                    foreach (var sample in samples) writer.Write((short)(sample * short.MaxValue));
                }
                return stream.ToArray();
            }
        }
    }
}
