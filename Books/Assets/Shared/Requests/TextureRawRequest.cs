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
            public ReactiveCommand<(byte[] data, string texturePath)> OnGetTextureRaw;
            public IObservable<string> GetTextureRaw;

            public Func<string, UnityWebRequest> GetRequest;
        }

        private readonly Ctx _ctx;

        public TextureRawRequest(Ctx ctx)
        {
            _ctx = ctx;

            _ctx.GetTextureRaw.Subscribe(path => GetTexture(path)).AddTo(this);
        }

        private async void GetTexture(string localPath)
        {
            using var request = _ctx.GetRequest.Invoke(localPath);

            Debug.Log($"Try load image from: {request.url}");

            await request.SendWebRequest();

            _ctx.OnGetTextureRaw.Execute((request.downloadHandler.data, localPath));
        }
    }
}
