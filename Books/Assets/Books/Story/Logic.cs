using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Shared.Disposable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UniRx;
using UnityEngine;

namespace Books.Story
{
    internal partial class Logic : BaseDisposable
    {
        public struct Ctx
        {
            public View.IScreen Screen;
            public string StoryPath;

            public IObservable<(string text, string textPath)> OnGetText;
            public ReactiveCommand<string> GetText;

            public IObservable<(string text, string textPath)> OnLoadText;
            public ReactiveCommand<string> LoadText;

            public ReactiveCommand<(string text, string textPath)> SaveText;

            public IObservable<(Texture2D texture, string key)> OnGetTexture;
            public ReactiveCommand<(string fileName, string key)> GetTexture;

            public IObservable<(AudioClip clip, string fileName)> OnGetMusic;
            public ReactiveCommand<string> GetMusic;

            public Action StoryDone;

            public Func<string, (string header, string attributes, string body)?> ProcessLine;
        }

        private readonly ReactiveProperty<string> _mainCharacterName;
        private readonly ReactiveProperty<string> _characterImagePath;
        private readonly ReactiveProperty<string> _locationImagePath;

        private List<int> _storyPath;

        private readonly Ctx _ctx;
        private Texture2D _characterImage;

        public Logic(Ctx ctx)
        {
            _mainCharacterName = new ReactiveProperty<string>().AddTo(this);
            _characterImagePath = new ReactiveProperty<string>().AddTo(this);
            _locationImagePath = new ReactiveProperty<string>().AddTo(this);

            _ctx = ctx;

            _ctx.Screen.HideBubbleImmediate();

            ShowStoryProcess(_ctx.StoryDone).Forget();
        }

        private async UniTask ShowStoryProcess(Action onDone)
        {
            var logics = GetDelegats<Func<string, string, string, UniTask<bool>>>();

            var storyDone = false;
            var storyPath = $"{_ctx.StoryPath}/Story.json";
            var storyText = string.Empty;

            _ctx.OnGetText.Where(data => data.textPath == storyPath).Subscribe(data =>
            {
                storyText = data.text;
                storyDone = true;
            }).AddTo(this);
            _ctx.GetText.Execute(storyPath);
            while (!storyDone) await UniTask.Yield();

            var story = new Ink.Runtime.Story(storyText);
            story.Continue();

            _ctx.Screen.SetCloseAction(onDone);
            _ctx.Screen.HideLocationImmediate();

            //asdasd
            _mainCharacterName.Value = string.Empty;
            _characterImagePath.Value = string.Empty;
            _locationImagePath.Value = string.Empty;
            var storyLoadText = string.Empty;

            var storyLoadDone = false;
            var storyLoadPath = $"{_ctx.StoryPath}/SaveStory.json";
            _ctx.OnLoadText.Where(data => data.textPath == storyLoadPath).Subscribe(data =>
            {
                storyLoadText = data.text;
                storyLoadDone = true;
            }).AddTo(this);
            _ctx.LoadText.Execute(storyLoadPath);
            while (!storyLoadDone) await UniTask.Yield();
            _storyPath = !string.IsNullOrEmpty(storyLoadText) ? JsonConvert.DeserializeObject<List<int>>(storyLoadText) : new List<int> { 0 };
            //asdasdas

            Debug.Log(storyLoadText);

            foreach (var index in _storyPath) 
            {
                if (story.currentChoices.Count > 0) 
                {
                    story.ChooseChoiceIndex(index);
                }
                else 
                {
                    for (var i = 0; i < index; i++)
                        story.Continue();
                } 
            }

            while (!IsDisposed)
            {
                _ctx.Screen.HideBubbleImmediate();

                if (!story.canContinue)
                    break;

                var lineData = _ctx.ProcessLine.Invoke(story.Continue());
                _storyPath[^1]++;

                if (!lineData.HasValue) continue;

                if (Enum.TryParse<LogicIdx>(lineData.Value.header, true, out var logicId)
                    && logics.TryGetValue(logicId, out var func)
                    && await func.Invoke(lineData.Value.header, lineData.Value.attributes, lineData.Value.body))
                    continue;

                _characterImage = null;
                if (!string.IsNullOrEmpty(lineData.Value.attributes)) 
                {
                    var characterDone = false;
                    _characterImagePath.Value = $"{_ctx.StoryPath}/Characters/{lineData.Value.attributes.Replace(" ", "_")}.png";
                    var characterKey = "char";

                    _ctx.OnGetTexture.Where(data => data.key == characterKey).Subscribe(data =>
                    {
                        _characterImage = data.texture;
                        characterDone = true;
                    }).AddTo(this);
                    _ctx.GetTexture.Execute((_characterImagePath.Value, characterKey));
                    while (!characterDone) await UniTask.Yield();
                }

                var buttons = story.currentChoices.Select(c => (c.text, c.index)).ToArray();
                var clicked = false;
                await _ctx.Screen.ShowBubble((index) =>
                {
                    if (index >= 0)
                    {
                        story.ChooseChoiceIndex(index);
                        _storyPath.Add(index);
                        _storyPath.Add(0);
                    }
                    _ctx.SaveText.Execute((JsonConvert.SerializeObject(_storyPath), storyLoadPath));
                    clicked = true;
                }, _mainCharacterName.Value, lineData.Value.header, lineData.Value.body, _characterImage, buttons);

                while (!clicked && !IsDisposed)
                    await UniTask.Yield();
            }
        }

        private Dictionary<LogicIdx, T> GetDelegats<T>() where T : Delegate
        {
            var flags = BindingFlags.NonPublic | BindingFlags.Instance;
            return typeof(Logic).GetMethods(flags)
                .Where(m => m.GetCustomAttribute<LogicAttribute>() != null)
                .SelectMany(m =>
                {
                    var attr = m.GetCustomAttribute<LogicAttribute>();
                    var del = (T)Delegate.CreateDelegate(typeof(T), this, m);
                    return attr.Idx.Select(n => (n, del));
                })
                .ToDictionary(m => m.n, m => m.del);
        }
    }
}

