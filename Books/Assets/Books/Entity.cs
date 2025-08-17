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

        private readonly Ctx _ctx;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        public async UniTask AsyncProcess()
        {
            var onGetBundle = new ReactiveCommand<(UnityEngine.Object bundle, string assetName)>().AddTo(this);
            var getBundle = new ReactiveCommand<(string assetPath, string assetName)>().AddTo(this);

            var onGetStory = new ReactiveCommand<(string story, string storyPath)>().AddTo(this);
            var getStory = new ReactiveCommand<string>().AddTo(this);

            var onGetTexture = new ReactiveCommand<(Texture2D texture, string key)>().AddTo(this);
            var getTexture = new ReactiveCommand<(string fileName, string key)>().AddTo(this);

            var cash = new Shared.Cash.Entity(new Shared.Cash.Entity.Ctx 
            {
                OnGetBundle = onGetBundle,
                GetBundle = getBundle,

                OnGetStory = onGetStory,
                GetStory = getStory,

                OnGetTexture = onGetTexture,
                GetTexture = getTexture,
            }).AddTo(this);

            var loadingDone = false;
            var loading = new Loading.Entity(new Loading.Entity.Ctx
            {
                Data = _ctx.Data.LoadingData,

                OnGetBundle = onGetBundle,
                GetBundle = getBundle,

                InitDone = () => loadingDone = true,
            }).AddTo(this);

            while (!loadingDone) await UniTask.Yield();

            loading.ShowImmediate();

            Menu.Entity.StoryManifest? storyManifest = null;

            while (!IsDisposed) //gameloop
            {
                UpdateStoryManifest();

                if (storyManifest == null) 
                {
                    await ProcessMainScreen();
                }

                await ProcessStoryScreen();
            }

            void UpdateStoryManifest()
            {
                storyManifest = null;

                var url = Application.absoluteURL;

#if UNITY_EDITOR
                url = _ctx.Data.TestURL;
#endif

                var storyPath = url.Contains("?") ? url.Split("?").Last() : null;

                if (storyPath != null)
                {
                    storyManifest = new Menu.Entity.StoryManifest
                    {
                        StoryPath = storyPath,
                    };
                }
            }

            async UniTask ProcessMainScreen()
            {
                await loading.Show();

                var mainDone = false;
                var mainScreen = new Menu.Entity(new Menu.Entity.Ctx
                {
                    Data = _ctx.Data.MenuData,
                    ManifestPath = "StoryManifest.json",
                    IsLightTheme = DateTime.Now.Hour > 9 && DateTime.Now.Hour < 20,
                    OnGetBundle = onGetBundle,
                    GetBundle = getBundle,
                    OnGetStory = onGetStory,
                    GetStory = getStory,
                    OnGetTexture = onGetTexture,
                    GetTexture = getTexture,
                    InitDone = () => mainDone = true,
                }, story => { storyManifest = story; }).AddTo(this);
                while (!mainDone) await UniTask.Yield();

                mainScreen.ShowImmediate();

                await loading.Hide();

                while (!storyManifest.HasValue) await UniTask.Yield();

                mainScreen.Dispose();
            }

            async UniTask ProcessStoryScreen()
            {
                await loading.Show();

                var storyDone = false;
                var storyScreen = new Story.Entity(new Story.Entity.Ctx
                {
                    Data = _ctx.Data.StoriesData,
                    StoryPath = storyManifest.Value.StoryPath,
                    OnGetBundle = onGetBundle,
                    GetBundle = getBundle,
                    OnGetStory = onGetStory,
                    GetStory = getStory,
                    OnGetTexture = onGetTexture,
                    GetTexture = getTexture,
                    InitDone = () => storyDone = true,
                }).AddTo(this);

                while (!storyDone) await UniTask.Yield();

                var storyClosed = false;
                storyScreen.ShowImmediate();
                storyScreen.ShowStoryProcess(() => { storyClosed = true; }).Forget();

                await loading.Hide();

                while (!storyClosed) await UniTask.Yield();

                storyScreen.Dispose();
            }
        }
    }
}
