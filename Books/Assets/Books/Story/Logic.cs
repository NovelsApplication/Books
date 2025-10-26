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

            public ReactiveCommand<(string path, string key, ReactiveProperty<Func<UniTask<Texture2D>>> task)> GetTexture;

            public ReactiveCommand<(string path, ReactiveProperty<Func<UniTask<AudioClip>>> task)> GetMusic;

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
            _ctx = ctx;
            _ctx.Screen.HideBubbleImmediate();
            ShowStoryProcess(_ctx.StoryDone).Forget();
        }

        private async UniTask ShowStoryProcess(Action<bool> onDone)
        {
            var logics = GetDelegats<Func<string, string, string, UniTask<bool>>>();
            var storyDone = false;
            var storyPath = $"{_ctx.StoryPath}/Story.json";
            var storyText = string.Empty;
            var onGetTextDisp = _ctx.OnGetText.Where(data => data.textPath == storyPath).Subscribe(data =>
            {
                storyText = data.text;
                storyDone = true;
            });
            _ctx.GetText.Execute(storyPath);
            while (!storyDone) await UniTask.Yield();
            onGetTextDisp.Dispose();

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
                var locationKey = "location";

                var task = new ReactiveProperty<Func<UniTask<Texture2D>>>();
                _ctx.GetTexture.Execute((_ctx.LocationImagePath.Value, locationKey, task));
                _locationImage = await task.Value.Invoke();
                task.Dispose();

                await _ctx.Screen.ShowLocation(_locationImage);
            }

            _characterImage = null;
            if (!string.IsNullOrEmpty(_ctx.CharacterImagePath.Value))
            {
                var characterKey = "char";

                var task = new ReactiveProperty<Func<UniTask<Texture2D>>>();
                _ctx.GetTexture.Execute((_ctx.CharacterImagePath.Value, characterKey, task));
                _characterImage = await task.Value.Invoke();
                task.Dispose();
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
                    _ctx.CharacterImagePath.Value = $"{_ctx.StoryPath}/Characters/{lineData.Value.attributes.Replace(" ", "_")}.png";
                    var characterKey = "char";

                    var task = new ReactiveProperty<Func<UniTask<Texture2D>>>();
                    _ctx.GetTexture.Execute((_ctx.CharacterImagePath.Value, characterKey, task));
                    _characterImage = await task.Value.Invoke();
                    task.Dispose();
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

