using UnityEngine;
using DigitalHuman.Core;
using DigitalHuman.Data;
using System.Threading.Tasks;

namespace DigitalHuman.Core
{
    public class ChatManager : MonoBehaviour
    {
        [SerializeField] private ChatApiSettings _apiSettings;
        [SerializeField] private AudioSource _characterAudioSource;
        [SerializeField] private AvatarLipSyncController _lipSyncController;
        [SerializeField] private AvatarAnimationController _animationController;
        [SerializeField] private MicrophoneManager _micManager;

        private LlmService _llmService;
        private TtsService _ttsService;
        private AsrService _asrService;
        private ChatNetworkClient _networkClient;

        private bool _isProcessing = false;

        void Awake()
        {
            if (_apiSettings == null)
            {
                Debug.LogError("[ChatManager] ApiSettings is not assigned!");
                return;
            }

            if (_micManager == null) _micManager = FindObjectOfType<MicrophoneManager>();
            if (_characterAudioSource == null) _characterAudioSource = GetComponent<AudioSource>();
            if (_animationController == null) _animationController = GetComponent<AvatarAnimationController>();
            
            // 检查关键组件
            if (_animationController == null)
            {
                Debug.LogError("[ChatManager] 警告：找不到 AvatarAnimationController！请确保 VRM 模型上有该组件。");
            }
            else
            {
                Debug.Log("[ChatManager] 成功找到 AvatarAnimationController");
            }

            _networkClient = new ChatNetworkClient(_apiSettings);
            _llmService = new LlmService(_apiSettings, _networkClient);
            _ttsService = new TtsService(_apiSettings, _networkClient);
            _asrService = new AsrService(_apiSettings);
            
            _llmService.SetSystemPrompt("RULE: Start EVERY response with ONE emotion tag: [Happy], [Sad], [Angry], [Surprised], or [Neutral].\n\nExample:\n[Happy] Hello! Great to see you!\n[Surprised] Wow, really? That's amazing!\n[Sad] I'm sorry to hear that.\n[Angry] That's not fair!\n[Neutral] I understand.\n\nAlways use the tag that matches your emotional response.");
        }

        public void StartUserRecording()
        {
            if (_isProcessing) return;
            Debug.Log("[ChatManager] 用户开始说话...");
            if (_characterAudioSource.isPlaying) _characterAudioSource.Stop();
            _micManager.StartRecording();
        }

        public async void StopUserRecordingAndProcess()
        {
            if (_isProcessing) return;
            _isProcessing = true;

            Debug.Log("[ChatManager] 停止录音，开始处理数据...");

            // 获取录音数据 (base64 格式，直接发给 GLM-4-Voice)
            string audioBase64 = _micManager.StopRecordingAsBase64();
            if (string.IsNullOrEmpty(audioBase64))
            {
                Debug.LogError("[ChatManager] 录音数据为空！");
                _isProcessing = false;
                return;
            }
            
            if (audioBase64.Length < 100) 
            {
                Debug.LogError("[ChatManager] 录音数据太短！请检查麦克风权限或是否实际录制到声音。");
                _isProcessing = false;
                return;
            }
            Debug.Log($"[ChatManager] 录音成功，base64 长度: {audioBase64.Length}");

            // 直接使用 GLM-4-Voice 处理音频（端到端，无需 ASR）
            await ProcessAiResponseWithAudio(audioBase64);

            _isProcessing = false;
        }

        private async Task ProcessAiResponseWithAudio(string audioBase64)
        {
            if (_lipSyncController != null) _lipSyncController.OnConversationStart();

            // AI 思考 - 直接发送音频给 GLM-4-Voice
            Debug.Log("[ChatManager] 正在请求 LLM (GLM-4-Voice) 处理音频...");
            var result = await _llmService.AskWithAudioAsync(audioBase64);
            
            if (result == null || string.IsNullOrEmpty(result.text))
            {
                Debug.LogError("[ChatManager] LLM 回复为空！");
                return;
            }
            
            Debug.Log($"[ChatManager] LLM 识别并回复文本: {result.text}");

            // 解析情绪
            string emotion = "Neutral";
            string cleanText = result.text;
            
            Debug.Log($"[ChatManager] 原始回复文本: '{result.text}'");
            Debug.Log($"[ChatManager] 是否以[开头: {result.text.StartsWith("[")}, 是否包含]: {result.text.Contains("]")}");
            
            if (result.text.StartsWith("[") && result.text.Contains("]"))
            {
                int endBracket = result.text.IndexOf("]");
                emotion = result.text.Substring(1, endBracket - 1);
                cleanText = result.text.Substring(endBracket + 1).Trim();
                Debug.Log($"[ChatManager] 从标签解析情绪: {emotion}");
            }
            else
            {
                // AI 没有返回情绪标签，从文本内容智能推断
                Debug.Log($"[ChatManager] 未检测到情绪标签，开始推断...");
                emotion = InferEmotionFromText(result.text);
                cleanText = result.text;
                Debug.Log($"[ChatManager] 从文本推断情绪: {emotion}");
            }
            Debug.Log($"[ChatManager] 最终情绪: {emotion}, 纯文本: {cleanText}");

            // 设置表情和动画
            if (_lipSyncController != null) _lipSyncController.SetExpression(emotion);
            if (_animationController != null)
            {
                _animationController.SetEmotion(emotion);
                // 根据情绪触发相应的手势
                TriggerEmotionGesture(emotion);
            }

            // 处理语音 (GLM-4-Voice 直接返回音频)
            AudioClip clip = null;
            if (!string.IsNullOrEmpty(result.audioBase64))
            {
                Debug.Log("[ChatManager] 收到集成语音数据，正在解码...");
                clip = await LoadAudioFromBase64(result.audioBase64);
            }
            else
            {
                Debug.LogError("[ChatManager] GLM-4-Voice 未返回音频数据！");
                return;
            }

            if (clip == null)
            {
                Debug.LogError("[ChatManager] 语音解码失败！");
                return;
            }

            Debug.Log("[ChatManager] 语音准备就绪，开始播放。");
            
            // 触发说话动画
            if (_animationController != null)
            {
                Debug.Log("[ChatManager] 调用动画控制器 - 开始说话");
                _animationController.OnStartSpeaking();
            }
            else
            {
                Debug.LogError("[ChatManager] AnimationController 为空！请检查 VRM 模型上是否添加了 AvatarAnimationController 组件");
            }
            
            // 播放语音并等待完成
            await PlayVoiceAsync(clip);
            
            // 播放完成后停止说话动画
            if (_animationController != null)
            {
                Debug.Log("[ChatManager] 调用动画控制器 - 停止说话");
                _animationController.OnStopSpeaking();
                
                // 表情复原到中性状态
                Debug.Log("[ChatManager] 表情复原到 Neutral");
                _animationController.SetEmotion("Neutral");
                _lipSyncController?.SetExpression("Neutral");
            }
        }

        /// <summary>
        /// 从文本内容推断情绪
        /// </summary>
        private string InferEmotionFromText(string text)
        {
            string lowerText = text.ToLower();
            Debug.Log($"[ChatManager] 推断情绪 - 原文本: '{text}', 小写: '{lowerText}'");
            
            // 开心关键词（优先检查）
            if (lowerText.Contains("开心") || lowerText.Contains("高兴") || lowerText.Contains("快乐") || 
                lowerText.Contains("great") || lowerText.Contains("happy") || lowerText.Contains("wonderful") ||
                lowerText.Contains("amazing") || lowerText.Contains("excellent") || lowerText.Contains("!"))
            {
                Debug.Log($"[ChatManager] 检测到开心关键词");
                return "Happy";
            }
            
            // 惊讶关键词
            if (lowerText.Contains("惊讶") || lowerText.Contains("震惊") || lowerText.Contains("真的吗") ||
                lowerText.Contains("wow") || lowerText.Contains("surprised") || lowerText.Contains("amazing") ||
                lowerText.Contains("unbelievable") || lowerText.Contains("?"))
            {
                Debug.Log($"[ChatManager] 检测到惊讶关键词");
                return "Surprised";
            }
            
            // 难过/悲伤关键词
            if (lowerText.Contains("难过") || lowerText.Contains("伤心") || lowerText.Contains("抱歉") ||
                lowerText.Contains("sorry") || lowerText.Contains("sad") || lowerText.Contains("unfortunately"))
            {
                Debug.Log($"[ChatManager] 检测到难过关键词");
                return "Sad";
            }
            
            // 生气关键词
            if (lowerText.Contains("生气") || lowerText.Contains("愤怒") || lowerText.Contains("不公平") ||
                lowerText.Contains("angry") || lowerText.Contains("unfair") || lowerText.Contains("wrong"))
            {
                Debug.Log($"[ChatManager] 检测到生气关键词");
                return "Angry";
            }
            
            // 默认中性
            Debug.Log($"[ChatManager] 未检测到情绪关键词，返回 Neutral");
            return "Neutral";
        }

        /// <summary>
        /// 根据情绪触发相应的手势动作
        /// </summary>
        private void TriggerEmotionGesture(string emotion)
        {
            if (_animationController == null) return;
            
            switch (emotion.ToLower())
            {
                case "happy":
                case "joy":
                    _animationController.TriggerGesture("nod"); // 开心时点头
                    break;
                case "surprised":
                case "surprise":
                    _animationController.TriggerGesture("tilt"); // 惊讶时歪头
                    break;
                case "sad":
                case "sorrow":
                    _animationController.TriggerGesture("shake"); // 难过时轻微摇头
                    break;
                case "angry":
                case "mad":
                    // 生气时不做太大动作，保持严肃
                    break;
                case "think":
                case "thinking":
                    _animationController.TriggerGesture("think"); // 思考姿势
                    break;
                default:
                    // 默认待机
                    break;
            }
        }

        private async Task<AudioClip> LoadAudioFromBase64(string base64)
        {
            try
            {
                byte[] audioBytes = System.Convert.FromBase64String(base64);
                Debug.Log($"[ChatManager] 解码后音频大小: {audioBytes.Length} bytes");

                // 打印前 20 个字节，用于判断格式
                string header = "[ChatManager] 音频文件头: ";
                for (int i = 0; i < Mathf.Min(20, audioBytes.Length); i++)
                {
                    header += audioBytes[i].ToString("X2") + " ";
                }
                Debug.Log(header);

                // 检查是否是 WAV 格式 (RIFF 头)
                if (audioBytes.Length > 4 && 
                    audioBytes[0] == 0x52 && audioBytes[1] == 0x49 && 
                    audioBytes[2] == 0x46 && audioBytes[3] == 0x46)
                {
                    Debug.Log("[ChatManager] 检测到 WAV 格式 (RIFF header)");
                }

                // 尝试多种格式加载
                AudioClip clip = await TryLoadAudio(audioBytes, AudioType.WAV);
                if (clip != null) return clip;

                clip = await TryLoadAudio(audioBytes, AudioType.MPEG);
                if (clip != null) return clip;

                clip = await TryLoadAudio(audioBytes, AudioType.OGGVORBIS);
                if (clip != null) return clip;

                // 如果都失败，尝试直接创建 PCM AudioClip
                clip = CreateAudioClipFromPCM(audioBytes);
                if (clip != null) return clip;

                Debug.LogError("[ChatManager] 所有音频格式尝试均失败");
                return null;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ChatManager] 解码语音数据异常: {e.Message}");
                return null;
            }
        }

        private async Task<AudioClip> TryLoadAudio(byte[] audioBytes, AudioType type)
        {
            string ext = type switch
            {
                AudioType.WAV => "wav",
                AudioType.MPEG => "mp3",
                AudioType.OGGVORBIS => "ogg",
                _ => "bin"
            };

            string tempPath = System.IO.Path.Combine(Application.temporaryCachePath, $"temp_voice.{ext}");
            System.IO.File.WriteAllBytes(tempPath, audioBytes);

            string url = "file://" + tempPath;
            using var request = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip(url, type);
            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                var clip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(request);
                if (clip != null && clip.length > 0)
                {
                    Debug.Log($"[ChatManager] 成功加载 {type} 格式音频，时长: {clip.length}s");
                    return clip;
                }
            }

            return null;
        }

        private AudioClip CreateAudioClipFromPCM(byte[] pcmData)
        {
            try
            {
                // 假设是 16-bit PCM, 24000Hz, 单声道（GLM-4-Voice 默认参数）
                int sampleRate = 24000;
                int channels = 1;
                int bitsPerSample = 16;

                // 计算样本数
                int totalSampleCount = pcmData.Length / (bitsPerSample / 8) / channels;
                
                // 跳过前 1 秒的样本（彻底消除开头的"di dii"杂音）
                int skipSamples = (int)(1.0f * sampleRate); // 约 24000 个样本
                if (skipSamples >= totalSampleCount) skipSamples = 0;
                
                int validSampleCount = totalSampleCount - skipSamples;

                // 转换为 float 数组
                float[] samples = new float[validSampleCount * channels];
                for (int i = 0; i < validSampleCount; i++)
                {
                    int sourceIndex = (i + skipSamples) * 2;
                    short sample = (short)(pcmData[sourceIndex] | (pcmData[sourceIndex + 1] << 8));
                    samples[i] = sample / 32768f; // 转换为 -1.0 到 1.0
                }

                AudioClip clip = AudioClip.Create("GLM4Voice", validSampleCount, channels, sampleRate, false);
                clip.SetData(samples, 0);

                Debug.Log($"[ChatManager] 成功创建 PCM AudioClip，时长: {clip.length}s, 跳过了前 {skipSamples} 个样本");
                return clip;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[ChatManager] PCM 创建失败: {e.Message}");
                return null;
            }
        }

        private async Task PlayVoiceAsync(AudioClip clip)
        {
            if (_characterAudioSource == null) return;

            // 创建新的 AudioClip，去掉前 1 秒
            AudioClip trimmedClip = TrimAudioClip(clip, 1.0f);
            if (trimmedClip == null)
            {
                trimmedClip = clip;
            }

            _characterAudioSource.clip = trimmedClip;
            _characterAudioSource.time = 0;
            _characterAudioSource.Play();

            if (_lipSyncController != null)
            {
                _lipSyncController.ConfigureForAudioSource(_characterAudioSource);
            }

            // 等待音频播放完成
            while (_characterAudioSource.isPlaying)
            {
                await Task.Yield();
            }
            
            // 清理临时创建的 clip
            if (trimmedClip != clip)
            {
                Destroy(trimmedClip);
            }
        }
        
        private AudioClip TrimAudioClip(AudioClip source, float skipSeconds)
        {
            if (source == null || source.length <= skipSeconds) return null;
            
            try
            {
                int sampleRate = source.frequency;
                int channels = source.channels;
                int skipSamples = (int)(skipSeconds * sampleRate);
                int totalSamples = source.samples;
                int newSampleCount = totalSamples - skipSamples;
                
                float[] samples = new float[totalSamples * channels];
                source.GetData(samples, 0);
                
                float[] trimmedSamples = new float[newSampleCount * channels];
                System.Array.Copy(samples, skipSamples * channels, trimmedSamples, 0, newSampleCount * channels);
                
                AudioClip newClip = AudioClip.Create("TrimmedVoice", newSampleCount, channels, sampleRate, false);
                newClip.SetData(trimmedSamples, 0);
                
                Debug.Log($"[ChatManager] 裁剪音频: 原时长 {source.length}s, 跳过前 {skipSeconds}s, 新时长 {newClip.length}s");
                return newClip;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ChatManager] 裁剪音频失败: {e.Message}");
                return null;
            }
        }

        private void PlayVoice(AudioClip clip)
        {
            if (_characterAudioSource != null)
            {
                _characterAudioSource.clip = clip;
                _characterAudioSource.Play();
                if (_lipSyncController != null) _lipSyncController.ConfigureForAudioSource(_characterAudioSource);
            }
        }
    }
}
