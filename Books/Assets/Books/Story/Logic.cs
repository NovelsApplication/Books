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

            public IObservable<(Texture2D texture, string key)> OnGetTexture;
            public ReactiveCommand<(string fileName, string key)> GetTexture;

            public IObservable<(AudioClip clip, string fileName)> OnGetMusic;
            public ReactiveCommand<string> GetMusic;

            public Func<string, (string header, string attributes, string body)?> ProcessLine;
        }

        private readonly Ctx _ctx;

        private string _mainCharacter;
        private Texture2D _characterImage;

        public Logic(Ctx ctx)
        {
            _ctx = ctx;

            _ctx.Screen.HideBubbleImmediate();
        }

        public async UniTask ShowStoryProcess(Action onDone)
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

            _mainCharacter = string.Empty;
            while (!IsDisposed)
            {
                _ctx.Screen.HideBubbleImmediate();

                if (!story.canContinue)
                    break;

                var lineData = _ctx.ProcessLine.Invoke(story.Continue());
                if (!lineData.HasValue) continue;

                if (Enum.TryParse<LogicIdx>(lineData.Value.header, true, out var logicId)
                    && logics.TryGetValue(logicId, out var func)
                    && await func.Invoke(lineData.Value.header, lineData.Value.attributes, lineData.Value.body))
                    continue;

                _characterImage = null;
                if (!string.IsNullOrEmpty(lineData.Value.attributes)) 
                {
                    var characterDone = false;
                    var characterPath = $"{_ctx.StoryPath}/Characters/{lineData.Value.attributes.Replace(" ", "_")}.png";
                    var characterKey = "char";

                    _ctx.OnGetTexture.Where(data => data.key == characterKey).Subscribe(data =>
                    {
                        _characterImage = data.texture;
                        characterDone = true;
                    }).AddTo(this);
                    _ctx.GetTexture.Execute((characterPath, characterKey));
                    while (!characterDone) await UniTask.Yield();
                }

                var buttons = story.currentChoices.Select(c => (c.text, c.index)).ToArray();
                var clicked = false;
                await _ctx.Screen.ShowBubble((index) => 
                {
                    if (index >= 0) story.ChooseChoiceIndex(index);
                    clicked = true;
                }, _mainCharacter, lineData.Value.header, lineData.Value.body, _characterImage, buttons);

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

