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
            public ReactiveCommand<(byte[] data, string assetPath)> OnGetBundle;
            public IObservable<string> GetBundle;

            public Func<string, UnityWebRequest> GetRequest;
        }

        private readonly Ctx _ctx;

        public BundleRequest(Ctx ctx)
        {
            _ctx = ctx;

            _ctx.GetBundle.Subscribe(path => GetBundle(path)).AddTo(this);
        }

        private async void GetBundle(string localPath)
        {
            var path = $"Remote/WebGL/{localPath}";
#if UNITY_EDITOR
            path = $"Remote/Win/{localPath}";
#endif

            using var request = _ctx.GetRequest.Invoke(path);
            await request.SendWebRequest();

            _ctx.OnGetBundle.Execute((request.downloadHandler.data, localPath));
        }
    }
}

