using Cysharp.Threading.Tasks;
using Shared.Disposable;
using Shared.Requests;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Shared.Cash 
{
    internal class CashBundles : BaseDisposable
    {
        public struct Ctx
        {
            public ReactiveCommand<UnityEngine.Object> OnGetBundle;
            public IObservable<(string assetPath, string assetName)> GetBundle;

            public Func<string, bool> IsCashed;
            public Func<string, string> GetPath;

            public Func<string, byte[]> FromCash;
            public Func<byte[], string, byte[]> ToCash;
        }

        private readonly Ctx _ctx;

        private readonly Dictionary<string, AssetBundle> _bundles = new();

        public CashBundles(Ctx ctx)
        {
            _ctx = ctx;

            _ctx.GetBundle.Subscribe(async data => await GetBundleAsync(data.assetPath, data.assetName)).AddTo(this);
        }

        public async UniTask GetBundleAsync(string assetPath, string assetName)
        {
            if (!_bundles.TryGetValue(assetPath, out var bundle))
            {
                if (_ctx.IsCashed.Invoke(assetPath))
                {
                    bundle = await BundleFromCache(assetPath);
                }
                else
                {
                    var bundleData = await new AssetRequests().GetBundle(assetPath);
                    bundle = await BundleToCache(bundleData, assetPath);
                }

                _bundles[assetPath] = bundle;
            }

            _ctx.OnGetBundle.Execute(await bundle.LoadAssetAsync(assetName));
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

