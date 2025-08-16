using Books.Loading.View;
using Cysharp.Threading.Tasks;
using Shared.Disposable;
using System;
using UniRx;
using UnityEngine;

namespace Books.Loading
{
    public sealed class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public Data Data;

            public IObservable<UnityEngine.Object> OnGetBundle;
            public ReactiveCommand<(string assetPath, string assetName)> GetBundle;

            public Action InitDone;
        }

        private IScreen _screen;
        private Ctx _ctx;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;

            _ctx.GetBundle.Execute(("main", _ctx.Data.ScreenName));
            _ctx.OnGetBundle.Subscribe(bundle => Init(bundle)).AddTo(this);
        }

        private void Init(UnityEngine.Object bundleObject) 
        {
            var go = GameObject.Instantiate(bundleObject as GameObject);
            _screen = go.GetComponent<IScreen>();

            Debug.Log($"screen: {_screen == null}");

            _ctx.InitDone.Invoke();
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
