using Cysharp.Threading.Tasks;
using Shared.LocalCache;
using UnityEngine;

namespace Books.Story
{
    internal partial class Logic
    {
        [Logic(LogicIdx.Location, LogicIdx.Локация)]
        private async UniTask<bool> RunLocation(string header, string attributes, string body)
        {
            await _ctx.Screen.HideLocation();

            Texture2D locationImage = null;

            if (!string.IsNullOrEmpty(body)) 
            {
                var locationName = $"{_ctx.RootFolderName}/Locations/{body.Replace(" ", "_")}.png";
                locationImage = await Cacher.GetTextureAsync(locationName);
            }

            await _ctx.Screen.ShowLocation(locationImage);

            return true;
        }
    }
}
