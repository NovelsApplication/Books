using Books.Menu.View;
using Cysharp.Threading.Tasks;
using Shared.Disposable;
using Shared.LocalCache;
using Shared.Requests;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Books.Menu 
{
    public sealed class Entity : BaseDisposable
    {
        public enum Labels : byte 
        {
            Next,
            InProgress,
            Done,
            Continue,
        }

        public enum MainTags : byte
        {
            Continue,
            All,
            Special,
            Popular,
            ForYou,
            InProgress,
            Done,
            Free,
            New,
        }

        public struct StoryManifest
        {
            public string StoryPath;
        }

        public struct Ctx
        {
            public Data Data;

            public string ManifestPath;
            public bool IsLightTheme;

            public IObservable<(UnityEngine.Object bundle, string assetName)> OnGetBundle;
            public ReactiveCommand<(string assetPath, string assetName)> GetBundle;

            public IObservable<(string story, string storyPath)> OnGetStory;
            public ReactiveCommand<string> GetStory;

            public Action InitDone;
        }

        private IScreen _screen;
        private readonly Ctx _ctx;

        public Entity(Ctx ctx, Action<StoryManifest> onClick)
        {
            _ctx = ctx;

            _ctx.OnGetBundle.Where(data => data.assetName == _ctx.Data.ScreenName).Subscribe(data => 
            { 
                Init(data.bundle, onClick); 
            }).AddTo(this);
            _ctx.GetBundle.Execute(("main", _ctx.Data.ScreenName));
        }

        private async void Init(UnityEngine.Object bundle, Action<StoryManifest> onClick)
        {
            var go = GameObject.Instantiate(bundle as GameObject);
            _screen = go.GetComponent<IScreen>();

            _screen.SetTheme(_ctx.IsLightTheme);

            var manifests = await new AssetRequests().GetData<List<StoryManifest>>(_ctx.ManifestPath);
            foreach (var storyManifest in manifests) 
            {
                var storyDone = false;
                var storyPath = $"{storyManifest.StoryPath}/Story.json";
                var storyText = string.Empty;

                _ctx.OnGetStory.Where(data => data.storyPath == storyPath).Subscribe(data => 
                {
                    storyText = data.story;
                    storyDone = true;
                }).AddTo(this);
                _ctx.GetStory.Execute(storyPath);
                while (!storyDone) await UniTask.Yield();

                var posterImage = await Cacher.GetTextureAsync($"{storyManifest.StoryPath}/Poster.png", "poster");
                await _screen.AddBookAsync(storyText, posterImage, storyManifest, () => onClick.Invoke(storyManifest));
            }

            _ctx.InitDone.Invoke();
        }

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

