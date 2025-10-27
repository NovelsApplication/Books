using Cysharp.Threading.Tasks;
using Shared.Disposable;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

namespace Shared.Requests 
{
    public class BundleRequest : BaseDisposable
    {
        public struct Ctx
        {
            public IObservable<(string path, ReactiveProperty<Func<UniTask<byte[]>>> task)> GetBundle;

            public Func<string, UnityWebRequest> GetRequest;
        }

        private readonly Ctx _ctx;

        public BundleRequest(Ctx ctx)
        {
            _ctx = ctx;

            _ctx.GetBundle.Subscribe(data => data.task.Value = async () => await GetBundle(data.path)).AddTo(this);
        }

        private async UniTask<byte[]> GetBundle(string localPath)
        {
            var path = $"Remote/WebGL/{localPath}";
#if UNITY_EDITOR
            path = $"Remote/Win/{localPath}";
#endif

            using (var request = _ctx.GetRequest.Invoke(path))
            {
                await request.SendWebRequest();

                return request.downloadHandler.data;
            }
        }
    }
}

