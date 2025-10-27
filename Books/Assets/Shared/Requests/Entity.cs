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
            public IObservable<(string path, ReactiveProperty<Func<UniTask<byte[]>>> task)> GetBundle;

            public IObservable<(string path, ReactiveProperty<Func<UniTask<byte[]>>> task)> GetTexture;

            public IObservable<(string path, ReactiveProperty<Func<UniTask<string>>> task)> GetText;

            public IObservable<(string path, ReactiveProperty<Func<UniTask<AudioClip>>> task)> GetAudio;
        }

        private readonly Ctx _ctx;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;

            new BundleRequest(new BundleRequest.Ctx
            {
                GetBundle = _ctx.GetBundle,

                GetRequest = GetRequest,
            }).AddTo(this);

            new TextureRawRequest(new TextureRawRequest.Ctx
            {
                GetTexture = _ctx.GetTexture,

                GetRequest = GetRequest,
            }).AddTo(this);

            new TextRequest(new TextRequest.Ctx
            {
                GetText = _ctx.GetText,

                GetRequest = GetRequest,
            }).AddTo(this);

            new AudioRequest(new AudioRequest.Ctx
            {
                GetAudio = _ctx.GetAudio,

                GetRequest = GetRequestMultimedia,
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

