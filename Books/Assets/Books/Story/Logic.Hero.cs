using Cysharp.Threading.Tasks;

namespace Books.Story
{
    internal partial class Logic
    {
        [Logic(LogicIdx.Hero, LogicIdx.Клавиатура)]
        private async UniTask<bool> RunHero(string header, string attributes, string body)
        {
            _mainCharacterName.Value = body.Trim();

            await UniTask.NextFrame();
            return true;
        }
    }
}
