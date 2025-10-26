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
        [Serializable]
        private struct SaveData
        {
            public string MainCharacterName;
            public string CharacterImagePath;
            public string LocationImagePath;
            public List<int> StoryProcess;
        }

        public struct Ctx 
        {
            public string StoryPath;

            public ReactiveProperty<string> MainCharacterName;
            public ReactiveProperty<string> CharacterImagePath;
            public ReactiveProperty<string> LocationImagePath;
            public ReactiveProperty<List<int>> StoryProcess;

            public ReactiveCommand<(string path, ReactiveProperty<Func<string>> task)> LoadText;

            public IObservable<Unit> SaveProgress;
            public IObservable<Unit> ClearProgress;
            public ReactiveCommand<(string text, string textPath)> SaveText;

            public Action OnInitDone;
        }

        private readonly Ctx _ctx;

        public Entity(Ctx ctx) 
        {
            _ctx = ctx;

            _ctx.SaveProgress.Subscribe(_ =>
            {
                var saveData = new SaveData
                {
                    MainCharacterName = _ctx.MainCharacterName.Value,
                    CharacterImagePath = _ctx.CharacterImagePath.Value,
                    LocationImagePath = _ctx.LocationImagePath.Value,
                    StoryProcess = _ctx.StoryProcess.Value,
                };
                _ctx.SaveText.Execute((JsonConvert.SerializeObject(saveData), GetStoryLoadPath()));
            }).AddTo(this);

            _ctx.ClearProgress.Subscribe(_ =>
            {
                _ctx.SaveText.Execute((string.Empty, GetStoryLoadPath()));
            }).AddTo(this);

            Init();
        }

        private string GetStoryLoadPath() 
        {
            return $"{_ctx.StoryPath}/SaveStory.json";
        }

        public void Init()
        {
            _ctx.MainCharacterName.Value = string.Empty;
            _ctx.CharacterImagePath.Value = string.Empty;
            _ctx.LocationImagePath.Value = string.Empty;
            _ctx.StoryProcess.Value = new List<int> { 0 };

            var storyLoadPath = GetStoryLoadPath();
            var loadTask = new ReactiveProperty<Func<string>>();
            _ctx.LoadText.Execute((storyLoadPath, loadTask));
            var storyLoadText = loadTask.Value.Invoke();

            if (!string.IsNullOrEmpty(storyLoadText)) 
            {
                var saveData = JsonConvert.DeserializeObject<SaveData>(storyLoadText);

                _ctx.MainCharacterName.Value = saveData.MainCharacterName;
                _ctx.CharacterImagePath.Value = saveData.CharacterImagePath;
                _ctx.LocationImagePath.Value = saveData.LocationImagePath;
                _ctx.StoryProcess.Value = saveData.StoryProcess;
            }

            _ctx.OnInitDone.Invoke();
        }
    }
}

