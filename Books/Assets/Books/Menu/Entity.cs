using Books.Menu.View;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Shared.Disposable;
using System;
using System.Collections.Generic;
using Books.Menu.MenuPopup;
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

            public IObservable<(string text, string textPath)> OnGetText;
            public ReactiveCommand<string> GetText;

            public IObservable<(Texture2D texture, string key)> OnGetTexture;
            public ReactiveCommand<(string fileName, string key)> GetTexture;

            public Func<string, (string header, string attributes, string body)?> ProcessLine;

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

            var manifests = new List<StoryManifest>();
            var manifestsDone = false;
            var manifestPath = _ctx.ManifestPath;
            var manifestText = string.Empty;

            var manifestProcess = _ctx.OnGetText.Where(data => data.textPath == manifestPath).Subscribe(data =>
            {
                manifestText = data.text;
                manifestsDone = true;
            });

            _ctx.GetText.Execute(manifestPath);
            while (!manifestsDone) await UniTask.Yield();
            manifestProcess.Dispose();

            manifests = JsonConvert.DeserializeObject<List<StoryManifest>>(manifestText);
            _screen.Init(new PopupFactory(_ctx.Data.PopupData), manifests.Count);

            foreach (var storyManifest in manifests) 
            {
                var storyDone = false;
                var storyPath = $"{storyManifest.StoryPath}/Story.json";
                var storyText = string.Empty;

                _ctx.OnGetText.Where(data => data.textPath == storyPath).Subscribe(data => 
                {
                    storyText = data.text;
                    storyDone = true;
                }).AddTo(this);
                _ctx.GetText.Execute(storyPath);
                while (!storyDone) await UniTask.Yield();

                var textureDone = false;
                var texturePath = $"{storyManifest.StoryPath}/Poster.png";
                Texture2D texture = null;
                var textureKey = texturePath;

                _ctx.OnGetTexture.Where(data => data.key == textureKey).Subscribe(data =>
                {
                    texture = data.texture;
                    textureDone = true;
                }).AddTo(this);
                _ctx.GetTexture.Execute((texturePath, textureKey));
                while (!textureDone) await UniTask.Yield();

                await _screen.AddBookAsync(storyText, texture, storyManifest, () => onClick.Invoke(storyManifest), _ctx.ProcessLine);
            }
            
            _screen.OnAllBooksAdded();

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

