using Cysharp.Threading.Tasks;
using Shared.Disposable;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Shared.Cash
{
    internal sealed class CashTextures : BaseDisposable
    {
        public struct Ctx
        {
            public ReactiveCommand<(string path, ReactiveProperty<Func<UniTask<byte[]>>> task)> GetTextureRequest;

            public IObservable<(string path, string key, ReactiveProperty<Func<UniTask<(Texture2D texture, string key)>>> task)> GetTexture;

            public Func<string, bool> IsCashed;

            public Func<string, byte[]> FromCash;
            public Action<byte[], string> ToCash;
        }

        private readonly Dictionary<string, Texture2D> _images;
        private readonly Ctx _ctx;

        public CashTextures(Ctx ctx)
        {
            _images = new Dictionary<string, Texture2D>();

            _ctx = ctx;

            _ctx.GetTexture.Subscribe(data => data.task.Value = async () => await GetTextureAsync(data.path, data.key)).AddTo(this);
        }

        private async UniTask<(Texture2D texture, string key)> GetTextureAsync(string path, string key)
        {
            if (_ctx.IsCashed.Invoke(path))
            {
                return (TextureFromCache(path, key), key);
            }
            else
            {
                var task = new ReactiveProperty<Func<UniTask<byte[]>>>().AddTo(this);
                _ctx.GetTextureRequest.Execute((path, task));
                var textureRawData = await task.Value.Invoke();
                task.Dispose();

                return (TextureToCache(textureRawData, path, key), key);
            }
        }

        private Texture2D TextureFromCache(string fileName, string key)
        {
            var rawData = _ctx.FromCash.Invoke(fileName);
            if (!_images.ContainsKey(key))
                _images[key] = new(0, 0);
            ImageConversion.LoadImage(_images[key], rawData);
            return _images[key];
        }

        private Texture2D TextureToCache(byte[] data, string fileName, string key)
        {
            _ctx.ToCash.Invoke(data, fileName);
            return TextureFromCache(fileName, key);
        }
    }
}

