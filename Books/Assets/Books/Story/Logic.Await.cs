using Cysharp.Threading.Tasks;

namespace Books.Story 
{
    internal partial class Logic
    {
        [Logic(LogicIdx.Await, LogicIdx.ќжидание)]
        private async UniTask<bool> RunAwake(string header, string attributes, string body) 
        {
            if (int.TryParse(body, out var waitTime))
            {
                await UniTask.Delay(waitTime * 1000);
            }

            return true;
        }
    }
}

