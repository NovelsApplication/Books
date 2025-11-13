using Books.Menu.View;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Shared.Disposable;
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

            public ReactiveCommand<(string path, string name, ReactiveProperty<Func<UniTask<UnityEngine.Object>>> task)> GetBundle;

            public ReactiveCommand<(string path, ReactiveProperty<Func<UniTask<string>>> task)> GetText;

            public IObservable<(Texture2D texture, string key)> OnGetTexture;
            public ReactiveCommand<(string path, string key, ReactiveProperty<Func<UniTask<Texture2D>>> task)> GetTexture;
            
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
            var task = new ReactiveProperty<Func<UniTask<UnityEngine.Object>>>();
            _ctx.GetBundle.Execute(("main", _ctx.Data.ScreenName, task));
            var bundle = await task.Value.Invoke();
            task.Dispose();

            var go = GameObject.Instantiate(bundle as GameObject);
            _screen = go.GetComponent<IScreen>();
            
            _screen.SetTheme(_ctx.IsLightTheme);

            var manifests = new List<StoryManifest>();

            var manifestTask = new ReactiveProperty<Func<UniTask<string>>>();
            _ctx.GetText.Execute((_ctx.ManifestPath, manifestTask));
            var manifestText = await manifestTask.Value.Invoke();
            manifestTask.Dispose();

            manifests = JsonConvert.DeserializeObject<List<StoryManifest>>(manifestText);
            _screen.Init(_ctx.Data.PopupData, manifests.Count);

            foreach (var storyManifest in manifests) 
            {
                var storyPath = $"{storyManifest.StoryPath}/Story.json";

                var storyTask = new ReactiveProperty<Func<UniTask<string>>>();
                _ctx.GetText.Execute((storyPath, storyTask));
                var storyText = await storyTask.Value.Invoke();
                storyTask.Dispose();

                var texturePath = $"{storyManifest.StoryPath}/Poster.png";
                var textureKey = texturePath;

                var textureTask = new ReactiveProperty<Func<UniTask<Texture2D>>>().AddTo(this);
                _ctx.GetTexture.Execute((texturePath, textureKey, textureTask));
                var texture = await textureTask.Value.Invoke();
                textureTask.Dispose();

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

