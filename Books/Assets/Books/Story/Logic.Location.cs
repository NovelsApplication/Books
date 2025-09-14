using Cysharp.Threading.Tasks;
using Shared.Disposable;
using UniRx;
using UnityEngine;

namespace Books.Story
{
    internal partial class Logic
    {
        private Texture _locationImage;

        [Logic(LogicIdx.Location, LogicIdx.Локация)]
        private async UniTask<bool> RunLocation(string header, string attributes, string body)
        {
            await _ctx.Screen.HideLocation();

            _locationImage = null;

            var locationDone = false;
            _ctx.LocationImagePath.Value = $"{_ctx.StoryPath}/Locations/{body.Replace(" ", "_")}.png";
            var locationKey = "location";

            var onGetLocationDisp = _ctx.OnGetTexture.Where(data => data.key == locationKey).Subscribe(data =>
            {
                _locationImage = data.texture;
                locationDone = true;
            });
            _ctx.GetTexture.Execute((_ctx.LocationImagePath.Value, locationKey));
            while (!locationDone) await UniTask.Yield();
            onGetLocationDisp.Dispose();

            await _ctx.Screen.ShowLocation(_locationImage);

            return true;
        }
    }
}
