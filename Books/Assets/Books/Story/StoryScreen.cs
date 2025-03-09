using Cysharp.Threading.Tasks;
using Shared.Disposable;
using Shared.Reactive;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

                    UpdateScreen();

                    _ctx.Data.TextPrefab.gameObject.SetActive(false);
                    _ctx.Data.ButtonPrefab.gameObject.SetActive(false);
                }

                private void UpdateScreen()
                {
                    
                }

                public async UniTask LoadStory()
                {
                    _story = new Ink.Runtime.Story(_ctx.Data.TextAsset.text);
                }

                public async UniTask ShowStory()
                {
                    var storyInProgress = true;
                    while (storyInProgress) 
                    {
                        ClearAll();
                        while (_story.canContinue)
                        {
                            ClearAll();

                            var text = _story.Continue();
                            text = text.Trim();
                            Debug.Log(text);
                            CreateContentView(text);

                            while (!Input.GetMouseButtonUp(0))
                                await UniTask.NextFrame();

                            await UniTask.Delay(100);
                        }

                        if (_story.currentChoices.Count > 0)
                        {
                            var waitChoice = true;
                            for (int i = 0; i < _story.currentChoices.Count; i++)
                            {
                                var choice = _story.currentChoices[i];
                                var button = CreateChoiceView(choice.text.Trim());

                                button.onClick.RemoveAllListeners();
                                button.onClick.AddListener(() =>
                                {
                                    _story.ChooseChoiceIndex(choice.index);
                                    waitChoice = false;
                                });
                            }
                            while (waitChoice)
                                await UniTask.NextFrame();
                        }
                        else
                        {
                            ClearAll();
                            _story = new Ink.Runtime.Story(_ctx.Data.TextAsset.text);
                        }
                    }
                }

                private void CreateContentView(string text)
                {
                    var storyText = UnityEngine.Object.Instantiate(_ctx.Data.TextPrefab);
                    storyText.text = text;
                    storyText.transform.SetParent(_ctx.Data.TextPrefab.transform.parent, false);
                    storyText.gameObject.SetActive(true);

                    _units.Push(storyText.gameObject);
                }

                private Button CreateChoiceView(string text)
                {
                    var choice = UnityEngine.Object.Instantiate(_ctx.Data.ButtonPrefab);
                    choice.transform.SetParent(_ctx.Data.ButtonPrefab.transform.parent, false);
                    choice.gameObject.SetActive(true);

                    var choiceText = choice.GetComponentInChildren<TMP_Text>();
                    choiceText.text = text;

                    var layoutGroup = choice.GetComponent<HorizontalLayoutGroup>();

                    _units.Push(choice.gameObject);

                    return choice;
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
                }).AddTo(this);
            }

            public async UniTask LoadStory()
            {
                await _logic.LoadStory();
            }

            public async UniTask ShowStory() 
            {
                await _logic.ShowStory();
            }
        }

        [Serializable]
        public struct Data
        {
            [SerializeField] private TMP_Text _textPrefab;
            [SerializeField] private Button _buttonPrefab;
            [SerializeField] private TextAsset _textAsset;

            public readonly TMP_Text TextPrefab => _textPrefab;
            public readonly Button ButtonPrefab => _buttonPrefab;
            public readonly TextAsset TextAsset => _textAsset;
        }
    }
}

