using Books.Story.View;
using Cysharp.Threading.Tasks;
using Shared.Disposable;
using System;
using UniRx;
using UnityEngine;

namespace Books.Story 
{
    public sealed class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public Data Data;
            public string StoryPath;

            public IObservable<(UnityEngine.Object bundle, string assetName)> OnGetBundle;
            public ReactiveCommand<(string assetPath, string assetName)> GetBundle;

            public IObservable<(string story, string storyPath)> OnGetStory;
            public ReactiveCommand<string> GetStory;

            public IObservable<(Texture2D texture, string key)> OnGetTexture;
            public ReactiveCommand<(string fileName, string key)> GetTexture;

            public Action InitDone;
        }

        private IScreen _screen;
        private Logic _logic;
        private readonly Ctx _ctx;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;

            _ctx.OnGetBundle.Where(data => data.assetName == _ctx.Data.ScreenName).Subscribe(data => Init(data.bundle)).AddTo(this);
            _ctx.GetBundle.Execute(("main", _ctx.Data.ScreenName));
        }

        private void Init(UnityEngine.Object bundle)
        {
            var go = GameObject.Instantiate(bundle as GameObject);
            _screen = go.GetComponent<IScreen>();

            _logic = new Logic(new Logic.Ctx
            {
                Screen = _screen,
                StoryPath = _ctx.StoryPath,

                OnGetStory = _ctx.OnGetStory,
                GetStory = _ctx.GetStory,

                OnGetTexture = _ctx.OnGetTexture,
                GetTexture = _ctx.GetTexture,
            }).AddTo(this);

            _ctx.InitDone.Invoke();
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
