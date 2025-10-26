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
            public ReactiveCommand<(string path, ReactiveProperty<Func<UniTask<string>>> task)> GetTextRequest;

            public ReactiveCommand<(string text, string textPath)> OnGetText;
            public IObservable<string> GetText;

            public ReactiveCommand<(string text, string textPath)> OnLoadText;
            public IObservable<string> LoadText;

            public IObservable<(string text, string textPath)> SaveText;

            public Func<string, bool> IsCashed;

            public Func<string, byte[]> FromCash;
            public Action<byte[], string> ToCash;
        }

        private readonly Ctx _ctx;

        public CashTexts(Ctx ctx)
        {
            _ctx = ctx;

            _ctx.GetText.Subscribe(async fileName => await GetTextAsync(fileName)).AddTo(this);
            _ctx.LoadText.Subscribe(fileName => LoadText(fileName)).AddTo(this);
            _ctx.SaveText.Subscribe(data => TextToCache(data.text, data.textPath)).AddTo(this);
        }

        private void LoadText(string fileName) 
        {
            if (_ctx.IsCashed.Invoke(fileName))
            {
                _ctx.OnLoadText.Execute((TextFromCache(fileName), fileName));
            }
            else 
            {
                _ctx.OnLoadText.Execute((string.Empty, fileName));
            }
        }

        private async UniTask GetTextAsync(string fileName)
        {
            if (_ctx.IsCashed.Invoke(fileName))
            { 
                _ctx.OnGetText.Execute((TextFromCache(fileName), fileName)); 
            }
            else
            {
                var task = new ReactiveProperty<Func<UniTask<string>>>();
                _ctx.GetTextRequest.Execute((fileName, task));
                var text = await task.Value.Invoke();
                task.Dispose();

                _ctx.OnGetText.Execute((TextToCache(text, fileName), fileName)); 
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

