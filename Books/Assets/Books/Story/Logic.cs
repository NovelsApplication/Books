using Cysharp.Threading.Tasks;
using Shared.Disposable;
using Shared.LocalCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Books.Story
{
    internal partial class Logic : BaseDisposable
    {
        public struct Ctx
        {
            public View.IScreen Screen;
            public string RootFolderName;
            public string StoryText;
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

            var story = new Ink.Runtime.Story(_ctx.StoryText);
            story.Continue();

            _ctx.Screen.SetCloseAction(onDone);
            _ctx.Screen.HideLocationImmediate();

            _mainCharacter = string.Empty;
            while (!IsDisposed)
            {
                _ctx.Screen.HideBubbleImmediate();

                if (!story.canContinue)
                    break;

                if (!story.Continue().TryProcessLine(out var header, out var attributes, out var body))
                    continue;

                if (Enum.TryParse<LogicIdx>(header, true, out var logicId)
                    && logics.TryGetValue(logicId, out var func)
                    && await func.Invoke(header, attributes, body))
                    continue;

                _characterImage = null;
                if (!string.IsNullOrEmpty(attributes)) 
                {
                    var characterName = $"{_ctx.RootFolderName}/Characters/{attributes.Replace(" ", "_")}.png";
                    _characterImage = await Cacher.GetTextureAsync(characterName, "char");
                }

                var buttons = story.currentChoices.Select(c => (c.text, c.index)).ToArray();
                var clicked = false;
                await _ctx.Screen.ShowBubble((index) => 
                {
                    if (index >= 0) story.ChooseChoiceIndex(index);
                    clicked = true;
                }, _mainCharacter, header, body, _characterImage, buttons);

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

