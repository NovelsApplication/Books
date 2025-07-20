using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.Networking;

namespace Shared.Requests 
{
    public class AssetRequests
    {
        public async UniTask<T> GetAsset<T>(string key)
        {
            Debug.Log($"Start loading {key}");
            var result = await Addressables.LoadAssetAsync<T>(key);
            Debug.Log(result.GetType().FullName);
            return result;
        }

        private async UniTask<IResourceLocator> GetCatalog(string localCatalogPath)
        {
            var path = GetPath(localCatalogPath);
            var result = await Addressables.LoadContentCatalogAsync(path);
            return result;
        }

        public async UniTask<T> GetData<T>(string localPath)
        {
            var path = GetPath(localPath);
            using var request = UnityWebRequest.Get(path);

            SetHeaders(request);

            await request.SendWebRequest();

            return JsonConvert.DeserializeObject<T>(request.downloadHandler.text);
        }

        public async UniTask<string> GetText(string localPath)
        {
            var path = GetPath(localPath);
            using var request = UnityWebRequest.Get(path);

            SetHeaders(request);

            await request.SendWebRequest();

            return request.downloadHandler.text;
        }

        public async UniTask<Texture2D> GetTexture(string localPath)
        {
            var path = GetPath(localPath);
            using var request = UnityWebRequestTexture.GetTexture(path);

            SetHeaders(request);

            await request.SendWebRequest();

            var result = DownloadHandlerTexture.GetContent(request);

            return result;
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

