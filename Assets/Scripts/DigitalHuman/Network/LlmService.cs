using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using DigitalHuman.Data;

namespace DigitalHuman.Core
{
    public class LlmService
    {
        private readonly ChatApiSettings _settings;
        private readonly ChatNetworkClient _networkClient;
        private List<LlmMessage> _chatHistory = new List<LlmMessage>();

        public LlmService(ChatApiSettings settings, ChatNetworkClient networkClient)
        {
            _settings = settings;
            _networkClient = networkClient;
        }

        public void SetSystemPrompt(string prompt)
        {
            _chatHistory.Clear();
            string fullPrompt = prompt + "\n\n请在你的回答开头包含一个情绪标签，格式为 [情绪]，可选的情绪有：[Neutral], [Happy], [Angry], [Sad], [Surprised]。例如：'[Happy] 你好！我很开心见到你。'";
            _chatHistory.Add(new LlmMessage { role = "system", content = fullPrompt });
        }

        public async Task<LlmResult> AskAsync(string userPrompt)
        {
            _chatHistory.Add(new LlmMessage { role = "user", content = userPrompt });
            
            // 只要模型名包含 voice，就启用多模态语音模式
            bool isVoiceModel = _settings.llmModel.ToLower().Contains("voice");

            string json;
            
            if (isVoiceModel)
            {
                // GLM-4-Voice 需要特殊格式，手动构建 JSON
                json = BuildVoiceModelJson();
                Debug.Log("[LlmService] 检测到语音模型，使用 content 数组格式");
            }
            else
            {
                // 普通模型使用标准格式
                var requestData = new LlmRequest
                {
                    model = _settings.llmModel,
                    messages = _chatHistory
                };
                json = JsonUtility.ToJson(requestData);
            }
            
            Debug.Log($"[LlmService] 发送请求: {json}");
            
            string responseJson = await _networkClient.PostRequestAsync(_settings.llmUrl, _settings.llmApiKey, json);
            
            if (string.IsNullOrEmpty(responseJson)) return null;
            Debug.Log($"[LlmService] 收到响应: {responseJson}");

            // 使用自定义解析来处理复杂的响应结构
            var result = ParseVoiceResponse(responseJson);
            
            if (result != null && !string.IsNullOrEmpty(result.text))
            {
                // 将回复添加到历史记录
                _chatHistory.Add(new LlmMessage { role = "assistant", content = result.text });
                return result;
            }

            return null;
        }

        private string BuildVoiceModelJson()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append($"\"model\":\"{_settings.llmModel}\",");
            sb.Append("\"messages\":[");

            // 收集 system prompt（GLM-4-Voice 不支持 system 角色，需要合并到 user 消息中）
            string systemPrompt = "";
            foreach (var msg in _chatHistory)
            {
                if (msg.role == "system")
                {
                    systemPrompt = msg.content;
                    break;
                }
            }

            bool first = true;
            bool hasAddedSystemPrompt = false;
            
            foreach (var msg in _chatHistory)
            {
                // 跳过 system 角色（已提取到 systemPrompt）
                if (msg.role == "system") continue;
                
                if (!first) sb.Append(",");
                first = false;

                // 构建 content 数组格式
                sb.Append("{");
                sb.Append($"\"role\":\"{msg.role}\",");
                sb.Append("\"content\":[");
                sb.Append("{");
                sb.Append("\"type\":\"text\",");
                
                // 如果是第一条 user 消息，把 system prompt 加在前面
                if (msg.role == "user" && !hasAddedSystemPrompt && !string.IsNullOrEmpty(systemPrompt))
                {
                    string combinedText = $"Instructions: {systemPrompt}\n\nUser: {msg.content}";
                    sb.Append($"\"text\":\"{EscapeJson(combinedText)}\"");
                    hasAddedSystemPrompt = true;
                    Debug.Log($"[LlmService] 合并 System Prompt 到用户消息，长度: {systemPrompt.Length}");
                }
                else
                {
                    sb.Append($"\"text\":\"{EscapeJson(msg.content)}\"");
                }
                
                sb.Append("}");
                sb.Append("]");
                sb.Append("}");
            }

            sb.Append("]}");
            return sb.ToString();
        }

        private string EscapeJson(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            return str.Replace("\\", "\\\\")
                     .Replace("\"", "\\\"")
                     .Replace("\n", "\\n")
                     .Replace("\r", "\\r")
                     .Replace("\t", "\\t");
        }

        private LlmResult ParseVoiceResponse(string json)
        {
            try
            {
                var result = new LlmResult();
                
                // 手动提取 text 内容 - 从 choices[0].message.content
                int choicesIndex = json.IndexOf("\"choices\"");
                if (choicesIndex > 0)
                {
                    int messageIndex = json.IndexOf("\"message\"", choicesIndex);
                    if (messageIndex > 0)
                    {
                        // 提取 content
                        int contentIndex = json.IndexOf("\"content\":", messageIndex);
                        if (contentIndex > 0)
                        {
                            int start = json.IndexOf("\"", contentIndex + 10) + 1;
                            int end = json.IndexOf("\"", start);
                            if (end > start)
                            {
                                result.text = json.Substring(start, end - start);
                                result.text = result.text.Replace("\\n", "\n").Replace("\\\"", "\"").Replace("\\\\", "\\");
                            }
                        }
                        
                        // 提取 audio data
                        int audioIndex = json.IndexOf("\"audio\":", messageIndex);
                        if (audioIndex > 0)
                        {
                            int dataIndex = json.IndexOf("\"data\":", audioIndex);
                            if (dataIndex > 0)
                            {
                                int start = json.IndexOf("\"", dataIndex + 7) + 1;
                                int end = json.IndexOf("\"", start);
                                if (end > start)
                                {
                                    result.audioBase64 = json.Substring(start, end - start);
                                    Debug.Log($"[LlmService] ✓ 成功提取音频数据，长度: {result.audioBase64.Length} 字符");
                                }
                            }
                        }
                    }
                }
                
                if (string.IsNullOrEmpty(result.audioBase64))
                {
                    Debug.LogWarning("[LlmService] ⚠ 未提取到音频数据，响应中可能没有 audio 字段");
                }
                
                return result;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[LlmService] 解析响应失败: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 直接发送音频给 GLM-4-Voice（端到端语音对话）
        /// </summary>
        public async Task<LlmResult> AskWithAudioAsync(string audioBase64)
        {
            // 构建包含音频输入的请求
            string json = BuildAudioInputJson(audioBase64);
            Debug.Log("[LlmService] 发送音频输入请求到 GLM-4-Voice");
            
            string responseJson = await _networkClient.PostRequestAsync(_settings.llmUrl, _settings.llmApiKey, json);
            
            if (string.IsNullOrEmpty(responseJson)) return null;
            Debug.Log($"[LlmService] 收到响应: {responseJson}");

            // 解析响应
            var result = ParseVoiceResponse(responseJson);
            
            if (result != null && !string.IsNullOrEmpty(result.text))
            {
                // 将对话添加到历史记录
                _chatHistory.Add(new LlmMessage { role = "user", content = "[语音输入]" });
                _chatHistory.Add(new LlmMessage { role = "assistant", content = result.text });
                return result;
            }

            return null;
        }

        private string BuildAudioInputJson(string audioBase64)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append($"\"model\":\"{_settings.llmModel}\",");
            sb.Append("\"messages\":[{");
            sb.Append("\"role\":\"user\",");
            sb.Append("\"content\":[");
            // 发送音频输入
            sb.Append("{");
            sb.Append("\"type\":\"input_audio\",");
            sb.Append("\"input_audio\":{");
            sb.Append($"\"data\":\"{audioBase64}\",");
            sb.Append("\"format\":\"wav\"");
            sb.Append("}");
            sb.Append("}");
            sb.Append("]");
            sb.Append("}]");
            sb.Append("}");
            return sb.ToString();
        }

        public class LlmResult
        {
            public string text;
            public string audioBase64;
        }
    }
}
