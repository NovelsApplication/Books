using Cysharp.Threading.Tasks;

namespace Books.Story
{
    internal partial class Logic
    {
        [Logic(LogicIdx.��������, LogicIdx.�����, LogicIdx.�����, LogicIdx.������, LogicIdx.������, LogicIdx.���������, LogicIdx.�����, LogicIdx.���������)]
        private async UniTask<bool> RunSkip(string header, string attributes, string body)
        {
            await UniTask.NextFrame();
            return true;
        }
    }
}
