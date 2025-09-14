using Books.Story.View;
using Cysharp.Threading.Tasks;
using Shared.Disposable;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Books.Story 
{
    public sealed class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public Data Data;
            public string StoryPath;

            public IObservable<(UnityEngine.Object bundle, string assetName)> OnGetBundle;
            public ReactiveCommand<(string assetPath, string assetName)> GetBundle;

            public IObservable<(string text, string textPath)> OnGetText;
            public ReactiveCommand<string> GetText;

            public IObservable<(string text, string textPath)> OnLoadText;
            public ReactiveCommand<string> LoadText;

            public ReactiveCommand ClearProgress;
            public ReactiveCommand<(string text, string textPath)> SaveText;

            public IObservable<(Texture2D texture, string key)> OnGetTexture;
            public ReactiveCommand<(string fileName, string key)> GetTexture;

            public IObservable<(AudioClip clip, string fileName)> OnGetMusic;
            public ReactiveCommand<string> GetMusic;

            public IObservable<(string path, string fileName)> OnGetVideo;
            public ReactiveCommand<string> GetVideo;

            public Action InitDone;
            public Action<bool> StoryDone;

            public Func<string, (string header, string attributes, string body)?> ProcessLine;
        }

        private IScreen _screen;
        private readonly Ctx _ctx;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;

            Init();
        }

        private async void Init()
        {
            var bundlesDone = false;
            UnityEngine.Object bundle = null;
            var onGetBundleDisp = _ctx.OnGetBundle.Where(data => data.assetName == _ctx.Data.ScreenName).Subscribe(data =>
            {
                bundle = data.bundle;
                bundlesDone = true;
            });
            _ctx.GetBundle.Execute(("main", _ctx.Data.ScreenName));
            while (!bundlesDone) await UniTask.Yield();
            onGetBundleDisp.Dispose();

            var go = GameObject.Instantiate(bundle as GameObject);
            _screen = go.GetComponent<IScreen>();

            var mainCharacterName = new ReactiveProperty<string>().AddTo(this);
            var characterImagePath = new ReactiveProperty<string>().AddTo(this);
            var locationImagePath = new ReactiveProperty<string>().AddTo(this);
            var storyProcess = new ReactiveProperty<List<int>>().AddTo(this);

            var saveProgress = new ReactiveCommand().AddTo(this);

            var saveDone = false;
            var save = new Save.Entity(new Save.Entity.Ctx
            {
                StoryPath = _ctx.StoryPath,

                MainCharacterName = mainCharacterName,
                CharacterImagePath = characterImagePath,
                LocationImagePath = locationImagePath,
                StoryProcess = storyProcess,

                OnLoadText = _ctx.OnLoadText,
                LoadText = _ctx.LoadText,

                SaveProgress = saveProgress,
                ClearProgress = _ctx.ClearProgress,
                SaveText = _ctx.SaveText,

                OnInitDone = () => saveDone = true,
            }).AddTo(this);

            while (!saveDone) await UniTask.Yield();

            var logic = new Logic(new Logic.Ctx
            {
                Screen = _screen,
                StoryPath = _ctx.StoryPath,

                OnGetText = _ctx.OnGetText,
                GetText = _ctx.GetText,

                SaveProgress = saveProgress,

                OnGetTexture = _ctx.OnGetTexture,
                GetTexture = _ctx.GetTexture,

                OnGetMusic = _ctx.OnGetMusic,
                GetMusic = _ctx.GetMusic,

                OnGetVideo = _ctx.OnGetVideo,
                GetVideo = _ctx.GetVideo,

                StoryDone = _ctx.StoryDone,

                ProcessLine = _ctx.ProcessLine,

                MainCharacterName = mainCharacterName,
                CharacterImagePath = characterImagePath,
                LocationImagePath = locationImagePath,
                StoryProcess = storyProcess,
            }).AddTo(this);

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
