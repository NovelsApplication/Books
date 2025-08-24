using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Shared.Disposable;
using System;
using System.Collections.Generic;
using UniRx;

namespace Books.Story.Save 
{
    public sealed class Entity : BaseDisposable
    {
        public struct Ctx 
        {
            public string StoryPath;

            public ReactiveProperty<string> MainCharacterName;
            public ReactiveProperty<string> CharacterImagePath;
            public ReactiveProperty<string> LocationImagePath;
            public ReactiveProperty<List<int>> StoryProcess;

            public IObservable<(string text, string textPath)> OnLoadText;
            public ReactiveCommand<string> LoadText;

            public IObservable<Unit> SaveProgress;
            public ReactiveCommand<(string text, string textPath)> SaveText;

            public Action OnInitDone;
        }

        private readonly Ctx _ctx;

        public Entity(Ctx ctx) 
        {
            _ctx = ctx;

            _ctx.SaveProgress.Subscribe(_ =>
            {
                _ctx.SaveText.Execute((JsonConvert.SerializeObject(_ctx.StoryProcess.Value), GetStoryLoadPath()));
            }).AddTo(this);

            Init();
        }

        private string GetStoryLoadPath() 
        {
            return $"{_ctx.StoryPath}/SaveStory.json";
        }

        public async void Init()
        {
            _ctx.MainCharacterName.Value = string.Empty;
            _ctx.CharacterImagePath.Value = string.Empty;
            _ctx.LocationImagePath.Value = string.Empty;
            var storyLoadText = string.Empty;

            var storyLoadDone = false;
            var storyLoadPath = GetStoryLoadPath();
            _ctx.OnLoadText.Where(data => data.textPath == storyLoadPath).Subscribe(data =>
            {
                storyLoadText = data.text;
                storyLoadDone = true;
            }).AddTo(this);
            _ctx.LoadText.Execute(storyLoadPath);
            while (!storyLoadDone) await UniTask.Yield();
            _ctx.StoryProcess.Value = !string.IsNullOrEmpty(storyLoadText) ? JsonConvert.DeserializeObject<List<int>>(storyLoadText) : new List<int> { 0 };

            _ctx.OnInitDone.Invoke();
        }
    }
}

