using Cysharp.Threading.Tasks;
using Shared.Disposable;
using Shared.Reactive;
using System;
using UnityEngine;
using UnityEngine.LowLevel;

namespace Books
{
    internal sealed class Books : BaseDisposableMB
    {
        private class Entity : BaseDisposable
        {
            public struct Ctx
            {
                public IReadOnlyReactiveCommand<float> OnUpdate;
                public Data Data;
            }

            private readonly Ctx _ctx;

            public Entity(Ctx ctx)
            {
                _ctx = ctx;

                AsyncInit();
            }

            private async void AsyncInit()
            {
                var loadingScreen = new LoadingScreen.LoadingScreen.Entity(new LoadingScreen.LoadingScreen.Entity.Ctx
                {
                    OnUpdate = _ctx.OnUpdate,
                    Data = _ctx.Data.LoadingScreenData,
                }).AddTo(this);

                loadingScreen.ShowImmediate();
                var bookScreenCompletionSource = new UniTaskCompletionSource<int>();
                var booksScreen = new UI.BooksScreen.Entity(new UI.BooksScreen.Entity.Ctx
                {
                    OnUpdate = _ctx.OnUpdate,
                    Data = _ctx.Data.BooksScreenData,
                },
                bookScreenCompletionSource).AddTo(this);
                await booksScreen.AsyncInit();
                await loadingScreen.Hide();

                while (bookScreenCompletionSource.GetStatus(0) != UniTaskStatus.Succeeded)
                    await UniTask.NextFrame();

                while (bookScreenCompletionSource.GetResult(0) != 1)
                    await UniTask.NextFrame();

                await loadingScreen.Show();
                await booksScreen.AsyncDispose();
                var storyScreen = new Story.StoryScreen.Entity(new Story.StoryScreen.Entity.Ctx
                {
                    OnUpdate = _ctx.OnUpdate,
                    Data = _ctx.Data.StoriesScreenData,
                }).AddTo(this);
                await storyScreen.LoadStory();
                await loadingScreen.Hide();
                await storyScreen.ShowStory();
            }
        }

        [Serializable]
        private struct Data
        {
            [SerializeField] private LoadingScreen.LoadingScreen.Data _loadingScreenData;
            [SerializeField] private UI.BooksScreen.Data _booksScreenData;
            [SerializeField] private Story.StoryScreen.Data _storyScreenData;

            public readonly LoadingScreen.LoadingScreen.Data LoadingScreenData => _loadingScreenData;
            public readonly UI.BooksScreen.Data BooksScreenData => _booksScreenData;
            public readonly Story.StoryScreen.Data StoriesScreenData => _storyScreenData;
        }

        [SerializeField] private Data _data;

        private IReactiveCommand<float> _onUpdate;

        private void OnEnable()
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            PlayerLoopHelper.Initialize(ref playerLoop);

            _onUpdate = new ReactiveCommand<float>().AddTo(this);
            _ = new Entity(new Entity.Ctx
            {
                OnUpdate = _onUpdate,
                Data = _data,
            }).AddTo(this);
        }

        private void Update()
        {
            _onUpdate.Execute(Time.deltaTime);
        }
    }
}

