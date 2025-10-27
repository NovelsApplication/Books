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
            public ReactiveCommand<(string path, ReactiveProperty<Func<UniTask<byte[]>>> task)> GetBundleRequest;

            public IObservable<(string path, string name, ReactiveProperty<Func<UniTask<UnityEngine.Object>>> task)> GetBundle;

            public Func<string, bool> IsCashed;

            public Func<string, byte[]> FromCash;
            public Action<byte[], string> ToCash;
        }

        private readonly Ctx _ctx;

        private readonly Dictionary<string, AssetBundle> _bundles = new();

        public CashBundles(Ctx ctx)
        {
            _ctx = ctx;

            _ctx.GetBundle.Subscribe(data => data.task.Value = async () => await GetBundleAsync(data.path, data.name)).AddTo(this);
        }

        private async UniTask<UnityEngine.Object> GetBundleAsync(string path, string name)
        {
            if (!_bundles.TryGetValue(path, out var bundle))
            {
                if (_ctx.IsCashed.Invoke(path))
                {
                    bundle = await BundleFromCache(path);
                }
                else
                {
                    var task = new ReactiveProperty<Func<UniTask<byte[]>>>();
                    _ctx.GetBundleRequest.Execute((path, task));
                    var bundleData = await task.Value.Invoke();
                    task.Dispose();

                    bundle = await BundleToCache(path, bundleData);
                }

                _bundles[path] = bundle;
            }

            return await bundle.LoadAssetAsync(name);
        }

        private async UniTask<AssetBundle> BundleFromCache(string path)
        {
            var rawData = _ctx.FromCash.Invoke(path);
            return await AssetBundle.LoadFromMemoryAsync(rawData);
        }

        private async UniTask<AssetBundle> BundleToCache(string path, byte[] data)
        {
            _ctx.ToCash.Invoke(data, path);
            return await BundleFromCache(path);
        }
    }
}

