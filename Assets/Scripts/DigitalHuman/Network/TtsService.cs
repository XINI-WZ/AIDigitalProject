using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.IO;
using DigitalHuman.Data;

namespace DigitalHuman.Core
{
    public class TtsService
    {
        private readonly ChatApiSettings _settings;
        private readonly ChatNetworkClient _networkClient;

        public TtsService(ChatApiSettings settings, ChatNetworkClient networkClient)
        {
            _settings = settings;
            _networkClient = networkClient;
        }

        public async Task<AudioClip> SynthesizeAsync(string text)
        {
            if (string.IsNullOrEmpty(_settings.ttsUrl)) return null;

            // 1. 优先尝试智谱 GLM-TTS
            if (_settings.ttsUrl.Contains("bigmodel.cn"))
            {
                var requestData = new ZhipuTtsRequest
                {
                    model = "glm-tts",
                    input = text,
                    voice = string.IsNullOrEmpty(_settings.ttsVoice) ? "puck" : _settings.ttsVoice,
                    response_format = "wav",
                    stream = false
                };

                string json = JsonUtility.ToJson(requestData);
                using var request = new UnityWebRequest(_settings.ttsUrl, "POST");
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
                request.downloadHandler = new DownloadHandlerAudioClip(_settings.ttsUrl, AudioType.WAV);
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", $"Bearer {_settings.llmApiKey}");

                var op = request.SendWebRequest();
                while (!op.isDone) await Task.Yield();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    return DownloadHandlerAudioClip.GetContent(request);
                }

                Debug.LogWarning($"[TtsService] 智谱 TTS 失败 (可能是 429 限流)，正在尝试百度免费备选方案... 错误: {request.error}");
            }

            // 2. 备选方案：百度公共 TTS (在国内极其稳定，不限流)
            string baiduUrl = $"https://tts.baidu.com/text2audio?lan=zh&ie=UTF-8&spd=5&text={UnityWebRequest.EscapeURL(text)}";
            
            // 使用 DownloadHandlerBuffer 先下载原始字节
            using var baiduReq = new UnityWebRequest(baiduUrl, "GET");
            baiduReq.downloadHandler = new DownloadHandlerBuffer();
            
            var baiduOp = baiduReq.SendWebRequest();
            while (!baiduOp.isDone) await Task.Yield();

            if (baiduReq.result == UnityWebRequest.Result.Success)
            {
                byte[] audioData = baiduReq.downloadHandler.data;
                if (audioData != null && audioData.Length > 0)
                {
                    // 保存为临时文件再加载，解决 FMOD 格式兼容问题
                    string tempPath = Path.Combine(Application.temporaryCachePath, "baidu_tts.mp3");
                    File.WriteAllBytes(tempPath, audioData);
                    
                    Debug.Log("[TtsService] 百度备选 TTS 成功，正在加载音频...");
                    return await LoadAudioFromFile(tempPath);
                }
            }

            Debug.LogError("[TtsService] 所有 TTS 尝试均失败");
            return null;
        }

        [System.Serializable]
        private class ZhipuTtsRequest
        {
            public string model;
            public string input;
            public string voice;
            public string response_format;
            public bool stream;
        }

        private async Task<AudioClip> LoadAudioFromFile(string filePath)
        {
            string url = "file://" + filePath;
            using var request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG);
            
            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[TtsService] Failed to load audio clip: {request.error}");
                return null;
            }

            return DownloadHandlerAudioClip.GetContent(request);
        }
    }
}
