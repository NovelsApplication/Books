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

            public IObservable<(string path, ReactiveProperty<Func<UniTask<string>>> task)> GetText;

            public IObservable<(string path, ReactiveProperty<Func<string>> task)> LoadText;

            public IObservable<(string path, string text)> SaveText;

            public Func<string, bool> IsCashed;

            public Func<string, byte[]> FromCash;
            public Action<byte[], string> ToCash;
        }

        private readonly Ctx _ctx;

        public CashTexts(Ctx ctx)
        {
            _ctx = ctx;

            _ctx.GetText.Subscribe(data => data.task.Value = async () => await GetTextAsync(data.path)).AddTo(this);
            _ctx.LoadText.Subscribe(data => data.task.Value = () => LoadText(data.path)).AddTo(this);
            _ctx.SaveText.Subscribe(data => TextToCache(data.path, data.text)).AddTo(this);
        }

        private string LoadText(string path) 
        {
            if (_ctx.IsCashed.Invoke(path))
            {
                return TextFromCache(path);
            }
            else 
            {
                return string.Empty;
            }
        }

        private async UniTask<string> GetTextAsync(string path)
        {
            if (_ctx.IsCashed.Invoke(path))
            { 
                return TextFromCache(path); 
            }
            else
            {
                var task = new ReactiveProperty<Func<UniTask<string>>>();
                _ctx.GetTextRequest.Execute((path, task));
                var text = await task.Value.Invoke();
                task.Dispose();

                return TextToCache(path, text); 
            }
        }

        private string TextFromCache(string path)
        {
            var rawData = _ctx.FromCash.Invoke(path);
            return Encoding.UTF8.GetString(rawData);
        }

        private string TextToCache(string path, string data)
        {
            var rawData = Encoding.UTF8.GetBytes(data);
            _ctx.ToCash.Invoke(rawData, path);
            return data;
        }
    }
}

