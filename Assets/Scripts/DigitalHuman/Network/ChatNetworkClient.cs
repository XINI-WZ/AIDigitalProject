using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Text;
using DigitalHuman.Data;

namespace DigitalHuman.Core
{
    public class ChatNetworkClient
    {
        private readonly ChatApiSettings _settings;

        public ChatNetworkClient(ChatApiSettings settings)
        {
            _settings = settings;
        }

        public async Task<string> PostRequestAsync(string url, string apiKey, string jsonData)
        {
            using var request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            if (!string.IsNullOrEmpty(apiKey))
            {
                request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            }

            request.timeout = _settings.timeout;

            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[ChatNetworkClient] Error: {request.error}\nResponse: {request.downloadHandler.text}");
                return null;
            }

            return request.downloadHandler.text;
        }
    }
}
