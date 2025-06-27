using Cysharp.Threading.Tasks;

namespace Books.Story
{
    internal partial class Logic
    {
        [Logic(LogicIdx.Music)]
        private async UniTask<bool> RunMusic(string header, string attributes, string body)
        {
            await UniTask.NextFrame();
            return true;
        }
    }
}
