using Books.Story.View;
using Cysharp.Threading.Tasks;
using Shared.Disposable;
using Shared.LocalCache;
using Shared.Requests;
using System;
using UnityEngine;

namespace Books.Story 
{
    public sealed class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public Data Data;
            public string RootFolderName;
            public string StoryText;
        }

        private IScreen _screen;
        private Logic _logic;
        private readonly Ctx _ctx;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        public async UniTask Init()
        {
            var asset = await Cacher.GetBundle("main", _ctx.Data.ScreenName);
            var go = GameObject.Instantiate(asset as GameObject);
            _screen = go.GetComponent<IScreen>();

            _logic = new Logic(new Logic.Ctx
            {
                Screen = _screen,
                RootFolderName = _ctx.RootFolderName,
                StoryText = _ctx.StoryText,
            }).AddTo(this);
        }

        public async UniTask ShowStoryProcess(Action onDone) => await _logic.ShowStoryProcess(onDone);

        public void ShowImmediate() => _screen.ShowImmediate();
        public void HideImmediate() => _screen.HideImmediate();

        protected override void OnDispose()
        {
            base.OnDispose();
            HideImmediate();
            _screen.Release();
        }
    }
}
