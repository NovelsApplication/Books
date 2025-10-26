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

            public IObservable<(string path, ReactiveProperty<Func<UniTask<string>>> task)> GetText;
            public IObservable<(string path, ReactiveProperty<Func<string>> task)> LoadText;
            public IObservable<(string path, string text)> SaveText;

            public IObservable<(string path, string key, ReactiveProperty<Func<UniTask<Texture2D>>> task)> GetTexture;

            public ReactiveCommand<(string path, ReactiveProperty<Func<UniTask<AudioClip>>> task)> GetMusic;

            public IObservable<Unit> ClearCash;
        }

        private readonly Ctx _ctx;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;

            var onGetBundleRequest = new ReactiveCommand<(byte[] data, string assetPath)>().AddTo(this);
            var getBundleRequest = new ReactiveCommand<string>().AddTo(this);

            var getTextureRequest = new ReactiveCommand<(string path, ReactiveProperty<Func<UniTask<byte[]>>> task)>().AddTo(this);

            var getTextRequest = new ReactiveCommand<(string path, ReactiveProperty<Func<UniTask<string>>> task)>().AddTo(this);

            var getAudioRequest = new ReactiveCommand<(string path, ReactiveProperty<Func<UniTask<AudioClip>>> task)>().AddTo(this);

            new Requests.Entity(new Requests.Entity.Ctx
            {
                OnGetBundle = onGetBundleRequest,
                GetBundle = getBundleRequest,

                GetTexture = getTextureRequest,

                GetText = getTextRequest,

                GetAudio = getAudioRequest,
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
                GetTextRequest = getTextRequest,

                GetText = _ctx.GetText,

                LoadText = _ctx.LoadText,

                SaveText = _ctx.SaveText,

                IsCashed = fileName => IsCashed(fileName),

                FromCash = fileName => FromCash(fileName),
                ToCash = (data, fileName) => ArrayToCash(data, fileName),
            }).AddTo(this);

            new CashTextures(new CashTextures.Ctx
            {
                GetTextureRequest = getTextureRequest,

                GetTexture = _ctx.GetTexture,

                IsCashed = fileName => IsCashed(fileName),

                FromCash = fileName => FromCash(fileName),
                ToCash = (data, fileName) => ArrayToCash(data, fileName),
            }).AddTo(this);

            new CashMusic(new CashMusic.Ctx
            {
                GetMusicRequest = getAudioRequest,

                GetMusic = _ctx.GetMusic,

                IsCashed = fileName => IsCashed(fileName),

                FromCash = fileName => FromCash(fileName),
                ToCash = (data, fileName) => ArrayToCash(data, fileName),

                ConvertPath = fileName => ConvertPath(fileName),
            }).AddTo(this);

            _ctx.ClearCash.Subscribe(_ => ClearCash()).AddTo(this);
        }

        private bool IsCashed(string fileName)
        {
            var result = File.Exists(ConvertPath(fileName));

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

