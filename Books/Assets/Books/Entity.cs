using Cysharp.Threading.Tasks;
using Shared.Disposable;
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
            public IObservable<Unit> ClearCash;
            public Data Data;
        }

        private readonly Ctx _ctx;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;

            AsyncProcess().Forget();
        }

        private async UniTask AsyncProcess()
        {
            var onGetBundle = new ReactiveCommand<(UnityEngine.Object bundle, string assetName)>().AddTo(this);
            var getBundle = new ReactiveCommand<(string assetPath, string assetName)>().AddTo(this);

            var getText = new ReactiveCommand<(string path, ReactiveProperty<Func<UniTask<string>>> task)>().AddTo(this);
            var loadText = new ReactiveCommand<(string path, ReactiveProperty<Func<string>> task)>().AddTo(this);
            var saveText = new ReactiveCommand<(string text, string textPath)>().AddTo(this);

            var getTexture = new ReactiveCommand<(string path, string key, ReactiveProperty<Func<UniTask<Texture2D>>> task)>().AddTo(this);

            var getMusic = new ReactiveCommand<(string path, ReactiveProperty<Func<UniTask<AudioClip>>> task)>().AddTo(this);

            var cash = new Shared.Cash.Entity(new Shared.Cash.Entity.Ctx 
            {
                OnGetBundle = onGetBundle,
                GetBundle = getBundle,

                GetText = getText,

                LoadText = loadText,

                SaveText = saveText,

                GetTexture = getTexture,

                GetMusic = getMusic,

                ClearCash = _ctx.ClearCash,
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

                if (storyManifest == null) 
                {
                    await loading.Show();

                    var mainDone = false;
                    var mainScreen = new Menu.Entity(new Menu.Entity.Ctx
                    {
                        Data = _ctx.Data.MenuData,
                        ManifestPath = "Configs/StoryManifest.json",
                        IsLightTheme = DateTime.Now.Hour > 9 && DateTime.Now.Hour < 20,
                        OnGetBundle = onGetBundle,
                        GetBundle = getBundle,
                        GetText = getText,
                        GetTexture = getTexture,
                        InitDone = () => mainDone = true,
                        ProcessLine = ProcessLine,
                    }, story => { storyManifest = story; }).AddTo(this);
                    while (!mainDone) await UniTask.Yield();

                    mainScreen.ShowImmediate();

                    await loading.Hide();

                    while (!storyManifest.HasValue) await UniTask.Yield();

                    mainScreen.Dispose();
                }

                await loading.Show();

                var clearStoryProgress = new ReactiveCommand().AddTo(this);

                var storyClosed = false;
                var storyInitDone = false;
                var storyScreen = new Story.Entity(new Story.Entity.Ctx
                {
                    Data = _ctx.Data.StoriesData,
                    StoryPath = storyManifest.Value.StoryPath,
                    OnGetBundle = onGetBundle,
                    GetBundle = getBundle,
                    GetText = getText,
                    LoadText = loadText,
                    SaveText = saveText,
                    ClearProgress = clearStoryProgress,
                    GetTexture = getTexture,
                    GetMusic = getMusic,
                    InitDone = () => storyInitDone = true,
                    StoryDone = isClearSave => 
                    { 
                        storyClosed = true;
                        if (isClearSave) clearStoryProgress.Execute();
                    },
                    ProcessLine = ProcessLine,
                }).AddTo(this);

                while (!storyInitDone) await UniTask.Yield();
                
                storyScreen.ShowImmediate();

                await loading.Hide();

                while (!storyClosed) await UniTask.Yield();

                storyScreen.Dispose();
            }
        }

        public (string header, string attributes, string body)? ProcessLine(string line)
        {
            line = line.Trim();

            if (string.IsNullOrEmpty(line)) return null;

            var rawTexts = line.Split(":");
            var header = rawTexts.Length > 1 ?
                rawTexts[0].Split("(").FirstOrDefault().Trim() :
                string.Empty;
            var attributes = rawTexts[0].Contains("(") ?
                rawTexts[0].Split("(").LastOrDefault().Split(")").FirstOrDefault().Trim() :
                string.Empty;
            var body = rawTexts.Length > 1 ?
                rawTexts[1].Trim() :
                line;

            return (header, attributes, body);
        }
    }
}
