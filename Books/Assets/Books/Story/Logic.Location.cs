using Cysharp.Threading.Tasks;
using Shared.Disposable;
using UniRx;
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

            var locationDone = false;
            var locationPath = $"{_ctx.StoryPath}/Locations/{body.Replace(" ", "_")}.png";
            var locationKey = "location";

            _ctx.OnGetTexture.Where(data => data.key == locationKey).Subscribe(data =>
            {
                _locationImage = data.texture;
                locationDone = true;
            }).AddTo(this);
            _ctx.GetTexture.Execute((locationPath, locationKey));
            while (!locationDone) await UniTask.Yield();

            await _ctx.Screen.ShowLocation(_locationImage);

            return true;
        }
    }
}
