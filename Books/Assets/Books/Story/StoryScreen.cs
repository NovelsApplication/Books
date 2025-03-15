using Cysharp.Threading.Tasks;
using Shared.Disposable;
using Shared.Reactive;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Books.Story.StoryBubble;

namespace Books.Story
{
    public sealed class StoryScreen
    {
        public class Entity : BaseDisposable 
        {
            private class Logic : BaseDisposable
            {
                public struct Ctx
                {
                    public IReadOnlyReactiveCommand<float> OnUpdate;
                    public Data Data;
                    public string StoryText;
                }

                private readonly Ctx _ctx;

                private Stack<GameObject> _units;

                private int _screenWidth;
                private int _screenHeight;

                private Ink.Runtime.Story _story;

                public Logic(Ctx ctx)
                {
                    _ctx = ctx;

                    _units = new();

                    _ctx.OnUpdate.Subscribe(deltaTime =>
                    {
                        if (_screenWidth != Screen.width || _screenHeight != Screen.height)
                        {
                            _screenWidth = Screen.width;
                            _screenHeight = Screen.height;

                            UpdateScreen();
                        }
                    }).AddTo(this);

                    _ctx.Data.StoryBubble.gameObject.SetActive(false);
                    _ctx.Data.RootTransform.gameObject.SetActive(true);

                    UpdateScreen();
                }

                private void UpdateScreen()
                {
                    
                }

                public async UniTask LoadStory()
                {
                    _story = new Ink.Runtime.Story(_ctx.StoryText);
                }

                public async UniTask ShowStory()
                {
                    _story.Continue();

                    var mainCharacter = string.Empty;
                    var storyInProgress = true;
                    while (storyInProgress) 
                    {
                        ClearAll();
                        StoryBubble storyBubble = null;
                        while (_story.canContinue)
                        {
                            ClearAll();
                            storyBubble = null;

                            var text = _story.Continue();
                            text = text.Trim();

                            if (string.IsNullOrEmpty(text)) continue;

                            var rawTexts = text.Split(":");
                            var header = rawTexts.Length > 1 ?
                                rawTexts[0].Split("(").FirstOrDefault() :
                                string.Empty;
                            var attributes = rawTexts[0].Contains("(") ?
                                rawTexts[0].Split("(").LastOrDefault().Split(")").FirstOrDefault() :
                                string.Empty;
                            var body = rawTexts.Length > 1 ?
                                rawTexts[1] :
                                text;

                            var headerForLogic = header.Trim().ToLower();
                            switch (headerForLogic) 
                            {
                                case "аннотация":
                                    continue;
                                case "камера":
                                    continue;
                                case "жанры":
                                    continue;
                                case "статы":
                                    continue;
                                case "локация":
                                    continue;
                                case "музыка":
                                    continue;
                                case "звук":
                                    continue;
                                case "звуки окружения":
                                    continue;
                                case "уведомление":
                                    continue;
                                case "ожидание":
                                    if (int.TryParse(body, out var waitTime)) 
                                    {
                                        await UniTask.Delay(waitTime * 1000);
                                    }
                                    continue;
                                case "кат-сцена":
                                    continue;
                                case "клавиатура":
                                    mainCharacter = body.Trim();
                                    continue;
                            }

                            var side = Side.Right;
                            if (headerForLogic == mainCharacter.ToLower()) side = Side.Left;
                            else if (headerForLogic == "...") side = Side.Center;

                            storyBubble = CreateStoryBubble(side, header, body);

                            if (_story.canContinue) 
                            {
                                while (!Input.GetMouseButtonUp(0))
                                    await UniTask.NextFrame();

                                await UniTask.Delay(100);
                            }
                        }

                        if (_story.currentChoices.Count > 0)
                        {
                            var waitChoice = true;

                            if (storyBubble == null)
                                storyBubble = CreateStoryBubble(Side.Center, string.Empty, string.Empty);

                            var buttons = _story.currentChoices.Select(c => (c.text, c.index)).ToArray();
                            storyBubble.UpdateButtons(idx => 
                            {
                                _story.ChooseChoiceIndex(idx);
                                waitChoice = false;
                            }, buttons);

                            if (buttons.Length == 1 && buttons[0].text.Trim().ToLower() == "играть") 
                            {
                                _story.ChooseChoiceIndex(buttons[0].index);
                                waitChoice = false;
                            }
                            else 
                            {
                                while (waitChoice)
                                    await UniTask.NextFrame();

                                await UniTask.Delay(100);
                            }
                        }
                        else
                        {
                            ClearAll();
                            _story = new Ink.Runtime.Story(_ctx.StoryText);
                        }
                    }
                }

                private StoryBubble CreateStoryBubble(Side side, string header, string body)
                {
                    var storyBubble = UnityEngine.Object.Instantiate(_ctx.Data.StoryBubble);
                    storyBubble.transform.SetParent(_ctx.Data.StoryBubble.transform.parent, false);
                    storyBubble.gameObject.SetActive(true);
                    storyBubble.UpdateText(side, header, body);

                    _units.Push(storyBubble.gameObject);

                    return storyBubble;
                }

                protected override UniTask OnAsyncDispose()
                {
                    ClearAll();
                    return base.OnAsyncDispose();
                }

                private void ClearAll() 
                {
                    while (_units.TryPop(out var unitGO))
                        UnityEngine.Object.Destroy(unitGO);
                }
            }

            public struct Ctx
            {
                public IReadOnlyReactiveCommand<float> OnUpdate;
                public Data Data;
                public string StoryText;
            }

            private readonly Ctx _ctx;

            private readonly Logic _logic;

            public Entity(Ctx ctx)
            {
                _ctx = ctx;

                _logic = new Logic(new Logic.Ctx
                {
                    OnUpdate = _ctx.OnUpdate,
                    Data = _ctx.Data,
                    StoryText = _ctx.StoryText,
                }).AddTo(this);
            }

            public async UniTask LoadStory() => await _logic.LoadStory();

            public async UniTask ShowStory() => await _logic.ShowStory();
        }

        [Serializable]
        public struct Data
        {
            [SerializeField] private RectTransform _rootTransform;
            [SerializeField] private StoryBubble _storyBubble;

            public readonly RectTransform RootTransform => _rootTransform;
            public readonly StoryBubble StoryBubble => _storyBubble;
        }
    }
}

