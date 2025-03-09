using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Shared.Disposable;
using Shared.Reactive;
using System;
using System.Collections.Generic;
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

                public Logic(Ctx ctx)
                {
                    _ctx = ctx;

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
                        var image = await GetTexture($"{bookPath}/Image.jpg");
                        var bookCardRaw = await GetText($"{bookPath}/Card.json");
                        var bookCard = JsonConvert.DeserializeObject<BookCard>(bookCardRaw);

                        AddBook(image, bookCard.Title, bookCard.Genres, bookCard.Description);
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

                private void AddBook(Texture mainImage, string title, List<string> genres, string description)
                {
                    _ctx.Data.BookScrollUnit.gameObject.SetActive(false);
                    _ctx.Data.BookUnit.gameObject.SetActive(false);

                    var bookScrollUnit = UnityEngine.Object.Instantiate(_ctx.Data.BookScrollUnit, _ctx.Data.BookScrollUnit.transform.parent);
                    bookScrollUnit.gameObject.SetActive(true);
                    bookScrollUnit.SetData(mainImage, title, genres, description);

                    _units.Push(bookScrollUnit.gameObject);

                    var bookUnit = UnityEngine.Object.Instantiate(_ctx.Data.BookUnit, _ctx.Data.BookUnit.transform.parent);
                    bookUnit.gameObject.SetActive(true);
                    bookUnit.SetData(mainImage, title, genres, description);

                    _units.Push(bookUnit.gameObject);

                    UpdateScreen();
                }

                protected override UniTask OnAsyncDispose()
                {
                    while (_units.TryPop(out var unitGO))
                        UnityEngine.Object.Destroy(unitGO);
                    return base.OnAsyncDispose();
                }
            }

            [Serializable]
            public struct BookCard
            {
                public string Title;
                public List<string> Genres;
                public string Description;
            }

            public struct Ctx
            {
                public IReadOnlyReactiveCommand<float> OnUpdate;
                public Data Data;
            }

            private readonly Ctx _ctx;

            private readonly Logic _logic;
            private UniTaskCompletionSource<int> _completeSource;

            public Entity(Ctx ctx, UniTaskCompletionSource<int> completeSource)
            {
                _ctx = ctx;

                _completeSource = completeSource;

                _logic = new Logic(new Logic.Ctx
                {
                    OnUpdate = _ctx.OnUpdate,
                    Data = _ctx.Data,
                }).AddTo(this);
            }

            public async UniTask AsyncInit()
            {
                await _logic.AsyncInit();

                TestWait();
            }

            private async void TestWait() 
            {
                await UniTask.Delay(5000);

                _completeSource.TrySetResult(1);
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

