using Cysharp.Threading.Tasks;
using Shared.Disposable;

namespace Books.Loading
{
    public sealed class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public Data Data;
        }

        private Ctx _ctx;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        public void ShowImmediate() => _ctx.Data.Screen.ShowImmediate();
        public void HideImmediate() => _ctx.Data.Screen.HideImmediate();

        public async UniTask Show() => await _ctx.Data.Screen.Show();
        public async UniTask Hide() => await _ctx.Data.Screen.Hide();
    }
}
