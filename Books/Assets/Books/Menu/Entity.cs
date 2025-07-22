using Books.Menu.View;
using Cysharp.Threading.Tasks;
using Shared.Disposable;
using Shared.Requests;
using System;
using System.Collections.Generic;
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
            public bool IsLightTheme;
            public Data Data;
            public string ManifestPath;
        }

        private IScreen _screen;
        private readonly Ctx _ctx;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        public async UniTask Init(Action<StoryManifest> onClick)
        {
            var asset = await new AssetRequests().GetBundle("Main", _ctx.Data.ScreenName);
            var go = GameObject.Instantiate(asset as GameObject);
            _screen = go.GetComponent<IScreen>();

            _screen.SetTheme(_ctx.IsLightTheme);

            var manifests = await new AssetRequests().GetData<List<StoryManifest>>(_ctx.ManifestPath);
            foreach (var storyManifest in manifests) 
            {
                await _screen.AddBookAsync(storyManifest, () => onClick.Invoke(storyManifest));
            }
        }

        public async UniTask Show() => await _screen.Show();
        public void HideImmediate() => _screen.HideImmediate();

        protected override void OnDispose()
        {
            base.OnDispose();
            HideImmediate();
            _screen.Release();
        }
    }
}

