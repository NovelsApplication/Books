using Cysharp.Threading.Tasks;
using Shared.Requests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Shared.LocalCache 
{
    public static class Cacher
    {
        private static readonly Dictionary<string, AssetBundle> _bundles = new();
        public static async UniTask<UnityEngine.Object> GetBundleAsync(string assetPath, string assetName)
        {
            if (!_bundles.TryGetValue(assetPath, out var bundle))
            {
                DirtyHackWithPlayerPrefs();
                if (IsCached(assetPath)) 
                {
                    bundle = await BundleFromCache(assetPath);
                }
                else
                {
                    var bundleData = await new AssetRequests().GetBundle(assetPath);
                    bundle = await BundleToCache(bundleData, assetPath);
                }

                _bundles[assetPath] = bundle;
            }

            return await bundle.LoadAssetAsync(assetName);
        }

        public static async UniTask<string> GetTextAsync(this string fileName) 
        {
            DirtyHackWithPlayerPrefs();

            return IsCached(fileName) ?
                TextFromCache(fileName) :
                TextToCache(await new AssetRequests().GetText(fileName), fileName);
        }

        public static async UniTask<Texture2D> GetTextureAsync(this string fileName) 
        {
            DirtyHackWithPlayerPrefs();

            return IsCached(fileName) ?
                TextureFromCache(fileName) :
                TextureToCache(await new AssetRequests().GetTexture(fileName), fileName);
        }

        public static async UniTask<AudioClip> GetAudioClipAsync(this string fileName)
        {
            DirtyHackWithPlayerPrefs();

            return IsCached(fileName) ?
                AudioClipFromCache(fileName) :
                AudioClipToCache(await new AssetRequests().GetAudio(fileName), fileName);
        }

        private static bool IsCached(this string fileName) 
        {
#if UNITY_EDITOR
            return false;
#endif
            var file = ConvertPath(fileName);
            return File.Exists(file);
        }

        private static async UniTask<AssetBundle> BundleFromCache(this string fileName)
        {
            var rawData = FromCache(fileName);
            return await AssetBundle.LoadFromMemoryAsync(rawData);
        }

        private static AudioClip AudioClipFromCache(this string fileName)
        {
            using (Stream stream = File.Open(ConvertPath(fileName), FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                var audioData = (AudioClipData)binaryFormatter.Deserialize(stream);

                var audioClip = AudioClip.Create(audioData.Name, audioData.Samples.Length, audioData.Channels, audioData.Frequency, false);
                audioClip.SetData(audioData.Samples, 0);

                return audioClip;
            }
        }

        private static Texture2D TextureFromCache(this string fileName) 
        {
            var rawData = FromCache(fileName);
            Texture2D image = new(0, 0);
            ImageConversion.LoadImage(image, rawData);
            return image;
        }

        private static string TextFromCache(this string fileName) 
        {
            var rawData = FromCache(fileName);
            return Encoding.UTF8.GetString(rawData);
        }

        private static byte[] FromCache(this string fileName) 
        {
            var file = ConvertPath(fileName);

            using (var fs = File.OpenRead(file)) 
            {
                var buffer = new byte[(int)fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }

        private static Texture2D TextureToCache(this Texture2D data, string fileName) 
        {
            var rawData = data.EncodeToPNG();
            rawData.ArrayToCache(fileName);
            return fileName.TextureFromCache();
        }

        private static string TextToCache(this string data, string fileName) 
        {
            var rawData = Encoding.UTF8.GetBytes(data);
            rawData.ArrayToCache(fileName);
            return fileName.TextFromCache();
        }

        private static async UniTask<AssetBundle> BundleToCache(this byte[] data, string fileName)
        {
            data.ArrayToCache(fileName);
            return await fileName.BundleFromCache();
        }

        [Serializable]
        public struct AudioClipData 
        {
            public string Name;
            public int Channels;
            public int Frequency;
            public float[] Samples;

        }
        private static AudioClip AudioClipToCache(this AudioClip data, string fileName)
        {
            var samples = new float[data.samples];
            data.LoadAudioData();
            data.GetData(samples, 0);

            var rawData = new AudioClipData
            {
                Name = fileName.Split("/").Last(),
                Channels = data.channels,
                Frequency = data.frequency,
                Samples = samples,
            };

            var file = ConvertPath(fileName);
            if (File.Exists(file))
                File.Delete(file);
            using (var stream = File.Open(ConvertPath(fileName), FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, rawData);
            }

            return fileName.AudioClipFromCache();
        }

        private static byte[] ArrayToCache(this byte[] data, string fileName) 
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

        private static void DirtyHackWithPlayerPrefs() 
        {
            PlayerPrefs.SetString("Cacher", DateTime.UtcNow.ToString());
        }

        private static string ConvertPath(string fileName) 
        {
            var localFilesPath = $"{Application.persistentDataPath}/CachedFiles";
#if !UNITY_EDITOR && UNITY_WEBGL
            localFilesPath = "idbfs/CachedFiles";
#endif

            if (!Directory.Exists(localFilesPath))
                Directory.CreateDirectory(localFilesPath);

            var localExtraPath = fileName.Split('/');
            for (var  i = 0; i < localExtraPath.Length - 1; i++) 
            {
                localFilesPath += "/" + localExtraPath[i];
                if (!Directory.Exists(localFilesPath))
                    Directory.CreateDirectory(localFilesPath);
            }

            return $"{localFilesPath}/{localExtraPath.Last()}";
        }
    }
}

