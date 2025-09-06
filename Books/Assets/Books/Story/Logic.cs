using Cysharp.Threading.Tasks;
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

            public ReactiveCommand SaveProgress;

            public IObservable<(Texture2D texture, string key)> OnGetTexture;
            public ReactiveCommand<(string fileName, string key)> GetTexture;

            public IObservable<(AudioClip clip, string fileName)> OnGetMusic;
            public ReactiveCommand<string> GetMusic;

            public IObservable<(string path, string fileName)> OnGetVideo;
            public ReactiveCommand<string> GetVideo;

            public Action<bool> StoryDone;

            public Func<string, (string header, string attributes, string body)?> ProcessLine;

            public ReactiveProperty<string> MainCharacterName;
            public ReactiveProperty<string> CharacterImagePath;
            public ReactiveProperty<string> LocationImagePath;
            public ReactiveProperty<List<int>> StoryProcess;
        }

        private readonly Ctx _ctx;
        private Texture2D _characterImage;

        public Logic(Ctx ctx)
        {
            Debug.Log("Logic -2");
            _ctx = ctx;
            Debug.Log("Logic -1");
            _ctx.Screen.HideBubbleImmediate();
            Debug.Log("Logic 0");
            ShowStoryProcess(_ctx.StoryDone).Forget();
        }

        private async UniTask ShowStoryProcess(Action<bool> onDone)
        {
            Debug.Log("Logic 1");
            var logics = GetDelegats<Func<string, string, string, UniTask<bool>>>();
            Debug.Log("Logic 2");
            var storyDone = false;
            var storyPath = $"{_ctx.StoryPath}/Story.json";
            var storyText = string.Empty;
            Debug.Log("Logic 3");
            _ctx.OnGetText.Where(data => data.textPath == storyPath).Subscribe(data =>
            {
                storyText = data.text;
                storyDone = true;
            }).AddTo(this);
            _ctx.GetText.Execute(storyPath);
            while (!storyDone) await UniTask.Yield();

            Debug.Log("Logic 4");

            var story = new Ink.Runtime.Story(storyText);
            story.Continue();

            _ctx.Screen.SetCloseAction(onDone);
            _ctx.Screen.HideLocationImmediate();

            foreach (var index in _ctx.StoryProcess.Value) 
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

            _locationImage = null;
            if (!string.IsNullOrEmpty(_ctx.LocationImagePath.Value)) 
            {
                var locationDone = false;
                var locationKey = "location";

                _ctx.OnGetTexture.Where(data => data.key == locationKey).Subscribe(data =>
                {
                    _locationImage = data.texture;
                    locationDone = true;
                }).AddTo(this);
                _ctx.GetTexture.Execute((_ctx.LocationImagePath.Value, locationKey));

                while (!locationDone) await UniTask.Yield();

                await _ctx.Screen.ShowLocation(_locationImage);
            }

            _characterImage = null;
            if (!string.IsNullOrEmpty(_ctx.CharacterImagePath.Value))
            {
                var characterDone = false;
                var characterKey = "char";

                _ctx.OnGetTexture.Where(data => data.key == characterKey).Subscribe(data =>
                {
                    _characterImage = data.texture;
                    characterDone = true;
                }).AddTo(this);
                _ctx.GetTexture.Execute((_ctx.CharacterImagePath.Value, characterKey));
                while (!characterDone) await UniTask.Yield();
            }

            while (!IsDisposed)
            {
                _ctx.Screen.HideBubbleImmediate();

                if (!story.canContinue)
                    break;

                var lineData = _ctx.ProcessLine.Invoke(story.Continue());
                _ctx.StoryProcess.Value[^1]++;

                if (!lineData.HasValue) continue;

                if (Enum.TryParse<LogicIdx>(lineData.Value.header, true, out var logicId)
                    && logics.TryGetValue(logicId, out var func)
                    && await func.Invoke(lineData.Value.header, lineData.Value.attributes, lineData.Value.body))
                    continue;

                _characterImage = null;
                if (!string.IsNullOrEmpty(lineData.Value.attributes)) 
                {
                    var characterDone = false;
                    _ctx.CharacterImagePath.Value = $"{_ctx.StoryPath}/Characters/{lineData.Value.attributes.Replace(" ", "_")}.png";
                    var characterKey = "char";

                    _ctx.OnGetTexture.Where(data => data.key == characterKey).Subscribe(data =>
                    {
                        _characterImage = data.texture;
                        characterDone = true;
                    }).AddTo(this);
                    _ctx.GetTexture.Execute((_ctx.CharacterImagePath.Value, characterKey));
                    while (!characterDone) await UniTask.Yield();
                }

                var buttons = story.currentChoices.Select(c => (c.text, c.index)).ToArray();
                var clicked = false;
                await _ctx.Screen.ShowBubble((index) =>
                {
                    if (index >= 0)
                    {
                        story.ChooseChoiceIndex(index);
                        _ctx.StoryProcess.Value.Add(index);
                        _ctx.StoryProcess.Value.Add(0);
                    }
                    _ctx.SaveProgress.Execute();
                    clicked = true;
                }, _ctx.MainCharacterName.Value, lineData.Value.header, lineData.Value.body, _characterImage, buttons);

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

