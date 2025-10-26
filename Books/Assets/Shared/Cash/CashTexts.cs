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

            _ctx.GetText.Subscribe(data => data.task.Value = async () => await GetTextAsync(data.path)).AddTo(this);

            _ctx.LoadText.Subscribe(fileName => LoadText(fileName)).AddTo(this);
            _ctx.SaveText.Subscribe(data => TextToCache(data.text, data.textPath)).AddTo(this);
        }

        private void LoadText(string path) 
        {
            if (_ctx.IsCashed.Invoke(path))
            {
                _ctx.OnLoadText.Execute((TextFromCache(path), path));
            }
            else 
            {
                _ctx.OnLoadText.Execute((string.Empty, path));
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

                return TextToCache(text, path); 
            }
        }

        private string TextFromCache(string path)
        {
            var rawData = _ctx.FromCash.Invoke(path);
            return Encoding.UTF8.GetString(rawData);
        }

        private string TextToCache(string data, string path)
        {
            var rawData = Encoding.UTF8.GetBytes(data);
            _ctx.ToCash.Invoke(rawData, path);
            return data;
        }
    }
}

