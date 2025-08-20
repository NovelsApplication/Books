using Cysharp.Threading.Tasks;
using Shared.Disposable;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

namespace Shared.Requests 
{
    public class TextRequest : BaseDisposable
    {
        public struct Ctx
        {
            public ReactiveCommand<(string text, string textPath)> OnGetText;
            public IObservable<string> GetText;

            public Func<string, UnityWebRequest> GetRequest;
        }

        private readonly Ctx _ctx;

        public TextRequest(Ctx ctx)
        {
            _ctx = ctx;

            _ctx.GetText.Subscribe(path => GetText(path)).AddTo(this);
        }

        private async void GetText(string localPath)
        {
            using var request = _ctx.GetRequest.Invoke(localPath);
            await request.SendWebRequest();

            _ctx.OnGetText.Execute((request.downloadHandler.text, localPath));
        }
    }
}
