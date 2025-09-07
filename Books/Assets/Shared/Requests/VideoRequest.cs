using Cysharp.Threading.Tasks;
using Shared.Disposable;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

namespace Shared.Requests 
{
    public class VideoRequest : BaseDisposable
    {
        public struct Ctx
        {
            public ReactiveCommand<(byte[] videoData, string videoPath)> OnGetVideo;
            public IObservable<string> GetVideo;

            public Func<string, UnityWebRequest> GetRequest;
        }

        private readonly Ctx _ctx;

        public VideoRequest(Ctx ctx)
        {
            _ctx = ctx;

            _ctx.GetVideo.Subscribe(path => GetVideo(path)).AddTo(this);
        }

        private async void GetVideo(string localPath)
        {

            using var request = _ctx.GetRequest.Invoke(localPath);

            Debug.Log($"Try load video from: {request.url}");

            await request.SendWebRequest();

            _ctx.OnGetVideo.Execute((request.downloadHandler.data, localPath));
        }
    }
}

