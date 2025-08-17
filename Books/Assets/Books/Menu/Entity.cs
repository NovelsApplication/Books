using Books.Menu.View;
using Cysharp.Threading.Tasks;
using Shared.Disposable;
using Shared.LocalCache;
using Shared.Requests;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using static Books.Menu.Entity;

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

            public IObservable<(Texture2D texture, string key)> OnGetTexture;
            public ReactiveCommand<(string fileName, string key)> GetTexture;

            public Action InitDone;
        }

        private IScreen _screen;
        private readonly Ctx _ctx;

        public Entity(Ctx ctx, Action<StoryManifest> onClick)
        {
            _ctx = ctx;

            Init(onClick);
        }

        private async void Init(Action<StoryManifest> onClick)
        {
            var bundlesDone = false;
            UnityEngine.Object bundle = null;
            _ctx.OnGetBundle.Where(data => data.assetName == _ctx.Data.ScreenName).Subscribe(data =>
            {
                bundle = data.bundle;
                bundlesDone = true;
            }).AddTo(this);
            _ctx.GetBundle.Execute(("main", _ctx.Data.ScreenName));
            while (!bundlesDone) await UniTask.Yield();

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

                var textureDone = false;
                var texturePath = $"{storyManifest.StoryPath}/Poster.png";
                Texture2D texture = null;
                var textureKey = "poster";

                _ctx.OnGetTexture.Where(data => data.key == textureKey).Subscribe(data =>
                {
                    texture = data.texture;
                    textureDone = true;
                }).AddTo(this);
                _ctx.GetTexture.Execute((texturePath, textureKey));
                while (!textureDone) await UniTask.Yield();

                await _screen.AddBookAsync(storyText, texture, storyManifest, () => onClick.Invoke(storyManifest));
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

