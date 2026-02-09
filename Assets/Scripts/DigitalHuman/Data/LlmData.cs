using System;
using System.Collections.Generic;

namespace DigitalHuman.Data
{
    [Serializable]
    public class LlmMessage
    {
        public string role;
        public string content; // 简单文本模式（兼容旧模型）
        public List<LlmContentItem> content_list; // 多模态模式（glm-4-voice 需要）
        public LlmAudio audio; // for glm-4-voice response
    }

    [Serializable]
    public class LlmContentItem
    {
        public string type; // "text" 或 "audio_url"
        public string text; // 当 type 为 text 时
        public LlmAudioUrl audio_url; // 当 type 为 audio_url 时
    }

    [Serializable]
    public class LlmAudioUrl
    {
        public string url; // base64 编码的音频数据
    }

    [Serializable]
    public class LlmAudio
    {
        public string id;
        public string data; // base64 encoded audio
        public string transcript;
        public string voice;
        public string format;
    }

    [Serializable]
    public class LlmRequest
    {
        public string model;
        public List<LlmMessage> messages;
        public List<string> modalities;
        public LlmAudioConfig audio;
    }

    [Serializable]
    public class LlmAudioConfig
    {
        public string voice;
    }

    [Serializable]
    public class LlmResponse
    {
        public LlmChoice[] choices;
    }

    [Serializable]
    public class LlmChoice
    {
        public LlmMessage message;
    }
}
