using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

namespace Shared.Requests 
{
    public class AssetRequests
    {
        public async UniTask<T> GetData<T>(string localPath)
        {
            var path = GetPath(localPath);
            using var request = UnityWebRequest.Get(path);

            SetHeaders(request);

            await request.SendWebRequest();

            return JsonConvert.DeserializeObject<T>(request.downloadHandler.text);
        }

        private string GetPath(string localPath)
        {
            var result = $"{Application.streamingAssetsPath}/{localPath}";
            #if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            result = $"file://{result}";
            #endif
            return result;
        }

        private void SetHeaders(UnityWebRequest request)
        {
            request.SetRequestHeader("Access-Control-Allow-Credentials", "true");
            request.SetRequestHeader("Access-Control-Allow-Headers", "Accept, X-Access-Token, X-Application-Name, X-Request-Sent-Time");
            request.SetRequestHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            request.SetRequestHeader("Access-Control-Allow-Origin", "*");
        }
    }
}

