using Cysharp.Threading.Tasks;
using Shared.Disposable;
using System;
using UniRx;
using UnityEngine.Networking;

namespace Shared.Requests 
{
    public class TextureRawRequest : BaseDisposable
    {
        public struct Ctx
        {
            public IObservable<(string path, ReactiveProperty<Func<UniTask<byte[]>>> task)> GetTexture;

            public Func<string, UnityWebRequest> GetRequest;
        }

        private readonly Ctx _ctx;

        public TextureRawRequest(Ctx ctx)
        {
            _ctx = ctx;

            _ctx.GetTexture.Subscribe(data => data.task.Value = async () => await GetTexture(data.path)).AddTo(this);
        }

        private async UniTask<byte[]> GetTexture(string localPath)
        {
            try
            {
                using var request = _ctx.GetRequest.Invoke(localPath);

                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    return request.downloadHandler.data;
                }
                else
                {
                    return null;
                }
            }
            catch (UnityWebRequestException)
            {
                return null;
            }
        }
    }
}
