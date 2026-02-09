using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

namespace DigitalHuman.Core
{
    public class AsrService
    {
        private readonly Data.ChatApiSettings _settings;

        public AsrService(Data.ChatApiSettings settings)
        {
            _settings = settings;
        }

        public async Task<string> RecognizeAsync(byte[] wavData)
        {
            if (string.IsNullOrEmpty(_settings.llmApiKey))
            {
                Debug.LogError("[AsrService] API Key is missing.");
                return null;
            }

            string modelName = (string.IsNullOrEmpty(_settings.asrModel) ? "whisper-1" : _settings.asrModel).Trim();
            Debug.Log($"[AsrService] 正在请求 ASR: URL={_settings.asrUrl}, Model='{modelName}', DataSize={wavData.Length}");

            WWWForm form = new WWWForm();
            form.AddBinaryData("file", wavData, "recording.wav", "audio/wav");
            form.AddField("model", modelName);

            using var request = UnityWebRequest.Post(_settings.asrUrl, form);
            request.SetRequestHeader("Authorization", $"Bearer {_settings.llmApiKey}");

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[AsrService] Error: {request.error}\nResponse: {request.downloadHandler.text}");
                return null;
            }

            // Simple JSON parsing to get "text" field
            var response = JsonUtility.FromJson<AsrResponse>(request.downloadHandler.text);
            return response?.text;
        }

        [System.Serializable]
        private class AsrResponse
        {
            public string text;
        }
    }
}
