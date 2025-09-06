using Cysharp.Threading.Tasks;
using Shared.Disposable;
using System;
using System.IO;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Shared.Cash
{
    public sealed class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public ReactiveCommand<(UnityEngine.Object bundle, string assetName)> OnGetBundle;
            public IObservable<(string assetPath, string assetName)> GetBundle;

            public ReactiveCommand<(string text, string textPath)> OnGetText;
            public IObservable<string> GetText;

            public ReactiveCommand<(string text, string textPath)> OnLoadText;
            public IObservable<string> LoadText;

            public IObservable<(string text, string textPath)> SaveText;

            public ReactiveCommand<(Texture2D texture, string key)> OnGetTexture;
            public IObservable<(string fileName, string key)> GetTexture;

            public ReactiveCommand<(AudioClip clip, string fileName)> OnGetMusic;
            public IObservable<string> GetMusic;

            public ReactiveCommand<(string path, string fileName)> OnGetVideo;
            public IObservable<string> GetVideo;

            public IObservable<Unit> ClearCash;
        }

        private readonly Ctx _ctx;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;

            var onGetBundleRequest = new ReactiveCommand<(byte[] data, string assetPath)>().AddTo(this);
            var getBundleRequest = new ReactiveCommand<string>().AddTo(this);

            var onGetTextureRawRequest = new ReactiveCommand<(byte[] data, string texturePath)>().AddTo(this);
            var getTextureRawRequest = new ReactiveCommand<string>().AddTo(this);

            var onGetTextRequest = new ReactiveCommand<(string text, string textPath)>().AddTo(this);
            var getTextRequest = new ReactiveCommand<string>().AddTo(this);

            var onGetAudioRequest = new ReactiveCommand<(AudioClip clip, string audioPath)>().AddTo(this);
            var getAudioRequest = new ReactiveCommand<string>().AddTo(this);

            var onGetVideoRequest = new ReactiveCommand<(byte[] data, string audioPath)>().AddTo(this);
            var getVideoRequest = new ReactiveCommand<string>().AddTo(this);

            new Requests.Entity(new Requests.Entity.Ctx
            {
                OnGetBundle = onGetBundleRequest,
                GetBundle = getBundleRequest,

                OnGetTextureRaw = onGetTextureRawRequest,
                GetTextureRaw = getTextureRawRequest,

                OnGetText = onGetTextRequest,
                GetText = getTextRequest,

                OnGetAudio = onGetAudioRequest,
                GetAudio = getAudioRequest,

                OnGetVideo = onGetVideoRequest,
                GetVideo = getVideoRequest,
            }).AddTo(this);

            PlayerPrefs.SetString("Cash", DateTime.UtcNow.ToString());

            new CashBundles(new CashBundles.Ctx
            {
                OnGetBundleRequest = onGetBundleRequest,
                GetBundleRequest = getBundleRequest,

                OnGetBundle = _ctx.OnGetBundle,
                GetBundle = _ctx.GetBundle,

                IsCashed = fileName => IsCashed(fileName),

                FromCash = fileName => FromCash(fileName),
                ToCash = (data, fileName) => ArrayToCash(data, fileName),
            }).AddTo(this);

            new CashTexts(new CashTexts.Ctx
            {
                OnGetTextRequest = onGetTextRequest,
                GetTextRequest = getTextRequest,

                OnGetText = _ctx.OnGetText,
                GetText = _ctx.GetText,

                OnLoadText = _ctx.OnLoadText,
                LoadText = _ctx.LoadText,

                SaveText = _ctx.SaveText,

                IsCashed = fileName => IsCashed(fileName),

                FromCash = fileName => FromCash(fileName),
                ToCash = (data, fileName) => ArrayToCash(data, fileName),
            }).AddTo(this);

            new CashTextures(new CashTextures.Ctx
            {
                OnGetTextureRawRequest = onGetTextureRawRequest,
                GetTextureRawRequest = getTextureRawRequest,

                OnGetTexture = _ctx.OnGetTexture,
                GetTexture = _ctx.GetTexture,

                IsCashed = fileName => IsCashed(fileName),

                FromCash = fileName => FromCash(fileName),
                ToCash = (data, fileName) => ArrayToCash(data, fileName),
            }).AddTo(this);

            new CashMusic(new CashMusic.Ctx
            {
                OnGetMusicRequest = onGetAudioRequest,
                GetMusicRequest = getAudioRequest,

                OnGetMusic = _ctx.OnGetMusic,
                GetMusic = _ctx.GetMusic,

                IsCashed = fileName => IsCashed(fileName),

                FromCash = fileName => FromCash(fileName),
                ToCash = (data, fileName) => ArrayToCash(data, fileName),

                ConvertPath = fileName => ConvertPath(fileName),
            }).AddTo(this);

            new CashVideo(new CashVideo.Ctx
            {
                OnGetVideoRequest = onGetVideoRequest,
                GetVideoRequest = getVideoRequest,

                OnGetVideo = _ctx.OnGetVideo,
                GetVideo = _ctx.GetVideo,

                IsCashed = fileName => IsCashed(fileName),

                ConvertPath = fileName => ConvertPath(fileName),
            }).AddTo(this);

            _ctx.ClearCash.Subscribe(_ => ClearCash()).AddTo(this);
        }

        private bool IsCashed(string fileName)
        {
            return false;

            var result = File.Exists(ConvertPath(fileName));

#if UNITY_EDITOR
            //result = false;
#endif

            return result;
        }

        private string GetLocalPath() 
        {
            var localFilesPath = $"{Application.persistentDataPath}/CachedFiles";
#if !UNITY_EDITOR && UNITY_WEBGL
            localFilesPath = "idbfs/CachedFiles";
#endif

            return localFilesPath;
        }

        private string ConvertPath(string fileName)
        {
            var localFilesPath = GetLocalPath();

            if (!Directory.Exists(localFilesPath))
                Directory.CreateDirectory(localFilesPath);

            var localExtraPath = fileName.Split('/');
            for (var i = 0; i < localExtraPath.Length - 1; i++)
            {
                localFilesPath += "/" + localExtraPath[i];
                if (!Directory.Exists(localFilesPath))
                    Directory.CreateDirectory(localFilesPath);
            }

            var result = $"{localFilesPath}/{localExtraPath.Last()}";

            return result;
        }

        private void ClearCash() 
        {
            var localFilesPath = GetLocalPath();

            if (Directory.Exists(localFilesPath))
                Directory.Delete(localFilesPath, true);
        }

        private byte[] FromCash(string fileName)
        {
            var file = ConvertPath(fileName);

            using (var fs = File.OpenRead(file))
            {
                var buffer = new byte[(int)fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }

        private void ArrayToCash(byte[] data, string fileName)
        {
            var file = ConvertPath(fileName);
            if (File.Exists(file))
                File.Delete(file);
            using (var fs = File.Create(file))
            {
                fs.Write(data, 0, data.Length);
            }
        }
    }
}

