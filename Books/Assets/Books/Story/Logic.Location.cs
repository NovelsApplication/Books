using Cysharp.Threading.Tasks;
using System;
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

            _ctx.LocationImagePath.Value = $"{_ctx.StoryPath}/Locations/{body.Replace(" ", "_")}.png";
            var locationKey = "location";

            var task = new ReactiveProperty<Func<UniTask<Texture2D>>>();
            _ctx.GetTexture.Execute((_ctx.LocationImagePath.Value, locationKey, task));
            _locationImage = await task.Value.Invoke();
            task.Dispose();

            await _ctx.Screen.ShowLocation(_locationImage);

            return true;
        }
    }
}
