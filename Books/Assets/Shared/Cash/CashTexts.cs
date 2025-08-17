using Cysharp.Threading.Tasks;
using Shared.Disposable;
using Shared.Requests;
using System;
using System.Collections.Generic;
using System.Text;
using UniRx;
using UnityEngine;

namespace Shared.Cash 
{
    internal sealed class CashTexts : BaseDisposable
    {
        public struct Ctx
        {
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
                _ctx.OnGetStory.Execute((TextFromCache(fileName), fileName));
            else
                _ctx.OnGetStory.Execute((TextToCache(await new AssetRequests().GetText(fileName), fileName), fileName));
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

