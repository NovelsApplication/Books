using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
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

            public ReactiveCommand<(AudioClip clip, string clipPath)> OnGetAudio;
            public IObservable<string> GetAudio;
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
                OnGetAudio = _ctx.OnGetAudio,
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

    public class BundleRequest : BaseDisposable 
    {
        public struct Ctx
        {
            public ReactiveCommand<(byte[] data, string assetPath)> OnGetBundle;
            public IObservable<string> GetBundle;

            public Func<string, UnityWebRequest> GetRequest;
        }

        private readonly Ctx _ctx;

        public BundleRequest(Ctx ctx) 
        {
            _ctx = ctx;

            _ctx.GetBundle.Subscribe(path => GetBundle(path)).AddTo(this);
        }

        private async void GetBundle(string localPath)
        {
            var path = $"Remote/WebGL/{localPath}";
#if UNITY_EDITOR
            path = $"Remote/Win/{localPath}";
#endif

            using var request = _ctx.GetRequest.Invoke(path);
            await request.SendWebRequest();

            _ctx.OnGetBundle.Execute((request.downloadHandler.data, localPath));
        }
    }

    public class TextureRawRequest : BaseDisposable
    {
        public struct Ctx
        {
            public ReactiveCommand<(byte[] data, string texturePath)> OnGetTextureRaw;
            public IObservable<string> GetTextureRaw;

            public Func<string, UnityWebRequest> GetRequest;
        }

        private readonly Ctx _ctx;

        public TextureRawRequest(Ctx ctx)
        {
            _ctx = ctx;

            _ctx.GetTextureRaw.Subscribe(path => GetTexture(path)).AddTo(this);
        }

        private async void GetTexture(string localPath)
        {
            using var request = _ctx.GetRequest.Invoke(localPath);
            await request.SendWebRequest();

            _ctx.OnGetTextureRaw.Execute((request.downloadHandler.data, localPath));
        }
    }

    public class TextRequest : BaseDisposable
    {
        public struct Ctx
        {
            public ReactiveCommand<(string text, string textPath)> OnGetText;
            public IObservable<string> GetText;

            public Func<string, UnityWebRequest> GetRequest;
        }

        private readonly Ctx _ctx;

        public TextRequest(Ctx ctx)
        {
            _ctx = ctx;

            _ctx.GetText.Subscribe(path => GetText(path)).AddTo(this);
        }

        private async void GetText(string localPath)
        {
            using var request = _ctx.GetRequest.Invoke(localPath);
            await request.SendWebRequest();

            _ctx.OnGetText.Execute((request.downloadHandler.text, localPath));
        }
    }

    public class AudioRequest : BaseDisposable
    {
        public struct Ctx
        {
            public ReactiveCommand<(AudioClip audio, string audioPath)> OnGetAudio;
            public IObservable<string> GetAudio;

            public Func<string, UnityWebRequest> GetRequest;
        }

        private readonly Ctx _ctx;

        public AudioRequest(Ctx ctx)
        {
            _ctx = ctx;

            _ctx.GetAudio.Subscribe(path => GetAudio(path)).AddTo(this);
        }

        private async void GetAudio(string localPath)
        {
            using var request = _ctx.GetRequest.Invoke(localPath);
            var dh = (DownloadHandlerAudioClip)request.downloadHandler;
            dh.compressed = false;
            dh.streamAudio = false;
            request.downloadHandler = dh;
            await request.SendWebRequest();

            dh.audioClip.LoadAudioData();
            while (dh.audioClip.loadState != AudioDataLoadState.Loaded) await UniTask.Yield();

            _ctx.OnGetAudio.Execute((dh.audioClip, localPath));
        }
    }

    public class AssetRequests
    {
        public async UniTask<T> GetData<T>(string localPath)
        {
            var path = GetPath(localPath);
            using var request = UnityWebRequest.Get(path);

            SetHeaders(request);

            await request.SendWebRequest();

            return JsonConvert.DeserializeObject<T>(request.downloadHandler.text);
        }

        private string GetPath(string localPath)
        {
            var result = $"{Application.streamingAssetsPath}/{localPath}";
            #if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            result = $"file://{result}";
            #endif
            return result;
        }

        private void SetHeaders(UnityWebRequest request)
        {
            request.SetRequestHeader("Access-Control-Allow-Credentials", "true");
            request.SetRequestHeader("Access-Control-Allow-Headers", "Accept, X-Access-Token, X-Application-Name, X-Request-Sent-Time");
            request.SetRequestHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            request.SetRequestHeader("Access-Control-Allow-Origin", "*");
        }
    }
}

