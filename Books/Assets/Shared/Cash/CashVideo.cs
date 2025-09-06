using Cysharp.Threading.Tasks;
using Shared.Disposable;
using System;
using System.IO;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Shared.Cash
{
    internal sealed class CashVideo : BaseDisposable
    {
        public struct Ctx
        {
            public IObservable<(byte[] data, string videoPath)> OnGetVideoRequest;
            public ReactiveCommand<string> GetVideoRequest;

            public ReactiveCommand<(string path, string fileName)> OnGetVideo;
            public IObservable<string> GetVideo;

            public Func<string, bool> IsCashed;

            public Func<string, string> ConvertPath;
        }

        private readonly Ctx _ctx;

        public CashVideo(Ctx ctx)
        {
            _ctx = ctx;

            _ctx.GetVideo.Subscribe(async fileName => await GetVideoAsync(fileName)).AddTo(this);
        }

        private async UniTask GetVideoAsync(string fileName)
        {
            if (_ctx.IsCashed.Invoke(fileName))
            {
                _ctx.OnGetVideo.Execute((_ctx.ConvertPath.Invoke(fileName), fileName));
            }
            else
            {
                var videoRequestDone = false;
                byte[] data = null;
                var disposable = _ctx.OnGetVideoRequest.Where(videoData => fileName == videoData.videoPath).Subscribe(videoData =>
                {
                    data = videoData.data;
                    videoRequestDone = true;
                });
                _ctx.GetVideoRequest.Execute(fileName);
                while (!videoRequestDone) await UniTask.Yield();
                disposable.Dispose();

                _ctx.OnGetVideo.Execute((VideoClipToCache(data, fileName), fileName));
            }
        }

        private string VideoClipToCache(byte[] data, string fileName)
        {
            var file = _ctx.ConvertPath.Invoke(fileName);
            if (File.Exists(file))
                File.Delete(file);

            File.WriteAllBytes(file, data);

            return file;
        }
    }
}

