using Cysharp.Threading.Tasks;
using Shared.LocalCache;

namespace Books.Story
{
    internal partial class Logic
    {
        [Logic(LogicIdx.Location, LogicIdx.Локация)]
        private async UniTask<bool> RunLocation(string header, string attributes, string body)
        {
            await _ctx.Screen.HideLocation();

            var locationName = $"{_ctx.RootFolderName}/Locations/{body.Replace(" ", "_")}.png";
            var locationImage = await Cacher.GetTextureAsync(locationName);

            await _ctx.Screen.ShowLocation(locationImage);

            return true;
        }
    }
}
