using Cysharp.Threading.Tasks;
using Shared.Disposable;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

namespace Shared.Requests 
{
    public class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public ReactiveCommand<(byte[] data, string assetPath)> OnGetBundle;
            public IObservable<string> GetBundle;

            public ReactiveCommand<(byte[] data, string texturePath)> OnGetTextureRaw;
            public IObservable<string> GetTextureRaw;

            public ReactiveCommand<(string text, string textPath)> OnGetText;
            public IObservable<string> GetText;

            public IObservable<(string path, ReactiveProperty<Func<UniTask<AudioClip>>> task)> GetAudio;

            public ReactiveCommand<(byte[] data, string videoPath)> OnGetVideo;
            public IObservable<string> GetVideo;
        }

        private readonly Ctx _ctx;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;

            new BundleRequest(new BundleRequest.Ctx
            {
                OnGetBundle = _ctx.OnGetBundle,
                GetBundle = _ctx.GetBundle,

                GetRequest = GetRequest,
            }).AddTo(this);

            new TextureRawRequest(new TextureRawRequest.Ctx
            {
                OnGetTextureRaw = _ctx.OnGetTextureRaw,
                GetTextureRaw = _ctx.GetTextureRaw,

                GetRequest = GetRequest,
            }).AddTo(this);

            new TextRequest(new TextRequest.Ctx
            {
                OnGetText = _ctx.OnGetText,
                GetText = _ctx.GetText,

                GetRequest = GetRequest,
            }).AddTo(this);

            new AudioRequest(new AudioRequest.Ctx
            {
                GetAudio = _ctx.GetAudio,

                GetRequest = GetRequestMultimedia,
            }).AddTo(this);

            new VideoRequest(new VideoRequest.Ctx
            {
                OnGetVideo = _ctx.OnGetVideo,
                GetVideo = _ctx.GetVideo,

                GetRequest = GetRequest,
            }).AddTo(this);
        }

        private UnityWebRequest GetRequest(string path)
        {
            var request = UnityWebRequest.Get(GetPath(path));

            SetHeaders(request);

            return request;
        }

        private UnityWebRequest GetRequestMultimedia(string path)
        {
            var request = UnityWebRequestMultimedia.GetAudioClip(GetPath(path), AudioType.MPEG);

            SetHeaders(request);

            return request;
        }

        private void SetHeaders(UnityWebRequest request)
        {
            request.SetRequestHeader("Access-Control-Allow-Credentials", "true");
            request.SetRequestHeader("Access-Control-Allow-Headers", "Accept, X-Access-Token, X-Application-Name, X-Request-Sent-Time");
            request.SetRequestHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            request.SetRequestHeader("Access-Control-Allow-Origin", "*");
        }
        private string GetPath(string localPath)
        {
            var result = $"{Application.streamingAssetsPath}/{localPath}";
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            result = $"file://{result}";
#endif
            return result;
        }
    }
}

