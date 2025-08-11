using Cysharp.Threading.Tasks;
using Shared.LocalCache;
using UnityEngine;

namespace Books.Story
{
    internal partial class Logic
    {
        private Texture2D _locationImage;

        [Logic(LogicIdx.Location, LogicIdx.Локация)]
        private async UniTask<bool> RunLocation(string header, string attributes, string body)
        {
            await _ctx.Screen.HideLocation();

            _locationImage = null;

            if (!string.IsNullOrEmpty(body)) 
            {
                var locationName = $"{_ctx.RootFolderName}/Locations/{body.Replace(" ", "_")}.png";
                _locationImage = await Cacher.GetTextureAsync(locationName);
            }

            await _ctx.Screen.ShowLocation(_locationImage);

            return true;
        }
    }
}
