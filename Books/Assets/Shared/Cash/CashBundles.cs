using Cysharp.Threading.Tasks;
using Shared.Disposable;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Shared.Cash 
{
    internal sealed class CashBundles : BaseDisposable
    {
        public struct Ctx
        {
            public IObservable<(byte[] data, string assetPath)> OnGetBundleRequest;
            public ReactiveCommand<string> GetBundleRequest;

            public ReactiveCommand<(UnityEngine.Object bundle, string assetName)> OnGetBundle;
            public IObservable<(string assetPath, string assetName)> GetBundle;

            public Func<string, bool> IsCashed;

            public Func<string, byte[]> FromCash;
            public Action<byte[], string> ToCash;
        }

        private readonly Ctx _ctx;

        private readonly Dictionary<string, AssetBundle> _bundles = new();

        public CashBundles(Ctx ctx)
        {
            _ctx = ctx;

            _ctx.GetBundle.Subscribe(async data => await GetBundleAsync(data.assetPath, data.assetName)).AddTo(this);
        }

        private async UniTask GetBundleAsync(string assetPath, string assetName)
        {
            if (!_bundles.TryGetValue(assetPath, out var bundle))
            {
                if (_ctx.IsCashed.Invoke(assetPath))
                {
                    bundle = await BundleFromCache(assetPath);
                }
                else
                {
                    var bundleRequestDone = false;
                    byte[] bundleData = null;
                    var disposable = _ctx.OnGetBundleRequest.Where(data => assetPath == data.assetPath).Subscribe(data => 
                    {
                        bundleData = data.data;
                        bundleRequestDone = true;
                    });
                    _ctx.GetBundleRequest.Execute(assetPath);
                    while (!bundleRequestDone) await UniTask.Yield();
                    disposable.Dispose();

                    bundle = await BundleToCache(bundleData, assetPath);
                }

                _bundles[assetPath] = bundle;
            }

            _ctx.OnGetBundle.Execute((await bundle.LoadAssetAsync(assetName), assetName));
        }

        private async UniTask<AssetBundle> BundleFromCache(string fileName)
        {
            var rawData = _ctx.FromCash.Invoke(fileName);
            return await AssetBundle.LoadFromMemoryAsync(rawData);
        }

        private async UniTask<AssetBundle> BundleToCache(byte[] data, string fileName)
        {
            _ctx.ToCash.Invoke(data, fileName);
            return await BundleFromCache(fileName);
        }
    }
}

