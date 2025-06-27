using Cysharp.Threading.Tasks;
using Shared.Disposable;
using System;

namespace Books.Story 
{
    public sealed class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public Data Data;
            public string RootFolderName;
            public string StoryText;
        }

        private readonly Ctx _ctx;

        private readonly Logic _logic;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;

            _logic = new Logic(new Logic.Ctx
            {
                Screen = _ctx.Data.Screen,
                RootFolderName = _ctx.RootFolderName,
                StoryText = _ctx.StoryText,
            }).AddTo(this);
        }

        public async UniTask ShowStoryProcess(Action onDone) => await _logic.ShowStoryProcess(onDone);

        public void ShowImmediate() => _ctx.Data.Screen.ShowImmediate();
        public void HideImmediate() => _ctx.Data.Screen.HideImmediate();

        protected override void OnDispose()
        {
            HideImmediate();
            base.OnDispose();
        }
    }
}
