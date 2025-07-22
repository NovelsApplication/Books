using Cysharp.Threading.Tasks;
using Shared.Disposable;
using Shared.LocalCache;
using System;

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
            _loading = new Loading.Entity(new Loading.Entity.Ctx
            {
                Data = _ctx.Data.LoadingData,
            }).AddTo(this);
            await _loading.Init();
            _loading.ShowImmediate();

            while (!IsDisposed) 
            {
                Menu.Entity.StoryManifest? storyManifest = null;
                using (var mainScreen = await ShowMainMenu(story => { storyManifest = story; }))
                {
                    while (!storyManifest.HasValue) await UniTask.Yield();
                }

                var done = false;
                using (var storyScreen = await ShowStory(storyManifest.Value.StoryPath, () => { done = true; })) 
                {
                    while (!done) await UniTask.Yield();
                }
            }
        }

        private async UniTask<Menu.Entity> ShowMainMenu(Action<Menu.Entity.StoryManifest> onClick) 
        {
            await _loading.Show();

            var mainScreen = new Menu.Entity(new Menu.Entity.Ctx 
            {
                IsLightTheme = DateTime.Now.Hour > 9 && DateTime.Now.Hour < 20,
                Data = _ctx.Data.MenuData,
                ManifestPath = "StoryManifest.json",
            }).AddTo(this);
            await mainScreen.Init(onClick);
            await mainScreen.Show();

            await _loading.Hide();

            return mainScreen;
        }

        private async UniTask<Story.Entity> ShowStory(string storyPath, Action onDone) 
        {
            await _loading.Show();

            var storyText = await Cacher.GetTextAsync($"{storyPath}/Story.json");

            var storyScreen = new Story.Entity(new Story.Entity.Ctx
            {
                Data = _ctx.Data.StoriesData,
                RootFolderName = storyPath,
                StoryText = storyText,
            }).AddTo(this);
            await storyScreen.Init();

            storyScreen.ShowImmediate();
            storyScreen.ShowStoryProcess(onDone).Forget();

            await _loading.Hide();

            return storyScreen;
        }
    }
}
