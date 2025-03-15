using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Shared.Disposable;
using Shared.Reactive;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Books.UI 
{
    public sealed class BooksScreen
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

                private UniTaskCompletionSource<string> _completeSource;

                public Logic(Ctx ctx, UniTaskCompletionSource<string> completeSource)
                {
                    _ctx = ctx;

                    _completeSource = completeSource;

                    _units = new ();

                    _ctx.OnUpdate.Subscribe(deltaTime => 
                    {
                        if (_screenWidth != Screen.width || _screenHeight != Screen.height)
                        {
                            _screenWidth = Screen.width;
                            _screenHeight = Screen.height;

                            UpdateScreen();
                        }
                    }).AddTo(this);

                    _ctx.Data.RootTransform.gameObject.SetActive(true);

                    UpdateScreen();
                }

                private void UpdateScreen()
                {
                    var verticalGroup = _ctx.Data.RootTransform.GetComponentInChildren<VerticalLayoutGroup>(true);
                    verticalGroup.padding.top = _ctx.Data.ScrollPadding;
                    verticalGroup.padding.right = _ctx.Data.ScrollPadding;
                    verticalGroup.padding.bottom = _ctx.Data.ScrollPadding;
                    verticalGroup.padding.left = _ctx.Data.ScrollPadding;
                    verticalGroup.spacing = _ctx.Data.ScrollPadding;

                    _ctx.Data.BookScrollLayout.minHeight = _ctx.Data.RootTransform.rect.width - _ctx.Data.ScrollPadding * 2;
                    var horizontalGroup = _ctx.Data.BookScrollLayout.GetComponentInChildren<HorizontalLayoutGroup>(true);
                    horizontalGroup.spacing = _ctx.Data.ScrollPadding * 2;
                    for (var j = 0; j < horizontalGroup.transform.childCount; j++)
                    {
                        if (!horizontalGroup.transform.GetChild(j).TryGetComponent<LayoutElement>(out var horizontalLayoutElement)) continue;
                        horizontalLayoutElement.minWidth = _ctx.Data.RootTransform.rect.width - _ctx.Data.ScrollPadding * 2;
                    }

                    _ctx.Data.BookLayout.padding.top = _ctx.Data.ScrollPadding;
                    _ctx.Data.BookLayout.padding.right = _ctx.Data.ScrollPadding;
                    _ctx.Data.BookLayout.padding.bottom = _ctx.Data.ScrollPadding;
                    _ctx.Data.BookLayout.padding.left = _ctx.Data.ScrollPadding;
                    _ctx.Data.BookLayout.spacing = Vector2.one * _ctx.Data.ScrollPadding;

                    _ctx.Data.BookLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;

                    var bookSize = _ctx.Data.RootTransform.rect.width / _ctx.Data.BookLayout.constraintCount;
                    bookSize -= (_ctx.Data.ScrollPadding * (_ctx.Data.BookLayout.constraintCount + 1)) / _ctx.Data.BookLayout.constraintCount;
                    _ctx.Data.BookLayout.cellSize = Vector2.one * bookSize;
                }

                public async UniTask AsyncInit()
                {
                    var rawBooks = await GetText("books.json");
                    var bookPaths = JsonConvert.DeserializeObject<List<string>>(rawBooks);

                    foreach (var bookPath in bookPaths)
                    {
                        var storyText = await GetText($"{bookPath}/story.json");
                        var story = new Ink.Runtime.Story(storyText);

                        var title = story.Continue();
                        var genres = new string[0];
                        var description = string.Empty;

                        while (story.canContinue)
                        {
                            var tempText = story.Continue();
                            tempText = tempText.Trim();

                            if (string.IsNullOrEmpty(tempText)) continue;

                            var rawTexts = tempText.Split(":");
                            var header = rawTexts.Length > 1 ?
                                rawTexts[0].Split("(").FirstOrDefault() :
                                string.Empty;
                            var attributes = rawTexts[0].Contains("(") ?
                                rawTexts[0].Split("(").LastOrDefault().Split(")").FirstOrDefault() :
                                string.Empty;
                            var body = rawTexts.Length > 1 ?
                                rawTexts[1] :
                                tempText;

                            var headerForLogic = header.Trim().ToLower();
                            switch (headerForLogic)
                            {
                                case "аннотация":
                                    description = body;
                                    continue;
                                case "жанры":
                                    genres = body.Split(",").ToArray();
                                    continue;
                            }
                        }

                        var image = await GetTexture($"{bookPath}/image.jpg");

                        AddBook(image, title, genres, description, () => 
                        {
                            _completeSource.TrySetResult($"{bookPath}/story.json");
                        });
                    }
                }

                private async UniTask<string> GetText(string localPath)
                {
                    using var request = UnityWebRequest.Get(GetPath(localPath));

                    SetHeaders(request);

                    await request.SendWebRequest();

                    return request.downloadHandler.text;
                }

                private async UniTask<Texture2D> GetTexture(string localPath)
                {
                    using var request = UnityWebRequestTexture.GetTexture(GetPath(localPath));

                    SetHeaders(request);

                    await request.SendWebRequest();

                    return DownloadHandlerTexture.GetContent(request);
                }

                private string GetPath(string localPath)
                {
                    return $"{Application.streamingAssetsPath}/Books/{localPath}";
                }

                private void SetHeaders(UnityWebRequest request)
                {
                    request.SetRequestHeader("Access-Control-Allow-Credentials", "true");
                    request.SetRequestHeader("Access-Control-Allow-Headers", "Accept, X-Access-Token, X-Application-Name, X-Request-Sent-Time");
                    request.SetRequestHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                    request.SetRequestHeader("Access-Control-Allow-Origin", "*");
                }

                private void AddBook(Texture mainImage, string title, string[] genres, string description, Action onClick)
                {
                    _ctx.Data.BookScrollUnit.gameObject.SetActive(false);
                    _ctx.Data.BookUnit.gameObject.SetActive(false);

                    var bookScrollUnit = UnityEngine.Object.Instantiate(_ctx.Data.BookScrollUnit, _ctx.Data.BookScrollUnit.transform.parent);
                    bookScrollUnit.gameObject.SetActive(true);
                    bookScrollUnit.SetData(mainImage, title, genres, description, onClick);

                    _units.Push(bookScrollUnit.gameObject);

                    var bookUnit = UnityEngine.Object.Instantiate(_ctx.Data.BookUnit, _ctx.Data.BookUnit.transform.parent);
                    bookUnit.gameObject.SetActive(true);
                    bookUnit.SetData(mainImage, title, genres, description, onClick);

                    _units.Push(bookUnit.gameObject);

                    UpdateScreen();
                }

                protected override UniTask OnAsyncDispose()
                {
                    while (_units.TryPop(out var unitGO))
                        UnityEngine.Object.Destroy(unitGO);

                    if (_ctx.Data.RootTransform != null)
                        _ctx.Data.RootTransform.gameObject.SetActive(false);
                    return base.OnAsyncDispose();
                }
            }

            public struct Ctx
            {
                public IReadOnlyReactiveCommand<float> OnUpdate;
                public Data Data;
            }

            private readonly Ctx _ctx;

            private readonly Logic _logic;

            public Entity(Ctx ctx, UniTaskCompletionSource<string> completeSource)
            {
                _ctx = ctx;

                _logic = new Logic(new Logic.Ctx
                {
                    OnUpdate = _ctx.OnUpdate,
                    Data = _ctx.Data
                }, completeSource).AddTo(this);
            }

            public async UniTask AsyncInit()
            {
                await _logic.AsyncInit();
            }
        }

        [Serializable]
        public struct Data
        {
            [SerializeField] private RectTransform _rootTransform;

            [SerializeField] private int _scrollPadding;
            [SerializeField] private LayoutElement _booksScrollLayout;
            [SerializeField] private GridLayoutGroup _booksLayout;

            [SerializeField] private BookUnit _bookScrollUnit;
            [SerializeField] private BookUnit _bookUnit;

            public readonly RectTransform RootTransform => _rootTransform;

            public readonly int ScrollPadding => _scrollPadding;
            public readonly LayoutElement BookScrollLayout => _booksScrollLayout;
            public readonly GridLayoutGroup BookLayout => _booksLayout;

            public readonly BookUnit BookScrollUnit => _bookScrollUnit;
            public readonly BookUnit BookUnit => _bookUnit;
        }
    }
}

