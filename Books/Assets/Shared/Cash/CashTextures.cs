using Cysharp.Threading.Tasks;
using Shared.Disposable;
using Shared.Requests;
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
            public IObservable<(byte[] data, string texturePath)> OnGetTextureRawRequest;
            public ReactiveCommand<string> GetTextureRawRequest;

            public ReactiveCommand<(Texture2D texture, string key)> OnGetTexture;
            public IObservable<(string fileName, string key)> GetTexture;

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

            _ctx.GetTexture.Subscribe(async data => await GetTextureAsync(data.fileName, data.key)).AddTo(this);
        }

        private async UniTask GetTextureAsync(string fileName, string key)
        {
            if (_ctx.IsCashed.Invoke(fileName))
            {
                _ctx.OnGetTexture.Execute((TextureFromCache(fileName, key), key));
            }
            else
            {
                var textureRawRequestDone = false;
                byte[] textureRawData = null;
                var disposable = _ctx.OnGetTextureRawRequest.Where(data => fileName == data.texturePath).Subscribe(data =>
                {
                    textureRawData = data.data;
                    textureRawRequestDone = true;
                });
                _ctx.GetTextureRawRequest.Execute(fileName);
                while (!textureRawRequestDone) await UniTask.Yield();
                disposable.Dispose();

                _ctx.OnGetTexture.Execute((TextureToCache(textureRawData, fileName, key), key));
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

