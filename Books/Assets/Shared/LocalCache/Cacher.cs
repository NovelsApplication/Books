using Cysharp.Threading.Tasks;
using System;
using System.IO;
using UnityEngine;

namespace Shared.LocalCache 
{
    public static class Cacher
    {
        public static bool IsCached(this string fileName) 
        {
            DirtyHackWitPlayerPrefs();
            var file = ConvertPath(fileName);

            return File.Exists(file);
        }

        public static Texture2D TextureFromCache(this string fileName) 
        {
            var rawData = FromCache(fileName);
            Texture2D image = new(0, 0);
            ImageConversion.LoadImage(image, rawData);
            return image;
        }

        private static byte[] FromCache(this string fileName) 
        {
            DirtyHackWitPlayerPrefs();

            var file = ConvertPath(fileName);

            using (var fs = File.OpenRead(file)) 
            {
                var buffer = new byte[(int)fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }

        public static Texture2D ToCache(this Texture2D data, string fileName) 
        {
            Debug.Log("ToCache0");
            var rawData = data.EncodeToPNG();
            Debug.Log($"ToCache1: {rawData.Length}");
            rawData.ToCache(fileName);
            Debug.Log($"ToCache2: {fileName}");
            return fileName.TextureFromCache();
        }

        private static byte[] ToCache(this byte[] data, string fileName) 
        {
            Debug.Log("ToCache1.1");
            DirtyHackWitPlayerPrefs();
            Debug.Log("ToCache1.2");
            var file = ConvertPath(fileName);
            Debug.Log("ToCache1.3");
            if (File.Exists(file))
                File.Delete(file);
            Debug.Log("ToCache1.4");
            using (var fs = File.Create(file))
            {
                Debug.Log("ToCache1.5");
                fs.Write(data, 0, data.Length);
            }
            Debug.Log("ToCache1.6");

            return data;
        }

        private static void DirtyHackWitPlayerPrefs() 
        {
            if (!PlayerPrefs.HasKey("Cacher"))
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

            return $"{localFilesPath}/{fileName}";
        }
    }
}

