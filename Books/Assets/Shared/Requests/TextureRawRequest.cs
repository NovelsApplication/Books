using Cysharp.Threading.Tasks;
using Shared.Disposable;
using System;
using UniRx;
using UnityEngine;
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
            using var request = _ctx.GetRequest.Invoke(localPath);

            await request.SendWebRequest();

            return request.downloadHandler.data;
        }
    }
}
