using Cysharp.Threading.Tasks;
using Shared.Disposable;
using System;
using System.Collections.Generic;

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

        private readonly Ctx _ctx;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        public async UniTask Init(Action<StoryManifest> onClick)
        {
            _ctx.Data.Screen.SetTheme(_ctx.IsLightTheme);

            var manifests = await new AssetRequests().GetData<List<StoryManifest>>(_ctx.ManifestPath);
            foreach (var storyManifest in manifests) 
            {
                await _ctx.Data.Screen.AddBookAsync(storyManifest, () => onClick.Invoke(storyManifest));
            }
        }

        public async UniTask Show() => await _ctx.Data.Screen.Show();
        public void HideImmediate() => _ctx.Data.Screen.HideImmediate();

        protected override void OnDispose()
        {
            HideImmediate();
            _ctx.Data.Screen.Release();
            base.OnDispose();
        }
    }
}

