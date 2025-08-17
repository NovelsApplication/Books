using Cysharp.Threading.Tasks;
using Shared.Requests;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Shared.LocalCache 
{
    public static class Cacher
    {
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

