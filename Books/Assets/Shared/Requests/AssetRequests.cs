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

        public async UniTask<Texture2D> GetTexture(string localPath)
        {
            var path = GetPath(localPath);
            using var request = UnityWebRequestTexture.GetTexture(path);

            SetHeaders(request);

            await request.SendWebRequest();

            var result = DownloadHandlerTexture.GetContent(request);

            return result;
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

            try 
            {
                Debug.Log($"Try1 {audioType} - {dh.audioClip.loadState} {dh.audioClip.loadType}");

                dh.audioClip.LoadAudioData();

                Debug.Log($"Try2 {audioType} - {dh.audioClip.loadState} {dh.audioClip.loadType}");

                while (dh.audioClip.loadState != AudioDataLoadState.Loaded) await UniTask.Yield();

                Debug.Log($"Try3 {audioType} - {dh.audioClip.loadState} {dh.audioClip.loadType}");

                var clip = dh.audioClip;

                Debug.Log($"Try {audioType} - {request.downloadHandler.data.Length}");

                return clip;
            }
            catch 
            {
                Debug.Log($"Catch {audioType}");
                return null;
            }
            
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

