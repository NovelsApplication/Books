using Books.Loading.View;
using Cysharp.Threading.Tasks;
using Shared.Disposable;
using Shared.LocalCache;
using Shared.Requests;
using UnityEngine;

namespace Books.Loading
{
    public sealed class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public Data Data;
        }

        private IScreen _screen;
        private Ctx _ctx;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        public async UniTask Init() 
        {
            var asset = await Cacher.GetBundleAsync("main", _ctx.Data.ScreenName);
            var go = GameObject.Instantiate(asset as GameObject);
            _screen = go.GetComponent<IScreen>();
        }

        public void ShowImmediate() => _screen.ShowImmediate();
        public void HideImmediate() => _screen.HideImmediate();

        public async UniTask Show() => await _screen.Show();
        public async UniTask Hide() => await _screen.Hide();

        protected override void OnDispose()
        {
            base.OnDispose();
            _screen.Release();
        }
    }
}
