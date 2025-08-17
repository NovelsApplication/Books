using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Shared.Requests 
{
    public class AssetRequests
    {
        public async UniTask<byte[]> GetBundle(string assetPath)
        {
            var path = GetPath($"Remote/WebGL/{assetPath}");
#if UNITY_EDITOR
            path = GetPath($"Remote/Win/{assetPath}");
#endif
            using var request = UnityWebRequest.Get(path);

            SetHeaders(request);

            await request.SendWebRequest();

            return request.downloadHandler.data;
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

        public async UniTask<byte[]> GetTextureRaw(string localPath)
        {
            var path = GetPath(localPath);
            using var request = UnityWebRequest.Get(path);

            SetHeaders(request);

            await request.SendWebRequest();

            return request.downloadHandler.data;
        }

        public async UniTask<AudioClip> GetAudio(string localPath, AudioType audioType = AudioType.MPEG)
        {
            var path = GetPath(localPath);
            using var request = UnityWebRequestMultimedia.GetAudioClip(path, audioType);

            var dh = (DownloadHandlerAudioClip)request.downloadHandler;
            dh.compressed = false;
            dh.streamAudio = false;
            request.downloadHandler = dh;

            SetHeaders(request);

            await request.SendWebRequest();

            dh.audioClip.LoadAudioData();
            while (dh.audioClip.loadState != AudioDataLoadState.Loaded) await UniTask.Yield();

            return dh.audioClip;
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

