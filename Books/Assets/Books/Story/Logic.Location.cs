using Cysharp.Threading.Tasks;
using Shared.Disposable;
using UniRx;
using UnityEngine;

namespace Books.Story
{
    internal partial class Logic
    {
        private Texture _locationImage;
        private RenderTexture _videoTexture;

        [Logic(LogicIdx.Location, LogicIdx.Локация)]
        private async UniTask<bool> RunLocation(string header, string attributes, string body)
        {
            await _ctx.Screen.HideLocation();

            _locationImage = null;

            var locationDone = false;
            _ctx.LocationImagePath.Value = $"{_ctx.StoryPath}/Locations/{body.Replace(" ", "_")}.png";
            var locationKey = "location";

            _ctx.OnGetTexture.Where(data => data.key == locationKey).Subscribe(data =>
            {
                Debug.Log($"OnGetImage {_ctx.LocationImagePath.Value}");
                _locationImage = data.texture;
                locationDone = true;
            }).AddTo(this);
            _ctx.GetTexture.Execute((_ctx.LocationImagePath.Value, locationKey));

            while (!locationDone) await UniTask.Yield();

            await _ctx.Screen.ShowLocation(_locationImage, null);

            //video

            await _ctx.Screen.HideLocation();

            _locationImage = null;

            var videoDone = false;
            var locationVideoPath = $"{_ctx.StoryPath}/Videos/{body.Replace(" ", "_")}.mp4";

            if (_videoTexture == null)
            {
                _videoTexture = new RenderTexture(1080, 1920, 32);
            }

            var videoPath = string.Empty;
            _ctx.OnGetVideo.Where(data => data.fileName == locationVideoPath).Subscribe(data =>
            {
                Debug.Log($"OnGetVideo {data.path} / {locationVideoPath}");
                _videoTexture.name = data.path;
                videoPath = data.path;
                _locationImage = _videoTexture;
                videoDone = true;
            }).AddTo(this);
            _ctx.GetVideo.Execute(locationVideoPath);

            while (!videoDone) await UniTask.Yield();

            await _ctx.Screen.ShowLocation(_locationImage, videoPath);

            return true;
        }
    }
}
