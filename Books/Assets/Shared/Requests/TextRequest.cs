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
            public IObservable<(string path, ReactiveProperty<Func<UniTask<string>>> task)> GetText;

            public Func<string, UnityWebRequest> GetRequest;
        }

        private readonly Ctx _ctx;

        public TextRequest(Ctx ctx)
        {
            _ctx = ctx;

            _ctx.GetText.Subscribe(data => data.task.Value = async () => await GetText(data.path)).AddTo(this);
        }

        private async UniTask<string> GetText(string path)
        {
            using var request = _ctx.GetRequest.Invoke(path);
            await request.SendWebRequest();

            return request.downloadHandler.text;
        }
    }
}
