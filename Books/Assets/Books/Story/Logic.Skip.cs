using Cysharp.Threading.Tasks;

namespace Books.Story
{
    internal partial class Logic
    {
        [Logic(LogicIdx.Название, LogicIdx.Жанры, LogicIdx.Бирка, LogicIdx.Раздел, LogicIdx.Постер, LogicIdx.Аннотация, LogicIdx.Серия)]
        private async UniTask<bool> RunSkip(string header, string attributes, string body)
        {
            await UniTask.NextFrame();
            return true;
        }
    }
}
