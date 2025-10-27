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

            public ReactiveCommand<(string path, string name, ReactiveProperty<Func<UniTask<UnityEngine.Object>>> task)> GetBundle;

            public ReactiveCommand<(string path, ReactiveProperty<Func<UniTask<string>>> task)> GetText;
            public ReactiveCommand<(string path, ReactiveProperty<Func<string>> task)> LoadText;
            public ReactiveCommand<(string path, string text)> SaveText;
            public ReactiveCommand ClearProgress;

            public ReactiveCommand<(string path, string key, ReactiveProperty<Func<UniTask<Texture2D>>> task)> GetTexture;

            public ReactiveCommand<(string path, ReactiveProperty<Func<UniTask<AudioClip>>> task)> GetMusic;

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
            var task = new ReactiveProperty<Func<UniTask<UnityEngine.Object>>>();
            _ctx.GetBundle.Execute(("main", _ctx.Data.ScreenName, task));
            var bundle = await task.Value.Invoke();
            task.Dispose();

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

                GetText = _ctx.GetText,

                SaveProgress = saveProgress,

                GetTexture = _ctx.GetTexture,

                GetMusic = _ctx.GetMusic,

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
