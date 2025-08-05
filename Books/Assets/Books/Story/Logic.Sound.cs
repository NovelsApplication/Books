using Cysharp.Threading.Tasks;

namespace Books.Story
{
    internal partial class Logic
    {
        [Logic(LogicIdx.Sound, LogicIdx.Звук)]
        private async UniTask<bool> RunSound(string header, string attributes, string body)
        {
            await UniTask.NextFrame();
            return true;
        }
    }
}
