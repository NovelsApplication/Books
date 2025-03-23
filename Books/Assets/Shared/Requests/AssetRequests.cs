using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Shared.Disposable 
{
    public class AssetRequests
    {
        public async UniTask<string> GetText(string localPath)
        {
            using var request = UnityWebRequest.Get(GetPath(localPath));

            SetHeaders(request);

            await request.SendWebRequest();

            return request.downloadHandler.text;
        }

        public async UniTask<Texture2D> GetTexture(string localPath)
        {
            using var request = UnityWebRequestTexture.GetTexture(GetPath(localPath));

            SetHeaders(request);

            await request.SendWebRequest();

            var result = DownloadHandlerTexture.GetContent(request);

            return result;
        }

        private string GetPath(string localPath)
        {
            return $"{Application.streamingAssetsPath}/Books/{localPath}";
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

