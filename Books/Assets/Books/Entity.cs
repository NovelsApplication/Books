using Cysharp.Threading.Tasks;
using Shared.Disposable;
using Shared.LocalCache;
using System;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Books
{
    internal sealed class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public Data Data;
        }

        private Loading.Entity _loading;
        private readonly Ctx _ctx;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        public async UniTask AsyncProcess()
        {
            var url = Application.absoluteURL;

#if UNITY_EDITOR
            url = _ctx.Data.TestURL;
#endif

            var storyPath = url.Contains("?") ? url.Split("?").Last() : null;

            var onGetBundle = new ReactiveCommand<(UnityEngine.Object bundle, string assetName)>().AddTo(this);
            var getBundle = new ReactiveCommand<(string assetPath, string assetName)>().AddTo(this);

            var cash = new Shared.Cash.Entity(new Shared.Cash.Entity.Ctx 
            {
                OnGetBundle = onGetBundle,
                GetBundle = getBundle,
            }).AddTo(this);

            var loadingDone = false;
            _loading = new Loading.Entity(new Loading.Entity.Ctx
            {
                Data = _ctx.Data.LoadingData,

                OnGetBundle = onGetBundle,
                GetBundle = getBundle,

                InitDone = () => loadingDone = true,
            }).AddTo(this);

            while (!loadingDone) await UniTask.Yield();

            _loading.ShowImmediate();

            while (!IsDisposed) 
            {
                Menu.Entity.StoryManifest? storyManifest = null;

                if (storyPath != null) 
                {
                    storyManifest = new Menu.Entity.StoryManifest
                    {
                        StoryPath = storyPath,
                    };
                }

                if (storyManifest == null) 
                {
                    await _loading.Show();

                    var mainDone = false;
                    var mainScreen = new Menu.Entity(new Menu.Entity.Ctx
                    {
                        Data = _ctx.Data.MenuData,

                        ManifestPath = "StoryManifest.json",
                        IsLightTheme = DateTime.Now.Hour > 9 && DateTime.Now.Hour < 20,

                        OnGetBundle = onGetBundle,
                        GetBundle = getBundle,

                        InitDone = () => mainDone = true,
                    }, story => { storyManifest = story; }).AddTo(this);

                    while (!mainDone) await UniTask.Yield();

                    await mainScreen.Show();

                    await _loading.Hide();

                    while (!storyManifest.HasValue) await UniTask.Yield();

                    mainScreen.Dispose();
                }

                await _loading.Show();

                var storyText = await Cacher.GetTextAsync($"{storyManifest.Value.StoryPath}/Story.json");

                var storyDone = false;
                var storyScreen = new Story.Entity(new Story.Entity.Ctx
                {
                    Data = _ctx.Data.StoriesData,

                    RootFolderName = storyPath,
                    StoryText = storyText,

                    OnGetBundle = onGetBundle,
                    GetBundle = getBundle,

                    InitDone = () => storyDone = true,
                }).AddTo(this);

                while (!storyDone) await UniTask.Yield();

                var storyClosed = false;
                storyScreen.ShowImmediate();
                storyScreen.ShowStoryProcess(() => { storyClosed = true; }).Forget();

                await _loading.Hide();

                while (!storyClosed) await UniTask.Yield();

                storyScreen.Dispose();
            }
        }
    }
}
