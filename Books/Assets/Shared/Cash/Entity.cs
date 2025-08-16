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
            public ReactiveCommand<UnityEngine.Object> OnGetBundle;
            public IObservable<(string assetPath, string assetName)> GetBundle;
        }

        private readonly Ctx _ctx;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;

            PlayerPrefs.SetString("Cash", DateTime.UtcNow.ToString());

            new CashBundles(new CashBundles.Ctx
            {
                OnGetBundle = ctx.OnGetBundle,
                GetBundle = ctx.GetBundle,

                IsCashed = fileName => IsCashed(fileName),
                GetPath = fileName => ConvertPath(fileName),

                FromCash = fileName => FromCash(fileName),
                ToCash = (data, fileName) => ArrayToCash(data, fileName),
            }).AddTo(this);
        }

        private byte[] ArrayToCash(byte[] data, string fileName)
        {
            var file = ConvertPath(fileName);
            if (File.Exists(file))
                File.Delete(file);
            using (var fs = File.Create(file))
            {
                fs.Write(data, 0, data.Length);
            }

            return data;
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

        private bool IsCashed(string fileName)
        {
            var result = File.Exists(ConvertPath(fileName));

#if UNITY_EDITOR
            result = false;
#endif

            return result;
        }

        private string ConvertPath(string fileName)
        {
            var localFilesPath = $"{Application.persistentDataPath}/CachedFiles";
#if !UNITY_EDITOR && UNITY_WEBGL
            localFilesPath = "idbfs/CachedFiles";
#endif

            if (!Directory.Exists(localFilesPath))
                Directory.CreateDirectory(localFilesPath);

            var localExtraPath = fileName.Split('/');
            for (var i = 0; i < localExtraPath.Length - 1; i++)
            {
                localFilesPath += "/" + localExtraPath[i];
                if (!Directory.Exists(localFilesPath))
                    Directory.CreateDirectory(localFilesPath);
            }

            return $"{localFilesPath}/{localExtraPath.Last()}";
        }
    }
}

