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
            public IObservable<(string path, ReactiveProperty<Func<UniTask<string[]>>> namesTask)> GetAllAssetNames;

            public Func<string, bool> IsCashed;

            public Func<string, byte[]> FromCash;
            public Action<byte[], string> ToCash;
        }

        private readonly Ctx _ctx;

        private readonly Dictionary<string, AssetBundle> _bundles = new();

        public CashBundles(Ctx ctx)
        {
            _ctx = ctx;

            _ctx.GetBundle.Subscribe(data => data.task.Value = async () => await GetAssetAsync(data.path, data.name)).AddTo(this);
            _ctx.GetAllAssetNames.Subscribe(data => data.namesTask.Value = async () => await GetAllAssetNamesFromBundle(data.path));
        }

        private async UniTask<UnityEngine.Object> GetAssetAsync(string path, string name)
        {
            var bundle = await GetBundleAsync(path);
            return await bundle.LoadAssetAsync(name);
        }

        private async UniTask<string[]> GetAllAssetNamesFromBundle(string path)
        {
            var bundle = await GetBundleAsync(path);
            return bundle.GetAllAssetNames();
        }

        private async UniTask<AssetBundle> GetBundleAsync(string path)
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

            return bundle;
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

