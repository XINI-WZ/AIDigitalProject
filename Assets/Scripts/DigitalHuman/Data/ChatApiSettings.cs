using UnityEngine;

namespace DigitalHuman.Data
{
    [CreateAssetMenu(fileName = "ChatApiSettings", menuName = "DigitalHuman/ApiSettings")]
    public class ChatApiSettings : ScriptableObject
    {
        [Header("LLM Settings")]
        public string llmUrl = "https://api.openai.com/v1/chat/completions";
        public string llmApiKey = "";
        public string llmModel = "gpt-3.5-turbo";

        [Header("TTS Settings")]
        public string ttsUrl = "https://open.bigmodel.cn/api/paas/v4/audio/speech";
        public string ttsModel = "cogview-3"; // 默认为智谱的合成模型
        public string ttsVoice = "charles"; // 可选：eagle, charles, stephen, bella, daisy

        [Header("ASR Settings")]
        public string asrUrl = "https://open.bigmodel.cn/api/paas/v4/audio/transcriptions";
        public string asrModel = "whisper-1";

        [Header("Common Settings")]
        [Range(1, 30)]
        public int timeout = 10;
    }
}
