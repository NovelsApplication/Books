using Cysharp.Threading.Tasks;
using Shared.Disposable;
using System;
using System.Text;
using UniRx;
using UnityEngine;

namespace Shared.Cash 
{
    internal sealed class CashTexts : BaseDisposable
    {
        public struct Ctx
        {
            public IObservable<(string text, string textPath)> OnGetTextRequest;
            public ReactiveCommand<string> GetTextRequest;

            public ReactiveCommand<(string story, string storyPath)> OnGetStory;
            public IObservable<string> GetStory;

            public Func<string, bool> IsCashed;

            public Func<string, byte[]> FromCash;
            public Action<byte[], string> ToCash;
        }

        private readonly Ctx _ctx;

        public CashTexts(Ctx ctx)
        {
            _ctx = ctx;

            _ctx.GetStory.Subscribe(async fileName => await GetTextAsync(fileName)).AddTo(this);
        }

        private async UniTask GetTextAsync(string fileName)
        {
            if (_ctx.IsCashed.Invoke(fileName))
            { 
                _ctx.OnGetStory.Execute((TextFromCache(fileName), fileName)); 
            }
            else
            {
                var textRequestDone = false;
                string text = null;
                var disposable = _ctx.OnGetTextRequest.Where(data => fileName == data.textPath).Subscribe(data =>
                {
                    text = data.text;
                    textRequestDone = true;
                });
                _ctx.GetTextRequest.Execute(fileName);
                while (!textRequestDone) await UniTask.Yield();
                disposable.Dispose();

                _ctx.OnGetStory.Execute((TextToCache(text, fileName), fileName)); 
            }
        }

        private string TextFromCache(string fileName)
        {
            var rawData = _ctx.FromCash.Invoke(fileName);
            return Encoding.UTF8.GetString(rawData);
        }

        private string TextToCache(string data, string fileName)
        {
            var rawData = Encoding.UTF8.GetBytes(data);
            _ctx.ToCash.Invoke(rawData, fileName);
            return data;
        }
    }
}

