using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Shared.Disposable;
using Shared.LocalCache;
using Shared.Reactive;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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

                            //UpdateScreen();
                        }
                    }).AddTo(this);

                    _ctx.Data.RootTransform.gameObject.SetActive(true);

                    //UpdateScreen();
                }

                private void UpdateScreen()
                {
                    var verticalGroup = _ctx.Data.RootTransform.GetComponentInChildren<VerticalLayoutGroup>(true);
                    verticalGroup.padding.top = _ctx.Data.ScrollPadding;
                    verticalGroup.padding.right = _ctx.Data.ScrollPadding;
                    verticalGroup.padding.bottom = _ctx.Data.ScrollPadding;
                    verticalGroup.padding.left = _ctx.Data.ScrollPadding;
                    verticalGroup.spacing = _ctx.Data.ScrollPadding;

                    //_ctx.Data.BookScrollLayout.minHeight = _ctx.Data.RootTransform.rect.width - _ctx.Data.ScrollPadding * 2;
                    var horizontalGroup = _ctx.Data.BookScrollLayout.GetComponentInChildren<HorizontalLayoutGroup>(true);
                    horizontalGroup.spacing = _ctx.Data.ScrollPadding * 2;
                    for (var j = 0; j < horizontalGroup.transform.childCount; j++)
                    {
                        if (!horizontalGroup.transform.GetChild(j).TryGetComponent<LayoutElement>(out var horizontalLayoutElement)) continue;
                        //horizontalLayoutElement.minWidth = _ctx.Data.RootTransform.rect.width - _ctx.Data.ScrollPadding * 2;
                    }

                    _ctx.Data.BookLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;

                    var bookSize = _ctx.Data.RootTransform.rect.width / _ctx.Data.BookLayout.constraintCount;
                    bookSize -= (_ctx.Data.BookLayout.spacing.x * (_ctx.Data.BookLayout.constraintCount + 1)) / _ctx.Data.BookLayout.constraintCount;
                    _ctx.Data.BookLayout.cellSize = new Vector2(bookSize, bookSize / _ctx.Data.AspectRatio);
                }

                public async UniTask AsyncInit()
                {
                    var rawBooks = await new AssetRequests().GetText("books.json");
                    var bookPaths = JsonConvert.DeserializeObject<List<string>>(rawBooks);

                    foreach (var bookPath in bookPaths)
                    {
                        var storyPath = $"{bookPath}/story.json";
                        var storyCacheName = storyPath.Replace("/", "_");
                        var storyText = string.Empty;
                        if (storyCacheName.IsCached()) 
                        {
                            Debug.Log($"Cached: {storyCacheName}");
                            storyText = storyCacheName.TextFromCache();
                        }
                        else 
                        {
                            Debug.Log($"NotCached: {storyCacheName}");
                            
                            var rawText = await new AssetRequests().GetText(storyPath);
                            storyText = rawText.ToCache(storyCacheName);
                        }
                        
                        var story = new Ink.Runtime.Story(storyText);

                        var title = story.Continue();
                        var genres = new string[0];
                        var description = string.Empty;

                        while (story.canContinue)
                        {
                            if (!story.Continue().TryProcessLine(out var header, out var attributes, out var body)) continue;

                            switch (header.ToLower())
                            {
                                case "���������":
                                    description = body;
                                    continue;
                                case "�����":
                                    genres = body.Split(",").ToArray();
                                    continue;
                            }
                        }

                        Texture2D image = null;
                        var imagePath = $"{bookPath}/image.jpg";
                        var imageCacheName = imagePath.Replace("/", "_");

                        if (imageCacheName.IsCached()) 
                        {
                            Debug.Log($"Cached: {imageCacheName}");
                            image = imageCacheName.TextureFromCache();
                        }
                        else 
                        {
                            Debug.Log($"NotCached: {imageCacheName}");
                            
                            var imageRaw = await new AssetRequests().GetTexture(imagePath);
                            image = imageRaw.ToCache(imageCacheName);
                        }

                        AddBook(image, title, genres, description, () => 
                        {
                            _completeSource.TrySetResult(storyPath);
                        });

                        _ctx.Data.BookScrollLayout.gameObject.SetActive(false);
                        await UniTask.NextFrame();
                        _ctx.Data.BookScrollLayout.gameObject.SetActive(true);
                    }
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

                    //UpdateScreen();
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
                await _logic.AsyncInit();
                await _logic.AsyncInit();
                await _logic.AsyncInit();
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
            [SerializeField] private float _aspectRatio;

            [SerializeField] private BookUnit _bookScrollUnit;
            [SerializeField] private BookUnit _bookUnit;

            public readonly RectTransform RootTransform => _rootTransform;

            public readonly int ScrollPadding => _scrollPadding;
            public readonly LayoutElement BookScrollLayout => _booksScrollLayout;
            public readonly GridLayoutGroup BookLayout => _booksLayout;
            public readonly float AspectRatio => _aspectRatio;

            public readonly BookUnit BookScrollUnit => _bookScrollUnit;
            public readonly BookUnit BookUnit => _bookUnit;
        }
    }
}

